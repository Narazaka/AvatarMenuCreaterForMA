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
        public RadialBlendShapeDictionary RadialBlendShapes = new RadialBlendShapeDictionary();
        [SerializeField]
        public RadialBlendShapeDictionary RadialShaderParameters = new RadialBlendShapeDictionary();
        [SerializeField]
        public float RadialDefaultValue;
        [SerializeField]
        public bool RadialInactiveRange;
        [SerializeField]
        public float RadialInactiveRangeMin = float.NaN;
        [SerializeField]
        public float RadialInactiveRangeMax = float.NaN;

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
#endif
    }
}
