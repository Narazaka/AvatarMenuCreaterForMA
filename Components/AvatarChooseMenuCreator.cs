using UnityEngine;

namespace net.narazaka.avatarmenucreator.components
{
    public class AvatarChooseMenuCreator : AvatarMenuCreatorBase
    {
        [SerializeField]
        public AvatarChooseMenu AvatarChooseMenu = new AvatarChooseMenu();
        public override AvatarMenuBase AvatarMenu => AvatarChooseMenu;
    }
}
