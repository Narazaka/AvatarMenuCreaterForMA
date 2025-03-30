using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace net.narazaka.avatarmenucreator.components
{
    public class AvatarChooseMenuCreator : AvatarMenuCreatorBase
    {
        [SerializeField]
        public AvatarChooseMenu AvatarChooseMenu = new AvatarChooseMenu();
        public override AvatarMenuBase AvatarMenu => AvatarChooseMenu;
#if UNITY_EDITOR
        public override UnityEditor.SerializedProperty AvatarMenuProperty(UnityEditor.SerializedObject serializedObject) => serializedObject.FindProperty(nameof(AvatarChooseMenu));

        public override IEnumerable<VRCExpressionParameters.Parameter> GetEffectiveParameterNameAndTypes()
        {
#if HAS_COMPRESSED_INT_PARAMETERS
            if (AvatarChooseMenu.UseCompressed && AvatarChooseMenu.CanUseCompressed)
            {
                return Narazaka.VRChat.CompressedIntParameters.CompressedParameterConfig.From(new nadena.dev.modular_avatar.core.ParameterConfig
                {
                    nameOrPrefix = ParameterName,
                    syncType = nadena.dev.modular_avatar.core.ParameterSyncType.Int,
                    defaultValue = AvatarChooseMenu.ChooseDefaultValue,
                    internalParameter = AvatarChooseMenu.InternalParameter,
                    saved = AvatarMenu.Saved,
                }, AvatarChooseMenu.ChooseCount).ToParameterConfigs().Select(parameterConfig => new VRCExpressionParameters.Parameter
                {
                    name = parameterConfig.nameOrPrefix,
                    valueType = parameterConfig.syncType == nadena.dev.modular_avatar.core.ParameterSyncType.Bool ? VRCExpressionParameters.ValueType.Bool : VRCExpressionParameters.ValueType.Int,
                    defaultValue = parameterConfig.defaultValue,
                    saved = parameterConfig.saved,
                    networkSynced = !parameterConfig.localOnly,
                });
            }
#endif
            return new VRCExpressionParameters.Parameter[]
            {
                new VRCExpressionParameters.Parameter
                {
                    name = ParameterName,
                    valueType = VRCExpressionParameters.ValueType.Int,
                    defaultValue = AvatarChooseMenu.ChooseDefaultValue,
                    saved = AvatarMenu.Saved,
                    networkSynced = AvatarMenu.Synced,
                },
            };
        }
#endif
    }
}
