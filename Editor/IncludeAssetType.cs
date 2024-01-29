using UnityEngine;

namespace net.narazaka.avatarmenucreater
{
    public enum IncludeAssetType
    {
        [InspectorName("全て個別に保存")]
        Extract,
        [InspectorName("prefabとanimator")]
        AnimatorAndInclude,
        [InspectorName("全てprefabに含める")]
        Include,
    }
}
