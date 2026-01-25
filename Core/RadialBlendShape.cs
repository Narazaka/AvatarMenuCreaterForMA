using System;

namespace net.narazaka.avatarmenucreator
{
    [Serializable]
    public class RadialBlendShape : RadialValueBase, System.IEquatable<RadialBlendShape>
    {
        public float Start;
        public float End;

        public bool Equals(RadialBlendShape other)
        {
            return Start == other.Start && End == other.End && StartOffsetPercent == other.StartOffsetPercent && EndOffsetPercent == other.EndOffsetPercent;
        }

        public string ChangedProp(RadialBlendShape other)
        {
            if (Start != other.Start) return nameof(Start);
            if (End != other.End) return nameof(End);
            if (StartOffsetPercent != other.StartOffsetPercent) return nameof(StartOffsetPercent);
            if (EndOffsetPercent != other.EndOffsetPercent) return nameof(EndOffsetPercent);
            return "";
        }

        public float GetProp(string name)
        {
            if (name == nameof(Start)) return Start;
            if (name == nameof(End)) return End;
            if (name == nameof(StartOffsetPercent)) return StartOffsetPercent;
            if (name == nameof(EndOffsetPercent)) return EndOffsetPercent;
            return 0;
        }

        public RadialBlendShape SetProp(string name, float value)
        {
            if (name == nameof(Start)) Start = value;
            if (name == nameof(End)) End = value;
            if (name == nameof(StartOffsetPercent)) StartOffsetPercent = value;
            if (name == nameof(EndOffsetPercent)) EndOffsetPercent = value;
            return this;
        }

        public float Value(float rate)
        {
            return IsPreStart(rate) ? Start : IsPostEnd(rate) ? End : (Start * StartFactor(rate) + End * EndFactor(rate)) / TotalFactor;
        }
    }
}
