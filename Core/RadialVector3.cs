using System;
using UnityEngine;

namespace net.narazaka.avatarmenucreator
{
    [Serializable]
    public class RadialVector3 : System.IEquatable<RadialVector3>
    {
        public Vector3 Start;
        public Vector3 End;
        public float StartOffsetPercent;
        public float EndOffsetPercent = 100;

        public bool Equals(RadialVector3 other)
        {
            return Start == other.Start && End == other.End && StartOffsetPercent == other.StartOffsetPercent && EndOffsetPercent == other.EndOffsetPercent;
        }

        public string ChangedProp(RadialVector3 other)
        {
            if (Start != other.Start) return nameof(Start);
            if (End != other.End) return nameof(End);
            if (StartOffsetPercent != other.StartOffsetPercent) return nameof(StartOffsetPercent);
            if (EndOffsetPercent != other.EndOffsetPercent) return nameof(EndOffsetPercent);
            return "";
        }

        public object GetProp(string name)
        {
            if (name == nameof(Start)) return Start;
            if (name == nameof(End)) return End;
            if (name == nameof(StartOffsetPercent)) return StartOffsetPercent;
            if (name == nameof(EndOffsetPercent)) return EndOffsetPercent;
            return null;
        }

        public RadialVector3 SetProp(string name, object value)
        {
            if (name == nameof(Start)) Start = (Vector3)value;
            if (name == nameof(End)) End = (Vector3)value;
            if (name == nameof(StartOffsetPercent)) StartOffsetPercent = (float)value;
            if (name == nameof(EndOffsetPercent)) EndOffsetPercent = (float)value;
            return this;
        }
    }
}
