using System;
using System.Collections.Generic;
using net.narazaka.avatarmenucreator.value;

namespace net.narazaka.avatarmenucreator
{
    [Serializable]
    public class RadialValue : System.IEquatable<RadialValue>
    {
        public Value Start;
        public Value End;
        public float StartOffsetPercent;
        public float EndOffsetPercent = 100;

        public bool Equals(RadialValue other)
        {
            return Start == other.Start && End == other.End && StartOffsetPercent == other.StartOffsetPercent && EndOffsetPercent == other.EndOffsetPercent;
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
    }
}
