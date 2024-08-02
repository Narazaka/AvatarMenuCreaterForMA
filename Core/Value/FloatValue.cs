using System;
using System.Collections.Generic;

namespace net.narazaka.avatarmenucreator.value
{
    [Serializable]
    public sealed class FloatValue : Value
    {
        public FloatValue(float value) : base(new float[] { value }) { }

        public static implicit operator float(FloatValue value) => value != null && value.value.Length > 0 ? value.value[0] : 0;
    }
}
