using nadena.dev.modular_avatar.core;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using VRC.SDK3.Dynamics.PhysBone.Components;

namespace net.narazaka.avatarmenucreator.editor
{
    public class CreateAvatarRadialMenu : CreateAvatarMenuBase
    {
        AvatarRadialMenu AvatarMenu;
        public CreateAvatarRadialMenu(AvatarRadialMenu avatarRadialMenu) => AvatarMenu = avatarRadialMenu;

        public override CreatedAssets CreateAssets(string baseName, IEnumerable<string> children = null)
        {
            var matchGameObjects = new HashSet<string>(children ?? AvatarMenu.GetStoredChildren());
            var parameterName = string.IsNullOrEmpty(AvatarMenu.ParameterName) ? baseName : AvatarMenu.ParameterName;
            // clip
            var clip = new AnimationClip();
            clip.name = baseName;
            foreach (var (child, name) in AvatarMenu.RadialBlendShapes.Keys)
            {
                if (!matchGameObjects.Contains(child)) continue;
                var value = AvatarMenu.RadialBlendShapes[(child, name)];
                clip.SetCurve(child, typeof(SkinnedMeshRenderer), $"blendShape.{name}", FullAnimationCurve(new Keyframe(value.StartOffsetPercent, value.Start), new Keyframe(value.EndOffsetPercent, value.End)));
            }
            foreach (var (child, name) in AvatarMenu.RadialShaderParameters.Keys)
            {
                if (!matchGameObjects.Contains(child)) continue;
                var value = AvatarMenu.RadialShaderParameters[(child, name)];
                clip.SetCurve(child, typeof(Renderer), $"material.{name}", FullAnimationCurve(new Keyframe(value.StartOffsetPercent, value.Start), new Keyframe(value.EndOffsetPercent, value.End)));
            }
            foreach (var (child, name) in AvatarMenu.RadialShaderVectorParameters.Keys)
            {
                if (!matchGameObjects.Contains(child)) continue;
                var value = AvatarMenu.RadialShaderVectorParameters[(child, name)];
                SetVector4Curve(clip, typeof(Renderer), child, $"material.{name}", value);
            }
            foreach (var (child, member) in AvatarMenu.RadialValues.Keys)
            {
                if (!matchGameObjects.Contains(child)) continue;
                var value = AvatarMenu.RadialValues[(child, member)];
                if (member.MemberType == typeof(float))
                {
                    clip.SetCurve(child, member.Type, member.AnimationMemberName, FullAnimationCurve(new Keyframe(value.StartOffsetPercent, (float)value.Start), new Keyframe(value.EndOffsetPercent, (float)value.End)));
                }
                else if (member.MemberType == typeof(Vector3))
                {
                    var start = (Vector3)value.Start;
                    var end = (Vector3)value.End;
                    clip.SetCurve(child, member.Type, $"{member.AnimationMemberName}.x", FullAnimationCurve(new Keyframe(value.StartOffsetPercent, start.x), new Keyframe(value.EndOffsetPercent, end.x)));
                    clip.SetCurve(child, member.Type, $"{member.AnimationMemberName}.y", FullAnimationCurve(new Keyframe(value.StartOffsetPercent, start.y), new Keyframe(value.EndOffsetPercent, end.y)));
                    clip.SetCurve(child, member.Type, $"{member.AnimationMemberName}.z", FullAnimationCurve(new Keyframe(value.StartOffsetPercent, start.z), new Keyframe(value.EndOffsetPercent, end.z)));
                }
            }
            foreach (var child in AvatarMenu.Positions.Keys)
            {
                if (!matchGameObjects.Contains(child)) continue;
                var value = AvatarMenu.Positions[child];
                SetTransformCurve(clip, child, "localPosition", value);
            }
            foreach (var child in AvatarMenu.Rotations.Keys)
            {
                if (!matchGameObjects.Contains(child)) continue;
                var value = AvatarMenu.Rotations[child];
                SetTransformCurve(clip, child, "localEulerAnglesRaw", value);
            }
            foreach (var child in AvatarMenu.Scales.Keys)
            {
                if (!matchGameObjects.Contains(child)) continue;
                var value = AvatarMenu.Scales[child];
                SetTransformCurve(clip, child, "localScale", value);
            }
            // controller
            var controller = new AnimatorController();
            controller.AddParameter(new AnimatorControllerParameter { name = parameterName, type = AnimatorControllerParameterType.Float, defaultFloat = AvatarMenu.RadialDefaultValue });
            if (controller.layers.Length == 0) controller.AddLayer(baseName);
            var layer = controller.layers[0];
            layer.name = baseName;
            layer.stateMachine.name = baseName;
            var state = layer.stateMachine.AddState(baseName, new Vector3(300, 0));
            state.timeParameterActive = true;
            state.timeParameter = parameterName;
            state.motion = clip;
            state.writeDefaultValues = false;
            AnimationClip emptyClip = null;
            if (AvatarMenu.RadialInactiveRange && !(float.IsNaN(AvatarMenu.RadialInactiveRangeMin) && float.IsNaN(AvatarMenu.RadialInactiveRangeMax)))
            {
                emptyClip = util.Util.GenerateEmptyAnimationClip(baseName);
                var inactiveState = layer.stateMachine.AddState($"{baseName}_inactive", new Vector3(300, 100));
                inactiveState.motion = emptyClip;
                inactiveState.writeDefaultValues = false;
                layer.stateMachine.defaultState = inactiveState;
                var toInactive = state.AddTransition(inactiveState);
                toInactive.exitTime = 0;
                toInactive.duration = 0;
                toInactive.hasExitTime = false;
                var toInactiveConditions = new List<AnimatorCondition>();
                if (!float.IsNaN(AvatarMenu.RadialInactiveRangeMin))
                {
                    toInactiveConditions.Add(new AnimatorCondition
                    {
                        mode = AnimatorConditionMode.Greater,
                        parameter = parameterName,
                        threshold = AvatarMenu.RadialInactiveRangeMin,
                    });
                }
                if (!float.IsNaN(AvatarMenu.RadialInactiveRangeMax))
                {
                    toInactiveConditions.Add(new AnimatorCondition
                    {
                        mode = AnimatorConditionMode.Less,
                        parameter = parameterName,
                        threshold = AvatarMenu.RadialInactiveRangeMax,
                    });
                }
                toInactive.conditions = toInactiveConditions.ToArray();

                if (!float.IsNaN(AvatarMenu.RadialInactiveRangeMin))
                {
                    var toActiveMin = inactiveState.AddTransition(state);
                    toActiveMin.exitTime = 0;
                    toActiveMin.duration = 0;
                    toActiveMin.hasExitTime = false;
                    toActiveMin.conditions = new AnimatorCondition[]
                    {
                        new AnimatorCondition
                        {
                            mode = AnimatorConditionMode.Less,
                            parameter = parameterName,
                            threshold = AvatarMenu.RadialInactiveRangeMin,
                        },
                    };
                }
                if (!float.IsNaN(AvatarMenu.RadialInactiveRangeMax))
                {
                    var toActiveMax = inactiveState.AddTransition(state);
                    toActiveMax.exitTime = 0;
                    toActiveMax.duration = 0;
                    toActiveMax.hasExitTime = false;
                    toActiveMax.conditions = new AnimatorCondition[]
                    {
                        new AnimatorCondition
                        {
                            mode = AnimatorConditionMode.Greater,
                            parameter = parameterName,
                            threshold = AvatarMenu.RadialInactiveRangeMax,
                        },
                    };
                }
            }
            var physBoneAutoResetEffectiveObjects = AvatarMenu.GetPhysBoneAutoResetEffectiveObjects(matchGameObjects, AvatarMenu.RadialValues.Keys).ToArray();
            var changingParameterName = parameterName + "_changing";
            var needChangingParameter = physBoneAutoResetEffectiveObjects.Length > 0 || AvatarMenu.LockRemoteDuringChange || AvatarMenu.PreventRemoteFloatBug;
            if (needChangingParameter)
            {
                controller.AddParameter(new AnimatorControllerParameter { name = changingParameterName, type = AnimatorControllerParameterType.Bool, defaultBool = false });
            }
            AnimationClip pbDisableClip = null;
            AnimationClip pbEnableClip = null;
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
                pbLayer.stateMachine.anyStatePosition = new Vector3(1500, 0);

                if (emptyClip == null) emptyClip = util.Util.GenerateEmptyAnimationClip(baseName);
                pbDisableClip = new AnimationClip { name = $"{baseName}_PB_disable" };
                pbEnableClip = new AnimationClip { name = $"{baseName}_PB_enable" };
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
                var changingState = pbLayer.stateMachine.AddState($"{baseName}_changing", new Vector3(300, 0));
                changingState.motion = emptyClip;
                changingState.writeDefaultValues = false;
                var disableState = pbLayer.stateMachine.AddState($"{baseName}_disable", new Vector3(300, 300));
                disableState.motion = pbDisableClip;
                disableState.writeDefaultValues = false;
                var enableState = pbLayer.stateMachine.AddState($"{baseName}_enable", new Vector3(-300, 300));
                enableState.motion = pbEnableClip;
                enableState.writeDefaultValues = false;

                var toChanging = initialState.AddTransition(changingState);
                toChanging.exitTime = 0;
                toChanging.duration = 0;
                toChanging.hasExitTime = false;
                toChanging.conditions = new AnimatorCondition[]
                {
                    new AnimatorCondition
                    {
                        mode = AnimatorConditionMode.If,
                        parameter = changingParameterName,
                        threshold = 1,
                    },
                };
                var toDisable = changingState.AddTransition(disableState);
                toDisable.exitTime = 0;
                toDisable.duration = 0;
                toDisable.hasExitTime = false;
                toDisable.conditions = new AnimatorCondition[]
                {
                    new AnimatorCondition
                    {
                        mode = AnimatorConditionMode.IfNot,
                        parameter = changingParameterName,
                        threshold = 1,
                    },
                };
                var toEnable = disableState.AddTransition(enableState);
                toEnable.exitTime = 1;
                toEnable.duration = 0;
                toEnable.hasExitTime = true;
                var toIdle = enableState.AddTransition(initialState);
                toIdle.exitTime = 1;
                toIdle.duration = 0;
                toIdle.hasExitTime = true;
            }
            var preventParameterName = parameterName + "_prevent";
            if (AvatarMenu.LockRemoteDuringChange || AvatarMenu.PreventRemoteFloatBug)
            {
                controller.AddParameter(new AnimatorControllerParameter { name = preventParameterName, type = AnimatorControllerParameterType.Float, defaultFloat = AvatarMenu.RadialDefaultValue });
                state.behaviours = new StateMachineBehaviour[]
                {
                    new VRCAvatarParameterDriver
                    {
                        parameters = new List<VRC.SDKBase.VRC_AvatarParameterDriver.Parameter>
                        {
                            new VRC.SDKBase.VRC_AvatarParameterDriver.Parameter
                            {
                                type = VRC.SDKBase.VRC_AvatarParameterDriver.ChangeType.Copy,
                                source = parameterName,
                                name = preventParameterName,
                            }
                        },
                    },
                };
            }
            if (AvatarMenu.LockRemoteDuringChange)
            {
                controller.AddParameter(new AnimatorControllerParameter { name = "IsLocal", type = AnimatorControllerParameterType.Bool, defaultBool = false });
                var preventState = layer.stateMachine.AddState($"{baseName}_prevent", new Vector3(300, -100));
                preventState.timeParameterActive = true;
                preventState.timeParameter = preventParameterName;
                preventState.motion = clip;
                preventState.writeDefaultValues = false;
                var toPrevent = state.AddTransition(preventState);
                toPrevent.exitTime = 0;
                toPrevent.duration = 0;
                toPrevent.hasExitTime = false;
                toPrevent.conditions = new AnimatorCondition[]
                {
                    new AnimatorCondition
                    {
                        mode = AnimatorConditionMode.IfNot,
                        parameter = "IsLocal",
                        threshold = 1f,
                    },
                    new AnimatorCondition
                    {
                        mode = AnimatorConditionMode.If,
                        parameter = changingParameterName,
                        threshold = 1,
                    },
                };
                var toActive = preventState.AddTransition(state);
                toActive.exitTime = 0;
                toActive.duration = 0;
                toActive.hasExitTime = false;
                toActive.conditions = new AnimatorCondition[]
                {
                    new AnimatorCondition
                    {
                        mode = AnimatorConditionMode.IfNot,
                        parameter = changingParameterName,
                        threshold = 1,
                    },
                };
            }
            else if (AvatarMenu.PreventRemoteFloatBug)
            {
                var preventState = layer.stateMachine.AddState($"{baseName}_prevent", new Vector3(300, -100));
                preventState.timeParameterActive = true;
                preventState.timeParameter = preventParameterName;
                preventState.motion = clip;
                preventState.writeDefaultValues = false;
                var preventWaitState = layer.stateMachine.AddState($"{baseName}_prevent_wait", new Vector3(600, -100));
                preventWaitState.timeParameterActive = true;
                preventWaitState.timeParameter = preventParameterName;
                preventWaitState.motion = clip;
                preventWaitState.writeDefaultValues = false;
                var preventedState = layer.stateMachine.AddState($"{baseName}_prevented", new Vector3(600, 0));
                preventedState.timeParameterActive = true;
                preventedState.timeParameter = parameterName;
                preventedState.motion = clip;
                preventedState.writeDefaultValues = false;
                // if change start
                var toPrevent = state.AddTransition(preventState);
                toPrevent.exitTime = 0;
                toPrevent.duration = 0;
                toPrevent.hasExitTime = false;
                toPrevent.conditions = new AnimatorCondition[]
                {
                    new AnimatorCondition
                    {
                        mode = AnimatorConditionMode.If,
                        parameter = changingParameterName,
                        threshold = 1,
                    },
                };
                // if 0.3s after change start
                var toPreventWait1 = preventState.AddTransition(preventWaitState);
                toPreventWait1.exitTime = 0;
                toPreventWait1.duration = 0.3f;
                toPreventWait1.hasExitTime = false;
                toPreventWait1.hasFixedDuration = true;
                toPreventWait1.conditions = new AnimatorCondition[]
                {
                    new AnimatorCondition
                    {
                        mode = AnimatorConditionMode.If,
                        parameter = changingParameterName,
                        threshold = 1,
                    },
                };
                var toPreventWait2 = preventState.AddTransition(preventWaitState);
                toPreventWait2.exitTime = 0;
                toPreventWait2.duration = 0.3f;
                toPreventWait2.hasExitTime = false;
                toPreventWait2.hasFixedDuration = true;
                toPreventWait2.conditions = new AnimatorCondition[]
                {
                    new AnimatorCondition
                    {
                        mode = AnimatorConditionMode.IfNot,
                        parameter = changingParameterName,
                        threshold = 1,
                    },
                };
                // 0.1s blending
                var toPrevented1 = preventWaitState.AddTransition(preventedState);
                toPrevented1.exitTime = 0;
                toPrevented1.duration = 0.1f;
                toPrevented1.hasExitTime = false;
                toPrevented1.interruptionSource = TransitionInterruptionSource.Destination;
                toPrevented1.conditions = new AnimatorCondition[]
                {
                    new AnimatorCondition
                    {
                        mode = AnimatorConditionMode.If,
                        parameter = changingParameterName,
                        threshold = 1,
                    },
                };
                var toPrevented2 = preventWaitState.AddTransition(preventedState);
                toPrevented2.exitTime = 0;
                toPrevented2.duration = 0.1f;
                toPrevented2.hasExitTime = false;
                toPrevented2.interruptionSource = TransitionInterruptionSource.Destination;
                toPrevented2.conditions = new AnimatorCondition[]
                {
                    new AnimatorCondition
                    {
                        mode = AnimatorConditionMode.IfNot,
                        parameter = changingParameterName,
                        threshold = 1,
                    },
                };
                var toActive = preventedState.AddTransition(state);
                toActive.exitTime = 0;
                toActive.duration = 0;
                toActive.hasExitTime = false;
                toActive.conditions = new AnimatorCondition[]
                {
                    new AnimatorCondition
                    {
                        mode = AnimatorConditionMode.IfNot,
                        parameter = changingParameterName,
                        threshold = 1,
                    },
                };
            }
            // menu
            var menu = new VRCExpressionsMenu
            {
                controls = new List<VRCExpressionsMenu.Control>
                {
                    new VRCExpressionsMenu.Control {
                        name = baseName,
                        type = VRCExpressionsMenu.Control.ControlType.RadialPuppet,
                        subParameters = new VRCExpressionsMenu.Control.Parameter[] {
                            new VRCExpressionsMenu.Control.Parameter
                            {
                                name = parameterName,
                            }
                        },
                        value = 1,
                        labels = new VRCExpressionsMenu.Control.Label[] { },
                        icon = AvatarMenu.RadialIcon,
                    },
                },
            };
            if (needChangingParameter)
            {
                menu.controls[0].parameter = new VRCExpressionsMenu.Control.Parameter
                {
                    name = changingParameterName,
                };
            }
            menu.name = baseName;
            var parameterConfig = new ParameterConfig
            {
                nameOrPrefix = parameterName,
                defaultValue = AvatarMenu.RadialDefaultValue,
                syncType = ParameterSyncType.Float,
                saved = AvatarMenu.Saved,
#if !NET_NARAZAKA_VRCHAT_AvatarMenuCreator_HAS_NO_MENU_MA
                localOnly = !AvatarMenu.Synced,
#endif
                internalParameter = AvatarMenu.InternalParameter,
            };
            var subParameterConfig = new ParameterConfig
            {
                nameOrPrefix = changingParameterName,
                defaultValue = 0,
                syncType = ParameterSyncType.Bool,
                saved = false,
#if !NET_NARAZAKA_VRCHAT_AvatarMenuCreator_HAS_NO_MENU_MA
                localOnly = !AvatarMenu.Synced,
#endif
                internalParameter = AvatarMenu.InternalParameter,
            };
            var parameterConfigs = needChangingParameter ? new ParameterConfig[] { parameterConfig, subParameterConfig } : new ParameterConfig[] { parameterConfig };
            var clips = new List<AnimationClip> { clip };
            if (emptyClip != null) clips.Add(emptyClip);
            if (pbDisableClip != null) clips.Add(pbDisableClip);
            if (pbEnableClip != null) clips.Add(pbEnableClip);
            // prefab
            return new CreatedAssets(baseName, controller, clips.ToArray(), menu, null, parameterConfigs);
        }

        AnimationCurve FullAnimationCurve(Keyframe start, Keyframe end)
        {
            if (start.time > end.time)
            {
                var temp = start;
                start = end;
                end = temp;
            }
            var curve = new AnimationCurve(start, end);
            if (end.time < 100)
            {
                curve.AddKey(100, end.value);
            }
            SetAutoTangentMode(curve);
            return curve;
        }

        AnimationCurve SetAutoTangentMode(AnimationCurve curve)
        {
            AnimationUtility.SetKeyLeftTangentMode(curve, 0, AnimationUtility.TangentMode.Constant);
            AnimationUtility.SetKeyRightTangentMode(curve, 0, AnimationUtility.TangentMode.Linear);
            AnimationUtility.SetKeyLeftTangentMode(curve, 1, AnimationUtility.TangentMode.Linear);
            AnimationUtility.SetKeyRightTangentMode(curve, 1, AnimationUtility.TangentMode.Constant);
            return curve;
        }

        void SetTransformCurve(AnimationClip clip, string child, string propertyName, RadialVector3 value)
        {
            clip.SetCurve(child, typeof(Transform), $"{propertyName}.x", FullAnimationCurve(new Keyframe(value.StartOffsetPercent, value.Start.x), new Keyframe(value.EndOffsetPercent, value.End.x)));
            clip.SetCurve(child, typeof(Transform), $"{propertyName}.y", FullAnimationCurve(new Keyframe(value.StartOffsetPercent, value.Start.y), new Keyframe(value.EndOffsetPercent, value.End.y)));
            clip.SetCurve(child, typeof(Transform), $"{propertyName}.z", FullAnimationCurve(new Keyframe(value.StartOffsetPercent, value.Start.z), new Keyframe(value.EndOffsetPercent, value.End.z)));
        }

        void SetVector4Curve(AnimationClip clip, System.Type type, string child, string propertyName, RadialVector4 value)
        {
            clip.SetCurve(child, type, $"{propertyName}.x", FullAnimationCurve(new Keyframe(value.StartOffsetPercent, value.Start.x), new Keyframe(value.EndOffsetPercent, value.End.x)));
            clip.SetCurve(child, type, $"{propertyName}.y", FullAnimationCurve(new Keyframe(value.StartOffsetPercent, value.Start.y), new Keyframe(value.EndOffsetPercent, value.End.y)));
            clip.SetCurve(child, type, $"{propertyName}.z", FullAnimationCurve(new Keyframe(value.StartOffsetPercent, value.Start.z), new Keyframe(value.EndOffsetPercent, value.End.z)));
            clip.SetCurve(child, type, $"{propertyName}.w", FullAnimationCurve(new Keyframe(value.StartOffsetPercent, value.Start.w), new Keyframe(value.EndOffsetPercent, value.End.w)));
        }
    }
}
