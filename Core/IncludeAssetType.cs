using UnityEngine;

namespace net.narazaka.avatarmenucreator
{
    public enum IncludeAssetType
    {
        [Japanese("全て個別に保存")]
        [English("All saved individually")]
        Extract,
        [Japanese("prefabとanimator")]
        [English("prefab & animator")]
        AnimatorAndInclude,
        [Japanese("全てprefabに含める")]
        [English("one prefab")]
        Include,
#if NET_NARAZAKA_VRCHAT_AvatarMenuCreator_HAS_NDMF && !NET_NARAZAKA_VRCHAT_AvatarMenuCreator_HAS_NO_MENU_MA
        [Japanese("コンポーネントとして保持")]
        [English("as component")]
        Component,
#endif
    }
}
