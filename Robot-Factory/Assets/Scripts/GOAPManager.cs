using System;
using System.Collections.Generic;
using System.Linq;
using GOAP;
using UnityEngine;

public class GOAPManager : MonoBehaviour
{
    public FactoryManager factoryManager;

    GOAP.Action[] currentPlan;
    WorldState[] allStates;
    WorldState currentState;
    // WorldState newState = new WorldState();
    Robot robot;
    int currentActionIndex = 0;

    Dictionary<ItemType, FloatGoal> itemGoals = new Dictionary<ItemType, FloatGoal>();
    private static Dictionary<ItemType, FloatGoal> craftedGoals = new Dictionary<ItemType, FloatGoal>();

    bool targetSet = false;
    PositionGoal robotPositionGoal;

    // Start is called before the first frame update
    void Start()
    {
        if (!this.TryGetComponent<Robot>(out robot))
        {
            Debug.LogError("GOAPManager script must be attached to a GameObject of type Robot!");
            return;
        }

        // Create initial world state
        Domain factoryDomain = new Domain();
        currentState = new WorldState();
        currentState.Debug = false;

        robotPositionGoal = new PositionGoal("Robot Position", 2, robot.transform.position);
        currentState.PositionGoals.Add(robotPositionGoal, robot.transform.position);

        foreach (ItemType item in Enum.GetValues(typeof(ItemType)))
        {
            int contentment = 0;
            if (Item.recipes[item] is not null)
            {
                contentment = 10 + Item.recipes[item].Count + 1;

                // Keep track of amount of crafted items sitting at factory
                if (!craftedGoals.ContainsKey(item))
                {
                    var craftedGoal = new FloatGoal("Crafted " + item.ToString(), contentment - 1);
                    craftedGoals.Add(item, craftedGoal);
                }
                currentState.FloatGoals.Add(craftedGoals[item], 0);
            }

            // Keep track of items in inventory
            var itemGoal = new FloatGoal(item.ToString(), contentment);
            itemGoals.Add(item, itemGoal);
            currentState.FloatGoals.Add(itemGoal, 0);
        }

        foreach (var (mineType, mineList) in factoryManager.itemMines)
        {
            foreach (var mine in mineList)
            {
                GOAP.Action goToMine = new("Move to Mine - " + mineType);
                goToMine.PositionEffects.Add(robotPositionGoal, mine.transform.position);
                goToMine.FloatRangePreconditions.Add(itemGoals[mineType], new(0, 5)); // Only try to go to mine when not full

                GOAP.Action mineAction = new("Mine " + mineType);
                mineAction.FloatEffects.Add(itemGoals[mineType], 1);
                mineAction.PositionPreconditions.Add(robotPositionGoal, mine.transform.position);
                mineAction.FloatRangePreconditions.Add(itemGoals[mineType], new(0, 5)); // Only try to actually mine when not full

                factoryDomain.Actions.Add(goToMine);
                factoryDomain.Actions.Add(mineAction);
            }
        }

        foreach (var (factoryType, factoryList) in factoryManager.factories)
        {
            foreach (var factory in factoryList)
            {
                GOAP.Action goToFactory = new("Move to Factory - " + factoryType);
                goToFactory.PositionEffects.Add(robotPositionGoal, factory.transform.position);

                GOAP.Action craft = new("Craft " + factoryType);
                craft.FloatEffects.Add(craftedGoals[factoryType], 1);
                craft.PositionPreconditions.Add(robotPositionGoal, factory.transform.position);

                GOAP.Action retrieve = new("Retrieve " + factoryType);
                retrieve.FloatEffects.Add(craftedGoals[factoryType], -1);
                retrieve.FloatEffects.Add(itemGoals[factoryType], 1);
                retrieve.PositionPreconditions.Add(robotPositionGoal, factory.transform.position);
                retrieve.FloatRangePreconditions.Add(craftedGoals[factoryType], new(1, 6));

                foreach (var recipe in Item.recipes[factoryType])
                {
                    goToFactory.FloatRangePreconditions.Add(itemGoals[recipe.Key], new(recipe.Value, 6));
                    craft.FloatEffects.Add(itemGoals[recipe.Key], -recipe.Value);
                    craft.FloatRangePreconditions.Add(itemGoals[recipe.Key], new(recipe.Value, 6));
                }

                factoryDomain.Actions.Add(goToFactory);
                factoryDomain.Actions.Add(craft);
                factoryDomain.Actions.Add(retrieve);
            }

            currentState.FloatGoals[craftedGoals[factoryType]] = factoryList.Sum((fact) => fact.producedNum);
        }

        currentState.Domain = factoryDomain;

        Debug.Log(currentState);

        currentPlan = DFSPlan.plan(currentState, 4);
        Debug.Log("DFS Plan:");
        foreach (GOAP.Action action in currentPlan)
            Debug.Log(action);
        Debug.Log("");
    }

    // Update is called once per frame
    void Update()
    {
        if (robot is null) return;
        if (currentPlan is null || currentPlan.All((act) => act is null))
        {
            Debug.LogError("Empty plan created!");
            return;
        };

        if (currentActionIndex >= currentPlan.Length)
        {
            currentState.SatisfiedActions = null;
            currentPlan = DFSPlan.plan(currentState, 4);
            currentActionIndex = 0;

            Debug.Log("DFS Plan:");
            foreach (GOAP.Action action in currentPlan)
                Debug.Log(action);
            Debug.Log("");
        };

        if (currentPlan.Length != 0)
        {
            GOAP.Action currentAction = currentPlan[currentActionIndex];
            if (currentAction is null)
            {
                currentActionIndex++;
                return;
            }

            if (currentAction.Name.Split(' ')[0].Trim() == "Move")
            {
                // Make sure entity isn't trying to mine
                if (robot.IsMining)
                    robot.StopMining();

                // Make sure target position is only set one time
                if (!targetSet)
                {
                    robot.SetTargetPosition(currentAction.PositionEffects.Values.First());
                    targetSet = true;
                }
                if (!robot.IsMoving)
                {
                    currentState = currentAction.GetSuccessor(currentState);
                    currentActionIndex++;
                    targetSet = false;
                }
            }
            else if (currentAction.Name.Split(' ')[0].Trim() == "Mine")
            {
                FloatGoal itemGoal = (FloatGoal)currentAction.FloatEffects.Keys.First();
                ItemType itemType = Enum.Parse<ItemType>(itemGoal.Name);

                // Start mining
                if (!robot.IsMining)
                    robot.StartMining();

                if (robot.items[itemType] > currentState.FloatGoals[itemGoal])
                {
                    currentState = currentAction.GetSuccessor(currentState);
                    robot.StopMining();
                    currentActionIndex++;
                }
            }
            else if (currentAction.Name.Split(' ')[0].Trim() == "Craft")
            {
                // Make sure entity isn't trying to mine
                if (robot.IsMining)
                    robot.StopMining();

                currentState = currentAction.GetSuccessor(currentState);
                robot.PlaceItems();
                currentActionIndex++;
            }
            else if (currentAction.Name.Split(' ')[0].Trim() == "Retrieve")
            {
                // Make sure entity isn't trying to mine
                if (robot.IsMining)
                    robot.StopMining();

                if (robot.RetrieveItems())
                {
                    currentState = currentAction.GetSuccessor(currentState);
                    currentActionIndex++;
                }
            }

            robotPositionGoal.Position = robot.transform.position;
        }
    }
}
