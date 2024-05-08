using nadena.dev.modular_avatar.core;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using YamlDotNet.Core.Tokens;

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
                emptyClip = new AnimationClip();
                emptyClip.name = $"{baseName}_empty";
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
                        value = AvatarMenu.RadialDefaultValue,
                        labels = new VRCExpressionsMenu.Control.Label[] { },
                        icon = AvatarMenu.RadialIcon,
                    },
                },
            };
            menu.name = baseName;
            // prefab
            return new CreatedAssets(baseName, controller, emptyClip == null ? new AnimationClip[] { clip } : new AnimationClip[] { clip, emptyClip }, menu, null, new ParameterConfig[]
            {
                new ParameterConfig
                {
                    nameOrPrefix = parameterName,
                    defaultValue = AvatarMenu.RadialDefaultValue,
                    syncType = ParameterSyncType.Float,
                    saved = AvatarMenu.Saved,
                    internalParameter = AvatarMenu.InternalParameter,
                },
            });
        }

        AnimationCurve FullAnimationCurve(Keyframe start, Keyframe end)
        {
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
    }
}
