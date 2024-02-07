using nadena.dev.modular_avatar.core;
using System.Linq;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace net.narazaka.avatarmenucreator.editor
{
    public class RestoreAvatarChooseMenu : RestoreAvatarMenuBase<AvatarChooseMenu>
    {
        public RestoreAvatarChooseMenu(GameObject go) : base(go)
        {
        }

        public RestoreAvatarChooseMenu(ParameterConfig parameterConfig, AnimatorController animator, VRCExpressionsMenu menu) : base(parameterConfig, animator, menu)
        {
        }

        public override AvatarChooseMenu RestoreAssets()
        {
            CheckAssets();
            var avatarMenu = new AvatarChooseMenu();
            avatarMenu.ChooseDefaultValue = Mathf.RoundToInt(ParameterConfig.defaultValue);
            avatarMenu.Saved = ParameterConfig.saved;
            avatarMenu.ChooseParentIcon = Menu.controls[0].icon;
            avatarMenu.TransitionSeconds = AnyTransition(0).duration;
            var childControls = Menu.controls[0].subMenu.controls;
            var values = childControls.Select((c, i) => Mathf.RoundToInt(c.value)).OrderBy(v => v).ToArray();
            avatarMenu.ChooseCount = values.Length;
            foreach (var control in childControls)
            {
                avatarMenu.ChooseIcons[Mathf.RoundToInt(control.value)] = control.icon;
                avatarMenu.ChooseNames[Mathf.RoundToInt(control.value)] = control.name;
            }


            var bindingGroups = MakeBindingGroups(values);
            foreach (var info in bindingGroups.Keys)
            {
                var bindingGroup = bindingGroups[info];
                if (info.IsGameObjectActive)
                {
                    var choose = avatarMenu.ChooseObjects[info.path] = new collections.instance.IntHashSet();
                    choose.UnionWith(values.Where(value => bindingGroup.GetCurve(value)[0].value > 0.5f));
                }
                else if (info.IsBlendShape)
                {
                    var choose = avatarMenu.ChooseBlendShapes[(info.path, info.BlendShapeName)] = new collections.instance.IntFloatDictionary();
                    foreach (var value in values)
                    {
                        choose[value] = bindingGroup.GetCurve(value)[0].value;
                    }
                }
                else if (info.IsShaderParameter)
                {
                    var choose = avatarMenu.ChooseShaderParameters[(info.path, info.ShaderParameterName)] = new collections.instance.IntFloatDictionary();
                    foreach (var value in values)
                    {
                        choose[value] = bindingGroup.GetCurve(value)[0].value;
                    }
                }
                else if (info.IsMaterial)
                {
                    var choose = avatarMenu.ChooseMaterials[(info.path, info.MaterialIndex)] = new collections.instance.IntMaterialDictionary();
                    foreach (var value in values)
                    {
                        choose[value] = bindingGroup.GetObjectReferenceCurve(value)[0].value as Material;
                    }
                }
            }
            return avatarMenu;
        }

        public override void CheckAssets()
        {
            base.CheckAssets();
            Assert(MenuControl.type == VRCExpressionsMenu.Control.ControlType.SubMenu, "VRCExpressionsMenu�̃^�C�v��SubMenu�ł���ׂ��ł�");
            Assert(MenuControl.subMenu != null, "VRCExpressionsMenu��SubMenu������ׂ��ł�");
            var childControls = Menu.controls[0].subMenu.controls;
            var values = childControls.Select((c, i) => Mathf.RoundToInt(c.value)).OrderBy(v => v).ToArray();
            Assert(childControls.All(c => c.parameter.name == ParameterName), "VRCExpressionsMenu�̃p�����[�^�[����MA Parameters�̃p�����[�^�[������v����ׂ��ł�");
            Assert(values.Select((v, i) => v == i).All(v => v), "VRCExpressionsMenu�̃p�����[�^�[�l�͑S�Ă̒l������ׂ��ł�");
            Assert(Animator.parameters[0].type == AnimatorControllerParameterType.Int, "Animator�̃p�����[�^�[�^��Int�ł���ׂ��ł�");
            Assert(ParameterConfig.syncType == ParameterSyncType.Int, "MA Parameters�̃p�����[�^�[�^��Int�ł���ׂ��ł�");
            Assert(values.All(v => Clip(v) != null), "Animator��State�ɂ͊e�I�����̃��[�V�������K�v�ł�");
            Assert(values.All(v => AnyTransition(v) != null), "Animator��AnyState�ɂ͊e�I�����ւ̑J�ڂ��K�v�ł�");
            Assert(values.Select(v => AnyTransition(v).duration).Distinct().Count() == 1, "Animator��AnyState��duration�͑S�ē���ł���ׂ��ł�");
            Assert(values.All(v => AnyTransition(v).conditions.Length == 1), "Animator��AnyState�ɂ͊e�I�����ւ̑J�ڏ�����1����ׂ��ł�");
            Assert(values.All(v => AnyTransition(v).conditions[0].mode == AnimatorConditionMode.Equals && (int)AnyTransition(v).conditions[0].threshold == v), "Animator��AnyState�ɂ͊e�I�����ւ̑J�ڏ������l�Ɠ������K�v������܂�");
        }
    }
}
