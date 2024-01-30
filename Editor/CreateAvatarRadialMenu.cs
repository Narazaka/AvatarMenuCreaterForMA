using nadena.dev.modular_avatar.core;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace net.narazaka.avatarmenucreator.editor
{
    public class CreateAvatarRadialMenu : CreateAvatarMenuBase
    {
        AvatarRadialMenu AvatarMenu;
        public CreateAvatarRadialMenu(AvatarRadialMenu avatarRadialMenu) => AvatarMenu = avatarRadialMenu;

        public override CreatedAssets CreateAssets(string baseName, IEnumerable<string> children = null)
        {
            var matchGameObjects = new HashSet<string>(children ?? AvatarMenu.GetStoredChildren());
            // clip
            var clip = new AnimationClip();
            clip.name = baseName;
            foreach (var (child, name) in AvatarMenu.RadialBlendShapes.Keys)
            {
                if (!matchGameObjects.Contains(child)) continue;
                var value = AvatarMenu.RadialBlendShapes[(child, name)];
                clip.SetCurve(child, typeof(SkinnedMeshRenderer), $"blendShape.{name}", SetAutoTangentMode(new AnimationCurve(new Keyframe(0 / 60.0f, value.Start), new Keyframe(1 / 60.0f, value.End))));
            }
            foreach (var (child, name) in AvatarMenu.RadialShaderParameters.Keys)
            {
                if (!matchGameObjects.Contains(child)) continue;
                var value = AvatarMenu.RadialShaderParameters[(child, name)];
                clip.SetCurve(child, typeof(Renderer), $"material.{name}", SetAutoTangentMode(new AnimationCurve(new Keyframe(0 / 60.0f, value.Start), new Keyframe(1 / 60.0f, value.End))));
            }
            // controller
            var controller = new AnimatorController();
            controller.AddParameter(new AnimatorControllerParameter { name = baseName, type = AnimatorControllerParameterType.Float, defaultFloat = AvatarMenu.RadialDefaultValue });
            if (controller.layers.Length == 0) controller.AddLayer(baseName);
            var layer = controller.layers[0];
            layer.name = baseName;
            layer.stateMachine.name = baseName;
            var state = layer.stateMachine.AddState(baseName, new Vector3(300, 0));
            state.timeParameterActive = true;
            state.timeParameter = baseName;
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
                        parameter = baseName,
                        threshold = AvatarMenu.RadialInactiveRangeMin,
                    });
                }
                if (!float.IsNaN(AvatarMenu.RadialInactiveRangeMax))
                {
                    toInactiveConditions.Add(new AnimatorCondition
                    {
                        mode = AnimatorConditionMode.Less,
                        parameter = baseName,
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
                            parameter = baseName,
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
                            parameter = baseName,
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
                                name = baseName,
                            }
                        },
                        value = AvatarMenu.RadialDefaultValue,
                        labels = new VRCExpressionsMenu.Control.Label[] { },
                    },
                },
            };
            menu.name = baseName;
            // prefab
            return new CreatedAssets(baseName, controller, emptyClip == null ? new AnimationClip[] { clip } : new AnimationClip[] { clip, emptyClip }, menu, null, (prefab) =>
            {
                var menuInstaller = prefab.GetOrAddComponent<ModularAvatarMenuInstaller>();
                menuInstaller.menuToAppend = menu;
                var parameters = prefab.GetOrAddComponent<ModularAvatarParameters>();
                parameters.parameters.Clear();
                parameters.parameters.Add(new ParameterConfig
                {
                    nameOrPrefix = baseName,
                    defaultValue = AvatarMenu.RadialDefaultValue,
                    syncType = ParameterSyncType.Float,
                    saved = AvatarMenu.Saved,
                });
                var mergeAnimator = prefab.GetOrAddComponent<ModularAvatarMergeAnimator>();
                mergeAnimator.animator = controller;
                mergeAnimator.layerType = VRCAvatarDescriptor.AnimLayerType.FX;
                mergeAnimator.pathMode = MergeAnimatorPathMode.Absolute;
                mergeAnimator.matchAvatarWriteDefaults = true;
            });
        }

        AnimationCurve SetAutoTangentMode(AnimationCurve curve)
        {
            for (var i = 0; i < curve.length; ++i)
            {
                AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.Auto);
                AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.Auto);
            }
            return curve;
        }
    }
}
