using nadena.dev.modular_avatar.core;
using net.narazaka.avatarmenucreator.collections.instance;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.Dynamics;
using VRC.SDK3.Avatars.ScriptableObjects;
using VRC.SDK3.Dynamics.PhysBone.Components;

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
            foreach (var (child, member) in AvatarMenu.ChooseValues.Keys)
            {
                if (!matchGameObjects.Contains(child)) continue;
                var value = AvatarMenu.ChooseValues[(child, member)];
                var curvePath = child;
                if (member.MemberTypeIsPrimitive)
                {
                    for (var i = 0; i < AvatarMenu.ChooseCount; ++i)
                    {
                        choices[i].SetCurve(curvePath, member.Type, member.AnimationMemberName, new AnimationCurve(new Keyframe(0, value.ContainsKey(i) ? Convert.ToSingle(value[i].As(member.MemberType)) : 0)));
                    }
                }
                else if (member.MemberType == typeof(Vector3))
                {
                    for (var i = 0; i < AvatarMenu.ChooseCount; ++i)
                    {
                        var v = value.ContainsKey(i) ? (Vector3)value[i] : Vector3.zero;
                        choices[i].SetCurve(curvePath, member.Type, member.AnimationMemberName + ".x", new AnimationCurve(new Keyframe(0, v.x)));
                        choices[i].SetCurve(curvePath, member.Type, member.AnimationMemberName + ".y", new AnimationCurve(new Keyframe(0, v.y)));
                        choices[i].SetCurve(curvePath, member.Type, member.AnimationMemberName + ".z", new AnimationCurve(new Keyframe(0, v.z)));
                    }
                }
                else if (member.MemberType == typeof(VRCPhysBoneBase.PermissionFilter))
                {
                    for (var i = 0; i < AvatarMenu.ChooseCount; ++i)
                    {
                        var v = value.ContainsKey(i) ? (VRCPhysBoneBase.PermissionFilter)value[i] : new VRCPhysBoneBase.PermissionFilter();
                        choices[i].SetCurve(curvePath, member.Type, member.AnimationMemberName + ".allowSelf", new AnimationCurve(new Keyframe(0, v.allowSelf ? 1 : 0)));
                        choices[i].SetCurve(curvePath, member.Type, member.AnimationMemberName + ".allowOthers", new AnimationCurve(new Keyframe(0, v.allowOthers ? 1 : 0)));
                    }
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
            var physBoneAutoResetEffectiveObjects = AvatarMenu.GetPhysBoneAutoResetEffectiveObjects(matchGameObjects, AvatarMenu.ChooseValues.Keys).ToArray();
            if (physBoneAutoResetEffectiveObjects.Length > 0)
            {
                controller.AddLayer(new AnimatorControllerLayer
                {
                    name = baseName + "_PB_auto_reset",
                    defaultWeight = 1,
                    stateMachine = new AnimatorStateMachine
                    {
                        name = baseName + "_PB_auto_reset",
                        hideFlags = HideFlags.HideInHierarchy,
                    },
                });
                var pbLayer = controller.layers[1];

                pbLayer.stateMachine.name = baseName;
                pbLayer.stateMachine.entryPosition = new Vector3(-600, 0);
                pbLayer.stateMachine.anyStatePosition = new Vector3(-900, 0);
                pbLayer.stateMachine.exitPosition = new Vector3(1700, 0);

                var emptyClip = util.Util.GenerateEmptyAnimationClip(baseName);
                var waitClip = util.Util.GenerateEmptyAnimationClip(baseName + "_wait", AvatarMenu.TransitionSeconds == 0 ? 1 / 60f : AvatarMenu.TransitionSeconds);
                var pbDisableClip = new AnimationClip { name = $"{baseName}_PB_disable" };
                var pbEnableClip = new AnimationClip { name = $"{baseName}_PB_enable" };
                var disableCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1 / 60f, 0));
                var enableCurve = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1 / 60f, 1));
                foreach (var child in physBoneAutoResetEffectiveObjects)
                {
                    var curvePath = child;
                    pbDisableClip.SetCurve(curvePath, typeof(VRCPhysBone), "m_Enabled", disableCurve);
                    pbEnableClip.SetCurve(curvePath, typeof(VRCPhysBone), "m_Enabled", enableCurve);
                }
                var initialState = pbLayer.stateMachine.AddState($"{baseName}_idle", new Vector3(-300, 0));
                initialState.motion = emptyClip;
                initialState.writeDefaultValues = false;
                pbLayer.stateMachine.defaultState = initialState;
                var waitState = pbLayer.stateMachine.AddState($"{baseName}_wait", new Vector3(800, 0));
                waitState.motion = waitClip;
                waitState.writeDefaultValues = false;
                var disableState = pbLayer.stateMachine.AddState($"{baseName}_disable", new Vector3(1100, 0));
                disableState.motion = pbDisableClip;
                disableState.writeDefaultValues = false;
                var enableState = pbLayer.stateMachine.AddState($"{baseName}_enable", new Vector3(1400, 0));
                enableState.motion = pbEnableClip;
                enableState.writeDefaultValues = false;
                var eachStates = Enumerable.Range(0, AvatarMenu.ChooseCount).Select((i) =>
                {
                    var state = pbLayer.stateMachine.AddState($"{baseName}_{i}", new Vector3(300, 50 * i));
                    state.motion = emptyClip;
                    state.writeDefaultValues = false;
                    return state;
                }).ToList();
                var toDisable = waitState.AddTransition(disableState);
                toDisable.exitTime = 1;
                toDisable.duration = 0;
                toDisable.hasExitTime = true;
                var toEnable = disableState.AddTransition(enableState);
                toEnable.exitTime = 1;
                toEnable.duration = 0;
                toEnable.hasExitTime = true;
                var toExit = enableState.AddExitTransition();
                toExit.exitTime = 1;
                toExit.duration = 0;
                toExit.hasExitTime = true;
                for (var i = 0; i < AvatarMenu.ChooseCount; ++i)
                {
                    var eachState = eachStates[i];
                    var initialToEach = initialState.AddTransition(eachState);
                    initialToEach.exitTime = 0;
                    initialToEach.duration = 0;
                    initialToEach.hasExitTime = false;
                    initialToEach.conditions = new AnimatorCondition[]
                    {
                        new AnimatorCondition
                        {
                            mode = AnimatorConditionMode.Equals,
                            parameter = parameterName,
                            threshold = i,
                        },
                    };
                    var eachToWait = eachState.AddTransition(waitState);
                    eachToWait.exitTime = 0;
                    eachToWait.duration = 0;
                    eachToWait.hasExitTime = false;
                    eachToWait.conditions = new AnimatorCondition[]
                    {
                        new AnimatorCondition
                        {
                            mode = AnimatorConditionMode.NotEqual,
                            parameter = parameterName,
                            threshold = i,
                        },
                    };
                }
                choices.Add(emptyClip);
                choices.Add(waitClip);
                choices.Add(pbDisableClip);
                choices.Add(pbEnableClip);
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
#if !NET_NARAZAKA_VRCHAT_AvatarMenuCreator_HAS_NO_MENU_MA
                    localOnly = !AvatarMenu.Synced,
#endif
                    internalParameter = AvatarMenu.InternalParameter,
                },
            });
        }

        void SetTransformCurve(List<AnimationClip> choices, string path, string property, IntVector3Dictionary value)
        {
            for (var i = 0; i < AvatarMenu.ChooseCount; ++i)
            {
                var v = value.ContainsKey(i) ? value[i] : Vector3.zero;
                choices[i].SetCurve(path, typeof(Transform), property + ".x", new AnimationCurve(new Keyframe(0, v.x)));
                choices[i].SetCurve(path, typeof(Transform), property + ".y", new AnimationCurve(new Keyframe(0, v.y)));
                choices[i].SetCurve(path, typeof(Transform), property + ".z", new AnimationCurve(new Keyframe(0, v.z)));
            }
        }
    }
}
