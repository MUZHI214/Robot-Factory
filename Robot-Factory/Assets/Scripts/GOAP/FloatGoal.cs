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
            return Contentment;
        }
    }
}
