using UnityEngine;

namespace GOAP
{
    public class PositionGoal : Goal
    {

        public PositionGoal(string name, float contentment)
        {
            Name = name;
            Contentment = contentment;
        }

        public override float GetContentment(WorldState state)
        {
            return Contentment;
        }
    }
}
