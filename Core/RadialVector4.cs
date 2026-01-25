using System;
using UnityEngine;

namespace net.narazaka.avatarmenucreator
{
    [Serializable]
    public class RadialVector4 : RadialValueBase, System.IEquatable<RadialVector4>
    {
        public Vector4 Start;
        public Vector4 End;

        public bool Equals(RadialVector4 other)
        {
            return Start == other.Start && End == other.End && StartOffsetPercent == other.StartOffsetPercent && EndOffsetPercent == other.EndOffsetPercent;
        }

        public string ChangedProp(RadialVector4 other)
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

        public RadialVector4 SetProp(string name, object value)
        {
            if (name == nameof(Start)) Start = (Vector4)value;
            if (name == nameof(End)) End = (Vector4)value;
            if (name == nameof(StartOffsetPercent)) StartOffsetPercent = (float)value;
            if (name == nameof(EndOffsetPercent)) EndOffsetPercent = (float)value;
            return this;
        }

        public Vector4 Value(float rate)
        {
            return IsPreStart(rate) ? Start : IsPostEnd(rate) ? End : (Start * StartFactor(rate) + End * EndFactor(rate)) / TotalFactor;
        }
    }
}
