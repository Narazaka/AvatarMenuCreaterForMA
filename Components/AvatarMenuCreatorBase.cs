using nadena.dev.modular_avatar.core;
using UnityEngine;
using VRC.SDKBase;

namespace net.narazaka.avatarmenucreator.components
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ModularAvatarMenuInstaller))]
    public abstract class AvatarMenuCreatorBase : MonoBehaviour, IEditorOnly
    {
        public abstract AvatarMenuBase AvatarMenu { get; }

        public bool IsEffective => GetComponent<ModularAvatarMergeAnimator>() == null && GetComponent<ModularAvatarParameters>() == null;
    }
}
