using UnityEngine;

namespace net.narazaka.avatarmenucreator.components
{
    public class AvatarToggleMenuCreator : AvatarMenuCreatorBase
    {
        [SerializeField]
        public AvatarToggleMenu AvatarToggleMenu = new AvatarToggleMenu();
        public override AvatarMenuBase AvatarMenu => AvatarToggleMenu;
    }
}
