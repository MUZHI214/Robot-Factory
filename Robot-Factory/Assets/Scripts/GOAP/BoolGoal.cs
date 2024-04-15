namespace GOAP
{
    public class BoolGoal : Goal
    {
        public BoolGoal (string name,float contentment)
        {
            Name = name;
            Contentment = contentment;
        }

        public override float GetContentment (WorldState state)
        {
            if (state.BoolGoals[this]) return Contentment;
            return 0;
        }
    }
}
