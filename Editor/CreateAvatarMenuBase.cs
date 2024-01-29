namespace net.narazaka.avatarmenucreator.editor
{
    public abstract class CreateAvatarMenuBase
    {
        public static CreateAvatarMenuBase GetCreateAvatarMenu<T>(T avatarMenu) where T : AvatarMenuBase
        {
            switch (avatarMenu)
            {
                case AvatarChooseMenu avatarChooseMenu:
                    return new CreateAvatarChooseMenu(avatarChooseMenu);
                case AvatarToggleMenu avatarToggleMenu:
                    return new CreateAvatarToggleMenu(avatarToggleMenu);
                case AvatarRadialMenu avatarSliderMenu:
                    return new CreateAvatarRadialMenu(avatarSliderMenu);
                default:
                    throw new System.ArgumentException($"unknown avatar menu type {avatarMenu.GetType()}");
            }
        }

        public abstract CreatedAssets CreateAssets(string baseName, string[] children);
    }
}