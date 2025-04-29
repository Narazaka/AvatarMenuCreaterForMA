#if NET_NARAZAKA_VRCHAT_AvatarMenuCreator_HAS_NDMF && !NET_NARAZAKA_VRCHAT_AvatarMenuCreator_HAS_NO_MENU_MA
using nadena.dev.modular_avatar.core;
using net.narazaka.avatarmenucreator.components;
#endif
using net.narazaka.avatarmenucreator.util;
using System.Collections.Generic;
using UnityEngine;

namespace net.narazaka.avatarmenucreator.editor
{
    public abstract class CreateAvatarMenuBase
    {
        public static CreateAvatarMenuBase GetCreateAvatarMenu<T>(Transform avatarRoot, T avatarMenu) where T : AvatarMenuBase
        {
            switch (avatarMenu)
            {
                case AvatarChooseMenu avatarChooseMenu:
                    return new CreateAvatarChooseMenu(avatarRoot, avatarChooseMenu);
                case AvatarToggleMenu avatarToggleMenu:
                    return new CreateAvatarToggleMenu(avatarRoot, avatarToggleMenu);
                case AvatarRadialMenu avatarSliderMenu:
                    return new CreateAvatarRadialMenu(avatarRoot, avatarSliderMenu);
                default:
                    throw new System.ArgumentException($"unknown avatar menu type {avatarMenu.GetType()}");
            }
        }

        Transform AvatarRoot;
        
        protected CreateAvatarMenuBase(Transform avatarRoot) => AvatarRoot = avatarRoot;

#if NET_NARAZAKA_VRCHAT_AvatarMenuCreator_HAS_NDMF
        public abstract CreatedAssets CreateAssets(string baseName, IEnumerable<string> children = null, nadena.dev.ndmf.BuildContext buildContext = null);
#else
        public abstract CreatedAssets CreateAssets(string baseName, IEnumerable<string> children = null);
#endif

#if NET_NARAZAKA_VRCHAT_AvatarMenuCreator_HAS_NDMF && !NET_NARAZAKA_VRCHAT_AvatarMenuCreator_HAS_NO_MENU_MA
        public static AvatarMenuCreatorBase GetOrAddMenuCreatorComponent(GameObject obj, AvatarMenuBase avatarMenu, bool addMenuInstaller)
        {
            var creator = GetOrAddMenuCreatorComponentOnly(obj, avatarMenu);
            if (addMenuInstaller) creator.gameObject.GetOrAddComponent<ModularAvatarMenuInstaller>();
            return creator;
        }

        public static AvatarMenuCreatorBase GetOrAddMenuCreatorComponentOnly(GameObject obj, AvatarMenuBase avatarMenu)
        {
            switch (avatarMenu)
            {
                case AvatarToggleMenu a:
                    {
                        var creator = obj.GetOrAddComponent<AvatarToggleMenuCreator>();
                        creator.AvatarToggleMenu = a.DeepCopy();
                        return creator;
                    }
                case AvatarChooseMenu a:
                    {
                        var creator = obj.GetOrAddComponent<AvatarChooseMenuCreator>();
                        creator.AvatarChooseMenu = a.DeepCopy();
                        return creator;
                    }
                case AvatarRadialMenu a:
                    {
                        var creator = obj.GetOrAddComponent<AvatarRadialMenuCreator>();
                        creator.AvatarRadialMenu = a.DeepCopy();
                        return creator;
                    }
                default:
                    throw new System.ArgumentException($"unknown menu type");
            }
        }
#endif

        protected Transform GetByPath(string path)
        {
            return AvatarRoot.Find(path);
        }

        protected System.Type GetRendererTypeByPath(string path)
        {
            var obj = GetByPath(path);
            if (obj == null) return typeof(Renderer);
            var component = obj.GetComponent<Renderer>();
            if (component == null) return typeof(Renderer);
            return component.GetType();
        }
    }
}