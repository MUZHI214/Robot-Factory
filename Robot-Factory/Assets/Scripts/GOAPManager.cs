using System.Collections.Generic;
using GOAP;
using UnityEngine;

public class GOAPManager : MonoBehaviour
{
    public FactoryManager factoryManager;

    Action[] currentPlan;
    WorldState currentState;
    // WorldState newState = new WorldState();
    Robot robot;
    int currentActionIndex = 0;

    FloatGoal items = new FloatGoal("Items", 5);
    PositionGoal minePosition = new PositionGoal("Mine", 1, Vector2.up);

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

        // Add Goals
        currentState.FloatGoals.Add(items, 0);
        currentState.PositionGoals.Add(minePosition, this.gameObject.transform.position);

        // Create actions
        Action moveToMine = new Action("Move to Mine");
        moveToMine.PositionEffects.Add(minePosition, Vector2.up);

        Action mineItems = new Action("Mine");
        mineItems.PositionPreconditions.Add(minePosition, Vector2.up);
        mineItems.FloatEffects.Add(items, 1);

        factoryDomain.Actions.Add(mineItems);
        factoryDomain.Actions.Add(moveToMine);

        currentState.Domain = factoryDomain;

        Debug.Log(currentState);

        currentPlan = DFSPlan.plan(currentState, 4);
        Debug.Log("DFS Plan:");
        foreach (Action action in currentPlan)
            Debug.Log(action);
        Debug.Log("");
    }

    // Update is called once per frame
    void Update()
    {
        if (robot is null) return;

        // TODO: Figure out when to reset the index
        if (currentActionIndex >= currentPlan.Length)
        {
            currentState.SatisfiedActions = null;
            currentPlan = DFSPlan.plan(currentState, 4);
            currentActionIndex = 0;
        };

        if (currentPlan.Length != 0)
        {
            if (currentPlan[currentActionIndex].Name == "Move to Mine")
            {
                // Make sure entity isn't trying to mine
                if (robot.IsMining)
                    robot.StopMining();

                // Move the entity
                if (!targetSet)
                {
                    robot.SetTargetPosition(
                        Pathfinding.Instance.GetGrid().GetWorldPosition(
                            (int)minePosition.Position.x, (int)minePosition.Position.y
                        ) + new Vector3(
                                Pathfinding.Instance.GetGrid().GetCellSize() / 2,
                                Pathfinding.Instance.GetGrid().GetCellSize() / 2,
                                0
                            )
                    );
                    targetSet = true;
                }
                if (!robot.IsMoving)
                {
                    currentState.PositionGoals[minePosition] += currentPlan[currentActionIndex].PositionEffects[minePosition];
                    currentActionIndex++;
                }
            }
            else
            {
                // Start mining
                if (!robot.IsMining)
                    robot.StartMining();
                else
                {
                    if (robot.items[ItemType.Wood] > currentState.FloatGoals[items])
                    {
                        robot.StopMining();
                        currentState.FloatGoals[items]++;
                        currentActionIndex++;
                    }
                }
            }
        }
    }
}
