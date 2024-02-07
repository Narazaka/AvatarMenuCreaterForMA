using nadena.dev.modular_avatar.core;
using System.Linq;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace net.narazaka.avatarmenucreator.editor
{
    public class RestoreAvatarRadialMenu : RestoreAvatarMenuBase<AvatarRadialMenu>
    {
        public RestoreAvatarRadialMenu(GameObject go) : base(go)
        {
        }

        public RestoreAvatarRadialMenu(ParameterConfig parameterConfig, AnimatorController animator, VRCExpressionsMenu menu) : base(parameterConfig, animator, menu)
        {
        }

        public override AvatarRadialMenu RestoreAssets()
        {
            CheckAssets();
            var avatarMenu = new AvatarRadialMenu();
            avatarMenu.RadialDefaultValue = ParameterConfig.defaultValue;
            avatarMenu.Saved = ParameterConfig.saved;
            avatarMenu.RadialIcon = Menu.controls[0].icon;

            avatarMenu.RadialInactiveRange = State(Postfix.inactive) != null;
            if (avatarMenu.RadialInactiveRange)
            {
                avatarMenu.RadialInactiveRangeMin = float.NaN;
                avatarMenu.RadialInactiveRangeMax = float.NaN;
                foreach (var transition in Transitions(Postfix.inactive, null))
                {
                    if (transition.conditions[0].mode == AnimatorConditionMode.Less)
                    {
                        avatarMenu.RadialInactiveRangeMin = transition.conditions[0].threshold;
                    }
                    else if (transition.conditions[0].mode == AnimatorConditionMode.Greater)
                    {
                        avatarMenu.RadialInactiveRangeMax = transition.conditions[0].threshold;
                    }
                }
                avatarMenu.RadialInactiveRangeMin = Transition(Postfix.inactive, null).conditions[0].threshold;
                avatarMenu.RadialInactiveRangeMax = Transition(null, Postfix.inactive).conditions[0].threshold;
            }

            var bindingGroups = MakeBindingGroups(null);
            foreach (var info in bindingGroups.Keys)
            {
                var bindingGroup = bindingGroups[info];
                if (info.IsBlendShape)
                {
                    avatarMenu.RadialBlendShapes[(info.path, info.BlendShapeName)] = new RadialBlendShape
                    {
                        Start = bindingGroup.GetCurve(null)[0].value,
                        End = bindingGroup.GetCurve(null)[1].value,
                    };
                }
                else if (info.IsShaderParameter)
                {
                    avatarMenu.RadialShaderParameters[(info.path, info.ShaderParameterName)] = new RadialBlendShape
                    {
                        Start = bindingGroup.GetCurve(null)[0].value,
                        End = bindingGroup.GetCurve(null)[1].value,
                    };
                }
            }
            return avatarMenu;
        }

        public override void CheckAssets()
        {
            base.CheckAssets();
            Assert(MenuControl.parameter.name == ParameterName, "VRCExpressionsMenuのパラメーター名とMA Parametersのパラメーター名が一致するべきです");
            Assert(MenuControl.type == VRCExpressionsMenu.Control.ControlType.RadialPuppet, "VRCExpressionsMenuのタイプはRadialPuppetであるべきです");
            Assert(Animator.parameters[0].type == AnimatorControllerParameterType.Float, "Animatorのパラメーター型はFloatであるべきです");
            Assert(ParameterConfig.syncType == ParameterSyncType.Float, "MA Parametersのパラメーター型はFloatであるべきです");
            Assert(Clip(null) != null, "AnimatorのStateには連続変化モーションが必要です");
            Assert(State(null).speedParameterActive, "連続変化のspeedParameterはtrueであるべきです");
            Assert(State(null).speedParameter == ParameterName, "連続変化のspeedParameter名とMA Parametersのパラメーター名が一致するべきです");

            if (State(Postfix.inactive) != null)
            {
                Assert(Transitions(null, Postfix.inactive).Count() == 1, "連続変化からinactiveへの遷移には条件が1つ必要です");
                var condCount = Transition(null, Postfix.inactive).conditions.Length;
                Assert(condCount == 1 || condCount == 2, "連続変化からinactiveへの遷移には条件が1つまたは2つ必要です");
                Assert(Transition(null, Postfix.inactive).hasExitTime == false, "連続変化からinactiveへの遷移はhasExitTimeがfalseであるべきです");
                Assert(Transitions(Postfix.inactive, null).Count() == condCount, "inactivateから連続変化への遷移数は逆の遷移条件数と等しい必要があります");
                Assert(Transitions(Postfix.inactive, null).All(t => t.hasExitTime == false), "inactivateから連続変化への遷移はhasExitTimeがfalseであるべきです");
            }
        }
    }
}
