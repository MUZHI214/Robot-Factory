using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GOAP;
using UnityEngine;
using UnityEngine.AI;

public class Robot : Entity
{
    private int currentPathIndex;
    private List<Vector3> pathVectorList;

    public bool IsMoving { get; private set; }

    public Queue<GOAP.Action> CurrentPlan { get; set; }
    public Domain RobotDomain { get; set; }

    private bool targetSet = false;

    private float retrieveTimer = 0;

    [SerializeField]
    private GameObject towerPrefab;

    // Update is called once per frame
    public override void Update()
    {
        base.Update();

        Movement();

        if (GOAPManager.currentState is null || CurrentPlan is null)
        {
            return;
        }

        if (CurrentPlan.Count != 0)
        {
            GOAP.Action currentAction = CurrentPlan.Peek();
            if (currentAction is null)
            {
                CurrentPlan.Dequeue();
                return;
            }

            ItemType itemType = Enum.Parse<ItemType>(currentAction.Name.Split(' ')[1].Trim());

            if (currentAction.Name.Split(' ')[0].Trim() == "Mine")
            {
                FloatGoal itemGoal = (FloatGoal)currentAction.FloatEffects.Keys.First();

                if (!targetSet)
                {
                    var closestDist = float.MaxValue;
                    ItemMine closestMine = null;
                    FactoryManager.Instance.itemMines[itemType].ForEach(mine =>
                    {
                        var distToMine = Vector2.Distance(this.transform.position, mine.transform.position);
                        if (distToMine < closestDist)
                        {
                            closestDist = distToMine;
                            closestMine = mine;
                        }
                    });

                    if (closestMine is not null)
                    {
                        SetTargetPosition(closestMine.transform.position);
                        targetSet = true;
                    }
                } // Only reach here once the robot can start mining
                else if (!this.IsMining && currentPathIndex >= pathVectorList.Count)
                {
                    // Start mining
                    this.StartMining();
                }
                else if (this.items[itemType] > GOAPManager.currentState.FloatGoals[itemGoal])
                {
                    targetSet = false;
                    GOAPManager.currentState = currentAction.GetSuccessor(GOAPManager.currentState);
                    this.StopMining();
                    CurrentPlan.Dequeue();
                }
            }
            else if (currentAction.Name.Split(' ')[0].Trim() == "Craft")
            {
                // Make sure entity isn't trying to mine
                if (this.IsMining)
                    this.StopMining();

                if (!targetSet)
                {
                    SetTargetPosition(currentAction.TargetPosition);
                    targetSet = true;
                }  // Only reach here once the robot can start crafting
                else if (currentPathIndex >= pathVectorList.Count)
                {
                    // If the bot fails to place items, recreate the plan
                    if (this.PlaceItems())
                    {
                        targetSet = false;
                        GOAPManager.currentState = currentAction.GetSuccessor(GOAPManager.currentState);
                        CurrentPlan.Dequeue();
                    }
                    else
                    {
                        targetSet = false;
                        CurrentPlan.Clear();
                    }
                }
            }
            else if (currentAction.Name.Split(' ')[0].Trim() == "Retrieve")
            {
                // Make sure entity isn't trying to mine
                if (this.IsMining)
                    this.StopMining();

                if (!targetSet)
                {
                    SetTargetPosition(currentAction.TargetPosition);
                    targetSet = true;
                    retrieveTimer = 0;
                } // Only reach here once the robot can start crafting
                else if (currentPathIndex >= pathVectorList.Count)
                {
                    // If the bot fails to retirieve items, recreate the plan
                    if (this.RetrieveItems())
                    {
                        targetSet = false;
                        GOAPManager.currentState = currentAction.GetSuccessor(GOAPManager.currentState);
                        CurrentPlan.Dequeue();
                    }
                    else if (factory is not null && retrieveTimer >= factory.craftTime)
                    {
                        // Force the plan to be remade if the robot is waiting for too long
                        targetSet = false;
                        CurrentPlan.Clear();
                    }

                    retrieveTimer += Time.deltaTime;
                }
            }
            else if (currentAction.Name == "Place Tower")
            {
                var towerPosition = Vector2.zero;
                var allTowers = GameObject.FindObjectsOfType<Tower>();
                // TODO: find a valid place to put a tower and Instantiate it at that location
                foreach (var point in TDManager.main.path)
                {
                    int gridX = 0;
                    int gridY = 0;
                    Pathfinding.Instance.GetGrid().GetXY(point.position, out gridX, out gridY);
                    var neighbors = Pathfinding.Instance.GetNeighbourList(
                        Pathfinding.Instance.GetNode(
                            gridX,
                            gridY
                        )
                    );

                    for (int i = 0; i < neighbors.Count; i++)
                    {
                        for (int j = 0; j < allTowers.Length; j++)
                        {

                            Pathfinding.Instance.GetGrid().GetXY(allTowers[j].transform.position, out gridX, out gridY);
                            if (neighbors[i].x != gridX || neighbors[i].y != gridY)
                            {
                                towerPosition = Pathfinding.Instance.GetGrid().GetWorldPosition(neighbors[i].x, neighbors[i].y);
                                towerPosition.x += Pathfinding.Instance.GetGrid().GetCellSize() / 2;
                                towerPosition.y += Pathfinding.Instance.GetGrid().GetCellSize() / 2;
                                break;
                            }
                        }
                    }

                    if (towerPosition != Vector2.zero)
                        break;
                }

                if (towerPosition != Vector2.zero)
                {
                    // Instantiate(towerPrefab, towerPosition, Quaternion.identity, null);
                    GOAPManager.currentState = currentAction.GetSuccessor(GOAPManager.currentState);
                }
                CurrentPlan.Dequeue();
            }
        }
        else
        {
            // Recreate plan
            CurrentPlan.Clear();
            GOAPManager.currentState.Domain = this.RobotDomain;

            var planArray = DFSPlan.plan(GOAPManager.currentState, GOAPManager.PlanDepth);
            for (int i = 0; i < planArray.Length; i++)
            {
                CurrentPlan.Enqueue(planArray[i]);
            }
        }

    }

    private void Movement()
    {
        if (pathVectorList != null && pathVectorList.Count > 0)
        {
            Vector3 targetPosition = pathVectorList[currentPathIndex];

            if (Vector3.Distance(transform.position, targetPosition) > 1f)
            {
                Vector3 moveDir = (targetPosition - transform.position).normalized;

                float distanceBefore = Vector3.Distance(transform.position, targetPosition);
                //set animation

                transform.position = transform.position + moveDir * speed * Time.deltaTime;
            }
            else
            {
                currentPathIndex++;
                if (currentPathIndex >= pathVectorList.Count)
                {
                    StopMoving();
                }
            }
        }
        else
        {
            //set animation
        }
    }

    private void StopMoving()
    {
        pathVectorList.Clear();
        IsMoving = false;
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }

    public void SetTargetPosition(Vector3 targetPosition)
    {
        currentPathIndex = 0;
        pathVectorList = Pathfinding.Instance.FindPath(GetPosition(), targetPosition);
        if (pathVectorList != null && pathVectorList.Count > 1)
        {
            pathVectorList.RemoveAt(0);
        }

        IsMoving = true;
    }
}
