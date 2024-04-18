using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GOAP;
using Unity.Content;
using UnityEngine;

public class GOAPManager : MonoBehaviour
{
    public FactoryManager factoryManager;
    public static int PlanDepth = 5;

    public static WorldState currentState = null;

    Dictionary<Robot, Dictionary<ItemType, FloatGoal>> itemGoals = new();
    Dictionary<Factory, FloatGoal> craftedGoals = new();
    FloatGoal placedTowers = new("Placed Towers", 1000);

    Stopwatch towerTimer = new();


    // Start is called before the first frame update
    void Start()
    {
        // Create initial world state
        currentState = new WorldState();
        currentState.Debug = false;
        currentState.FloatGoals.Add(placedTowers, 0);

        foreach (Robot robot in factoryManager.robots)
        {
            itemGoals.Add(robot, new());
            Domain factoryDomain = robot.RobotDomain = new();

            foreach (ItemType item in Enum.GetValues(typeof(ItemType)))
            {
                int contentment = 0;
                if (Item.recipes[item] is not null)
                    contentment = 10;
                if (item == ItemType.Tower)
                    contentment = 100;

                // Keep track of items in inventory
                var itemGoal = new FloatGoal(item.ToString() + " " + robot.name, contentment);
                itemGoals[robot].Add(item, itemGoal);
                currentState.FloatGoals.Add(itemGoal, 0);
            }

            foreach (var (mineType, mineList) in factoryManager.itemMines)
            {
                GOAP.Action mineAction = new("Mine " + mineType + " " + robot.name, robot);
                mineAction.FloatEffects.Add(itemGoals[robot][mineType], 1);
                mineAction.FloatRangePreconditions.Add(itemGoals[robot][mineType], new(0, 5)); // Only try to actually mine when not full

                factoryDomain.Actions.Add(mineAction);
            }

            foreach (var (factoryType, factoryList) in factoryManager.factories)
            {
                foreach (var factory in factoryList)
                {
                    if (!craftedGoals.ContainsKey(factory))
                    {
                        var craftedGoal = new FloatGoal("Crafted " + factory, 5);
                        craftedGoals.Add(factory, craftedGoal);
                        currentState.FloatGoals.Add(craftedGoals[factory], 0);
                    }

                    GOAP.Action craft = new("Craft " + factoryType, robot);
                    craft.FloatEffects.Add(craftedGoals[factory], 1);
                    craft.TargetPosition = factory.transform.position;

                    GOAP.Action retrieve = new("Retrieve " + factoryType, robot);
                    retrieve.FloatEffects.Add(craftedGoals[factory], -1);
                    retrieve.FloatEffects.Add(itemGoals[robot][factoryType], 1);
                    retrieve.FloatRangePreconditions.Add(craftedGoals[factory], new(1, 20));
                    retrieve.TargetPosition = factory.transform.position;

                    foreach (var recipe in Item.recipes[factoryType])
                    {
                        craft.FloatEffects.Add(itemGoals[robot][recipe.Key], -recipe.Value);
                        craft.FloatRangePreconditions.Add(itemGoals[robot][recipe.Key], new(recipe.Value, 6));
                    }

                    factoryDomain.Actions.Add(craft);
                    factoryDomain.Actions.Add(retrieve);
                }
            }

            GOAP.Action placeTower = new("Place Tower", robot);
            placeTower.FloatEffects.Add(placedTowers, 1);
            placeTower.FloatEffects.Add(itemGoals[robot][ItemType.Tower], -1);
            placeTower.FloatRangePreconditions.Add(itemGoals[robot][ItemType.Tower], new(1, 20));
            placeTower.FloatRangePreconditions.Add(placedTowers, new(0, 20));

            factoryDomain.Actions.Add(placeTower);

            robot.CurrentPlan = new();
        }

        // Recreate plan
        foreach (Robot robot in factoryManager.robots)
        {
            currentState.Domain = robot.RobotDomain;

            var planArray = DFSPlan.plan(currentState, PlanDepth);
            for (int i = 0; i < planArray.Length; i++)
            {
                if (planArray[i] is null) continue;
                planArray[i].Robot.CurrentPlan.Enqueue(planArray[i]);
            }
        }
        towerTimer.Start();
    }

    // Update is called once per frame
    void Update()
    {
        // Keep number of crafted items up to date
        foreach (var (factoryType, factoryList) in factoryManager.factories)
        {
            foreach (var factory in factoryList)
            {
                currentState.FloatGoals[craftedGoals[factory]] = factory.producedNum;
            }
        }

        if (currentState.FloatGoals[placedTowers] >= 10 && towerTimer.IsRunning)
        {
            towerTimer.Stop();
            UnityEngine.Debug.Log(towerTimer.Elapsed);
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