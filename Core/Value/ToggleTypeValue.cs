using System;
using UnityEngine;

namespace net.narazaka.avatarmenucreator.value
{
    [Serializable]
    public sealed class ToggleTypeValue : Value
    {
        public ToggleTypeValue(ToggleType value) : base(new float[] { (int)value }) { }
        public static implicit operator ToggleType(ToggleTypeValue value) => value != null && value.value.Length > 0 ? (ToggleType)Mathf.RoundToInt(value.value[0]) : ToggleType.None;
    }
}
