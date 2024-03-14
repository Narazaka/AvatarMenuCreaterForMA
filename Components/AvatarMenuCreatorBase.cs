using nadena.dev.modular_avatar.core;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VRC.SDK3.Avatars.ScriptableObjects;
using VRC.SDKBase;

namespace net.narazaka.avatarmenucreator.components
{
    [DisallowMultipleComponent]
    public abstract class AvatarMenuCreatorBase : MonoBehaviour, IEditorOnly
#if NET_NARAZAKA_VRCHAT_AvatarMenuCreator_HAS_AvatarParametersUtil
        , Narazaka.VRChat.AvatarParametersUtil.IParameterNameAndTypesProvider
#elif NET_NARAZAKA_VRCHAT_AvatarMenuCreator_HAS_AvatarParametersDriver
        , net.narazaka.vrchat.avatar_parameters_driver.IParameterNameAndTypesProvider
#endif
    {
        public abstract AvatarMenuBase AvatarMenu { get; }

        public bool IsEffective => GetComponent<ModularAvatarMergeAnimator>() == null && GetComponent<ModularAvatarParameters>() == null;

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

        public string ParameterName => string.IsNullOrEmpty(AvatarMenu.ParameterName) ? name : AvatarMenu.ParameterName;
    }
}
