using UnityEngine;

namespace net.narazaka.avatarmenucreator
{
    public enum IncludeAssetType
    {
        [InspectorName("全て個別に保存")]
        Extract,
        [InspectorName("prefabとanimator")]
        AnimatorAndInclude,
        [InspectorName("全てprefabに含める")]
        Include,
#if NET_NARAZAKA_VRCHAT_AvatarMenuCreator_HAS_NDMF
        [InspectorName("コンポーネントとして保持")]
        Component,
#endif
    }
}
