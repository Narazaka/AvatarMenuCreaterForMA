using nadena.dev.modular_avatar.core;
using net.narazaka.avatarmenucreator.collections.instance;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace net.narazaka.avatarmenucreator.editor
{
    public class CreateAvatarChooseMenu : CreateAvatarMenuBase
    {
        AvatarChooseMenu AvatarMenu;
        public CreateAvatarChooseMenu(AvatarChooseMenu avatarChooseMenu) => AvatarMenu = avatarChooseMenu;

        public override CreatedAssets CreateAssets(string baseName, IEnumerable<string> children = null)
        {
            var matchGameObjects = new HashSet<string>(children ?? AvatarMenu.GetStoredChildren());
            var parameterName = string.IsNullOrEmpty(AvatarMenu.ParameterName) ? baseName : AvatarMenu.ParameterName;
            // clip
            var choices = Enumerable.Range(0, AvatarMenu.ChooseCount).Select(i => new AnimationClip { name = $"{baseName}_{i}" }).ToList();
            foreach (var child in AvatarMenu.ChooseObjects.Keys)
            {
                if (!matchGameObjects.Contains(child)) continue;
                var curvePath = child;
                for (var i = 0; i < AvatarMenu.ChooseCount; ++i)
                {
                    choices[i].SetCurve(curvePath, typeof(GameObject), "m_IsActive", new AnimationCurve(new Keyframe(0, AvatarMenu.ChooseObjects[child].Contains(i) ? 1 : 0)));
                }
            }
            foreach (var (child, index) in AvatarMenu.ChooseMaterials.Keys)
            {
                if (!matchGameObjects.Contains(child)) continue;
                var value = AvatarMenu.ChooseMaterials[(child, index)];
                var curvePath = child;
                var curveName = $"m_Materials.Array.data[{index}]";
                for (var i = 0; i < AvatarMenu.ChooseCount; ++i)
                {
                    AnimationUtility.SetObjectReferenceCurve(choices[i], EditorCurveBinding.PPtrCurve(curvePath, typeof(Renderer), curveName), new ObjectReferenceKeyframe[] { new ObjectReferenceKeyframe { time = 0, value = value.ContainsKey(i) ? value[i] : null } });
                }
            }
            foreach (var (child, name) in AvatarMenu.ChooseBlendShapes.Keys)
            {
                if (!matchGameObjects.Contains(child)) continue;
                var value = AvatarMenu.ChooseBlendShapes[(child, name)];
                var curvePath = child;
                var curveName = $"blendShape.{name}";
                for (var i = 0; i < AvatarMenu.ChooseCount; ++i)
                {
                    choices[i].SetCurve(curvePath, typeof(SkinnedMeshRenderer), curveName, new AnimationCurve(new Keyframe(0, value.ContainsKey(i) ? value[i] : 0)));
                }
            }
            foreach (var (child, name) in AvatarMenu.ChooseShaderParameters.Keys)
            {
                if (!matchGameObjects.Contains(child)) continue;
                var value = AvatarMenu.ChooseShaderParameters[(child, name)];
                var curvePath = child;
                var curveName = $"material.{name}";
                for (var i = 0; i < AvatarMenu.ChooseCount; ++i)
                {
                    choices[i].SetCurve(curvePath, typeof(Renderer), curveName, new AnimationCurve(new Keyframe(0, value.ContainsKey(i) ? value[i] : 0)));
                }
            }
            foreach (var child in AvatarMenu.Positions.Keys)
            {
                if (!matchGameObjects.Contains(child)) continue;
                SetTransformCurve(choices, child, "localPosition", AvatarMenu.Positions[child]);
            }
            foreach (var child in AvatarMenu.Rotations.Keys)
            {
                if (!matchGameObjects.Contains(child)) continue;
                SetTransformCurve(choices, child, "localEulerAnglesRaw", AvatarMenu.Rotations[child]);
            }
            foreach (var child in AvatarMenu.Scales.Keys)
            {
                if (!matchGameObjects.Contains(child)) continue;
                SetTransformCurve(choices, child, "localScale", AvatarMenu.Scales[child]);
            }
            // controller
            var controller = new AnimatorController();
            controller.AddParameter(new AnimatorControllerParameter { name = parameterName, type = AnimatorControllerParameterType.Int, defaultInt = 0 });
            if (controller.layers.Length == 0) controller.AddLayer(baseName);
            var layer = controller.layers[0];
            layer.name = baseName;
            layer.stateMachine.name = baseName;
            layer.stateMachine.entryPosition = new Vector3(-600, 0);
            layer.stateMachine.anyStatePosition = new Vector3(800, 0);
            var idleState = layer.stateMachine.AddState($"{baseName}_idle", new Vector3(-300, 0));
            idleState.motion = choices[0];
            idleState.writeDefaultValues = false;
            layer.stateMachine.defaultState = idleState;
            var states = choices.Select((clip, i) =>
            {
                var state = layer.stateMachine.AddState($"{baseName}_{i}", new Vector3(300, 50 * i));
                state.motion = clip;
                state.writeDefaultValues = false;
                return state;
            }).ToList();
            for (var i = 0; i < AvatarMenu.ChooseCount; ++i)
            {
                var state = states[i];
                var toNext = layer.stateMachine.AddAnyStateTransition(state);
                toNext.exitTime = 0;
                toNext.duration = AvatarMenu.TransitionSeconds;
                toNext.hasExitTime = false;
                toNext.conditions = new AnimatorCondition[]
                {
                    new AnimatorCondition
                    {
                        mode = AnimatorConditionMode.Equals,
                        parameter = parameterName,
                        threshold = i,
                    },
                };
                toNext.canTransitionToSelf = false;
                var fromIdle = idleState.AddTransition(state);
                fromIdle.exitTime = 0;
                fromIdle.duration = 0;
                fromIdle.hasExitTime = false;
                fromIdle.conditions = new AnimatorCondition[]
                {
                    new AnimatorCondition
                    {
                        mode = AnimatorConditionMode.Equals,
                        parameter = parameterName,
                        threshold = i,
                    },
                };
            }
            // menu
            var menu = new VRCExpressionsMenu
            {
                controls = Enumerable.Range(0, AvatarMenu.ChooseCount).Select(i => new VRCExpressionsMenu.Control
                {
                    name = AvatarMenu.ChooseName(i),
                    type = VRCExpressionsMenu.Control.ControlType.Toggle,
                    parameter = new VRCExpressionsMenu.Control.Parameter
                    {
                        name = parameterName,
                    },
                    subParameters = new VRCExpressionsMenu.Control.Parameter[] { },
                    value = i,
                    labels = new VRCExpressionsMenu.Control.Label[] { },
                    icon = AvatarMenu.ChooseIcon(i),
                }).ToList(),
            };
            menu.name = baseName;
            var parentMenu = new VRCExpressionsMenu
            {
                controls = new List<VRCExpressionsMenu.Control>
                {
                    new VRCExpressionsMenu.Control {
                        name = baseName,
                        type = VRCExpressionsMenu.Control.ControlType.SubMenu,
                        parameter = new VRCExpressionsMenu.Control.Parameter
                        {
                            name = "",
                        },
                        subParameters = new VRCExpressionsMenu.Control.Parameter[] { },
                        value = 1,
                        labels = new VRCExpressionsMenu.Control.Label[] { },
                        subMenu = menu,
                        icon = AvatarMenu.ChooseParentIcon,
                    },
                },
            };
            parentMenu.name = $"{baseName}_parent";
            return new CreatedAssets(baseName, controller, choices, menu, parentMenu, new ParameterConfig[]
            {
                new ParameterConfig
                {
                    nameOrPrefix = parameterName,
                    defaultValue = AvatarMenu.ChooseDefaultValue,
                    syncType = ParameterSyncType.Int,
                    saved = AvatarMenu.Saved,
                    internalParameter = AvatarMenu.InternalParameter,
                },
            });
        }

        void SetTransformCurve(List<AnimationClip> choices, string path, string property, IntVector3Dictionary value)
        {
            for (var i = 0; i < AvatarMenu.ChooseCount; ++i)
            {
                choices[i].SetCurve(path, typeof(Transform), property + ".x", new AnimationCurve(new Keyframe(0, value[i].x)));
                choices[i].SetCurve(path, typeof(Transform), property + ".y", new AnimationCurve(new Keyframe(0, value[i].y)));
                choices[i].SetCurve(path, typeof(Transform), property + ".z", new AnimationCurve(new Keyframe(0, value[i].z)));
            }
        }
    }
}
