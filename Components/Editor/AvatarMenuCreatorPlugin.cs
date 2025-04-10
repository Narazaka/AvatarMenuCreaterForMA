﻿using nadena.dev.ndmf;
using net.narazaka.avatarmenucreator.editor;
using nadena.dev.modular_avatar.core;

[assembly: ExportsPlugin(typeof(net.narazaka.avatarmenucreator.components.editor.AvatarMenuCreatorPlugin))]

namespace net.narazaka.avatarmenucreator.components.editor
{
    public class AvatarMenuCreatorPlugin : Plugin<AvatarMenuCreatorPlugin>
    {
        public override string QualifiedName => "net.narazaka.vrchat.avatar-menu-creater-for-ma";

        public override string DisplayName => "Avatar Menu Creator for MA";

        protected override void Configure()
        {
            InPhase(BuildPhase.Generating).BeforePlugin("nadena.dev.modular-avatar").Run("AvatarMenuCreatorForMA", ctx =>
            {
                var creators = ctx.AvatarRootTransform.GetComponentsInChildren<AvatarMenuCreatorBase>(true);
                foreach (var creator in creators)
                {
                    if (!creator.IsEffective) continue;
                    CreateAvatarMenuBase.GetCreateAvatarMenu(ctx.AvatarRootTransform, creator.AvatarMenu).CreateAssets(creator.name).StoreAssets(creator.gameObject, false);
                    UnityEngine.Object.DestroyImmediate(creator);
                }
            });
        }
    }
}
