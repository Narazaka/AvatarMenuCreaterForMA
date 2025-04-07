using System;
using UnityEngine;

namespace net.narazaka.avatarmenucreator.value
{
    [Serializable]
    public sealed class QuaternionValue : Value
    {
        public QuaternionValue(Quaternion value) : base(new float[] { value.x, value.y, value.z, value.w }) { }
        public static implicit operator Quaternion(QuaternionValue value) => value != null && value.value.Length >= 4 ? new Quaternion(value.value[0], value.value[1], value.value[2], value.value[3]) : Quaternion.identity;
    }
}
