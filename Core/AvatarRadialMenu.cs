using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using net.narazaka.avatarmenucreator.editor.util;
#endif
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
        public override void FilterStoredTargets(IEnumerable<string> children)
        {
            WillChange();
            var filter = new HashSet<string>(children);
            foreach (var key in RadialBlendShapes.Keys.Where(k => !filter.Contains(k.Item1)).ToList())
            {
                RadialBlendShapes.Remove(key);
            }
            foreach (var key in RadialShaderParameters.Keys.Where(k => !filter.Contains(k.Item1)).ToList())
            {
                RadialShaderParameters.Remove(key);
            }
        }
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
                    EditorGUILayout.LabelField(path, EditorStyles.boldLabel);
                    EditorGUI.indentLevel++;
                    if (names.Count > 0 &&
                        FoldoutHeaderWithAddStringButton(
                            child,
                            "BlendShapes",
                            RadialBlendShapes.HasChild(child),
                            () => names,
                            () => RadialBlendShapes.Names(child).ToImmutableHashSet(),
                            name => AddRadialBlendShape(RadialBlendShapes, child, name),
                            name => RemoveRadialBlendShape(RadialBlendShapes, child, name)
                            ))
                    {
                        EditorGUI.indentLevel++;
                        ShowRadialBlendShapeControl(child, RadialBlendShapes, names.ToNames());
                        EditorGUI.indentLevel--;
                    }
                    if (parameters.Count > 0 &&
                        FoldoutHeaderWithAddStringButton(
                            child,
                            "Shader Parameters",
                            RadialShaderParameters.HasChild(child),
                            () => parameters.ToStrings().ToList(),
                            () => RadialShaderParameters.Names(child).ToImmutableHashSet(),
                            name => AddRadialBlendShape(RadialShaderParameters, child, name, 1),
                            name => RemoveRadialBlendShape(RadialShaderParameters, child, name)
                            ))
                    {
                        EditorGUI.indentLevel++;
                        ShowRadialBlendShapeControl(child, RadialShaderParameters, parameters, minValue: null, maxValue: null);
                        EditorGUI.indentLevel--;
                    }
                    EditorGUI.indentLevel--;
                }
                else
                {
                    EditorGUILayout.LabelField($"{path} (BlendShape/Shader Parameterなし)", EditorStyles.boldLabel);
                }
            }
        }

        void ShowRadialBlendShapeControl(
            string child,
            RadialBlendShapeDictionary radials,
            IEnumerable<Util.INameAndDescription> names,
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

        void AddRadialBlendShape(RadialBlendShapeDictionary radials, string child, string name, float defaultEndValue = 100)
        {
            var key = (child, name);
            if (radials.ContainsKey(key)) return;
            WillChange();
            radials[key] = new RadialBlendShape { Start = 0, End = defaultEndValue };
        }

        void RemoveRadialBlendShape(RadialBlendShapeDictionary radials, string child, string name)
        {
            var key = (child, name);
            if (!radials.ContainsKey(key)) return;
            WillChange();
            radials.Remove(key);
        }
#endif
    }
}
