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
            Assert(MenuControl.parameter.name == ParameterName, "VRCExpressionsMenu�̃p�����[�^�[����MA Parameters�̃p�����[�^�[������v����ׂ��ł�");
            Assert(MenuControl.type == VRCExpressionsMenu.Control.ControlType.Toggle, "VRCExpressionsMenu�̃^�C�v��Toggle�ł���ׂ��ł�");
            Assert(MenuControl.value == 1, "VRCExpressionsMenu�̒l��1�ł���ׂ��ł�");
            Assert(Animator.parameters[0].type == AnimatorControllerParameterType.Bool, "Animator�̃p�����[�^�[�^��Bool�ł���ׂ��ł�");
            Assert(ParameterConfig.syncType == ParameterSyncType.Bool, "MA Parameters�̃p�����[�^�[�^��Bool�ł���ׂ��ł�");
            Assert(Clip(Postfix.inactive) != null, "Animator��State�ɂ�inactive���[�V�������K�v�ł�");
            Assert(Clip(Postfix.active) != null, "Animator��State�ɂ�active���[�V�������K�v�ł�");

            var hasTransitionSeconds = State(Postfix.activate) != null;
            if (hasTransitionSeconds)
            {
                Assert(Clip(Postfix.inactivate) != null, "Animator��State�ɂ�inactivate���[�V�������K�v�ł�");
                Assert(Clip(Postfix.activate) != null, "Animator��State�ɂ�activate���[�V�������K�v�ł�");
            }
            if (hasTransitionSeconds)
            {
                Assert(Transition(Postfix.inactive, Postfix.activate)?.conditions.Length == 1, "inactive����activate�ւ̑J�ڂɂ͏�����1�K�v�ł�");
                Assert(Transition(Postfix.activate, Postfix.active)?.conditions.Length == 0, "activate����active�ւ̑J�ڂɂ͏������s�v�ł�");
                Assert(Transition(Postfix.active, Postfix.inactivate)?.conditions.Length == 1, "active����inactivate�ւ̑J�ڂɂ͏�����1�K�v�ł�");
                Assert(Transition(Postfix.inactivate, Postfix.inactive)?.conditions.Length == 0, "inactivate����inactive�ւ̑J�ڂɂ͏������s�v�ł�");

                Assert(Transition(Postfix.inactive, Postfix.activate).hasExitTime == false, "inactive����activate�ւ̑J�ڂ�hasExitTime��false�ł���ׂ��ł�");
                var activateCondition = Transition(Postfix.inactive, Postfix.activate).conditions[0];
                Assert(activateCondition.mode == AnimatorConditionMode.If, "inactive����activate�ւ̑J�ڂ͏�����true�ł���ׂ��ł�");
                Assert(activateCondition.parameter == ParameterName, "inactive����activate�ւ̑J�ڂ̃p�����[�^�[����MA Parameters�̃p�����[�^�[������v����ׂ��ł�");
                Assert(Transition(Postfix.active, Postfix.inactivate).hasExitTime == false, "active����inactivate�ւ̑J�ڂ�hasExitTime��false�ł���ׂ��ł�");
                var inactivateCondition = Transition(Postfix.active, Postfix.inactivate).conditions[0];
                Assert(inactivateCondition.mode == AnimatorConditionMode.IfNot, "active����inactivate�ւ̑J�ڂ͏�����false�ł���ׂ��ł�");
                Assert(inactivateCondition.parameter == ParameterName, "active����inactivate�ւ̑J�ڂ̃p�����[�^�[����MA Parameters�̃p�����[�^�[������v����ׂ��ł�");
                Assert(Transition(Postfix.activate, Postfix.active).hasExitTime == true, "activate����active�ւ̑J�ڂ�hasExitTime��true�ł���ׂ��ł�");
                Assert(Transition(Postfix.inactivate, Postfix.inactive).hasExitTime == true, "inactivate����inactive�ւ̑J�ڂ�hasExitTime��true�ł���ׂ��ł�");
                Assert(Transition(Postfix.activate, Postfix.active).exitTime == 1, "activate����active�ւ̑J�ڂ�exitTime��1�ł���ׂ��ł�");
                Assert(Transition(Postfix.inactivate, Postfix.inactive).exitTime == 1, "inactivate����inactive�ւ̑J�ڂ�exitTime��1�ł���ׂ��ł�");

                Assert(Clip(Postfix.activate).length == Clip(Postfix.inactivate).length, "activate��inactivate�̒����͓����ł���ׂ��ł�");
            }
            else
            {
                Assert(Transition(Postfix.inactive, Postfix.active)?.conditions.Length == 1, "inactive����active�ւ̑J�ڂɂ͏�����1�K�v�ł�");
                Assert(Transition(Postfix.active, Postfix.inactive)?.conditions.Length == 1, "active����inactive�ւ̑J�ڂɂ͏�����1�K�v�ł�");

                Assert(Transition(Postfix.inactive, Postfix.active).hasExitTime == false, "inactive����active�ւ̑J�ڂ�hasExitTime��false�ł���ׂ��ł�");
                var activeCondition = Transition(Postfix.inactive, Postfix.active).conditions[0];
                Assert(activeCondition.mode == AnimatorConditionMode.If, "inactive����active�ւ̑J�ڂ͏�����true�ł���ׂ��ł�");
                Assert(activeCondition.parameter == ParameterName, "inactive����active�ւ̑J�ڂ̃p�����[�^�[����MA Parameters�̃p�����[�^�[������v����ׂ��ł�");
                Assert(Transition(Postfix.active, Postfix.inactive).hasExitTime == false, "active����inactive�ւ̑J�ڂ�hasExitTime��false�ł���ׂ��ł�");
                var inactiveCondition = Transition(Postfix.active, Postfix.inactive).conditions[0];
                Assert(inactiveCondition.mode == AnimatorConditionMode.IfNot, "active����inactive�ւ̑J�ڂ͏�����false�ł���ׂ��ł�");
                Assert(inactiveCondition.parameter == ParameterName, "active����inactive�ւ̑J�ڂ̃p�����[�^�[����MA Parameters�̃p�����[�^�[������v����ׂ��ł�");
            }
        }
    }
}
