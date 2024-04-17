using UnityEngine;

namespace GOAP
{
    public class PositionGoal : Goal
    {
        public Vector2 Position { get; set; }

        public PositionGoal(string name, float contentment, Vector2 position)
        {
            Name = name;
            Contentment = contentment;
            Position = position;
        }

        public override float GetContentment(WorldState state)
        {
            var dist = Vector2.Distance(state.PositionGoals[this], Position) / 100;
            return dist * Contentment;
        }
    }
}
