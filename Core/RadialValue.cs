using System;
using System.Collections.Generic;
using net.narazaka.avatarmenucreator.util;
using net.narazaka.avatarmenucreator.value;
using UnityEngine;

namespace net.narazaka.avatarmenucreator
{
    [Serializable]
    public class RadialValue : RadialValueBase, System.IEquatable<RadialValue>
    {
        public Value Start;
        public Value End;

        public bool Equals(RadialValue other)
        {
            return other != null && Start == other.Start && End == other.End && StartOffsetPercent == other.StartOffsetPercent && EndOffsetPercent == other.EndOffsetPercent;
        }

        public IEnumerable<string> ChangedProps(RadialValue other)
        {
            var changed = new List<string>();
            if (Start != other.Start) changed.Add(nameof(Start));
            if (End != other.End) changed.Add(nameof(End));
            if (StartOffsetPercent != other.StartOffsetPercent) changed.Add(nameof(StartOffsetPercent));
            if (EndOffsetPercent != other.EndOffsetPercent) changed.Add(nameof(EndOffsetPercent));
            return changed;
        }

        public object GetProp(string name)
        {
            if (name == nameof(Start)) return Start;
            if (name == nameof(End)) return End;
            if (name == nameof(StartOffsetPercent)) return StartOffsetPercent;
            if (name == nameof(EndOffsetPercent)) return EndOffsetPercent;
            return null;
        }

        public RadialValue SetProp(string name, object value)
        {
            if (name == nameof(Start)) Start = (Value)value;
            if (name == nameof(End)) End = (Value)value;
            if (name == nameof(StartOffsetPercent)) StartOffsetPercent = (float)value;
            if (name == nameof(EndOffsetPercent)) EndOffsetPercent = (float)value;
            return this;
        }

        public float FloatValue(float rate)
        {
            return IsPreStart(rate) ? (float)Start : IsPostEnd(rate) ? (float)End : ((float)Start * StartFactor(rate) + (float)End * EndFactor(rate)) / TotalFactor;
        }

        public Vector3 Vector3Value(float rate)
        {
            return IsPreStart(rate) ? (Vector3)Start : IsPostEnd(rate) ? (Vector3)End : ((Vector3)Start * StartFactor(rate) + (Vector3)End * EndFactor(rate)) / TotalFactor;
        }

        public Quaternion QuaternionValue(float rate)
        {
            return IsPreStart(rate) ? (Quaternion)Start : IsPostEnd(rate) ? (Quaternion)End : ((((Quaternion)Start).ToVector4() * StartFactor(rate) + ((Quaternion)End).ToVector4() * EndFactor(rate)) / TotalFactor).ToQuaternion();
        }

        public Color ColorValue(float rate)
        {
            return IsPreStart(rate) ? (Color)Start : IsPostEnd(rate) ? (Color)End : ((Color)Start * StartFactor(rate) + (Color)End * EndFactor(rate)) / TotalFactor;
        }
    }
}
