using UnityEngine;

namespace net.narazaka.avatarmenucreator.components
{
    public class AvatarRadialMenuCreator : AvatarMenuCreatorBase
    {
        [SerializeField]
        public AvatarRadialMenu AvatarRadialMenu = new AvatarRadialMenu();
        public override AvatarMenuBase AvatarMenu => AvatarRadialMenu;
    }
}
