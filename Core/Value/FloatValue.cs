using System;
using System.Collections.Generic;

namespace net.narazaka.avatarmenucreator.value
{
    [Serializable]
    public sealed class FloatValue : Value
    {
        public FloatValue(float value) : base(new List<float> { value }) { }

        public static implicit operator float(FloatValue value) => value.Count > 0 ? value[0] : 0;
    }
}
