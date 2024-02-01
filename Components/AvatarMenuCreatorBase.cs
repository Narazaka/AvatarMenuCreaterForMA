using nadena.dev.modular_avatar.core;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VRC.SDK3.Avatars.ScriptableObjects;
using VRC.SDKBase;

namespace net.narazaka.avatarmenucreator.components
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ModularAvatarMenuInstaller))]
    public abstract class AvatarMenuCreatorBase : MonoBehaviour, IEditorOnly
#if NET_NARAZAKA_VRCHAT_AvatarMenuCreator_HAS_AvatarParametersDriver
        , net.narazaka.vrchat.avatar_parameters_driver.IParameterNameAndTypesProvider
#endif
    {
        public abstract AvatarMenuBase AvatarMenu { get; }

        public bool IsEffective => GetComponent<ModularAvatarMergeAnimator>() == null && GetComponent<ModularAvatarParameters>() == null;

#if NET_NARAZAKA_VRCHAT_AvatarMenuCreator_HAS_AvatarParametersDriver
        public IEnumerable<VRCExpressionParameters.Parameter> GetParameterNameAndTypes()
        {
            if (IsEffective)
            {
                return GetEffectiveParameterNameAndTypes();
            }
            else
            {
                return Enumerable.Empty<VRCExpressionParameters.Parameter>();
            }
        }

        public abstract IEnumerable<VRCExpressionParameters.Parameter> GetEffectiveParameterNameAndTypes();
#endif
    }
}
