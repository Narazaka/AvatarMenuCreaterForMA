using System;
using System.Collections.Generic;
using UnityEngine;

namespace net.narazaka.avatarmenucreator.value
{
    [Serializable]
    public sealed class IntValue : Value
    {
        public IntValue(int value) : base(new List<float> { value }) { }
        public static implicit operator int(IntValue value) => value != null && value.Count > 0 ? Mathf.RoundToInt(value[0]) : 0;
    }
}
