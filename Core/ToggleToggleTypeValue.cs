using System;
using UnityEngine;
using net.narazaka.avatarmenucreator.value;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace net.narazaka.avatarmenucreator
{
    [Serializable]
    public sealed class ToggleToggleTypeValue : ToggleValue
    {
        public new ToggleTypeValue Active { get => base.Active.AsToggleTypeValue(); set => base.Active = value; }
        public new ToggleTypeValue Inactive { get => base.Inactive.AsToggleTypeValue(); set => base.Inactive = value; }
    }
}
