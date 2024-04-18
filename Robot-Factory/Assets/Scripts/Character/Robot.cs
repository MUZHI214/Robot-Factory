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

            if (currentAction.Name.Split(' ')[0].Trim() == "Mine")
            {
                FloatGoal itemGoal = (FloatGoal)currentAction.FloatEffects.Keys.First();
                ItemType itemType = Enum.Parse<ItemType>(itemGoal.Name.Split(' ')[0].Trim());

                // Start mining
                if (!currentAction.Robot.IsMining)
                    currentAction.Robot.StartMining();

                if (this.itemMine is null && !targetSet)
                {
                    float dist = float.PositiveInfinity;
                    ItemMine closestMine = null;
                    FactoryManager.Instance.itemMines[itemType].ForEach(mine =>
                    {
                        var distToMine = Vector2.Distance(this.transform.position, mine.transform.position);
                        if (distToMine < dist)
                        {
                            dist = distToMine;
                            closestMine = mine;
                        }
                    });

                    if (closestMine is not null)
                    {
                        SetTargetPosition(closestMine.transform.position);
                        targetSet = true;
                    }

                }
                else if (currentAction.Robot.items[itemType] > GOAPManager.currentState.FloatGoals[itemGoal])
                {
                    GOAPManager.currentState = currentAction.GetSuccessor(GOAPManager.currentState);
                    currentAction.Robot.StopMining();
                    CurrentPlan.Dequeue();
                    targetSet = false;
                }
            }
            else if (currentAction.Name.Split(' ')[0].Trim() == "Craft")
            {
                var itemType = Enum.Parse<ItemType>(currentAction.Name.Split(' ')[1].Trim());

                // Make sure entity isn't trying to mine
                if (currentAction.Robot.IsMining)
                    currentAction.Robot.StopMining();


                if (this.factory is null && !targetSet)
                {
                    float dist = float.PositiveInfinity;
                    Factory closestFact = null;
                    FactoryManager.Instance.factories[itemType].ForEach(fact =>
                    {
                        var distToFact = Vector2.Distance(this.transform.position, fact.transform.position);
                        if (distToFact < dist)
                        {
                            dist = distToFact;
                            closestFact = fact;
                        }
                    });

                    if (closestFact is not null)
                    {
                        SetTargetPosition(closestFact.transform.position);
                        targetSet = true;
                    }

                }
                else if (currentAction.Robot.PlaceItems())
                {
                    GOAPManager.currentState = currentAction.GetSuccessor(GOAPManager.currentState);
                    CurrentPlan.Dequeue();
                }
            }
            else if (currentAction.Name.Split(' ')[0].Trim() == "Retrieve")
            {
                var itemType = Enum.Parse<ItemType>(currentAction.Name.Split(' ')[1].Trim());

                // Make sure entity isn't trying to mine
                if (currentAction.Robot.IsMining)
                    currentAction.Robot.StopMining();

                if (this.factory is null && !targetSet)
                {
                    float dist = float.PositiveInfinity;
                    Factory closestFact = null;
                    FactoryManager.Instance.factories[itemType].ForEach(fact =>
                    {
                        var distToFact = Vector2.Distance(this.transform.position, fact.transform.position);
                        if (distToFact < dist)
                        {
                            dist = distToFact;
                            closestFact = fact;
                        }
                    });

                    if (closestFact is not null)
                    {
                        SetTargetPosition(closestFact.transform.position);
                        targetSet = true;
                    }

                }
                else if (currentAction.Robot.RetrieveItems())
                {
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
        if (pathVectorList != null)
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
        pathVectorList = null;
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
