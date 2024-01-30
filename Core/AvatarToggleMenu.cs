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
    public class AvatarToggleMenu : AvatarMenuBase
    {
        [SerializeField]
        public ToggleTypeDictionary ToggleObjects = new ToggleTypeDictionary();
        [SerializeField]
        public ToggleBlendShapeDictionary ToggleBlendShapes = new ToggleBlendShapeDictionary();
        [SerializeField]
        public ToggleBlendShapeDictionary ToggleShaderParameters = new ToggleBlendShapeDictionary();
        [SerializeField]
        public bool ToggleDefaultValue;

#if UNITY_EDITOR

        public override IEnumerable<string> GetStoredChildren() => ToggleObjects.Keys.Concat(ToggleBlendShapes.Keys.Select(k => k.Item1)).Concat(ToggleShaderParameters.Keys.Select(k => k.Item1)).Distinct();
        public override void FilterStoredTargets(IEnumerable<string> children)
        {
            WillChange();
            var filter = new HashSet<string>(children);
            foreach (var key in ToggleObjects.Keys.Where(k => !filter.Contains(k)).ToList())
            {
                ToggleObjects.Remove(key);
            }
            foreach (var key in ToggleBlendShapes.Keys.Where(k => !filter.Contains(k.Item1)).ToList())
            {
                ToggleBlendShapes.Remove(key);
            }
            foreach (var key in ToggleShaderParameters.Keys.Where(k => !filter.Contains(k.Item1)).ToList())
            {
                ToggleShaderParameters.Remove(key);
            }
        }
        public override void RemoveStoredChild(string child)
        {
            WillChange();
            ToggleObjects.Remove(child);
            foreach (var key in ToggleBlendShapes.Keys.Where(k => k.Item1 == child).ToList())
            {
                ToggleBlendShapes.Remove(key);
            }
            foreach (var key in ToggleShaderParameters.Keys.Where(k => k.Item1 == child).ToList())
            {
                ToggleShaderParameters.Remove(key);
            }
        }
        protected override bool IsSuitableForTransition() => ToggleBlendShapes.Count > 0 || ToggleShaderParameters.Count > 0;

        protected override void OnHeaderGUI(IList<string> children)
        {
            ShowTransitionSeconds();
            ToggleDefaultValue = Toggle("パラメーター初期値", ToggleDefaultValue);
            ShowSaved();
        }

        protected override void OnMainGUI(IList<string> children)
        {
            foreach (var child in children)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField(child, EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                ShowToggleObjectControl(child);

                var gameObjectRef = GetGameObject(child);
                var names = Util.GetBlendShapeNames(gameObjectRef);
                var parameters = ShaderParametersCache.GetFilteredShaderParameters(gameObjectRef);

                if (names.Count > 0 &&
                    FoldoutHeaderWithAddStringButton(
                        child,
                        "BlendShapes",
                        ToggleBlendShapes.HasChild(child),
                        () => names,
                        () => ToggleBlendShapes.Names(child).ToImmutableHashSet(),
                        (name) => AddToggleBlendShape(ToggleBlendShapes, child, name),
                        (name) => RemoveToggleBlendShape(ToggleBlendShapes, child, name)
                        ))
                {
                    EditorGUI.indentLevel++;
                    ShowToggleBlendShapeControl(child, ToggleBlendShapes, names.ToNames());
                    EditorGUI.indentLevel--;
                }
                if (parameters.Count > 0 &&
                    FoldoutHeaderWithAddStringButton(
                        child,
                        "Shader Parameters",
                        ToggleShaderParameters.HasChild(child),
                        () => parameters.ToStrings().ToList(),
                        () => ToggleShaderParameters.Names(child).ToImmutableHashSet(),
                        (name) => AddToggleBlendShape(ToggleShaderParameters, child, name, 1),
                        (name) => RemoveToggleBlendShape(ToggleShaderParameters, child, name)
                        ))
                {
                    EditorGUI.indentLevel++;
                    ShowToggleBlendShapeControl(child, ToggleShaderParameters, parameters, minValue: null, maxValue: null);
                    EditorGUI.indentLevel--;
                }
                EditorGUI.indentLevel--;
            }
        }

        void ShowToggleObjectControl(string child)
        {
            ToggleType type;
            if (!ToggleObjects.TryGetValue(child, out type))
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
                WillChange();
                if (newType == ToggleType.None)
                {
                    ToggleObjects.Remove(child);
                }
                else
                {
                    ToggleObjects[child] = newType;
                }
            }

        }

        void ShowToggleBlendShapeControl(
            string child,
            ToggleBlendShapeDictionary toggles,
            IEnumerable<Util.INameAndDescription> names,
            float? minValue = 0,
            float? maxValue = 100
            )
        {
            foreach (var name in names)
            {
                var key = (child, name.Name);
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
                            WillChange();
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
                        WillChange();
                        toggles.Remove(key);
                    }
                }
            }
        }

        void BulkSetToggleBlendShape(ToggleBlendShapeDictionary toggles, string toggleName, ToggleBlendShape toggleBlendShape, string changedProp)
        {
            var matches = new List<(string, string)>();
            foreach (var (child, name) in toggles.Keys)
            {
                if (name == toggleName)
                {
                    matches.Add((child, name));
                }
            }
            foreach (var key in matches)
            {
                toggles[key] = toggles[key].SetProp(changedProp, toggleBlendShape.GetProp(changedProp));
            }
        }

        void AddToggleBlendShape(ToggleBlendShapeDictionary toggles, string child, string name, float defaultActiveValue = 100)
        {
            var key = (child, name);
            if (toggles.ContainsKey(key)) return;
            WillChange();
            toggles[key] = new ToggleBlendShape { Inactive = 0, Active = defaultActiveValue, TransitionDurationPercent = 100 };
        }

        void RemoveToggleBlendShape(ToggleBlendShapeDictionary toggles, string child, string name)
        {
            var key = (child, name);
            if (!toggles.ContainsKey(key)) return;
            WillChange();
            toggles.Remove(key);
        }
#endif
    }
}
