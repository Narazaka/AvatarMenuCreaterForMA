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
#if UNITY_EDITOR && NET_NARAZAKA_VRCHAT_AvatarMenuCreator_HAS_AvatarParametersUtil
        , Narazaka.VRChat.AvatarParametersUtil.IParameterNameAndTypesProvider
#elif UNITY_EDITOR && NET_NARAZAKA_VRCHAT_AvatarMenuCreator_HAS_AvatarParametersDriver
        , net.narazaka.vrchat.avatar_parameters_driver.IParameterNameAndTypesProvider
#endif
    {
        public abstract AvatarMenuBase AvatarMenu { get; }

        void Reset()
        {
            AvatarMenu.Reset();
        }
#if UNITY_EDITOR
        public abstract UnityEditor.SerializedProperty AvatarMenuProperty(UnityEditor.SerializedObject serializedObject);

        public bool IsEffective => GetComponent<ModularAvatarMergeAnimator>() == null && GetComponent<ModularAvatarParameters>() == null
#if HAS_COMPRESSED_INT_PARAMETERS
            && GetComponent<Narazaka.VRChat.CompressedIntParameters.CompressedIntParameters>() == null
#endif
            ;

        public void DestroyMAComponents()
        {
            var mergeAnimator = GetComponent<ModularAvatarMergeAnimator>();
            if (mergeAnimator != null) UnityEditor.Undo.DestroyObjectImmediate(mergeAnimator);
            var parameters = GetComponent<ModularAvatarParameters>();
            if (parameters != null) UnityEditor.Undo.DestroyObjectImmediate(parameters);
#if HAS_COMPRESSED_INT_PARAMETERS
            var compressed = GetComponent<Narazaka.VRChat.CompressedIntParameters.CompressedIntParameters>();
            if (compressed != null) UnityEditor.Undo.DestroyObjectImmediate(compressed);
#endif
        }

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
#endif
    }
}
