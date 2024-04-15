namespace GOAP
{
    public abstract class Goal
    {
        public string Name { get; set; } = "";
        public float Contentment { get; set; } = 0;

        public abstract float GetContentment(WorldState state);

        public override string ToString()
        {
            return Name;
        }
    }
}
