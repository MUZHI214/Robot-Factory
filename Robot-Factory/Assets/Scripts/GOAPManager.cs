using System;
using System.Collections.Generic;
using System.Linq;
using GOAP;
using Unity.VisualScripting;
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

        var factoryPosition = factoryManager.factories[ItemType.Tool][0].transform.position;
        var minePosition = factoryManager.itemMines[ItemType.Wood][0].transform.position;

        PositionGoal robotPositionGoal = new PositionGoal("Robot Position", 2, robot.transform.position);

        FloatGoal heldWood = new FloatGoal("Mine - Wood", 1);
        FloatGoal heldTools = new FloatGoal("Held Tools", 5);
        FloatGoal toolGoal = new FloatGoal("Crafted Tools", 4);

        // Initial state
        currentState.PositionGoals.Add(robotPositionGoal, robot.transform.position);
        currentState.FloatGoals.Add(heldWood, 0);
        currentState.FloatGoals.Add(heldTools, 0);
        currentState.FloatGoals.Add(toolGoal, 0);

        GOAP.Action retrieveTools = new GOAP.Action("Retrieve");
        retrieveTools.FloatEffects.Add(toolGoal, -1);
        retrieveTools.FloatEffects.Add(heldTools, 1);
        retrieveTools.FloatRangePreconditions.Add(toolGoal, new Tuple<float, float>(1, 6));

        GOAP.Action craftTool = new GOAP.Action("Craft");
        craftTool.FloatEffects.Add(heldWood, -5);
        craftTool.FloatEffects.Add(toolGoal, 1);
        craftTool.PositionPreconditions.Add(robotPositionGoal, factoryPosition);
        craftTool.FloatRangePreconditions.Add(toolGoal, new Tuple<float, float>(0, 5));
        craftTool.FloatPreconditions.Add(heldWood, 5);

        GOAP.Action moveToFactory = new GOAP.Action("Move to Factory");
        moveToFactory.PositionEffects.Add(robotPositionGoal, factoryPosition);
        moveToFactory.FloatPreconditions.Add(heldWood, 5);

        GOAP.Action mineWood = new GOAP.Action("Mine");
        mineWood.FloatEffects.Add(heldWood, 1);
        mineWood.PositionPreconditions.Add(robotPositionGoal, minePosition);
        mineWood.FloatRangePreconditions.Add(heldWood, new Tuple<float, float>(0, 5));

        GOAP.Action moveToMine = new GOAP.Action("Move to Mine");
        moveToMine.PositionEffects.Add(robotPositionGoal, minePosition);
        moveToMine.FloatRangePreconditions.Add(heldWood, new Tuple<float, float>(0, 5));

        factoryDomain.Actions.Add(retrieveTools);
        factoryDomain.Actions.Add(craftTool);
        factoryDomain.Actions.Add(retrieveTools);
        factoryDomain.Actions.Add(moveToFactory);
        factoryDomain.Actions.Add(mineWood);
        factoryDomain.Actions.Add(moveToMine);

        // Add Goals & Actions
        // foreach (ItemType type in Enum.GetValues(typeof(ItemType)))
        // {
        //     var itemGoal = new FloatGoal("Item - " + type, 1);
        //     itemGoals.Add(type, itemGoal);
        //     currentState.FloatGoals.Add(itemGoals[type], 0);

        //     if (factoryManager.itemMines.ContainsKey(type))
        //     {
        //         // Mining items
        //         GOAP.Action mineItems = new GOAP.Action("Mine");
        //         mineItems.PositionPreconditionsUseOr = true;
        //         mineItems.FloatRangePreconditions.Add(itemGoal, new Tuple<float, float>(0, 5));
        //         mineItems.FloatEffects.Add(itemGoal, 1);

        //         minePositions.Add(type, new List<PositionGoal>());
        //         foreach (var mine in factoryManager.itemMines[type])
        //         {
        //             var posGoal = new PositionGoal("Mine - " + type, 1, mine.transform.position);
        //             minePositions[type].Add(posGoal);
        //             currentState.PositionGoals.Add(posGoal, robot.transform.position);

        //             GOAP.Action moveToMine = new GOAP.Action("Move to Mine");
        //             moveToMine.PositionEffects.Add(posGoal, mine.transform.position);

        //             mineItems.PositionPreconditions.Add(posGoal, mine.transform.position);

        //             factoryDomain.Actions.Add(moveToMine);
        //         }

        //         factoryDomain.Actions.Add(mineItems);
        //     }

        //     if (factoryManager.factories.ContainsKey(type))
        //     {// Retrieving/Crafting factory made items
        //         FloatGoal craftedGoal = new FloatGoal("Crafted - " + type, 5);
        //         craftedGoals.Add(type, craftedGoal);
        //         currentState.FloatGoals.Add(craftedGoals[type], 0);

        //         GOAP.Action retrieveItems = new GOAP.Action("Retrieve");
        //         retrieveItems.PositionPreconditionsUseOr = true; // Robot can be at any factory that has an item
        //         retrieveItems.FloatRangePreconditions.Add(itemGoal, new Tuple<float, float>(0, 5));
        //         retrieveItems.FloatRangePreconditions.Add(craftedGoal, new Tuple<float, float>(1, 6));

        //         GOAP.Action craftItems = new GOAP.Action("Craft");
        //         craftItems.PositionPreconditionsUseOr = true; // Robot can be at any factory that has an item
        //         craftItems.FloatRangePreconditions.Add(craftedGoal, new Tuple<float, float>(0, 5));

        //         factoryPositions.Add(type, new List<PositionGoal>());
        //         foreach (var factory in factoryManager.factories[type])
        //         {
        //             var posGoal = new PositionGoal("Factory - " + type, 2, factory.transform.position);
        //             factoryPositions[type].Add(posGoal);
        //             currentState.PositionGoals.Add(posGoal, robot.transform.position);

        //             GOAP.Action moveToFactoryCraft = new GOAP.Action("Move to Factory Craft");
        //             moveToFactoryCraft.PositionEffects.Add(posGoal, factory.transform.position);

        //             foreach (var item in Item.recipes[factory.itemToProduce])
        //             {
        //                 moveToFactoryCraft.FloatPreconditions.Add(itemGoals[item.Key], item.Value);
        //             }
        //             factoryDomain.Actions.Add(moveToFactoryCraft);

        //             GOAP.Action moveToFactoryRetrieve = new GOAP.Action("Move to Factory Retrieve");
        //             moveToFactoryRetrieve.PositionEffects.Add(posGoal, factory.transform.position);
        //             moveToFactoryRetrieve.FloatRangePreconditions.Add(craftedGoal, new Tuple<float, float>(0, 5));

        //             retrieveItems.PositionPreconditions.Add(posGoal, factory.transform.position);
        //             craftItems.PositionPreconditions.Add(posGoal, factory.transform.position);

        //             itemGoals[factory.itemToProduce].Contentment = 5;

        //             foreach (var item in Item.recipes[factory.itemToProduce])
        //             {
        //                 craftItems.FloatPreconditions.Add(itemGoals[item.Key], item.Value);
        //                 craftItems.FloatEffects.Add(itemGoals[item.Key], -item.Value);
        //             }
        //         }

        //         retrieveItems.FloatEffects.Add(itemGoal, 1);
        //         factoryDomain.Actions.Add(retrieveItems);
        //         craftItems.FloatEffects.Add(craftedGoal, 1);
        //         factoryDomain.Actions.Add(craftItems);
        //     }
        // }

        currentState.Domain = factoryDomain;

        Debug.Log(currentState);

        var tup = DFSPlan.plan(currentState, 4);
        currentPlan = tup.Item1;
        allStates = tup.Item2;
        Debug.Log("DFS Plan:");
        foreach (GOAP.Action action in currentPlan)
            Debug.Log(action);
        Debug.Log("");
    }

    // Update is called once per frame
    void Update()
    {
        if (robot is null) return;
        if (currentPlan is null)
        {
            currentState.SatisfiedActions = null;
            var tup = DFSPlan.plan(currentState, 4);
            currentPlan = tup.Item1;
            allStates = tup.Item2;
            currentActionIndex = 0;
            return;
        };

        if (currentActionIndex >= currentPlan.Length)
        {
            //currentState.SatisfiedActions = null;
            var tup = DFSPlan.plan(currentState, 4);
            currentPlan = tup.Item1;
            allStates = tup.Item2;
            currentActionIndex = 0;
        };

        if (currentPlan.Length != 0)
        {
            GOAP.Action currentAction = currentPlan[currentActionIndex];
            if (currentPlan is null)
            {
                currentActionIndex++;
                return;
            }

            if (currentAction.Name == "Move to Mine" || currentAction.Name == "Move to Factory")
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
            else if (currentAction.Name == "Mine")
            {
                FloatGoal itemGoal = (FloatGoal)currentAction.FloatEffects.Keys.First();
                ItemType itemType = Enum.Parse<ItemType>(itemGoal.Name.Split('-')[1].Trim());

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
            else if (currentAction.Name == "Craft")
            {
                // Make sure entity isn't trying to mine
                if (robot.IsMining)
                    robot.StopMining();

                currentState = currentAction.GetSuccessor(currentState);
                robot.PlaceItems();
                currentActionIndex++;
            }
            else if (currentAction.Name == "Retrieve")
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

        }
    }

    // TODO: Look into Action.GetSuccessor
    // void UpdateState(WorldState state, GOAP.Action action)
    // {
    //     foreach (var goal in itemGoals)
    //     {
    //         state.FloatGoals[goal.Value] += action.FloatEffects[goal.Value];
    //     }

    //     foreach (var goal in minePositions)
    //     {
    //         foreach (var posGoal in goal.Value)
    //         {
    //             state.PositionGoals[posGoal] = action.PositionEffects[posGoal];
    //         }
    //     }

    //     foreach (var goal in factoryPositions)
    //     {
    //         foreach (var posGoal in goal.Value)
    //         {
    //             state.PositionGoals[posGoal] = action.PositionEffects[posGoal];
    //         }
    //     }

    //     foreach (var goal in craftedGoals)
    //     {
    //         state.FloatGoals[goal.Value] += action.FloatEffects[goal.Value];
    //     }
    // }
}
