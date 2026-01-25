using System;
using UnityEngine;
using System.Collections.Generic;
using VRC.Dynamics;

namespace net.narazaka.avatarmenucreator.value
{
    [Serializable]
    public class Value : IEquatable<Value>
    {
        public static explicit operator bool(Value value) => value.AsBool();
        public static explicit operator float(Value value) => value.AsFloat();
        public static explicit operator int(Value value) => value.AsInt();
        public static explicit operator Vector3(Value value) => value.AsVector3();
        public static explicit operator Quaternion(Value value) => value.AsQuaternion();
        public static explicit operator Color(Value value) => value.AsColor();
        public static explicit operator VRCPhysBoneBase.PermissionFilter(Value value) => value.AsPermissionFilterValue();

        public static implicit operator Value(bool value) => new BoolValue(value);
        public static implicit operator Value(float value) => new FloatValue(value);
        public static implicit operator Value(int value) => new IntValue(value);
        public static implicit operator Value(Vector3 value) => new Vector3Value(value);
        public static implicit operator Value(Quaternion value) => new QuaternionValue(value);
        public static implicit operator Value(Color value) => new ColorValue(value);
        public static implicit operator Value(VRCPhysBoneBase.PermissionFilter value) => new PermissionFilterValue(value);

        [SerializeField]
        protected float[] value;

        public Value() { value = new float[0]; }
        public Value(float[] value) { this.value = value; }

        public object As(Type type)
        {
            if (type == typeof(bool)) return (bool)this;
            if (type == typeof(float)) return (float)this;
            if (type == typeof(int)) return (int)this;
            if (type == typeof(Vector3)) return (Vector3)this;
            if (type == typeof(Quaternion)) return (Quaternion)this;
            if (type == typeof(Color)) return (Color)this;
            if (type == typeof(VRCPhysBoneBase.PermissionFilter)) return (VRCPhysBoneBase.PermissionFilter)this;
            return null;
        }

        public bool Equals(Value other)
        {
            if (other == null) return false;
            if (value.Length != other.value.Length) return false;

            for (int i = 0; i < value.Length; i++)
            {
                if (this.value[i] != other.value[i]) return false;
            }
            return true;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Value);
        }

        public override int GetHashCode() {
            if (value.Length == 1) return value[0].GetHashCode();
#if NET_UNITY_4_8 || UNITY_2021_2_OR_NEWER
            if (value.Length == 2) return HashCode.Combine(value[0], value[1]);
            if (value.Length == 3) return HashCode.Combine(value[0], value[1], value[2]);
            if (value.Length == 4) return HashCode.Combine(value[0], value[1], value[2], value[3]);
#else
            if (value.Length == 2) return (value[0].GetHashCode() * 397) ^ value[1].GetHashCode();
            if (value.Length == 3) return (((value[0].GetHashCode() * 397) ^ value[1].GetHashCode()) * 397) ^ value[2].GetHashCode();
            if (value.Length == 4) return ((((value[0].GetHashCode() * 397) ^ value[1].GetHashCode()) * 397) ^ value[2].GetHashCode()) * 397 ^ value[3].GetHashCode();
#endif
            return 0;
        }

        public static bool operator ==(Value left, Value right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (ReferenceEquals(left, null)) return false;
            return left.Equals(right);
        }

        public static bool operator !=(Value left, Value right)
        {
            return !(left == right);
        }
    }
}
