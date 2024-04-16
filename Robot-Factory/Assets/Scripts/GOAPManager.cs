using System;
using System.Collections.Generic;
using System.Linq;
using GOAP;
using UnityEngine;

public class GOAPManager : MonoBehaviour
{
    public FactoryManager factoryManager;

    GOAP.Action[] currentPlan;
    WorldState currentState;
    // WorldState newState = new WorldState();
    Robot robot;
    int currentActionIndex = 0;

    Dictionary<ItemType, FloatGoal> itemGoals = new Dictionary<ItemType, FloatGoal>();
    private static Dictionary<ItemType, List<PositionGoal>> minePositions = new Dictionary<ItemType, List<PositionGoal>>();

    bool targetSet = false;

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
        currentState.Debug = true;

        // Add Goals & Actions
        foreach (ItemType type in Enum.GetValues(typeof(ItemType)))
        {
            var itemGoal = new FloatGoal("Item - " + type, 1);
            itemGoals.Add(type, itemGoal);
            currentState.FloatGoals.Add(itemGoals[type], 0);

            GOAP.Action mineItems = new GOAP.Action("Mine");
            mineItems.PositionPreconditionsUseOr = true;
            mineItems.FloatRangePreconditions.Add(itemGoal, new Tuple<float, float>(0, 5));

            minePositions.Add(type, new List<PositionGoal>());
            foreach (var mine in factoryManager.itemMines[type])
            {
                var posGoal = new PositionGoal("Mine - " + type, 1, mine.transform.position);
                minePositions[type].Add(posGoal);
                currentState.PositionGoals.Add(posGoal, robot.transform.position);

                GOAP.Action moveToMine = new GOAP.Action("Move to Mine");
                moveToMine.PositionEffects.Add(posGoal, mine.transform.position);

                mineItems.PositionPreconditions.Add(posGoal, mine.transform.position);

                factoryDomain.Actions.Add(moveToMine);
            }

            mineItems.FloatEffects.Add(itemGoal, 1);

            factoryDomain.Actions.Add(mineItems);
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

        if (currentActionIndex >= currentPlan.Length)
        {
            currentState.SatisfiedActions = null;
            currentPlan = DFSPlan.plan(currentState, 4);
            currentActionIndex = 0;
        };

        if (currentPlan is null) return;
        if (currentPlan.Length != 0)
        {
            GOAP.Action currentAction = currentPlan[currentActionIndex];

            if (currentAction.Name == "Move to Mine")
            {
                // Make sure entity isn't trying to mine
                if (robot.IsMining)
                    robot.StopMining();

                PositionGoal posGoal = (PositionGoal)currentPlan[currentActionIndex].PositionEffects.Keys.First();

                // Make sure target position is only set one time
                if (!targetSet)
                {
                    robot.SetTargetPosition(posGoal.Position);
                    targetSet = true;
                }
                if (!robot.IsMoving)
                {
                    currentActionIndex++;
                    currentState.PositionGoals[posGoal] = robot.transform.position;
                    targetSet = false;
                }
            }
            else if (currentAction.Name == "Mine")
            {
                FloatGoal itemGoal = (FloatGoal)currentAction.FloatEffects.Keys.First();
                ItemType itemType = Enum.Parse<ItemType>(itemGoal.Name.Split('-')[1].Trim());

                // Start mining
                if (!robot.IsMining)
                    robot.StartMining();

                if (robot.items[itemType] > currentState.FloatGoals[itemGoal])
                {
                    robot.StopMining();
                    currentState.FloatGoals[itemGoal]++;
                    currentActionIndex++;
                }
            }
        }
    }
}
