namespace net.narazaka.avatarmenucreater
{
    struct RadialBlendShape : System.IEquatable<RadialBlendShape>
    {
        public float Start;
        public float End;

        public bool Equals(RadialBlendShape other)
        {
            return Start == other.Start && End == other.End;
        }

        public string ChangedProp(RadialBlendShape other)
        {
            if (Start != other.Start) return nameof(Start);
            if (End != other.End) return nameof(End);
            return "";
        }

        public float GetProp(string name)
        {
            if (name == nameof(Start)) return Start;
            if (name == nameof(End)) return End;
            return 0;
        }

        public RadialBlendShape SetProp(string name, float value)
        {
            if (name == nameof(Start)) Start = value;
            if (name == nameof(End)) End = value;
            return this;
        }
    }
}
