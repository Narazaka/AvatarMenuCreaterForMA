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
            Assert(MenuControl.type == VRCExpressionsMenu.Control.ControlType.SubMenu, "VRCExpressionsMenuのタイプはSubMenuであるべきです");
            Assert(MenuControl.subMenu != null, "VRCExpressionsMenuのSubMenuがあるべきです");
            var childControls = Menu.controls[0].subMenu.controls;
            var values = childControls.Select((c, i) => Mathf.RoundToInt(c.value)).OrderBy(v => v).ToArray();
            Assert(childControls.All(c => c.parameter.name == ParameterName), "VRCExpressionsMenuのパラメーター名とMA Parametersのパラメーター名が一致するべきです");
            Assert(values.Select((v, i) => v == i).All(v => v), "VRCExpressionsMenuのパラメーター値は全ての値があるべきです");
            Assert(Animator.parameters[0].type == AnimatorControllerParameterType.Int, "Animatorのパラメーター型はIntであるべきです");
            Assert(ParameterConfig.syncType == ParameterSyncType.Int, "MA Parametersのパラメーター型はIntであるべきです");
            Assert(values.All(v => Clip(v) != null), "AnimatorのStateには各選択肢のモーションが必要です");
            Assert(values.All(v => AnyTransition(v) != null), "AnimatorのAnyStateには各選択肢への遷移が必要です");
            Assert(values.Select(v => AnyTransition(v).duration).Distinct().Count() == 1, "AnimatorのAnyStateのdurationは全て同一であるべきです");
            Assert(values.All(v => AnyTransition(v).conditions.Length == 1), "AnimatorのAnyStateには各選択肢への遷移条件が1つあるべきです");
            Assert(values.All(v => AnyTransition(v).conditions[0].mode == AnimatorConditionMode.Equals && (int)AnyTransition(v).conditions[0].threshold == v), "AnimatorのAnyStateには各選択肢への遷移条件が値と等しい必要があります");
        }
    }
}
