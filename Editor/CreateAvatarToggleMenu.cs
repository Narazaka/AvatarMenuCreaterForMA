using nadena.dev.modular_avatar.core;
using net.narazaka.avatarmenucreator.valuecurve;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.ScriptableObjects;
using VRC.SDK3.Dynamics.PhysBone.Components;

namespace net.narazaka.avatarmenucreator.editor
{
    public class CreateAvatarToggleMenu : CreateAvatarMenuBase
    {
        AvatarToggleMenu AvatarMenu;
        public CreateAvatarToggleMenu(Transform avatarRoot, AvatarToggleMenu avatarToggleMenu) : base(avatarRoot) => AvatarMenu = avatarToggleMenu;

        public override CreatedAssets CreateAssets(string baseName, IEnumerable<string> children = null)
        {
            var matchGameObjects = new HashSet<string>(children ?? AvatarMenu.GetStoredChildren());
            var parameterName = string.IsNullOrEmpty(AvatarMenu.ParameterName) ? baseName : AvatarMenu.ParameterName;
            // clip
            var active = new AnimationClip();
            active.name = $"{baseName}_active";
            var activate = new AnimationClip();
            activate.name = $"{baseName}_activate";
            var inactive = new AnimationClip();
            inactive.name = $"{baseName}_inactive";
            var inactivate = new AnimationClip();
            inactivate.name = $"{baseName}_inactivate";
            var clipSet = new ToggleAnimationClipSet(
                active: active,
                inactive: inactive,
                activate: activate,
                inactivate: inactivate,
                transitionSeconds: AvatarMenu.TransitionSeconds
                );
            var transitionSeconds = AvatarMenu.TransitionSeconds;
            foreach (var child in AvatarMenu.ToggleObjects.Keys)
            {
                if (!matchGameObjects.Contains(child)) continue;
                var activeValue = AvatarMenu.ToggleObjects[child] == ToggleType.ON;
                var use = AvatarMenu.ToggleObjectUsings.TryGetValue(child, out var usingValue) ? usingValue : ToggleUsing.Both;
                var curvePath = child;
                if (use != ToggleUsing.OFF) active.SetCurve(curvePath, typeof(GameObject), "m_IsActive", new AnimationCurve(new Keyframe(0 / 60.0f, activeValue ? 1 : 0)));
                if (use != ToggleUsing.ON) inactive.SetCurve(curvePath, typeof(GameObject), "m_IsActive", new AnimationCurve(new Keyframe(0 / 60.0f, activeValue ? 0 : 1)));
                if (transitionSeconds > 0)
                {
                    activate.SetCurve(curvePath, typeof(GameObject), "m_IsActive", new AnimationCurve(new Keyframe(0 / 60.0f, 1), new Keyframe(transitionSeconds, activeValue ? 1 : 0)));
                    inactivate.SetCurve(curvePath, typeof(GameObject), "m_IsActive", new AnimationCurve(new Keyframe(0 / 60.0f, 1), new Keyframe(transitionSeconds, activeValue ? 0 : 1)));
                }
            }
            foreach (var (child, index) in AvatarMenu.ToggleMaterials.Keys)
            {
                if (!matchGameObjects.Contains(child)) continue;
                var value = AvatarMenu.ToggleMaterials[(child, index)];
                var curvePath = child;
                var curveName = $"m_Materials.Array.data[{index}]";
                var binding = EditorCurveBinding.PPtrCurve(curvePath, GetRendererTypeByPath(child), curveName);
                if (value.UseActive) AnimationUtility.SetObjectReferenceCurve(active, binding, value.ActiveCurve());
                if (value.UseInactive) AnimationUtility.SetObjectReferenceCurve(inactive, binding, value.InactiveCurve());
                if (transitionSeconds > 0)
                {
                    AnimationUtility.SetObjectReferenceCurve(activate, binding, value.ActivateCurve(transitionSeconds));
                    AnimationUtility.SetObjectReferenceCurve(inactivate, binding, value.InactivateCurve(transitionSeconds));
                }
            }
            foreach (var (child, name) in AvatarMenu.ToggleBlendShapes.Keys)
            {
                if (!matchGameObjects.Contains(child)) continue;
                clipSet.SetupAnimationToggleCurve(AvatarMenu.ToggleBlendShapes[(child, name)], path: child, type: typeof(SkinnedMeshRenderer), propertyName: $"blendShape.{name}");
            }
            foreach (var (child, name) in AvatarMenu.ToggleShaderParameters.Keys)
            {
                if (!matchGameObjects.Contains(child)) continue;
                clipSet.SetupAnimationToggleCurve(AvatarMenu.ToggleShaderParameters[(child, name)], path: child, type: GetRendererTypeByPath(child), propertyName: $"material.{name}");
            }
            foreach (var (child, name) in AvatarMenu.ToggleShaderVectorParameters.Keys)
            {
                if (!matchGameObjects.Contains(child)) continue;
                clipSet.SetupComplexAnimationToggleCurve(AvatarMenu.ToggleShaderVectorParameters[(child, name)], path: child, type: GetRendererTypeByPath(child), prefix: $"material.{name}");
            }
            foreach (var (child, member) in AvatarMenu.ToggleValues.Keys)
            {
                if (!matchGameObjects.Contains(child)) continue;
                var value = AvatarMenu.ToggleValues[(child, member)];
                if (member.MemberTypeIsPrimitive)
                {
                    clipSet.SetupAnimationToggleCurve(value, member.MemberType, path: child, type: member.Type, propertyName: member.AnimationMemberName);
                }
                else
                {
                    clipSet.SetupComplexAnimationToggleCurve(value, member.MemberType, path: child, type: member.Type, prefix: member.AnimationMemberName);
                }
            }
            foreach (var child in AvatarMenu.Positions.Keys)
            {
                if (!matchGameObjects.Contains(child)) continue;
                clipSet.SetupComplexAnimationToggleCurve(AvatarMenu.Positions[child], path: child, type: typeof(Transform), prefix: "localPosition");
            }
            foreach (var child in AvatarMenu.Rotations.Keys)
            {
                if (!matchGameObjects.Contains(child)) continue;
                clipSet.SetupComplexAnimationToggleCurve(AvatarMenu.Rotations[child], path: child, type: typeof(Transform), prefix: "localEulerAnglesRaw");
            }
            foreach (var child in AvatarMenu.Scales.Keys)
            {
                if (!matchGameObjects.Contains(child)) continue;
                clipSet.SetupComplexAnimationToggleCurve(AvatarMenu.Scales[child], path: child, type: typeof(Transform), prefix: "localScale");
            }
            // controller
            var controller = new AnimatorController();
            controller.AddParameter(new AnimatorControllerParameter { name = parameterName, type = AnimatorControllerParameterType.Bool, defaultBool = false });
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
                    parameter = parameterName,
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
                    parameter = parameterName,
                    threshold = 1,
                },
            };
            if (transitionSeconds > 0)
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
                        parameter = parameterName,
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
                        parameter = parameterName,
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
                        parameter = parameterName,
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
                        parameter = parameterName,
                        threshold = 1,
                    },
                };
            }
            var physBoneAutoResetEffectiveObjects = AvatarMenu.GetPhysBoneAutoResetEffectiveObjects(matchGameObjects, AvatarMenu.ToggleValues.Keys).ToArray();
            AnimationClip emptyClip = null;
            AnimationClip waitClip = null;
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

                emptyClip = util.Util.GenerateEmptyAnimationClip(baseName);
                if (transitionSeconds > 0) waitClip = util.Util.GenerateEmptyAnimationClip(baseName + "_wait", transitionSeconds);
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

                pbLayer.stateMachine.entryPosition = new Vector3(-600, 0);
                pbLayer.stateMachine.anyStatePosition = new Vector3(900, 0);
                var pbIdleState = pbLayer.stateMachine.AddState($"{baseName}_idle", new Vector3(300, 0));
                pbIdleState.motion = emptyClip;
                pbIdleState.writeDefaultValues = false;
                pbLayer.stateMachine.defaultState = pbIdleState;
                var pbInactiveState = pbLayer.stateMachine.AddState($"{baseName}_inactive", new Vector3(300, 100));
                pbInactiveState.motion = emptyClip;
                pbInactiveState.writeDefaultValues = false;
                var pbActiveState = pbLayer.stateMachine.AddState($"{baseName}_active", new Vector3(300, -100));
                pbActiveState.motion = emptyClip;
                pbActiveState.writeDefaultValues = false;
                AnimatorState pbInactivateWaitState = null;
                if (transitionSeconds > 0)
                {
                    pbInactivateWaitState = pbLayer.stateMachine.AddState($"{baseName}_inactivate_wait", new Vector3(600, 300));
                    pbInactivateWaitState.motion = waitClip;
                    pbInactivateWaitState.writeDefaultValues = false;
                }
                var pbInactivateDisableState = pbLayer.stateMachine.AddState($"{baseName}_inactivate_disable", new Vector3(600, 100));
                pbInactivateDisableState.motion = pbDisableClip;
                pbInactivateDisableState.writeDefaultValues = false;
                var pbInactivateEnableState = pbLayer.stateMachine.AddState($"{baseName}_inactivate_enable", new Vector3(600, -100));
                pbInactivateEnableState.motion = pbEnableClip;
                pbInactivateEnableState.writeDefaultValues = false;
                AnimatorState pbActivateWaitState = null;
                if (transitionSeconds > 0)
                {
                    pbActivateWaitState = pbLayer.stateMachine.AddState($"{baseName}_activate_wait", new Vector3(0, -300));
                    pbActivateWaitState.motion = waitClip;
                    pbActivateWaitState.writeDefaultValues = false;
                }
                var pbActivateDisableState = pbLayer.stateMachine.AddState($"{baseName}_activate_disable", new Vector3(0, -100));
                pbActivateDisableState.motion = pbDisableClip;
                pbActivateDisableState.writeDefaultValues = false;
                var pbActivateEnableState = pbLayer.stateMachine.AddState($"{baseName}_activate_enable", new Vector3(0, 100));
                pbActivateEnableState.motion = pbEnableClip;
                pbActivateEnableState.writeDefaultValues = false;

                var pbIdleToActive = pbIdleState.AddTransition(pbActiveState);
                pbIdleToActive.exitTime = 0;
                pbIdleToActive.duration = 0;
                pbIdleToActive.hasExitTime = false;
                pbIdleToActive.conditions = new AnimatorCondition[]
                {
                    new AnimatorCondition
                    {
                        mode = AnimatorConditionMode.If,
                        parameter = parameterName,
                        threshold = 1,
                    },
                };
                var pbIdleToInactive = pbIdleState.AddTransition(pbInactiveState);
                pbIdleToInactive.exitTime = 0;
                pbIdleToInactive.duration = 0;
                pbIdleToInactive.hasExitTime = false;
                pbIdleToInactive.conditions = new AnimatorCondition[]
                {
                    new AnimatorCondition
                    {
                        mode = AnimatorConditionMode.IfNot,
                        parameter = parameterName,
                        threshold = 1,
                    },
                };
                var pbInactivate = pbInactiveState.AddTransition(transitionSeconds > 0 ? pbInactivateWaitState : pbInactivateDisableState);
                pbInactivate.exitTime = 0;
                pbInactivate.duration = 0;
                pbInactivate.hasExitTime = false;
                pbInactivate.conditions = new AnimatorCondition[]
                {
                    new AnimatorCondition
                    {
                        mode = AnimatorConditionMode.If,
                        parameter = parameterName,
                        threshold = 1,
                    },
                };
                if (transitionSeconds > 0)
                {
                    var pbInactivateToDisable = pbInactivateWaitState.AddTransition(pbInactivateDisableState);
                    pbInactivateToDisable.exitTime = 1;
                    pbInactivateToDisable.duration = 0;
                    pbInactivateToDisable.hasExitTime = true;
                }
                var pbInactivateToEnable = pbInactivateDisableState.AddTransition(pbInactivateEnableState);
                pbInactivateToEnable.exitTime = 1;
                pbInactivateToEnable.duration = 0;
                pbInactivateToEnable.hasExitTime = true;
                var pbToActive = pbInactivateEnableState.AddTransition(pbActiveState);
                pbToActive.exitTime = 1;
                pbToActive.duration = 0;
                pbToActive.hasExitTime = true;
                var pbActivate = pbActiveState.AddTransition(transitionSeconds > 0 ? pbActivateWaitState : pbActivateDisableState);
                pbActivate.exitTime = 0;
                pbActivate.duration = 0;
                pbActivate.hasExitTime = false;
                pbActivate.conditions = new AnimatorCondition[]
                {
                    new AnimatorCondition
                    {
                        mode = AnimatorConditionMode.IfNot,
                        parameter = parameterName,
                        threshold = 1,
                    },
                };
                if (transitionSeconds > 0)
                {
                    var pbActivateToDisable = pbActivateWaitState.AddTransition(pbActivateDisableState);
                    pbActivateToDisable.exitTime = 1;
                    pbActivateToDisable.duration = 0;
                    pbActivateToDisable.hasExitTime = true;
                }
                var pbActivateToEnable = pbActivateDisableState.AddTransition(pbActivateEnableState);
                pbActivateToEnable.exitTime = 1;
                pbActivateToEnable.duration = 0;
                pbActivateToEnable.hasExitTime = true;
                var pbToInactive = pbActivateEnableState.AddTransition(pbInactiveState);
                pbToInactive.exitTime = 1;
                pbToInactive.duration = 0;
                pbToInactive.hasExitTime = true;
            }
            foreach (var child in physBoneAutoResetEffectiveObjects)
            {
                var curvePath = child;
                var curve = new AnimationCurve(new Keyframe(transitionSeconds - 2 / 60f, 0), new Keyframe(transitionSeconds - 1 / 60f, 1), new Keyframe(transitionSeconds, 1));
                if (transitionSeconds >= 3 / 60f) curve.AddKey(transitionSeconds - 3 / 60f, 1);
                activate.SetCurve(curvePath, typeof(VRCPhysBone), "m_Enabled", curve);
                inactivate.SetCurve(curvePath, typeof(VRCPhysBone), "m_Enabled", curve);
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
                            name = parameterName,
                        },
                        subParameters = new VRCExpressionsMenu.Control.Parameter[] { },
                        value = 1,
                        labels = new VRCExpressionsMenu.Control.Label[] { },
                        icon = AvatarMenu.ToggleIcon,
                    },
                },
            };
            menu.name = baseName;
            var clips = new List<AnimationClip> { active, inactive };
            if (transitionSeconds > 0) clips.AddRange(new AnimationClip[] { activate, inactivate });
            if (physBoneAutoResetEffectiveObjects.Length > 0) clips.AddRange(new AnimationClip[] { emptyClip, pbDisableClip, pbEnableClip });
            if (physBoneAutoResetEffectiveObjects.Length > 0 && transitionSeconds > 0) clips.Add(waitClip);
            return new CreatedAssets(baseName, controller, clips.ToArray(), menu, null, new ParameterConfig[]
            {
                new ParameterConfig
                {
                    nameOrPrefix = parameterName,
                    defaultValue = AvatarMenu.ToggleDefaultValue ? 1 : 0,
                    syncType = ParameterSyncType.Bool,
                    saved = AvatarMenu.Saved,
#if !NET_NARAZAKA_VRCHAT_AvatarMenuCreator_HAS_NO_MENU_MA
                    localOnly = !AvatarMenu.Synced,
#endif
                    internalParameter = AvatarMenu.InternalParameter,
                },
            });
        }
    }
}