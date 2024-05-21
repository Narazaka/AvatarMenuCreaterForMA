using System.Collections.Generic;
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
#endif

        public override IEnumerable<VRCExpressionParameters.Parameter> GetEffectiveParameterNameAndTypes()
        {
            return new VRCExpressionParameters.Parameter[]
            {
                new VRCExpressionParameters.Parameter
                {
                    name = ParameterName,
                    valueType = VRCExpressionParameters.ValueType.Float,
                    defaultValue = AvatarRadialMenu.RadialDefaultValue,
                    saved = AvatarMenu.Saved,
                    networkSynced = true,
                },
            };
        }
    }
}
