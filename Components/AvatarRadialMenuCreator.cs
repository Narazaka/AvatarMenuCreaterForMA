using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace net.narazaka.avatarmenucreator.components
{
    public class AvatarRadialMenuCreator : AvatarMenuCreatorBase
    {
        [SerializeField]
        public AvatarRadialMenu AvatarRadialMenu = new AvatarRadialMenu();
        public override AvatarMenuBase AvatarMenu => AvatarRadialMenu;
#if UNITY_EDITOR
        public override UnityEditor.SerializedProperty AvatarMenuProperty(UnityEditor.SerializedObject serializedObject) => serializedObject.FindProperty(nameof(AvatarRadialMenu));

        public override IEnumerable<VRCExpressionParameters.Parameter> GetEffectiveParameterNameAndTypes()
        {
            var parameterConfig = new VRCExpressionParameters.Parameter
            {
                name = ParameterName,
                valueType = VRCExpressionParameters.ValueType.Float,
                defaultValue = AvatarRadialMenu.RadialDefaultValue,
                saved = AvatarMenu.Saved,
                networkSynced = true,
            };
            var subParameterConfig = new VRCExpressionParameters.Parameter
            {
                name = ParameterName + "_changing",
                valueType = VRCExpressionParameters.ValueType.Bool,
                defaultValue = 0,
                saved = false,
                networkSynced = true,
            };
            if (AvatarRadialMenu.GetPhysBoneAutoResetEffectiveObjects(AvatarRadialMenu.GetStoredChildren(), AvatarRadialMenu.RadialValues.Keys).Any())
            {
                return new VRCExpressionParameters.Parameter[]
                {
                    parameterConfig,
                    subParameterConfig,
                };
            }
            else
            {
                return new VRCExpressionParameters.Parameter[] { parameterConfig };
            }
        }
#endif
    }
}
