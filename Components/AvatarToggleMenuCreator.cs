using System.Collections.Generic;
using UnityEngine;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace net.narazaka.avatarmenucreator.components
{
    public class AvatarToggleMenuCreator : AvatarMenuCreatorBase
    {
        [SerializeField]
        public AvatarToggleMenu AvatarToggleMenu = new AvatarToggleMenu();
        public override AvatarMenuBase AvatarMenu => AvatarToggleMenu;
#if UNITY_EDITOR
        public override UnityEditor.SerializedProperty AvatarMenuProperty(UnityEditor.SerializedObject serializedObject) => serializedObject.FindProperty(nameof(AvatarToggleMenu));

        public override IEnumerable<VRCExpressionParameters.Parameter> GetEffectiveParameterNameAndTypes()
        {
            return new VRCExpressionParameters.Parameter[]
            {
                new VRCExpressionParameters.Parameter
                {
                    name = ParameterName,
                    valueType = VRCExpressionParameters.ValueType.Bool,
                    defaultValue = AvatarToggleMenu.ToggleDefaultValue ? 1 : 0,
                    saved = AvatarMenu.Saved,
                    networkSynced = AvatarMenu.Synced,
                },
            };
        }
#endif
    }
}
