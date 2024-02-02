#if NET_NARAZAKA_VRCHAT_AvatarMenuCreator_HAS_NDMF
using nadena.dev.modular_avatar.core;
using net.narazaka.avatarmenucreator.components;
#endif
using net.narazaka.avatarmenucreator.editor.util;
using System.Collections.Generic;
using UnityEngine;

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

        public abstract CreatedAssets CreateAssets(string baseName, IEnumerable<string> children = null);

#if NET_NARAZAKA_VRCHAT_AvatarMenuCreator_HAS_NDMF
        public static AvatarMenuCreatorBase GetOrAddMenuCreatorComponent(GameObject obj, AvatarMenuBase avatarMenu)
        {
            switch (avatarMenu)
            {
                case AvatarToggleMenu a:
                    {
                        var creator = obj.GetOrAddComponent<AvatarToggleMenuCreator>();
                        creator.AvatarToggleMenu = a.DeepCopy();
                        obj.GetOrAddComponent<ModularAvatarMenuInstaller>();
                        return creator;
                    }
                case AvatarChooseMenu a:
                    {
                        var creator = obj.GetOrAddComponent<AvatarChooseMenuCreator>();
                        creator.AvatarChooseMenu = a.DeepCopy();
                        obj.GetOrAddComponent<ModularAvatarMenuInstaller>();
                        return creator;
                    }
                case AvatarRadialMenu a:
                    {
                        var creator = obj.GetOrAddComponent<AvatarRadialMenuCreator>();
                        creator.AvatarRadialMenu = a.DeepCopy();
                        obj.GetOrAddComponent<ModularAvatarMenuInstaller>();
                        return creator;
                    }
                default:
                    throw new System.ArgumentException($"unknown menu type");
            }
        }
#endif
    }
}