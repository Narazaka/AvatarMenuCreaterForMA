using System;
using UnityEngine;

namespace net.narazaka.avatarmenucreator.value
{
    [Serializable]
    public sealed class ColorValue : Value
    {
        public ColorValue(Color value) : base(new float[] { value.r, value.g, value.b, value.a }) { }
        public static implicit operator Color(ColorValue value) => value != null && value.value.Length >= 4 ? new Color(value.value[0], value.value[1], value.value[2], value.value[3]) : default(Color);
    }
}
