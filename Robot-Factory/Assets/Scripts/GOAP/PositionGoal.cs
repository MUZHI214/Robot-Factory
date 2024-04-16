using UnityEngine;

namespace GOAP
{
    public class PositionGoal : Goal
    {
        public Vector2 Position { get; set; } = Vector2.zero;

        public PositionGoal(string name, float contentment, Vector2 currentPosition)
        {
            Name = name;
            Contentment = contentment;
            Position = currentPosition;
        }

        public override float GetContentment(WorldState state)
        {
            return Contentment;
        }
    }
}
