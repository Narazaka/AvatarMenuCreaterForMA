using System;
using UnityEngine;

namespace net.narazaka.avatarmenucreator.value
{
    [Serializable]
    public sealed class Vector3Value : Value
    {
        public Vector3Value(Vector3 value) : base(new float[] { value.x, value.y, value.z }) { }
        public static implicit operator Vector3(Vector3Value value) => value != null && value.Count >= 3 ? new Vector3(value[0], value[1], value[2]) : Vector3.zero;
    }
}
