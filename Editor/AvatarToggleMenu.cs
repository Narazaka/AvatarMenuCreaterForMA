using nadena.dev.modular_avatar.core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace net.narazaka.avatarmenucreater
{
    public class AvatarToggleMenu : AvatarMenuBase
    {
        Dictionary<GameObject, ToggleType> ToggleObjects = new Dictionary<GameObject, ToggleType>();
        Dictionary<(GameObject, string), ToggleBlendShape> ToggleBlendShapes = new Dictionary<(GameObject, string), ToggleBlendShape>();
        Dictionary<(GameObject, string), ToggleBlendShape> ToggleShaderParameters = new Dictionary<(GameObject, string), ToggleBlendShape>();

        protected override bool IsSuitableForTransition() => ToggleBlendShapes.Count > 0 || ToggleShaderParameters.Count > 0;

        protected override void OnHeaderGUI(GameObject baseObject, GameObject[] gameObjects)
        {
            ShowBulkSet();
            ShowTransitionSeconds();
        }

        protected override void OnMainGUI(GameObject baseObject, GameObject[] gameObjects)
        {
            foreach (var gameObject in gameObjects)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField(Util.ChildPath(baseObject, gameObject));
                EditorGUI.indentLevel++;
                ShowToggleObjectControl(gameObject);

                var names = Util.GetBlendShapeNames(gameObject);
                var parameters = ShaderParametersCache.GetFilteredShaderParameters(gameObject);
                if (names.Count > 0 && FoldoutBlendShapeHeader(gameObject, "BlendShapes"))
                {
                    EditorGUI.indentLevel++;
                    ShowToggleBlendShapeControl(gameObject, ToggleBlendShapes, names.ToNames());
                    EditorGUI.indentLevel--;
                }
                if (parameters.Count > 0 && FoldoutShaderParameterHeader(gameObject, "Shader Parameters"))
                {
                    EditorGUI.indentLevel++;
                    ShowToggleBlendShapeControl(gameObject, ToggleShaderParameters, parameters, 1, minValue: null, maxValue: null);
                    EditorGUI.indentLevel--;
                }
                EditorGUI.indentLevel--;
            }
        }

        void ShowToggleObjectControl(GameObject gameObject)
        {
            ToggleType type;
            if (!ToggleObjects.TryGetValue(gameObject, out type))
            {
                type = ToggleType.None;
            }
            var newType = type;
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUIUtility.labelWidth = 75;
                if (EditorGUILayout.Toggle("制御しない", type == ToggleType.None) && type != ToggleType.None) newType = ToggleType.None;
                if (EditorGUILayout.Toggle("ON=表示", type == ToggleType.ON) && type != ToggleType.ON) newType = ToggleType.ON;
                EditorGUIUtility.labelWidth = 80;
                if (EditorGUILayout.Toggle("ON=非表示", type == ToggleType.OFF) && type != ToggleType.OFF) newType = ToggleType.OFF;
                EditorGUIUtility.labelWidth = 0;
            }
            // Debug.Log($"{type} {newType}");
            if (type != newType)
            {
                if (newType == ToggleType.None)
                {
                    ToggleObjects.Remove(gameObject);
                }
                else
                {
                    ToggleObjects[gameObject] = newType;
                }
            }

        }

        void ShowToggleBlendShapeControl(
            GameObject gameObject,
            Dictionary<(GameObject, string), ToggleBlendShape> toggles,
            IEnumerable<Util.INameAndDescription> names,
            float defaultActiveValue = 100,
            float? minValue = 0,
            float? maxValue = 100
            )
        {
            foreach (var name in names)
            {
                var key = (gameObject, name.Name);
                ToggleBlendShape value;
                if (toggles.TryGetValue(key, out value))
                {
                    if (EditorGUILayout.ToggleLeft(name.Description, true))
                    {
                        var newValue = new ToggleBlendShape { TransitionDurationPercent = 100 };
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            EditorGUI.indentLevel++;
                            EditorGUIUtility.labelWidth = 70;
                            newValue.Inactive = EditorGUILayout.FloatField("OFF", value.Inactive, GUILayout.Width(100));
                            newValue.Active = EditorGUILayout.FloatField("ON", value.Active, GUILayout.Width(100));
                            EditorGUIUtility.labelWidth = 0;
                            EditorGUI.indentLevel--;
                        }
                        if (TransitionSeconds > 0)
                        {
                            using (new EditorGUILayout.HorizontalScope())
                            {
                                EditorGUI.indentLevel++;
                                EditorGUIUtility.labelWidth = 110;
                                newValue.TransitionOffsetPercent = EditorGUILayout.FloatField("変化待機%", value.TransitionOffsetPercent, GUILayout.Width(140));
                                newValue.TransitionDurationPercent = EditorGUILayout.FloatField("変化時間%", value.TransitionDurationPercent, GUILayout.Width(140));
                                EditorGUIUtility.labelWidth = 0;
                                EditorGUI.indentLevel--;
                            }
                        }
                        if (!value.Equals(newValue))
                        {
                            if (minValue != null)
                            {
                                if (newValue.Inactive < (float)minValue) newValue.Inactive = (float)minValue;
                                if (newValue.Active < (float)minValue) newValue.Active = (float)minValue;
                            }
                            if (maxValue != null)
                            {
                                if (newValue.Inactive > (float)maxValue) newValue.Inactive = (float)maxValue;
                                if (newValue.Active > (float)maxValue) newValue.Active = (float)maxValue;
                            }
                            if (newValue.TransitionOffsetPercent < 0) newValue.TransitionOffsetPercent = 0;
                            if (newValue.TransitionOffsetPercent > 100) newValue.TransitionOffsetPercent = 100;
                            if (newValue.TransitionDurationPercent < 0) newValue.TransitionDurationPercent = 0;
                            if (newValue.TransitionDurationPercent > 100) newValue.TransitionDurationPercent = 100;
                            if (newValue.TransitionOffsetPercent + newValue.TransitionDurationPercent > 100)
                            {
                                newValue.TransitionDurationPercent = 100 - newValue.TransitionOffsetPercent;
                            }

                            toggles[key] = newValue;
                            if (BulkSet)
                            {
                                BulkSetToggleBlendShape(toggles, name.Name, newValue, value.ChangedProp(newValue));
                            }
                        }
                    }
                    else
                    {
                        toggles.Remove(key);
                    }
                }
                else
                {
                    if (EditorGUILayout.ToggleLeft(name.Description, false))
                    {
                        toggles[key] = new ToggleBlendShape { Inactive = 0, Active = defaultActiveValue, TransitionDurationPercent = 100 };
                    }
                }
            }
        }

        void BulkSetToggleBlendShape(Dictionary<(GameObject, string), ToggleBlendShape> toggles, string toggleName, ToggleBlendShape toggleBlendShape, string changedProp)
        {
            var matches = new List<(GameObject, string)>();
            foreach (var (gameObject, name) in toggles.Keys)
            {
                if (name == toggleName)
                {
                    matches.Add((gameObject, name));
                }
            }
            foreach (var key in matches)
            {
                toggles[key] = toggles[key].SetProp(changedProp, toggleBlendShape.GetProp(changedProp));
            }
        }


        public override void CreateAssets(IncludeAssetType includeAssetType, GameObject baseObject, string baseName, string basePath, GameObject[] gameObjects)
        {
            var matchGameObjects = new HashSet<GameObject>(gameObjects);
            // clip
            var active = new AnimationClip();
            active.name = $"{baseName}_active";
            var activate = new AnimationClip();
            activate.name = $"{baseName}_activate";
            var inactive = new AnimationClip();
            inactive.name = $"{baseName}_inactive";
            var inactivate = new AnimationClip();
            inactivate.name = $"{baseName}_inactivate";
            foreach (var gameObject in ToggleObjects.Keys)
            {
                if (!matchGameObjects.Contains(gameObject)) continue;
                var activeValue = ToggleObjects[gameObject] == ToggleType.ON;
                var curvePath = Util.ChildPath(baseObject, gameObject);
                active.SetCurve(curvePath, typeof(GameObject), "m_IsActive", new AnimationCurve(new Keyframe(0 / 60.0f, activeValue ? 1 : 0)));
                inactive.SetCurve(curvePath, typeof(GameObject), "m_IsActive", new AnimationCurve(new Keyframe(0 / 60.0f, activeValue ? 0 : 1)));
                if (TransitionSeconds > 0)
                {
                    activate.SetCurve(curvePath, typeof(GameObject), "m_IsActive", new AnimationCurve(new Keyframe(0 / 60.0f, 1), new Keyframe(TransitionSeconds, activeValue ? 1 : 0)));
                    inactivate.SetCurve(curvePath, typeof(GameObject), "m_IsActive", new AnimationCurve(new Keyframe(0 / 60.0f, 1), new Keyframe(TransitionSeconds, activeValue ? 0 : 1)));
                }
            }
            foreach (var (gameObject, name) in ToggleBlendShapes.Keys)
            {
                if (!matchGameObjects.Contains(gameObject)) continue;
                var value = ToggleBlendShapes[(gameObject, name)];
                var curvePath = Util.ChildPath(baseObject, gameObject);
                var curveName = $"blendShape.{name}";
                active.SetCurve(curvePath, typeof(SkinnedMeshRenderer), curveName, new AnimationCurve(new Keyframe(0 / 60.0f, value.Active)));
                inactive.SetCurve(curvePath, typeof(SkinnedMeshRenderer), curveName, new AnimationCurve(new Keyframe(0 / 60.0f, value.Inactive)));
                if (TransitionSeconds > 0)
                {
                    activate.SetCurve(curvePath, typeof(SkinnedMeshRenderer), curveName, new AnimationCurve(new Keyframe(TransitionSeconds * value.ActivateStartRate, value.Inactive), new Keyframe(TransitionSeconds * value.ActivateEndRate, value.Active)));
                    inactivate.SetCurve(curvePath, typeof(SkinnedMeshRenderer), curveName, new AnimationCurve(new Keyframe(TransitionSeconds * value.InactivateStartRate, value.Active), new Keyframe(TransitionSeconds * value.InactivateEndRate, value.Inactive)));
                }
            }
            foreach (var (gameObject, name) in ToggleShaderParameters.Keys)
            {
                if (!matchGameObjects.Contains(gameObject)) continue;
                var value = ToggleShaderParameters[(gameObject, name)];
                var curvePath = Util.ChildPath(baseObject, gameObject);
                var curveName = $"material.{name}";
                active.SetCurve(curvePath, typeof(Renderer), curveName, new AnimationCurve(new Keyframe(0 / 60.0f, value.Active)));
                inactive.SetCurve(curvePath, typeof(Renderer), curveName, new AnimationCurve(new Keyframe(0 / 60.0f, value.Inactive)));
                if (TransitionSeconds > 0)
                {
                    activate.SetCurve(curvePath, typeof(Renderer), curveName, new AnimationCurve(new Keyframe(TransitionSeconds * value.ActivateStartRate, value.Inactive), new Keyframe(TransitionSeconds * value.ActivateEndRate, value.Active)));
                    inactivate.SetCurve(curvePath, typeof(Renderer), curveName, new AnimationCurve(new Keyframe(TransitionSeconds * value.InactivateStartRate, value.Active), new Keyframe(TransitionSeconds * value.InactivateEndRate, value.Inactive)));
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
            if (TransitionSeconds > 0)
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
            // prefab
            var prefabPath = $"{basePath}.prefab";
            var prefab = new GameObject(baseName);
            PrefabUtility.SaveAsPrefabAsset(prefab, prefabPath);
            UnityEngine.Object.DestroyImmediate(prefab);
            SaveAssets(includeAssetType, baseName, basePath, controller, TransitionSeconds > 0 ? new AnimationClip[] { active, inactive, activate, inactivate } : new AnimationClip[] { active, inactive }, menu);
            prefab = PrefabUtility.LoadPrefabContents(prefabPath);
            var menuInstaller = prefab.GetOrAddComponent<ModularAvatarMenuInstaller>();
            menuInstaller.menuToAppend = menu;
            var parameters = prefab.GetOrAddComponent<ModularAvatarParameters>();
            parameters.parameters.Add(new ParameterConfig
            {
                nameOrPrefix = baseName,
                defaultValue = 0,
                syncType = ParameterSyncType.Bool,
                saved = true,
            });
            var mergeAnimator = prefab.GetOrAddComponent<ModularAvatarMergeAnimator>();
            mergeAnimator.animator = controller;
            mergeAnimator.layerType = VRCAvatarDescriptor.AnimLayerType.FX;
            mergeAnimator.pathMode = MergeAnimatorPathMode.Absolute;
            mergeAnimator.matchAvatarWriteDefaults = true;

            PrefabUtility.SaveAsPrefabAsset(prefab, prefabPath);
            PrefabUtility.UnloadPrefabContents(prefab);
            AssetDatabase.SaveAssets();
        }
    }
}
