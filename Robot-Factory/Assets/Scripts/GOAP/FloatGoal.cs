using UnityEngine;

namespace GOAP
{
    public class FloatGoal : Goal
    {
        public FloatGoal(string name, float contentment)
        {
            Name = name;
            Contentment = contentment;
        }

        public override float GetContentment(WorldState state)
        {
            return Mathf.Max(Contentment - state.FloatGoals[this], 0);
        }
    }
}
