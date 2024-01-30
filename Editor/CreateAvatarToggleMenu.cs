using nadena.dev.modular_avatar.core;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace net.narazaka.avatarmenucreator.editor
{
    public class CreateAvatarToggleMenu : CreateAvatarMenuBase
    {
        AvatarToggleMenu AvatarMenu;
        public CreateAvatarToggleMenu(AvatarToggleMenu avatarToggleMenu) => AvatarMenu = avatarToggleMenu;

        public override CreatedAssets CreateAssets(string baseName, IEnumerable<string> children = null)
        {
            var matchGameObjects = new HashSet<string>(children ?? AvatarMenu.GetStoredChildren());
            // clip
            var active = new AnimationClip();
            active.name = $"{baseName}_active";
            var activate = new AnimationClip();
            activate.name = $"{baseName}_activate";
            var inactive = new AnimationClip();
            inactive.name = $"{baseName}_inactive";
            var inactivate = new AnimationClip();
            inactivate.name = $"{baseName}_inactivate";
            foreach (var child in AvatarMenu.ToggleObjects.Keys)
            {
                if (!matchGameObjects.Contains(child)) continue;
                var activeValue = AvatarMenu.ToggleObjects[child] == ToggleType.ON;
                var curvePath = child;
                active.SetCurve(curvePath, typeof(GameObject), "m_IsActive", new AnimationCurve(new Keyframe(0 / 60.0f, activeValue ? 1 : 0)));
                inactive.SetCurve(curvePath, typeof(GameObject), "m_IsActive", new AnimationCurve(new Keyframe(0 / 60.0f, activeValue ? 0 : 1)));
                if (AvatarMenu.TransitionSeconds > 0)
                {
                    activate.SetCurve(curvePath, typeof(GameObject), "m_IsActive", new AnimationCurve(new Keyframe(0 / 60.0f, 1), new Keyframe(AvatarMenu.TransitionSeconds, activeValue ? 1 : 0)));
                    inactivate.SetCurve(curvePath, typeof(GameObject), "m_IsActive", new AnimationCurve(new Keyframe(0 / 60.0f, 1), new Keyframe(AvatarMenu.TransitionSeconds, activeValue ? 0 : 1)));
                }
            }
            foreach (var (child, name) in AvatarMenu.ToggleBlendShapes.Keys)
            {
                if (!matchGameObjects.Contains(child)) continue;
                var value = AvatarMenu.ToggleBlendShapes[(child, name)];
                var curvePath = child;
                var curveName = $"blendShape.{name}";
                active.SetCurve(curvePath, typeof(SkinnedMeshRenderer), curveName, new AnimationCurve(new Keyframe(0 / 60.0f, value.Active)));
                inactive.SetCurve(curvePath, typeof(SkinnedMeshRenderer), curveName, new AnimationCurve(new Keyframe(0 / 60.0f, value.Inactive)));
                if (AvatarMenu.TransitionSeconds > 0)
                {
                    activate.SetCurve(curvePath, typeof(SkinnedMeshRenderer), curveName, new AnimationCurve(new Keyframe(AvatarMenu.TransitionSeconds * value.ActivateStartRate, value.Inactive), new Keyframe(AvatarMenu.TransitionSeconds * value.ActivateEndRate, value.Active)));
                    inactivate.SetCurve(curvePath, typeof(SkinnedMeshRenderer), curveName, new AnimationCurve(new Keyframe(AvatarMenu.TransitionSeconds * value.InactivateStartRate, value.Active), new Keyframe(AvatarMenu.TransitionSeconds * value.InactivateEndRate, value.Inactive)));
                }
            }
            foreach (var (child, name) in AvatarMenu.ToggleShaderParameters.Keys)
            {
                if (!matchGameObjects.Contains(child)) continue;
                var value = AvatarMenu.ToggleShaderParameters[(child, name)];
                var curvePath = child;
                var curveName = $"material.{name}";
                active.SetCurve(curvePath, typeof(Renderer), curveName, new AnimationCurve(new Keyframe(0 / 60.0f, value.Active)));
                inactive.SetCurve(curvePath, typeof(Renderer), curveName, new AnimationCurve(new Keyframe(0 / 60.0f, value.Inactive)));
                if (AvatarMenu.TransitionSeconds > 0)
                {
                    activate.SetCurve(curvePath, typeof(Renderer), curveName, new AnimationCurve(new Keyframe(AvatarMenu.TransitionSeconds * value.ActivateStartRate, value.Inactive), new Keyframe(AvatarMenu.TransitionSeconds * value.ActivateEndRate, value.Active)));
                    inactivate.SetCurve(curvePath, typeof(Renderer), curveName, new AnimationCurve(new Keyframe(AvatarMenu.TransitionSeconds * value.InactivateStartRate, value.Active), new Keyframe(AvatarMenu.TransitionSeconds * value.InactivateEndRate, value.Inactive)));
                }
            }
            // controller
            var controller = new AnimatorController();
            controller.AddParameter(new AnimatorControllerParameter { name = baseName, type = AnimatorControllerParameterType.Bool, defaultBool = false });
            if (controller.layers.Length == 0) controller.AddLayer(baseName);
            var layer = controller.layers[0];
            layer.name = baseName;
            layer.stateMachine.name = baseName;
            layer.stateMachine.entryPosition = new Vector3(-600, 0);
            layer.stateMachine.anyStatePosition = new Vector3(800, 0);
            var idleState = layer.stateMachine.AddState($"{baseName}_idle", new Vector3(-300, 0));
            idleState.motion = inactive;
            idleState.writeDefaultValues = false;
            var inactiveState = layer.stateMachine.AddState($"{baseName}_inactive", new Vector3(300, 100));
            inactiveState.motion = inactive;
            inactiveState.writeDefaultValues = false;
            var activeState = layer.stateMachine.AddState($"{baseName}_active", new Vector3(300, -100));
            activeState.motion = active;
            activeState.writeDefaultValues = false;
            layer.stateMachine.defaultState = idleState;
            var idleToActive = idleState.AddTransition(activeState);
            idleToActive.exitTime = 0;
            idleToActive.duration = 0;
            idleToActive.hasExitTime = false;
            idleToActive.conditions = new AnimatorCondition[]
            {
                new AnimatorCondition
                {
                    mode = AnimatorConditionMode.If,
                    parameter = baseName,
                    threshold = 1,
                },
            };
            var idleToInactive = idleState.AddTransition(inactiveState);
            idleToInactive.exitTime = 0;
            idleToInactive.duration = 0;
            idleToInactive.hasExitTime = false;
            idleToInactive.conditions = new AnimatorCondition[]
            {
                new AnimatorCondition
                {
                    mode = AnimatorConditionMode.IfNot,
                    parameter = baseName,
                    threshold = 1,
                },
            };
            if (AvatarMenu.TransitionSeconds > 0)
            {
                var inactivateState = layer.stateMachine.AddState($"{baseName}_inactivate", new Vector3(500, 0));
                inactivateState.motion = inactivate;
                inactivateState.writeDefaultValues = false;
                var activateState = layer.stateMachine.AddState($"{baseName}_activate", new Vector3(100, 0));
                activateState.motion = activate;
                activateState.writeDefaultValues = false;
                var toActivate = inactiveState.AddTransition(activateState);
                toActivate.exitTime = 0;
                toActivate.duration = 0;
                toActivate.hasExitTime = false;
                toActivate.conditions = new AnimatorCondition[]
                {
                    new AnimatorCondition
                    {
                        mode = AnimatorConditionMode.If,
                        parameter = baseName,
                        threshold = 1,
                    },
                };
                var toActive = activateState.AddTransition(activeState);
                toActive.exitTime = 1;
                toActive.duration = 0;
                toActive.hasExitTime = true;
                var toInactivate = activeState.AddTransition(inactivateState);
                toInactivate.exitTime = 0;
                toInactivate.duration = 0;
                toInactivate.hasExitTime = false;
                toInactivate.conditions = new AnimatorCondition[]
                {
                    new AnimatorCondition
                    {
                        mode = AnimatorConditionMode.IfNot,
                        parameter = baseName,
                        threshold = 1,
                    },
                };
                var toInactive = inactivateState.AddTransition(inactiveState);
                toInactive.exitTime = 1;
                toInactive.duration = 0;
                toInactive.hasExitTime = true;
            }
            else
            {
                var toActive = inactiveState.AddTransition(activeState);
                toActive.exitTime = 0;
                toActive.duration = 0;
                toActive.hasExitTime = false;
                toActive.conditions = new AnimatorCondition[]
                {
                    new AnimatorCondition
                    {
                        mode = AnimatorConditionMode.If,
                        parameter = baseName,
                        threshold = 1,
                    },
                };
                var toInactive = activeState.AddTransition(inactiveState);
                toInactive.exitTime = 0;
                toInactive.duration = 0;
                toInactive.hasExitTime = false;
                toInactive.conditions = new AnimatorCondition[]
                {
                    new AnimatorCondition
                    {
                        mode = AnimatorConditionMode.IfNot,
                        parameter = baseName,
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
                        type = VRCExpressionsMenu.Control.ControlType.Toggle,
                        parameter = new VRCExpressionsMenu.Control.Parameter
                        {
                            name = baseName,
                        },
                        subParameters = new VRCExpressionsMenu.Control.Parameter[] { },
                        value = 1,
                        labels = new VRCExpressionsMenu.Control.Label[] { },
                    },
                },
            };
            menu.name = baseName;
            return new CreatedAssets(baseName, controller, AvatarMenu.TransitionSeconds > 0 ? new AnimationClip[] { active, inactive, activate, inactivate } : new AnimationClip[] { active, inactive }, menu, null, (prefab) =>
            {
                var menuInstaller = prefab.GetOrAddComponent<ModularAvatarMenuInstaller>();
                menuInstaller.menuToAppend = menu;
                var parameters = prefab.GetOrAddComponent<ModularAvatarParameters>();
                parameters.parameters.Clear();
                parameters.parameters.Add(new ParameterConfig
                {
                    nameOrPrefix = baseName,
                    defaultValue = AvatarMenu.ToggleDefaultValue ? 1 : 0,
                    syncType = ParameterSyncType.Bool,
                    saved = AvatarMenu.Saved,
                });
                var mergeAnimator = prefab.GetOrAddComponent<ModularAvatarMergeAnimator>();
                mergeAnimator.animator = controller;
                mergeAnimator.layerType = VRCAvatarDescriptor.AnimLayerType.FX;
                mergeAnimator.pathMode = MergeAnimatorPathMode.Absolute;
                mergeAnimator.matchAvatarWriteDefaults = true;
            });
        }
    }
}