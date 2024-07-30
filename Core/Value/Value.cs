using System;
using UnityEngine;
using System.Collections.Generic;

namespace net.narazaka.avatarmenucreator.value
{
    [Serializable]
    public class Value : List<float>, IEquatable<Value>
    {
        public static explicit operator bool(Value value) => value.AsBool();
        public static explicit operator float(Value value) => value.AsFloat();
        public static explicit operator int(Value value) => value.AsInt();
        public static explicit operator Vector3(Value value) => value.AsVector3();
        public static explicit operator ToggleType(Value value) => value.AsToggleTypeValue();

        public static implicit operator Value(bool value) => new BoolValue(value);
        public static implicit operator Value(float value) => new FloatValue(value);
        public static implicit operator Value(int value) => new IntValue(value);
        public static implicit operator Value(Vector3 value) => new Vector3Value(value);
        public static implicit operator Value(ToggleType value) => new ToggleTypeValue(value);

        public Value(IEnumerable<float> values) : base(values) { }

        public bool Equals(Value other)
        {
            if (other == null) return false;
            if (Count != other.Count) return false;

            for (int i = 0; i < Count; i++)
            {
                if (this[i] != other[i]) return false;
            }
            return true;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Value);
        }

        public override int GetHashCode() {
            if (Count == 1) return this[0].GetHashCode();
            if (Count == 3) return HashCode.Combine(this[0], this[1], this[2]);
            return 0;
        }
    }
}
