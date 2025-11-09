namespace SS3D.Systems.Characters
{

    public readonly struct BlendShape
    {
        public readonly string Name;
        public readonly float Value;

        public BlendShape(string name, float value)
        {
            Name = name;
            Value = value;
        }
    }
}