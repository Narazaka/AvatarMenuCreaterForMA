using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using net.narazaka.avatarmenucreator.util;
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
        public RadialVector3Dictionary Positions = new RadialVector3Dictionary();
        [SerializeField]
        public RadialVector3Dictionary Rotations = new RadialVector3Dictionary();
        [SerializeField]
        public RadialVector3Dictionary Scales = new RadialVector3Dictionary();
        [SerializeField]
        public float RadialDefaultValue;
        [SerializeField]
        public bool RadialInactiveRange;
        [SerializeField]
        public float RadialInactiveRangeMin = float.NaN;
        [SerializeField]
        public float RadialInactiveRangeMax = float.NaN;
        [SerializeField]
        public Texture2D RadialIcon;

#if UNITY_EDITOR

        static readonly string[] TransformComponentNames = new[] { "Position", "Rotation", "Scale" };
        RadialVector3Dictionary TransformComponent(string transformComponentName)
        {
            switch (transformComponentName)
            {
                case "Position": return Positions;
                case "Rotation": return Rotations;
                case "Scale": return Scales;
                default: throw new ArgumentException();
            }
        }

        public override IEnumerable<string> GetStoredChildren() => RadialBlendShapes.Keys.Select(key => key.Item1).Concat(RadialShaderParameters.Keys.Select(key => key.Item1)).Concat(Positions.Keys).Concat(Rotations.Keys).Concat(Scales.Keys).Distinct();
        public override void ReplaceStoredChild(string oldChild, string newChild)
        {
            if (RadialBlendShapes.ContainsPrimaryKey(oldChild) || RadialShaderParameters.ContainsPrimaryKey(oldChild) || Positions.ContainsKey(oldChild) || Rotations.ContainsKey(oldChild) || Scales.ContainsKey(oldChild))
            {
                WillChange();
                RadialBlendShapes.ReplacePrimaryKey(oldChild, newChild);
                RadialShaderParameters.ReplacePrimaryKey(oldChild, newChild);
                Positions.ReplaceKey(oldChild, newChild);
                Rotations.ReplaceKey(oldChild, newChild);
                Scales.ReplaceKey(oldChild, newChild);
            }
        }
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
            foreach (var key in Positions.Keys.Where(k => !filter.Contains(k)).ToList())
            {
                Positions.Remove(key);
            }
            foreach (var key in Rotations.Keys.Where(k => !filter.Contains(k)).ToList())
            {
                Rotations.Remove(key);
            }
            foreach (var key in Scales.Keys.Where(k => !filter.Contains(k)).ToList())
            {
                Scales.Remove(key);
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
            Positions.Remove(child);
            Rotations.Remove(child);
            Scales.Remove(child);
        }
        protected override bool IsSuitableForTransition()
        {
            return false;
        }

        protected override void OnMultiGUI(SerializedProperty serializedProperty)
        {
            var serializedObject = serializedProperty.serializedObject;
            serializedObject.Update();
            serializedObject.FindProperty(nameof(AvatarRadialMenu));
            EditorGUILayout.PropertyField(serializedProperty.FindPropertyRelative(nameof(RadialIcon)), new GUIContent(T.アイコン));
            EditorGUILayout.PropertyField(serializedProperty.FindPropertyRelative(nameof(RadialDefaultValue)), new GUIContent(T.パラメーター初期値));
            ShowSavedMulti(serializedProperty);
            ShowDetailMenuMulti(serializedProperty);
            serializedObject.ApplyModifiedProperties();
        }

        protected override void OnHeaderGUI(IList<string> children)
        {
            RadialIcon = TextureField(T.アイコン, RadialIcon);
            var labelFontStyle = EditorStyles.label.fontStyle;
            EditorStyles.label.fontStyle = FontStyle.Bold;
            RadialDefaultValue = FloatField(T.パラメーター初期値, RadialDefaultValue);
            EditorStyles.label.fontStyle = labelFontStyle;
            ShowSaved();
            ShowDetailMenu();
            if (RadialDefaultValue < 0) RadialDefaultValue = 0;
            if (RadialDefaultValue > 1) RadialDefaultValue = 1;

            EditorGUILayout.Space();

            if (RadialInactiveRange)
            {
                if (!EditorGUILayout.Toggle(T.無効領域を設定, true))
                {
                    WillChange();
                    RadialInactiveRange = false;
                }
                EditorGUILayout.HelpBox(T.アニメーションが影響しないパラメーター領域を設定します, MessageType.Info);
                using (new EditorGUI.IndentLevelScope())
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        if (float.IsNaN(RadialInactiveRangeMin))
                        {
                            var active = EditorGUILayout.ToggleLeft(T.これより大きい場合, false);
                            if (active)
                            {
                                WillChange();
                                RadialInactiveRangeMin = 0.49f;
                            }
                        }
                        else
                        {
                            var active = EditorGUILayout.ToggleLeft(T.これより大きい場合, true);
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
                            var active = EditorGUILayout.ToggleLeft(T.これより小さい場合, false);
                            if (active)
                            {
                                WillChange();
                                RadialInactiveRangeMax = 0.01f;
                            }
                        }
                        else
                        {
                            var active = EditorGUILayout.ToggleLeft(T.これより小さい場合, true);
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
                if (EditorGUILayout.Toggle(T.無効領域を設定, false))
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
                var names = gameObjectRef == null ? RadialBlendShapes.Names(child).ToList() : Util.GetBlendShapeNames(gameObjectRef);
                var parameters = gameObjectRef == null ? RadialShaderParameters.Names(child).ToFakeShaderParameters().ToList() : ShaderParametersCache.GetFilteredShaderParameters(gameObjectRef);
                var path = child;
                GameObjectHeader(child);
                EditorGUI.indentLevel++;
                if (names.Count > 0 &&
                    FoldoutHeaderWithAddItemButton(
                        child,
                        "BlendShapes",
                        RadialBlendShapes.HasChild(child),
                        () => names,
                        () => RadialBlendShapes.Names(child).ToImmutableHashSet(),
                        name => AddRadialBlendShape(RadialBlendShapes, children, child, name),
                        name => RemoveRadialBlendShape(RadialBlendShapes, children, child, name)
                        ))
                {
                    EditorGUI.indentLevel++;
                    ShowRadialBlendShapeControl(children, child, RadialBlendShapes, names.ToNames());
                    EditorGUI.indentLevel--;
                }
                if (parameters.Count > 0 &&
                    FoldoutHeaderWithAddItemButton(
                        child,
                        "Shader Parameters",
                        RadialShaderParameters.HasChild(child),
                        () => parameters.Select(p => new NameAndDescriptionItemContainer(p) as ListTreeViewItemContainer<string>).ToList(),
                        () => RadialShaderParameters.Names(child).ToImmutableHashSet(),
                        name => AddRadialBlendShape(RadialShaderParameters, children, child, name, 1),
                        name => RemoveRadialBlendShape(RadialShaderParameters, children, child, name)
                        ))
                {
                    EditorGUI.indentLevel++;
                    ShowRadialBlendShapeControl(children, child, RadialShaderParameters, parameters, minValue: null, maxValue: null);
                    EditorGUI.indentLevel--;
                }
                if (FoldoutHeaderWithAddItemButton(
                    child,
                    "Transform",
                    Positions.ContainsKey(child) || Rotations.ContainsKey(child) || Scales.ContainsKey(child),
                    () => TransformComponentNames.Select(s => new NameAndDescriptionItemContainer(new Util.NameWithDescription { Name = s }) as ListTreeViewItemContainer<string>).ToList(),
                    () => TransformComponentNames.Where(s => TransformComponent(s).ContainsKey(child)).ToImmutableHashSet(),
                    name => AddTransformComponent(TransformComponent(name), children, child),
                    name => RemoveTransformComponent(TransformComponent(name), children, child)
                    ))
                {
                    EditorGUI.indentLevel++;
                    if (Positions.ContainsKey(child)) ShowTransformComponentControl(children, child, Positions, "Position");
                    if (Rotations.ContainsKey(child)) ShowTransformComponentControl(children, child, Rotations, "Rotation");
                    if (Scales.ContainsKey(child)) ShowTransformComponentControl(children, child, Scales, "Scale");
                    EditorGUI.indentLevel--;
                }
                EditorGUI.indentLevel--;
            }
        }

        void ShowRadialBlendShapeControl(
            IList<string> children,
            string child,
            RadialBlendShapeDictionary radials,
            IEnumerable<INameAndDescription> names,
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
                            EditorGUIUtility.labelWidth = 75;
                            newValue.Start = EditorGUILayout.FloatField(T.始, value.Start, GUILayout.Width(105));
                            newValue.End = EditorGUILayout.FloatField(T.終, value.End, GUILayout.Width(105));
                            EditorGUIUtility.labelWidth = 70;
                            using (new EditorGUI.DisabledGroupScope(true))
                            {
                                EditorGUILayout.FloatField(
                                    T.初期,
                                    RadialDefaultValue * 100 < value.StartOffsetPercent ? value.Start :
                                    RadialDefaultValue * 100 > value.EndOffsetPercent ? value.End :
                                    (value.Start * (value.EndOffsetPercent - RadialDefaultValue * 100) + value.End * (RadialDefaultValue * 100 - value.StartOffsetPercent)) / (value.EndOffsetPercent - value.StartOffsetPercent),
                                    GUILayout.Width(100)
                                    );
                            }
                            EditorGUIUtility.labelWidth = 0;
                            EditorGUI.indentLevel--;
                        }
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            EditorGUI.indentLevel++;
                            EditorGUIUtility.labelWidth = 120;
                            newValue.StartOffsetPercent = EditorGUILayout.FloatField(T.始offset_per_, value.StartOffsetPercent, GUILayout.Width(150));
                            newValue.EndOffsetPercent = EditorGUILayout.FloatField(T.終offset_per_, value.EndOffsetPercent, GUILayout.Width(150));
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
                            if (newValue.StartOffsetPercent < 0) newValue.StartOffsetPercent = 0;
                            if (newValue.EndOffsetPercent < 0) newValue.EndOffsetPercent = 0;
                            if (newValue.StartOffsetPercent > 100) newValue.StartOffsetPercent = 100;
                            if (newValue.EndOffsetPercent > 100) newValue.EndOffsetPercent = 100;

                            radials[key] = newValue;
                            if (BulkSet)
                            {
                                BulkSetRadialBlendShape(radials, name.Name, newValue, value.ChangedProp(newValue));
                            }
                        }
                    }
                    else
                    {
                        RemoveRadialBlendShape(radials, children, child, name.Name);
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

        void AddRadialBlendShape(RadialBlendShapeDictionary radials, IList<string> children, string child, string name, float defaultEndValue = 100)
        {
            if (BulkSet)
            {
                foreach (var c in children)
                {
                    AddRadialBlendShapeSingle(radials, c, name, defaultEndValue);
                }
            }
            else
            {
                AddRadialBlendShapeSingle(radials, child, name, defaultEndValue);
            }
        }

        void AddRadialBlendShapeSingle(RadialBlendShapeDictionary radials, string child, string name, float defaultEndValue = 100)
        {
            var key = (child, name);
            if (radials.ContainsKey(key)) return;
            WillChange();
            radials[key] = new RadialBlendShape { Start = 0, End = defaultEndValue };
        }

        void RemoveRadialBlendShape(RadialBlendShapeDictionary radials, IList<string> children, string child, string name)
        {
            if (BulkSet)
            {
                foreach (var c in children)
                {
                    RemoveRadialBlendShapeSingle(radials, c, name);
                }
            }
            else
            {
                RemoveRadialBlendShapeSingle(radials, child, name);
            }
        }

        void RemoveRadialBlendShapeSingle(RadialBlendShapeDictionary radials, string child, string name)
        {
            var key = (child, name);
            if (!radials.ContainsKey(key)) return;
            WillChange();
            radials.Remove(key);
        }

        void ShowTransformComponentControl(IList<string> children, string child, RadialVector3Dictionary values, string title)
        {
            if (values.TryGetValue(child, out var value))
            {
                if (EditorGUILayout.ToggleLeft(title, true))
                {
                    var newValue = new RadialVector3();
                    {
                        EditorGUI.indentLevel++;
                        var widemode = EditorGUIUtility.wideMode;
                        EditorGUIUtility.wideMode = true;
                        EditorGUIUtility.labelWidth = 75;
                        newValue.Start = EditorGUILayout.Vector3Field(T.始, value.Start);
                        newValue.End = EditorGUILayout.Vector3Field(T.終, value.End);
                        EditorGUIUtility.labelWidth = 70;
                        using (new EditorGUI.DisabledGroupScope(true))
                        {
                            EditorGUILayout.Vector3Field(
                                T.初期,
                                RadialDefaultValue * 100 < value.StartOffsetPercent ? value.Start :
                                RadialDefaultValue * 100 > value.EndOffsetPercent ? value.End :
                                (value.Start * (value.EndOffsetPercent - RadialDefaultValue * 100) + value.End * (RadialDefaultValue * 100 - value.StartOffsetPercent)) / (value.EndOffsetPercent - value.StartOffsetPercent)
                                );
                        }
                        EditorGUIUtility.wideMode = widemode;
                        EditorGUIUtility.labelWidth = 0;
                        EditorGUI.indentLevel--;
                    }
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUI.indentLevel++;
                        EditorGUIUtility.labelWidth = 120;
                        newValue.StartOffsetPercent = EditorGUILayout.FloatField(T.始offset_per_, value.StartOffsetPercent, GUILayout.Width(150));
                        newValue.EndOffsetPercent = EditorGUILayout.FloatField(T.終offset_per_, value.EndOffsetPercent, GUILayout.Width(150));
                        EditorGUIUtility.labelWidth = 0;
                        EditorGUI.indentLevel--;
                    }
                    if (!value.Equals(newValue))
                    {
                        WillChange();
                        if (newValue.StartOffsetPercent < 0) newValue.StartOffsetPercent = 0;
                        if (newValue.EndOffsetPercent < 0) newValue.EndOffsetPercent = 0;
                        if (newValue.StartOffsetPercent > 100) newValue.StartOffsetPercent = 100;
                        if (newValue.EndOffsetPercent > 100) newValue.EndOffsetPercent = 100;

                        values[child] = newValue;
                        if (BulkSet)
                        {
                            BulkSetTransformComponent(values, newValue, value.ChangedProp(newValue));
                        }
                    }
                }
                else
                {
                    RemoveTransformComponent(values, children, child);
                }
            }
        }

        void BulkSetTransformComponent(RadialVector3Dictionary values, RadialVector3 value, string changedProp)
        {
            foreach (var key in values.Keys.ToArray())
            {
                values[key] = values[key].SetProp(changedProp, value.GetProp(changedProp));
            }
        }

        void AddTransformComponent(RadialVector3Dictionary values, IList<string> children, string child)
        {
            if (BulkSet)
            {
                foreach (var c in children)
                {
                    AddTransformComponentSingle(values, c);
                }
            }
            else
            {
                AddTransformComponentSingle(values, child);
            }
        }

        void AddTransformComponentSingle(RadialVector3Dictionary values, string child)
        {
            if (values.ContainsKey(child)) return;
            WillChange();
            values[child] = new RadialVector3();
        }

        void RemoveTransformComponent(RadialVector3Dictionary values, IList<string> children, string child)
        {
            if (BulkSet)
            {
                foreach (var c in children)
                {
                    RemoveTransformComponentSingle(values, c);
                }
            }
            else
            {
                RemoveTransformComponentSingle(values, child);
            }
        }

        void RemoveTransformComponentSingle(RadialVector3Dictionary values, string child)
        {
            if (!values.ContainsKey(child)) return;
            WillChange();
            values.Remove(child);
        }
#endif
    }
}
