using System;
using System.Diagnostics;
using Unity.VisualScripting;
using UnityEngine;

namespace GOAP
{
    public static class DFSPlan
    {
        public static Tuple<Action[], WorldState[]> plan(WorldState state, int maxDepth)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            WorldState[] states = new WorldState[maxDepth + 1];
            Action[] actions = new Action[maxDepth];

            states[0] = state;
            int currentDepth = 0;

            Action[] currentPlan = new Action[maxDepth];
            float bestUtility = float.MinValue;

            if (state.Debug) UnityEngine.Debug.Log("Begin DFS Loop");

            while (currentDepth >= 0)
            {
                if (currentDepth >= maxDepth)
                {
                    float currentUtility = states[currentDepth].GetContentment();

                    if (state.Debug)
                    {
                        UnityEngine.Debug.Log("Reached Leaf Node with Utility: " + currentUtility);
                        UnityEngine.Debug.Log("Best Utility: " + bestUtility);
                        if (state.Debug)
                        {
                            UnityEngine.Debug.Log("Current Plan:");
                            foreach (Action action in actions)
                                UnityEngine.Debug.Log(action);
                        }
                        UnityEngine.Debug.Log(states[currentDepth]);
                    }

                    if (currentUtility > bestUtility)
                    {
                        bestUtility = currentUtility;
                        actions.CopyTo(currentPlan, 0);


                        if (state.Debug)
                        {
                            UnityEngine.Debug.Log("Updated Plan:");
                            foreach (Action action in currentPlan)
                                UnityEngine.Debug.Log(action);
                            UnityEngine.Debug.Log("");
                        }
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
            watch.Stop();
            if (state.Debug) UnityEngine.Debug.Log("DFS Loop Ended, Total Time: " + (watch.ElapsedMilliseconds / 1000f).ToString() + " seconds" + Environment.NewLine);

            return new Tuple<Action[], WorldState[]>(currentPlan, states);
        }
    }
}
