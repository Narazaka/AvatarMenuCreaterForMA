using nadena.dev.modular_avatar.core;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace net.narazaka.avatarmenucreator.editor
{
    public class RestoreAvatarToggleMenu : RestoreAvatarMenuBase<AvatarToggleMenu>
    {
        public RestoreAvatarToggleMenu(GameObject go) : base(go)
        {
        }

        public RestoreAvatarToggleMenu(ParameterConfig parameterConfig, AnimatorController animator, VRCExpressionsMenu menu) : base(parameterConfig, animator, menu)
        {
        }

        public override AvatarToggleMenu RestoreAssets()
        {
            CheckAssets();
            var avatarMenu = new AvatarToggleMenu();
            avatarMenu.ToggleDefaultValue = ParameterConfig.defaultValue >= 0.5f;
            avatarMenu.Saved = ParameterConfig.saved;
            avatarMenu.ToggleIcon = Menu.controls[0].icon;

            var hasTransitionSeconds = State(Postfix.activate) != null;
            if (hasTransitionSeconds)
            {
                avatarMenu.TransitionSeconds = Clip(Postfix.activate).length;
            }

            var bindingGroups = hasTransitionSeconds ? MakeBindingGroups(Postfix.active, Postfix.inactive, Postfix.activate, Postfix.inactivate) : MakeBindingGroups(Postfix.active, Postfix.inactive);
            foreach (var info in bindingGroups.Keys)
            {
                var bindingGroup = bindingGroups[info];
                if (info.IsGameObjectActive)
                {
                    avatarMenu.ToggleObjects[info.path] = bindingGroup.GetCurve(Postfix.active)[0].value >= 0.5f ? ToggleType.ON : ToggleType.OFF;
                }
                else if (info.IsBlendShape)
                {
                    avatarMenu.ToggleBlendShapes[(info.path, info.BlendShapeName)] = new ToggleBlendShape
                    {
                        Inactive = bindingGroup.GetCurve(Postfix.inactive)[0].value,
                        Active = bindingGroup.GetCurve(Postfix.active)[0].value,
                        TransitionOffsetPercent = hasTransitionSeconds ? bindingGroup.GetCurve(Postfix.activate)[0].time / avatarMenu.TransitionSeconds : 0,
                        TransitionDurationPercent = hasTransitionSeconds ? (bindingGroup.GetCurve(Postfix.activate)[1].time - bindingGroup.GetCurve(Postfix.activate)[0].time) / avatarMenu.TransitionSeconds : 100,
                    };
                }
                else if (info.IsShaderParameter)
                {
                    avatarMenu.ToggleShaderParameters[(info.path, info.ShaderParameterName)] = new ToggleBlendShape
                    {
                        Inactive = bindingGroup.GetCurve(Postfix.inactive)[0].value,
                        Active = bindingGroup.GetCurve(Postfix.active)[0].value,
                        TransitionOffsetPercent = hasTransitionSeconds ? bindingGroup.GetCurve(Postfix.activate)[0].time / avatarMenu.TransitionSeconds : 0,
                        TransitionDurationPercent = hasTransitionSeconds ? (bindingGroup.GetCurve(Postfix.activate)[1].time - bindingGroup.GetCurve(Postfix.activate)[0].time) / avatarMenu.TransitionSeconds : 100,
                    };
                }
            }
            return avatarMenu;
        }

        public override void CheckAssets()
        {
            base.CheckAssets();
            Assert(MenuControl.parameter.name == ParameterName, "VRCExpressionsMenuのパラメーター名とMA Parametersのパラメーター名が一致するべきです");
            Assert(MenuControl.type == VRCExpressionsMenu.Control.ControlType.Toggle, "VRCExpressionsMenuのタイプはToggleであるべきです");
            Assert(MenuControl.value == 1, "VRCExpressionsMenuの値は1であるべきです");
            Assert(Animator.parameters[0].type == AnimatorControllerParameterType.Bool, "Animatorのパラメーター型はBoolであるべきです");
            Assert(ParameterConfig.syncType == ParameterSyncType.Bool, "MA Parametersのパラメーター型はBoolであるべきです");
            Assert(Clip(Postfix.inactive) != null, "AnimatorのStateにはinactiveモーションが必要です");
            Assert(Clip(Postfix.active) != null, "AnimatorのStateにはactiveモーションが必要です");

            var hasTransitionSeconds = State(Postfix.activate) != null;
            if (hasTransitionSeconds)
            {
                Assert(Clip(Postfix.inactivate) != null, "AnimatorのStateにはinactivateモーションが必要です");
                Assert(Clip(Postfix.activate) != null, "AnimatorのStateにはactivateモーションが必要です");
            }
            if (hasTransitionSeconds)
            {
                Assert(Transition(Postfix.inactive, Postfix.activate)?.conditions.Length == 1, "inactiveからactivateへの遷移には条件が1つ必要です");
                Assert(Transition(Postfix.activate, Postfix.active)?.conditions.Length == 0, "activateからactiveへの遷移には条件が不要です");
                Assert(Transition(Postfix.active, Postfix.inactivate)?.conditions.Length == 1, "activeからinactivateへの遷移には条件が1つ必要です");
                Assert(Transition(Postfix.inactivate, Postfix.inactive)?.conditions.Length == 0, "inactivateからinactiveへの遷移には条件が不要です");

                Assert(Transition(Postfix.inactive, Postfix.activate).hasExitTime == false, "inactiveからactivateへの遷移はhasExitTimeがfalseであるべきです");
                var activateCondition = Transition(Postfix.inactive, Postfix.activate).conditions[0];
                Assert(activateCondition.mode == AnimatorConditionMode.If, "inactiveからactivateへの遷移は条件がtrueであるべきです");
                Assert(activateCondition.parameter == ParameterName, "inactiveからactivateへの遷移のパラメーター名とMA Parametersのパラメーター名が一致するべきです");
                Assert(Transition(Postfix.active, Postfix.inactivate).hasExitTime == false, "activeからinactivateへの遷移はhasExitTimeがfalseであるべきです");
                var inactivateCondition = Transition(Postfix.active, Postfix.inactivate).conditions[0];
                Assert(inactivateCondition.mode == AnimatorConditionMode.IfNot, "activeからinactivateへの遷移は条件がfalseであるべきです");
                Assert(inactivateCondition.parameter == ParameterName, "activeからinactivateへの遷移のパラメーター名とMA Parametersのパラメーター名が一致するべきです");
                Assert(Transition(Postfix.activate, Postfix.active).hasExitTime == true, "activateからactiveへの遷移はhasExitTimeがtrueであるべきです");
                Assert(Transition(Postfix.inactivate, Postfix.inactive).hasExitTime == true, "inactivateからinactiveへの遷移はhasExitTimeがtrueであるべきです");
                Assert(Transition(Postfix.activate, Postfix.active).exitTime == 1, "activateからactiveへの遷移のexitTimeは1であるべきです");
                Assert(Transition(Postfix.inactivate, Postfix.inactive).exitTime == 1, "inactivateからinactiveへの遷移のexitTimeは1であるべきです");

                Assert(Clip(Postfix.activate).length == Clip(Postfix.inactivate).length, "activateとinactivateの長さは同じであるべきです");
            }
            else
            {
                Assert(Transition(Postfix.inactive, Postfix.active)?.conditions.Length == 1, "inactiveからactiveへの遷移には条件が1つ必要です");
                Assert(Transition(Postfix.active, Postfix.inactive)?.conditions.Length == 1, "activeからinactiveへの遷移には条件が1つ必要です");

                Assert(Transition(Postfix.inactive, Postfix.active).hasExitTime == false, "inactiveからactiveへの遷移はhasExitTimeがfalseであるべきです");
                var activeCondition = Transition(Postfix.inactive, Postfix.active).conditions[0];
                Assert(activeCondition.mode == AnimatorConditionMode.If, "inactiveからactiveへの遷移は条件がtrueであるべきです");
                Assert(activeCondition.parameter == ParameterName, "inactiveからactiveへの遷移のパラメーター名とMA Parametersのパラメーター名が一致するべきです");
                Assert(Transition(Postfix.active, Postfix.inactive).hasExitTime == false, "activeからinactiveへの遷移はhasExitTimeがfalseであるべきです");
                var inactiveCondition = Transition(Postfix.active, Postfix.inactive).conditions[0];
                Assert(inactiveCondition.mode == AnimatorConditionMode.IfNot, "activeからinactiveへの遷移は条件がfalseであるべきです");
                Assert(inactiveCondition.parameter == ParameterName, "activeからinactiveへの遷移のパラメーター名とMA Parametersのパラメーター名が一致するべきです");
            }
        }
    }
}
