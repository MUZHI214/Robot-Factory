using System.Diagnostics;
using UnityEngine;

namespace GOAP
{
    public static class DFSPlan
    {
        public static Action[] plan(WorldState state, int maxDepth)
        {
            WorldState[] states = new WorldState[maxDepth + 1];
            Action[] actions = new Action[maxDepth];

            states[0] = state;
            int currentDepth = 0;

            Action[] currentPlan = new Action[maxDepth];
            float bestUtility = float.MinValue;

            while (currentDepth >= 0)
            {
                if (currentDepth >= maxDepth)
                {
                    float currentUtility = states[currentDepth].GetContentment();

                    if (currentUtility > bestUtility)
                    {
                        bestUtility = currentUtility;
                        actions.CopyTo(currentPlan, 0);
                    }
                    currentDepth -= 1;
                }
                else
                {
                    Action nextAction = states[currentDepth].NextAction();

                    if (nextAction != null)
                    {
                        states[currentDepth + 1] = nextAction.GetSuccessor(states[currentDepth]);
                        actions[currentDepth] = nextAction;
                        currentDepth += 1;
                    }
                    else currentDepth -= 1;
                }
            }
            return currentPlan;
        }
    }
}
