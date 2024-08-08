using System;
using System.Collections.Generic;
using UnityEngine;

namespace net.narazaka.avatarmenucreator.value
{
    [Serializable]
    public sealed class IntValue : Value
    {
        public IntValue(int value) : base(new float[] { value }) { }
        public static implicit operator int(IntValue value) => value != null && value.value.Length > 0 ? Mathf.RoundToInt(value.value[0]) : 0;
    }
}
