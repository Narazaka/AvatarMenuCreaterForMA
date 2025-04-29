using System;
using UnityEngine;

namespace net.narazaka.avatarmenucreator
{
    [Flags]
    public enum ToggleTransitionUsing
    {
        [InspectorName("両方制御")]
        NotSpecified = 0,
        [InspectorName("ONを無視")]
        OmitON = 1 << 0,
        [InspectorName("OFFを無視")]
        OmitOFF = 1 << 1,
    }
}
