using System;
using System.Collections.Generic;
using System.Linq;
using GOAP;
using UnityEngine;

public class GOAPManager : MonoBehaviour
{
    public FactoryManager factoryManager;
    public static int PlanDepth = 5;

    public static WorldState currentState = null;

    Dictionary<Robot, Dictionary<ItemType, FloatGoal>> itemGoals = new();
    Dictionary<ItemType, FloatGoal> craftedGoals = new();
    Dictionary<ItemType, FloatGoal> beingCraftedGoals = new();
    public static Dictionary<Robot, PositionGoal> robotPositions = new();

    // Start is called before the first frame update
    void Start()
    {
        // Create initial world state
        currentState = new WorldState();
        currentState.Debug = false;

        foreach (Robot robot in factoryManager.robots)
        {
            itemGoals.Add(robot, new());
            Domain factoryDomain = robot.RobotDomain = new();

            var robotPositionGoal = new PositionGoal("Position " + robot.name, 2, robot.transform.position);
            robotPositions.Add(robot, robotPositionGoal);
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
                        var beingCraftedGoal = new FloatGoal("Being Crafted " + item.ToString(), contentment - 5);
                        beingCraftedGoals.Add(item, beingCraftedGoal);
                        currentState.FloatGoals.Add(beingCraftedGoals[item], 0);

                        var craftedGoal = new FloatGoal("Crafted " + item.ToString(), contentment - 5);
                        craftedGoals.Add(item, craftedGoal);
                        currentState.FloatGoals.Add(craftedGoals[item], 0);
                    }
                }

                // Keep track of items in inventory
                var itemGoal = new FloatGoal(item.ToString() + " " + robot.name, contentment);
                itemGoals[robot].Add(item, itemGoal);
                currentState.FloatGoals.Add(itemGoal, 0);
            }

            foreach (var (mineType, mineList) in factoryManager.itemMines)
            {
                foreach (var mine in mineList)
                {
                    GOAP.Action goToMine = new("Move to Mine - " + mineType + " " + robot.name, robot);
                    goToMine.PositionEffects.Add(robotPositionGoal, mine.transform.position);
                    goToMine.FloatRangePreconditions.Add(itemGoals[robot][mineType], new(0, 5)); // Only try to go to mine when not full

                    GOAP.Action mineAction = new("Mine " + mineType + " " + robot.name, robot);
                    mineAction.FloatEffects.Add(itemGoals[robot][mineType], 1);
                    mineAction.PositionPreconditions.Add(robotPositionGoal, mine.transform.position);
                    mineAction.FloatRangePreconditions.Add(itemGoals[robot][mineType], new(0, 5)); // Only try to actually mine when not full

                    factoryDomain.Actions.Add(goToMine);
                    factoryDomain.Actions.Add(mineAction);
                }
            }

            // TODO: Make craft action robot independent?
            foreach (var (factoryType, factoryList) in factoryManager.factories)
            {
                foreach (var factory in factoryList)
                {
                    GOAP.Action goToFactory = new("Move to Factory - " + factoryType + " " + robot.name, robot);
                    goToFactory.PositionEffects.Add(robotPositionGoal, factory.transform.position);

                    GOAP.Action craft = new("Craft " + factoryType + " " + robot.name, robot);
                    craft.PositionPreconditions.Add(robotPositionGoal, factory.transform.position);
                    craft.FloatEffects.Add(beingCraftedGoals[factoryType], 1);

                    GOAP.Action retrieve = new("Retrieve " + factoryType + " " + robot.name, robot);
                    retrieve.FloatEffects.Add(craftedGoals[factoryType], -1);
                    retrieve.FloatEffects.Add(itemGoals[robot][factoryType], 1);
                    retrieve.PositionPreconditions.Add(robotPositionGoal, factory.transform.position);
                    retrieve.FloatRangePreconditions.Add(craftedGoals[factoryType], new(1, 20));

                    foreach (var recipe in Item.recipes[factoryType])
                    {
                        goToFactory.FloatRangePreconditions.Add(itemGoals[robot][recipe.Key], new(recipe.Value, 6));
                        craft.FloatEffects.Add(itemGoals[robot][recipe.Key], -recipe.Value);
                        craft.FloatRangePreconditions.Add(itemGoals[robot][recipe.Key], new(recipe.Value, 6));
                    }

                    factoryDomain.Actions.Add(goToFactory);
                    factoryDomain.Actions.Add(craft);
                    factoryDomain.Actions.Add(retrieve);
                }

                currentState.FloatGoals[craftedGoals[factoryType]] = factoryList.Sum((fact) => fact.producedNum);
                robot.CurrentPlan = new();

            }
        }

        // Recreate plan
        foreach (Robot robot in factoryManager.robots)
        {
            currentState.Domain = robot.RobotDomain;

            var planArray = DFSPlan.plan(currentState, PlanDepth);
            Debug.Log("DFS Plan:");
            for (int i = 0; i < planArray.Length; i++)
            {
                planArray[i].Robot.CurrentPlan.Enqueue(planArray[i]);
                Debug.Log(planArray[i]);
            }
            Debug.Log("");
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Keep number of crafted items up to date
        foreach (var (factoryType, factoryList) in factoryManager.factories)
        {
            currentState.FloatGoals[craftedGoals[factoryType]] = factoryList.Sum(fact => fact.producedNum);
            currentState.FloatGoals[beingCraftedGoals[factoryType]] = factoryList.Sum(fact => fact.numInProgress);
        }

        // if (factoryManager.robots.All(robot => robot.CurrentPlan.Count == 0))
        // {
        //     factoryManager.robots.ForEach(robot => robot.CurrentPlan.Clear());

        //     var planArray = DFSPlan.plan(currentState, PlanDepth);
        //     Debug.Log("DFS Plan:");
        //     for (int i = 0; i < planArray.Length; i++)
        //     {
        //         planArray[i].Robot.CurrentPlan.Enqueue(planArray[i]);
        //         Debug.Log(planArray[i]);
        //     }
        //     Debug.Log("");
        // }

        // if (currentPlan is null || currentPlan.All((act) => act is null))
        // {
        //     Debug.LogError("Empty plan created!");
        //     return;
        // };

        // if (factoryManager.robots.Any(robot => robot.CurrentPlan.Count <= 0))
        // {
        //     currentPlan = DFSPlan.plan(currentState, 4);

        //     factoryManager.robots.ForEach(robot => robot.CurrentPlan.Clear());

        //     for (int i = 0; i < currentPlan.Length; i++)
        //     {
        //         currentPlan[i].Robot.CurrentPlan.Enqueue(currentPlan[i]);
        //     }
        // }

        // if (currentActionIndex >= currentPlan.Length)
        // {
        //     currentState.SatisfiedActions = null;
        //     currentPlan = DFSPlan.plan(currentState, 4);
        //     currentActionIndex = 0;

        //     Debug.Log("DFS Plan:");
        //     foreach (GOAP.Action action in currentPlan)
        //         Debug.Log(action);
        //     Debug.Log("");
        // };

        // if (currentPlan.Length != 0)
        // {
        //     GOAP.Action currentAction = currentPlan[currentActionIndex];
        //     if (currentAction is null)
        //     {
        //         currentActionIndex++;
        //         return;
        //     }

        //     if (currentAction.Name.Split(' ')[0].Trim() == "Move")
        //     {
        //         // Make sure entity isn't trying to mine
        //         if (currentAction.Robot.IsMining)
        //             currentAction.Robot.StopMining();

        //         // Make sure target position is only set one time
        //         if (!targetSet[currentAction.Robot])
        //         {
        //             currentAction.Robot.SetTargetPosition(currentAction.PositionEffects.Values.First());
        //             targetSet[currentAction.Robot] = true;
        //         }
        //         if (!currentAction.Robot.IsMoving)
        //         {
        //             currentState = currentAction.GetSuccessor(currentState);
        //             currentActionIndex++;
        //             targetSet[currentAction.Robot] = false;
        //         }
        //     }
        //     else if (currentAction.Name.Split(' ')[0].Trim() == "Mine")
        //     {
        //         FloatGoal itemGoal = (FloatGoal)currentAction.FloatEffects.Keys.First();
        //         ItemType itemType = Enum.Parse<ItemType>(itemGoal.Name);

        //         // Start mining
        //         if (!currentAction.Robot.IsMining)
        //             currentAction.Robot.StartMining();

        //         if (currentAction.Robot.items[itemType] > currentState.FloatGoals[itemGoal])
        //         {
        //             currentState = currentAction.GetSuccessor(currentState);
        //             currentAction.Robot.StopMining();
        //             currentActionIndex++;
        //         }
        //     }
        //     else if (currentAction.Name.Split(' ')[0].Trim() == "Craft")
        //     {
        //         // Make sure entity isn't trying to mine
        //         if (currentAction.Robot.IsMining)
        //             currentAction.Robot.StopMining();

        //         currentState = currentAction.GetSuccessor(currentState);
        //         currentAction.Robot.PlaceItems();
        //         currentActionIndex++;
        //     }
        //     else if (currentAction.Name.Split(' ')[0].Trim() == "Retrieve")
        //     {
        //         // Make sure entity isn't trying to mine
        //         if (currentAction.Robot.IsMining)
        //             currentAction.Robot.StopMining();

        //         if (currentAction.Robot.RetrieveItems())
        //         {
        //             currentState = currentAction.GetSuccessor(currentState);
        //             currentActionIndex++;
        //         }
        //     }
        // }
    }
}