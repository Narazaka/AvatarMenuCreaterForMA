using UnityEngine;

namespace net.narazaka.avatarmenucreator
{
    public enum ToggleType
    {
        [InspectorName("制御しない")]
        None,
        [InspectorName("ON=表示")]
        ON,
        [InspectorName("ON=非表示")]
        OFF,
    }
}
