using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
using net.narazaka.avatarmenucreator.editor.util;
#endif
using VRC.SDK3.Avatars.Components;
using nadena.dev.modular_avatar.core;
using VRC.SDK3.Avatars.ScriptableObjects;
using net.narazaka.avatarmenucreator.collections.instance;

namespace net.narazaka.avatarmenucreator
{
    [Serializable]
    public class AvatarRadialMenu : AvatarMenuBase
    {
        [SerializeField]
        RadialBlendShapeDictionary RadialBlendShapes = new RadialBlendShapeDictionary();
        [SerializeField]
        RadialBlendShapeDictionary RadialShaderParameters = new RadialBlendShapeDictionary();
        [SerializeField]
        float RadialDefaultValue;
        [SerializeField]
        bool RadialInactiveRange;
        [SerializeField]
        float RadialInactiveRangeMin = float.NaN;
        [SerializeField]
        float RadialInactiveRangeMax = float.NaN;

#if UNITY_EDITOR

        public override IEnumerable<string> GetStoredChildren() => RadialBlendShapes.Keys.Select(key => key.Item1).Concat(RadialShaderParameters.Keys.Select(key => key.Item1)).Distinct();
        public override void RemoveStoredChild(string child)
        {
            WillChange();
            foreach (var key in RadialBlendShapes.Keys.Where(k => k.Item1 == child).ToList())
            {
                RadialBlendShapes.Remove(key);
            }
            foreach (var key in RadialShaderParameters.Keys.Where(k => k.Item1 == child).ToList())
            {
                RadialShaderParameters.Remove(key);
            }
        }
        protected override bool IsSuitableForTransition()
        {
            return false;
        }

        protected override void OnHeaderGUI(IList<string> children)
        {
            RadialDefaultValue = FloatField("パラメーター初期値", RadialDefaultValue);
            ShowSaved();
            if (RadialDefaultValue < 0) RadialDefaultValue = 0;
            if (RadialDefaultValue > 1) RadialDefaultValue = 1;
            if (RadialInactiveRange)
            {
                if (!EditorGUILayout.Toggle("無効領域を設定", true))
                {
                    WillChange();
                    RadialInactiveRange = false;
                }
                EditorGUILayout.HelpBox("アニメーションが影響しないパラメーター領域を設定します", MessageType.Info);
                using (new EditorGUI.IndentLevelScope())
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        if (float.IsNaN(RadialInactiveRangeMin))
                        {
                            var active = EditorGUILayout.ToggleLeft("これより大きい場合", false);
                            if (active)
                            {
                                WillChange();
                                RadialInactiveRangeMin = 0.49f;
                            }
                        }
                        else
                        {
                            var active = EditorGUILayout.ToggleLeft("これより大きい場合", true);
                            if (active)
                            {
                                RadialInactiveRangeMin = FloatField(RadialInactiveRangeMin);
                            }
                            else
                            {
                                WillChange();
                                RadialInactiveRangeMin = float.NaN;
                            }
                        }
                    }
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        if (float.IsNaN(RadialInactiveRangeMax))
                        {
                            var active = EditorGUILayout.ToggleLeft("これより小さい場合", false);
                            if (active)
                            {
                                WillChange();
                                RadialInactiveRangeMax = 0.01f;
                            }
                        }
                        else
                        {
                            var active = EditorGUILayout.ToggleLeft("これより小さい場合", true);
                            if (active)
                            {
                                RadialInactiveRangeMax = FloatField(RadialInactiveRangeMax);
                            }
                            else
                            {
                                WillChange();
                                RadialInactiveRangeMax = float.NaN;
                            }
                        }
                    }
                }
            }
            else
            {
                if (EditorGUILayout.Toggle("無効領域を設定", false))
                {
                    WillChange();
                    RadialInactiveRange = true;
                }
            }
        }

        protected override void OnMainGUI(IList<string> children)
        {
            foreach (var child in children)
            {
                EditorGUILayout.Space();
                var gameObjectRef = GetGameObject(child);
                var names = Util.GetBlendShapeNames(gameObjectRef);
                var parameters = ShaderParametersCache.GetFilteredShaderParameters(gameObjectRef);
                var path = child;
                if (names.Count > 0 || parameters.Count > 0)
                {
                    EditorGUILayout.LabelField(path);
                    EditorGUI.indentLevel++;
                    if (names.Count > 0 && FoldoutBlendShapeHeader(child, "BlendShapes"))
                    {
                        EditorGUI.indentLevel++;
                        ShowRadialBlendShapeControl(child, RadialBlendShapes, names.ToNames());
                        EditorGUI.indentLevel--;
                    }
                    if (parameters.Count > 0 && FoldoutShaderParameterHeader(child, "Shader Parameters"))
                    {
                        EditorGUI.indentLevel++;
                        ShowRadialBlendShapeControl(child, RadialShaderParameters, parameters, 1, minValue: null, maxValue: null);
                        EditorGUI.indentLevel--;
                    }
                    EditorGUI.indentLevel--;
                }
                else
                {
                    EditorGUILayout.LabelField($"{path} (BlendShapeなし)");
                }
            }
        }

        void ShowRadialBlendShapeControl(
            string child,
            RadialBlendShapeDictionary radials,
            IEnumerable<Util.INameAndDescription> names,
            float defaultEndValue = 100,
            float? minValue = 0,
            float? maxValue = 100
            )
        {
            foreach (var name in names)
            {
                var key = (child, name.Name);
                RadialBlendShape value;
                if (radials.TryGetValue(key, out value))
                {
                    if (EditorGUILayout.ToggleLeft(name.Description, true))
                    {
                        var newValue = new RadialBlendShape();
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            EditorGUI.indentLevel++;
                            EditorGUIUtility.labelWidth = 60;
                            newValue.Start = EditorGUILayout.FloatField("始", value.Start, GUILayout.Width(90));
                            newValue.End = EditorGUILayout.FloatField("終", value.End, GUILayout.Width(90));
                            EditorGUIUtility.labelWidth = 70;
                            using (new EditorGUI.DisabledGroupScope(true))
                            {
                                EditorGUILayout.FloatField("初期", value.Start * (1 - RadialDefaultValue) + value.End * RadialDefaultValue, GUILayout.Width(100));
                            }
                            EditorGUIUtility.labelWidth = 0;
                            EditorGUI.indentLevel--;
                        }
                        if (!value.Equals(newValue))
                        {
                            WillChange();
                            if (minValue != null)
                            {
                                if (newValue.Start < (float)minValue) newValue.Start = (float)minValue;
                                if (newValue.End < (float)minValue) newValue.End = (float)minValue;
                            }
                            if (maxValue != null)
                            {
                                if (newValue.Start > (float)maxValue) newValue.Start = (float)maxValue;
                                if (newValue.End > (float)maxValue) newValue.End = (float)maxValue;
                            }

                            radials[key] = newValue;
                            if (BulkSet)
                            {
                                BulkSetRadialBlendShape(radials, name.Name, newValue, value.ChangedProp(newValue));
                            }
                        }
                    }
                    else
                    {
                        WillChange();
                        radials.Remove(key);
                    }
                }
                else
                {
                    if (EditorGUILayout.ToggleLeft(name.Description, false))
                    {
                        WillChange();
                        radials[key] = new RadialBlendShape { Start = 0, End = defaultEndValue };
                    }
                }
            }
        }

        void BulkSetRadialBlendShape(RadialBlendShapeDictionary radials, string radialName, RadialBlendShape radialBlendShape, string changedProp)
        {
            var matches = new List<(string, string)>();
            foreach (var (child, name) in radials.Keys)
            {
                if (name == radialName)
                {
                    matches.Add((child, name));
                }
            }
            foreach (var key in matches)
            {
                radials[key] = radials[key].SetProp(changedProp, radialBlendShape.GetProp(changedProp));
            }
        }

        public override void CreateAssets(IncludeAssetType includeAssetType, string baseName, string basePath, string[] children)
        {
            var matchGameObjects = new HashSet<string>(children);
            // clip
            var clip = new AnimationClip();
            clip.name = baseName;
            foreach (var (child, name) in RadialBlendShapes.Keys)
            {
                if (!matchGameObjects.Contains(child)) continue;
                var value = RadialBlendShapes[(child, name)];
                clip.SetCurve(child, typeof(SkinnedMeshRenderer), $"blendShape.{name}", SetAutoTangentMode(new AnimationCurve(new Keyframe(0 / 60.0f, value.Start), new Keyframe(1 / 60.0f, value.End))));
            }
            foreach (var (child, name) in RadialShaderParameters.Keys)
            {
                if (!matchGameObjects.Contains(child)) continue;
                var value = RadialShaderParameters[(child, name)];
                clip.SetCurve(child, typeof(Renderer), $"material.{name}", SetAutoTangentMode(new AnimationCurve(new Keyframe(0 / 60.0f, value.Start), new Keyframe(1 / 60.0f, value.End))));
            }
            // controller
            var controller = new AnimatorController();
            controller.AddParameter(new AnimatorControllerParameter { name = baseName, type = AnimatorControllerParameterType.Float, defaultFloat = RadialDefaultValue });
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
            if (RadialInactiveRange && !(float.IsNaN(RadialInactiveRangeMin) && float.IsNaN(RadialInactiveRangeMax)))
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
                if (!float.IsNaN(RadialInactiveRangeMin))
                {
                    toInactiveConditions.Add(new AnimatorCondition
                    {
                        mode = AnimatorConditionMode.Greater,
                        parameter = baseName,
                        threshold = RadialInactiveRangeMin,
                    });
                }
                if (!float.IsNaN(RadialInactiveRangeMax))
                {
                    toInactiveConditions.Add(new AnimatorCondition
                    {
                        mode = AnimatorConditionMode.Less,
                        parameter = baseName,
                        threshold = RadialInactiveRangeMax,
                    });
                }
                toInactive.conditions = toInactiveConditions.ToArray();

                if (!float.IsNaN(RadialInactiveRangeMin))
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
                            threshold = RadialInactiveRangeMin,
                        },
                    };
                }
                if (!float.IsNaN(RadialInactiveRangeMax))
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
                            threshold = RadialInactiveRangeMax,
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
                        value = RadialDefaultValue,
                        labels = new VRCExpressionsMenu.Control.Label[] { },
                    },
                },
            };
            menu.name = baseName;
            // prefab
            SaveAssets(includeAssetType, baseName, basePath, controller, emptyClip == null ? new AnimationClip[] { clip } : new AnimationClip[] { clip, emptyClip }, menu, null, (prefab) =>
            {
                var menuInstaller = prefab.GetOrAddComponent<ModularAvatarMenuInstaller>();
                menuInstaller.menuToAppend = menu;
                var parameters = prefab.GetOrAddComponent<ModularAvatarParameters>();
                parameters.parameters.Clear();
                parameters.parameters.Add(new ParameterConfig
                {
                    nameOrPrefix = baseName,
                    defaultValue = RadialDefaultValue,
                    syncType = ParameterSyncType.Float,
                    saved = Saved,
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
#endif
    }
}
