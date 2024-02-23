using UnityEngine;

namespace net.narazaka.avatarmenucreator
{
    public enum ToggleUsing
    {
        [InspectorName("両方制御")]
        Both,
        [InspectorName("ONのみ制御")]
        ON,
        [InspectorName("OFFのみ制御")]
        OFF,
    }
}
