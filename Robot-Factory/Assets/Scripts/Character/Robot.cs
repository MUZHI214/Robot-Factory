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
                    var closestDist = float.MaxValue;
                    Factory closestFact = null;
                    FactoryManager.Instance.factories[itemType].ForEach(fact =>
                    {
                        var distToFact = Vector2.Distance(this.transform.position, fact.transform.position);
                        if (distToFact < closestDist)
                        {
                            closestDist = distToFact;
                            closestFact = fact;
                        }
                    });

                    if (closestFact is not null)
                    {
                        SetTargetPosition(closestFact.transform.position);
                        targetSet = true;
                    }
                }  // Only reach here once the robot can start crafting
                else if (currentPathIndex >= pathVectorList.Count && this.PlaceItems())
                {
                    targetSet = false;
                    GOAPManager.currentState = currentAction.GetSuccessor(GOAPManager.currentState);
                    CurrentPlan.Dequeue();
                }
            }
            else if (currentAction.Name.Split(' ')[0].Trim() == "Retrieve")
            {
                // Make sure entity isn't trying to mine
                if (this.IsMining)
                    this.StopMining();

                if (!targetSet)
                {
                    var closestDist = float.MaxValue;
                    Factory closestFact = null;
                    FactoryManager.Instance.factories[itemType].ForEach(fact =>
                    {
                        var distToFact = Vector2.Distance(this.transform.position, fact.transform.position);
                        if (distToFact < closestDist)
                        {
                            closestDist = distToFact;
                            closestFact = fact;
                        }
                    });

                    if (closestFact is not null)
                    {
                        SetTargetPosition(closestFact.transform.position);
                        targetSet = true;
                    }
                } // Only reach here once the robot can start crafting
                else if (currentPathIndex >= pathVectorList.Count)
                {
                    targetSet = false;
                    // If the bot fails to retirieve items, recreate the plan
                    if (this.RetrieveItems())
                    {
                        GOAPManager.currentState = currentAction.GetSuccessor(GOAPManager.currentState);
                        CurrentPlan.Dequeue();
                    }
                    else
                        CurrentPlan.Clear();
                }
            }
        }
        else
        {
            // Recreate plan
            CurrentPlan.Clear();
            GOAPManager.currentState.Domain = this.RobotDomain;

            var planArray = DFSPlan.plan(GOAPManager.currentState, GOAPManager.PlanDepth);
            Debug.Log(this.name + " DFS Plan:");
            for (int i = 0; i < planArray.Length; i++)
            {
                CurrentPlan.Enqueue(planArray[i]);
                Debug.Log(planArray[i]);
            }
            Debug.Log("");
        }

    }

    private void RecreatePlan()
    {
        CurrentPlan.Clear();
        GOAPManager.currentState.Domain = this.RobotDomain;

        var planArray = DFSPlan.plan(GOAPManager.currentState, GOAPManager.PlanDepth);
        Debug.Log(this.name + " DFS Plan:");
        for (int i = 0; i < planArray.Length; i++)
        {
            CurrentPlan.Enqueue(planArray[i]);
            Debug.Log(planArray[i]);
        }
        Debug.Log("");
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
