using System;
using System.Collections.Generic;

namespace net.narazaka.avatarmenucreator.value
{
    [Serializable]
    public sealed class BoolValue : Value
    {
        public BoolValue(bool value) : base(new List<float> { value ? 1 : 0 }) { }
        public static implicit operator bool(BoolValue value) => value.Count > 0 ? value[0] != 0 : false;
    }
}
