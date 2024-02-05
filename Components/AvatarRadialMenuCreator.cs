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

        public override IEnumerable<VRCExpressionParameters.Parameter> GetEffectiveParameterNameAndTypes()
        {
            return new VRCExpressionParameters.Parameter[]
            {
                new VRCExpressionParameters.Parameter
                {
                    name = name,
                    valueType = VRCExpressionParameters.ValueType.Float,
                    defaultValue = AvatarRadialMenu.RadialDefaultValue,
                    saved = AvatarMenu.Saved,
                    networkSynced = true,
                },
            };
        }
    }
}
