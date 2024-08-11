using System;
using UnityEngine;

namespace net.narazaka.avatarmenucreator.value
{
    [Serializable]
    public sealed class Vector3Value : Value
    {
        public Vector3Value(Vector3 value) : base(new float[] { value.x, value.y, value.z }) { }
        public static implicit operator Vector3(Vector3Value value) => value != null && value.value.Length >= 3 ? new Vector3(value.value[0], value.value[1], value.value[2]) : Vector3.zero;
    }
}
