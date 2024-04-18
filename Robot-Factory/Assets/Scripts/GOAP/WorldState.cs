using System;
using System.Collections.Generic;
using UnityEngine;

namespace GOAP
{
    public class WorldState
    {
        public bool Debug { get; set; } = true;
        public Domain Domain { get; set; }
        public Dictionary<Goal, float> FloatGoals { get; set; } = new Dictionary<Goal, float>();
        public Dictionary<Goal, bool> BoolGoals { get; set; } = new Dictionary<Goal, bool>();
        public Stack<Action> SatisfiedActions { get; set; } = null;

        public Action NextAction()
        {
            if (SatisfiedActions == null)
                GenerateActions();

            if (SatisfiedActions.Count > 0)
                return SatisfiedActions.Pop();

            return null;
        }

        private void GenerateActions()
        {
            SatisfiedActions = new Stack<Action>();
            foreach (Action action in Domain.Actions)
                if (action.PreconditionsSatisfied(this))
                    SatisfiedActions.Push(action);
        }

        public float GetContentment()
        {
            float contentment = 0;

            foreach (Goal goal in FloatGoals.Keys)
                contentment += goal.GetContentment(this);

            foreach (Goal goal in BoolGoals.Keys)
                contentment += goal.GetContentment(this);

            return contentment;
        }

        public WorldState Clone()
        {
            WorldState stateClone = new WorldState();
            stateClone.Domain = Domain;
            stateClone.FloatGoals = new Dictionary<Goal, float>(FloatGoals);
            stateClone.BoolGoals = new Dictionary<Goal, bool>(BoolGoals);
            return stateClone;
        }

        public override string ToString()
        {
            string output = "";

            foreach (Goal goal in FloatGoals.Keys)
                output += goal + ": " + FloatGoals[goal].ToString() + Environment.NewLine;

            foreach (Goal goal in BoolGoals.Keys)
                output += goal + ": " + BoolGoals[goal].ToString() + Environment.NewLine;

            if (SatisfiedActions == null) GenerateActions();

            output += "Available Actions:" + Environment.NewLine;
            foreach (Action action in SatisfiedActions)
                output += action + Environment.NewLine;

            return output;
        }
    }
}
