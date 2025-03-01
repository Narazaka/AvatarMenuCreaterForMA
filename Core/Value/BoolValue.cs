using System;
using System.Collections.Generic;

namespace net.narazaka.avatarmenucreator.value
{
    [Serializable]
    public sealed class BoolValue : Value
    {
        public BoolValue(bool value) : base(new float[] { value ? 1 : 0 }) { }
        public static implicit operator bool(BoolValue value) => value != null && value.value.Length > 0 ? value.value[0] != 0 : false;
    }
}
