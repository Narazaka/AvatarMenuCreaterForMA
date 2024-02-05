using System.Collections.Generic;
using UnityEngine;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace net.narazaka.avatarmenucreator.components
{
    public class AvatarChooseMenuCreator : AvatarMenuCreatorBase
    {
        [SerializeField]
        public AvatarChooseMenu AvatarChooseMenu = new AvatarChooseMenu();
        public override AvatarMenuBase AvatarMenu => AvatarChooseMenu;

        public override IEnumerable<VRCExpressionParameters.Parameter> GetEffectiveParameterNameAndTypes()
        {
            return new VRCExpressionParameters.Parameter[]
            {
                new VRCExpressionParameters.Parameter
                {
                    name = name,
                    valueType = VRCExpressionParameters.ValueType.Int,
                    defaultValue = AvatarChooseMenu.ChooseDefaultValue,
                    saved = AvatarMenu.Saved,
                    networkSynced = true,
                },
            };
        }
    }
}
