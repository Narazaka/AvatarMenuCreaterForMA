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
        public RadialShaderVectorParameterDictionary RadialShaderVectorParameters = new RadialShaderVectorParameterDictionary();
        [SerializeField]
        public RadialValueDictionary RadialValues = new RadialValueDictionary();
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
        [SerializeField]
        public bool LockRemoteDuringChange;
        [SerializeField, Obsolete("Fixed by VRChat")]
        public bool PreventRemoteFloatBug;
        [SerializeField, Obsolete("Fixed by VRChat")]
        public float PreventRemoteFloatBugDuration = 0.8f;

        public override void Reset()
        {
            base.Reset();
        }

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

        public override IEnumerable<string> GetStoredChildren() => RadialBlendShapes.Keys.Select(key => key.Item1).Concat(RadialShaderParameters.Keys.Select(key => key.Item1)).Concat(RadialShaderVectorParameters.Keys.Select(key => key.Item1)).Concat(RadialValues.Keys.Select(key => key.Item1)).Concat(Positions.Keys).Concat(Rotations.Keys).Concat(Scales.Keys).Distinct();
        public override void ReplaceStoredChild(string oldChild, string newChild)
        {
            if (RadialBlendShapes.ContainsPrimaryKey(oldChild) || RadialShaderParameters.ContainsPrimaryKey(oldChild) || RadialShaderVectorParameters.ContainsPrimaryKey(oldChild) || RadialValues.ContainsPrimaryKey(oldChild) || Positions.ContainsKey(oldChild) || Rotations.ContainsKey(oldChild) || Scales.ContainsKey(oldChild))
            {
                WillChange();
                RadialBlendShapes.ReplacePrimaryKey(oldChild, newChild);
                RadialShaderParameters.ReplacePrimaryKey(oldChild, newChild);
                RadialShaderVectorParameters.ReplacePrimaryKey(oldChild, newChild);
                RadialValues.ReplacePrimaryKey(oldChild, newChild);
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
            foreach (var key in RadialShaderVectorParameters.Keys.Where(k => !filter.Contains(k.Item1)).ToList())
            {
                RadialShaderVectorParameters.Remove(key);
            }
            foreach (var key in RadialValues.Keys.Where(k => !filter.Contains(k.Item1)).ToList())
            {
                RadialValues.Remove(key);
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
            foreach (var key in RadialShaderVectorParameters.Keys.Where(k => k.Item1 == child).ToList())
            {
                RadialShaderVectorParameters.Remove(key);
            }
            foreach (var key in RadialValues.Keys.Where(k => k.Item1 == child).ToList())
            {
                RadialValues.Remove(key);
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
            EditorGUILayout.PropertyField(serializedProperty.FindPropertyRelative(nameof(RadialIcon)), new GUIContent(T.アイコン));
            EditorGUILayout.PropertyField(serializedProperty.FindPropertyRelative(nameof(RadialDefaultValue)), new GUIContent(T.パラメーター初期値));
            ShowDetailMenuMulti(serializedProperty);
            var lockRemoteDuringChange = serializedProperty.FindPropertyRelative(nameof(LockRemoteDuringChange));
            EditorGUILayout.PropertyField(lockRemoteDuringChange, new GUIContent(T.変更中リモートをロック));
            serializedObject.ApplyModifiedProperties();
        }

        protected override void OnHeaderGUI(IList<string> children)
        {
            RadialIcon = TextureField(T.アイコン, RadialIcon);
            var labelFontStyle = EditorStyles.label.fontStyle;
            EditorStyles.label.fontStyle = FontStyle.Bold;
            RadialDefaultValue = FloatField(T.パラメーター初期値, RadialDefaultValue);
            EditorStyles.label.fontStyle = labelFontStyle;
            ShowDetailMenu();
            if (RadialDefaultValue < 0) RadialDefaultValue = 0;
            if (RadialDefaultValue > 1) RadialDefaultValue = 1;

            EditorGUILayout.Space();

            var newLockRemoteDuringChange = EditorGUILayout.Toggle(T.変更中リモートをロック, LockRemoteDuringChange);
            if (newLockRemoteDuringChange != LockRemoteDuringChange)
            {
                WillChange();
                LockRemoteDuringChange = newLockRemoteDuringChange;
            }

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

            EditorGUILayout.Space();

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(T.値を抽出_sl_適用, GUILayout.Width(EditorGUIUtility.labelWidth));
                EditorGUILayout.LabelField(T.始, GUILayout.Width(30));
                if (PickerButton()) { PickAll(0f); }
                if (ApplyButton()) { ApplyAll(0f); }
                EditorGUILayout.LabelField(T.終, GUILayout.Width(25));
                if (PickerButton()) { PickAll(1f); }
                if (ApplyButton()) { ApplyAll(1f); }
                EditorGUILayout.LabelField(T.初期, GUILayout.Width(25));
                if (ApplyButton()) { ApplyAll(RadialDefaultValue); }
            }
        }

        protected override bool HasHeaderBulkGUI => false;

        protected override void OnMainGUI(IList<string> children)
        {
            foreach (var child in children)
            {
                EditorGUILayout.Space();
                var gameObjectRef = GetGameObject(child);
                var existNames = RadialBlendShapes.Names(child).ToList();
                var names = gameObjectRef == null ? existNames : Util.GetBlendShapeNames(gameObjectRef).UnionBlendShapeNames(existNames);
                var existParameters = RadialShaderParameters.Names(child).ToFakeShaderFloatParameters().Concat(RadialShaderVectorParameters.Names(child).ToFakeShaderVectorParameters()).ToList();
                var parameters = gameObjectRef == null ? existParameters : ShaderParametersCache.GetFilteredShaderParameters(gameObjectRef).UnionShaderParameters(existParameters);
                var components = gameObjectRef == null ? RadialValues.Names(child).Select(n => n.Type) : gameObjectRef.GetAllComponents().Select(c => TypeUtil.GetType(c)).FilterByVRCWhitelist();
                var members = components.GetAvailableMembersOnlySuitableForTransition();
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
                    ShowRadialBlendShapeControl(true, children, child, RadialBlendShapes, names.ToNames());
                    EditorGUI.indentLevel--;
                }
                if (parameters.Count > 0 &&
                    FoldoutHeaderWithAddItemButton(
                        child,
                        "Shader Parameters",
                        RadialShaderParameters.HasChild(child) || RadialShaderVectorParameters.HasChild(child),
                        () => parameters.Select(p => new NameAndDescriptionItemContainer(p) as ListTreeViewItemContainer<string>).ToList(),
                        () => RadialShaderParameters.Names(child).Concat(RadialShaderVectorParameters.Names(child)).ToImmutableHashSet(),
                        name =>
                        {
                            if (parameters.Find(p => p.Name == name).IsFloatLike)
                            {
                                AddRadialBlendShape(RadialShaderParameters, children, child, name, 1);
                            }
                            else
                            {
                                AddRadialShaderVectorParameter(children, child, name);
                            }
                        },
                        name =>
                        {
                            if (parameters.Find(p => p.Name == name).IsFloatLike)
                            {
                                RemoveRadialBlendShape(RadialShaderParameters, children, child, name);
                            }
                            else
                            {
                                RemoveRadialShaderVectorParameter(children, child, name);
                            }
                        }
                        ))
                {
                    EditorGUI.indentLevel++;
                    ShowRadialBlendShapeControl(false, children, child, RadialShaderParameters, parameters, minValue: null, maxValue: null);
                    ShowRadialShaderVectorParameterControl(children, child, parameters);
                    EditorGUI.indentLevel--;
                }
                if (members.Count > 0 &&
                    FoldoutHeaderWithAddItemButton(
                        child,
                        "Components",
                        RadialValues.HasChild(child),
                        () => members.Select(m => new NameAndDescriptionItemContainer(m) as ListTreeViewItemContainer<string>).ToList(),
                        () => RadialValues.Names(child).Select(n => n.Name).ToImmutableHashSet(),
                        name => AddRadialValue(children, child, TypeMember.FromName(name)),
                        name => RemoveRadialValue(children, child, TypeMember.FromName(name))
                        ))
                {
                    EditorGUI.indentLevel++;
                    ShowRadialValueControl(children, child, members);
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
            bool isBlendShape,
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
                            BlendShapeLikePickerButton(isBlendShape, child, name.Name, ref newValue.Start);
                            BlendShapeLikeApplyButton(isBlendShape, child, name.Name, newValue.Start);
                            newValue.End = EditorGUILayout.FloatField(T.終, value.End, GUILayout.Width(105));
                            BlendShapeLikePickerButton(isBlendShape, child, name.Name, ref newValue.End);
                            BlendShapeLikeApplyButton(isBlendShape, child, name.Name, newValue.End);
                            EditorGUIUtility.labelWidth = 70;
                            var initialValue = value.Value(RadialDefaultValue);
                            using (new EditorGUI.DisabledGroupScope(true))
                            {
                                EditorGUILayout.FloatField(
                                    T.初期,
                                    initialValue,
                                    GUILayout.Width(105)
                                    );
                            }
                            BlendShapeLikeApplyButton(isBlendShape, child, name.Name, initialValue);
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

        void ShowRadialShaderVectorParameterControl(
            IList<string> children,
            string child,
            IEnumerable<INameAndDescription> names
            )
        {
            foreach (var name in names)
            {
                var key = (child, name.Name);
                RadialVector4 value;
                if (RadialShaderVectorParameters.TryGetValue(key, out value))
                {
                    if (EditorGUILayout.ToggleLeft(name.Description, true))
                    {
                        var newValue = new RadialVector4();
                        {
                            EditorGUI.indentLevel++;
                            var widemode = EditorGUIUtility.wideMode;
                            EditorGUIUtility.wideMode = true;
                            EditorGUIUtility.labelWidth = 75;
                            using (new EditorGUILayout.HorizontalScope())
                            {
                                newValue.Start = EditorGUILayout.Vector4Field(T.始, value.Start);
                                ShaderVectorParameterPickerButton(child, name.Name, ref newValue.Start);
                                ShaderVectorParameterApplyButton(child, name.Name, newValue.Start);
                            }
                            using (new EditorGUILayout.HorizontalScope())
                            {
                                newValue.End = EditorGUILayout.Vector4Field(T.終, value.End);
                                ShaderVectorParameterPickerButton(child, name.Name, ref newValue.End);
                                ShaderVectorParameterApplyButton(child, name.Name, newValue.End);
                            }
                            EditorGUIUtility.labelWidth = 70;
                            var initialValue = value.Value(RadialDefaultValue);

                            using (new EditorGUILayout.HorizontalScope())
                            {
                                using (new EditorGUI.DisabledGroupScope(true))
                                {
                                    EditorGUILayout.Vector4Field(
                                        T.初期,
                                        initialValue
                                        );
                                }
                                ShaderVectorParameterApplyButton(child, name.Name, initialValue);
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

                            RadialShaderVectorParameters[key] = newValue;
                            if (BulkSet)
                            {
                                BulkSetRadialShaderVectorParameter(name.Name, newValue, value.ChangedProp(newValue));
                            }
                        }
                    }
                    else
                    {
                        RemoveRadialShaderVectorParameter(children, child, name.Name);
                    }
                }
            }
        }

        void BulkSetRadialShaderVectorParameter(string radialName, RadialVector4 radialBlendShape, string changedProp)
        {
            var matches = new List<(string, string)>();
            foreach (var (child, name) in RadialShaderVectorParameters.Keys)
            {
                if (name == radialName)
                {
                    matches.Add((child, name));
                }
            }
            foreach (var key in matches)
            {
                RadialShaderVectorParameters[key] = RadialShaderVectorParameters[key].SetProp(changedProp, radialBlendShape.GetProp(changedProp));
            }
        }

        void AddRadialShaderVectorParameter(IList<string> children, string child, string name)
        {
            if (BulkSet)
            {
                foreach (var c in children)
                {
                    AddRadialShaderVectorParameterSingle(c, name);
                }
            }
            else
            {
                AddRadialShaderVectorParameterSingle(child, name);
            }
        }

        void AddRadialShaderVectorParameterSingle(string child, string name)
        {
            var key = (child, name);
            if (RadialShaderVectorParameters.ContainsKey(key)) return;
            WillChange();
            RadialShaderVectorParameters[key] = new RadialVector4();
        }

        void RemoveRadialShaderVectorParameter(IList<string> children, string child, string name)
        {
            if (BulkSet)
            {
                foreach (var c in children)
                {
                    RemoveRadialShaderVectorParameterSingle(c, name);
                }
            }
            else
            {
                RemoveRadialShaderVectorParameterSingle(child, name);
            }
        }

        void RemoveRadialShaderVectorParameterSingle(string child, string name)
        {
            var key = (child, name);
            if (!RadialShaderVectorParameters.ContainsKey(key)) return;
            WillChange();
            RadialShaderVectorParameters.Remove(key);
        }

        void ShowRadialValueControl(
            IList<string> children,
            string child,
            IEnumerable<TypeMember> members
            )
        {
            ShowPhysBoneAutoResetMenu(child, RadialValues.Names(child).ToArray());
            foreach (var member in members)
            {
                var key = (child, member);
                if (RadialValues.TryGetValue(key, out var value))
                {
                    if (EditorGUILayout.ToggleLeft(member.Description, true))
                    {
                        var newValue = new RadialValue();
                        if (member.MemberType == typeof(float))
                        {
                            using (new EditorGUILayout.HorizontalScope())
                            {
                                EditorGUI.indentLevel++;
                                EditorGUIUtility.labelWidth = 75;
                                newValue.Start = EditorGUILayout.FloatField(T.始, (float)value.Start, GUILayout.Width(105));
                                ValuePickerButton(child, member, ref newValue.Start);
                                ValueApplyButton(child, member, newValue.Start);
                                newValue.End = EditorGUILayout.FloatField(T.終, (float)value.End, GUILayout.Width(105));
                                ValuePickerButton(child, member, ref newValue.End);
                                ValueApplyButton(child, member, newValue.End);
                                EditorGUIUtility.labelWidth = 70;
                                var initialValue = value.FloatValue(RadialDefaultValue);
                                using (new EditorGUI.DisabledGroupScope(true))
                                {
                                    EditorGUILayout.FloatField(
                                        T.初期,
                                        initialValue,
                                        GUILayout.Width(105)
                                        );
                                }
                                ValueApplyButton(child, member, initialValue);
                                EditorGUIUtility.labelWidth = 0;
                                EditorGUI.indentLevel--;
                            }
                        }
                        else if (member.MemberType == typeof(Vector3))
                        {
                            EditorGUI.indentLevel++;
                            var widemode = EditorGUIUtility.wideMode;
                            EditorGUIUtility.wideMode = true;
                            EditorGUIUtility.labelWidth = 75;
                            using (new EditorGUILayout.HorizontalScope())
                            {
                                newValue.Start = EditorGUILayout.Vector3Field(T.始, (Vector3)value.Start);
                                ValuePickerButton(child, member, ref newValue.Start);
                                ValueApplyButton(child, member, newValue.Start);
                            }
                            using (new EditorGUILayout.HorizontalScope())
                            {
                                newValue.End = EditorGUILayout.Vector3Field(T.終, (Vector3)value.End);
                                ValuePickerButton(child, member, ref newValue.End);
                                ValueApplyButton(child, member, newValue.End);
                            }
                            EditorGUIUtility.labelWidth = 70;
                            var initialValue = value.Vector3Value(RadialDefaultValue);

                            using (new EditorGUILayout.HorizontalScope())
                            {
                                using (new EditorGUI.DisabledGroupScope(true))
                                {
                                    EditorGUILayout.Vector3Field(
                                        T.初期,
                                        initialValue
                                        );
                                }
                                ValueApplyButton(child, member, initialValue);
                            }
                            EditorGUIUtility.wideMode = widemode;
                            EditorGUIUtility.labelWidth = 0;
                            EditorGUI.indentLevel--;
                        }
                        else if (member.MemberType == typeof(Quaternion))
                        {
                            EditorGUI.indentLevel++;
                            var widemode = EditorGUIUtility.wideMode;
                            EditorGUIUtility.wideMode = true;
                            EditorGUIUtility.labelWidth = 75;
                            using (new EditorGUILayout.HorizontalScope())
                            {
                                newValue.Start = EditorGUILayout.Vector4Field(T.始, ((Quaternion)value.Start).ToVector4()).ToQuaternion();
                                ValuePickerButton(child, member, ref newValue.Start);
                                ValueApplyButton(child, member, newValue.Start);
                            }
                            using (new EditorGUILayout.HorizontalScope())
                            {
                                newValue.End = EditorGUILayout.Vector4Field(T.終, ((Quaternion)value.End).ToVector4()).ToQuaternion();
                                ValuePickerButton(child, member, ref newValue.End);
                                ValueApplyButton(child, member, newValue.End);
                            }
                            EditorGUIUtility.labelWidth = 70;
                            var initialValue = value.QuaternionValue(RadialDefaultValue);

                            using (new EditorGUILayout.HorizontalScope())
                            {
                                using (new EditorGUI.DisabledGroupScope(true))
                                {
                                    EditorGUILayout.Vector4Field(
                                        T.初期,
                                        initialValue.ToVector4()
                                        );
                                }
                                ValueApplyButton(child, member, initialValue);
                            }
                            EditorGUIUtility.wideMode = widemode;
                            EditorGUIUtility.labelWidth = 0;
                            EditorGUI.indentLevel--;
                        }
                        else if (member.MemberType == typeof(Color))
                        {
                            EditorGUI.indentLevel++;
                            var widemode = EditorGUIUtility.wideMode;
                            EditorGUIUtility.wideMode = true;
                            EditorGUIUtility.labelWidth = 75;
                            using (new EditorGUILayout.HorizontalScope())
                            {
                                newValue.Start = EditorGUILayout.ColorField(T.始, (Color)value.Start);
                                ValuePickerButton(child, member, ref newValue.Start);
                                ValueApplyButton(child, member, newValue.Start);
                            }
                            using (new EditorGUILayout.HorizontalScope())
                            {
                                newValue.End = EditorGUILayout.ColorField(T.終, (Color)value.End);
                                ValuePickerButton(child, member, ref newValue.End);
                                ValueApplyButton(child, member, newValue.End);
                            }
                            EditorGUIUtility.labelWidth = 70;
                            var initialValue = value.ColorValue(RadialDefaultValue);

                            using (new EditorGUILayout.HorizontalScope())
                            {
                                using (new EditorGUI.DisabledGroupScope(true))
                                {
                                    EditorGUILayout.ColorField(
                                        T.初期,
                                        initialValue
                                        );
                                }
                                ValueApplyButton(child, member, initialValue);
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

                            RadialValues[key] = newValue;
                            if (BulkSet)
                            {
                                BulkSetRadialValue(member, newValue, value.ChangedProps(newValue));
                            }
                        }
                    }
                    else
                    {
                        RemoveRadialValue(children, child, member);
                    }
                }
            }
        }

        void BulkSetRadialValue(TypeMember member, RadialValue value, IEnumerable<string> changedProps)
        {
            foreach (var key in RadialValues.Keys.ToArray())
            {
                if (key.Item2 == member)
                {
                    foreach (var changedProp in changedProps)
                    {
                        RadialValues[key] = RadialValues[key].SetProp(changedProp, value.GetProp(changedProp));
                    }
                }
            }
        }

        void AddRadialValue(IList<string> children, string child, TypeMember member)
        {
            if (BulkSet)
            {
                foreach (var c in children)
                {
                    AddRadialValueSingle(c, member);
                }
            }
            else
            {
                AddRadialValueSingle(child, member);
            }
        }

        void AddRadialValueSingle(string child, TypeMember member)
        {
            var key = (child, member);
            if (RadialValues.ContainsKey(key)) return;
            WillChange();
            RadialValues[key] = new RadialValue();
        }

        void RemoveRadialValue(IList<string> children, string child, TypeMember member)
        {
            if (BulkSet)
            {
                foreach (var c in children)
                {
                    RemoveRadialValueSingle(c, member);
                }
            }
            else
            {
                RemoveRadialValueSingle(child, member);
            }
        }

        void RemoveRadialValueSingle(string child, TypeMember member)
        {
            var key = (child, member);
            if (!RadialValues.ContainsKey(key)) return;
            WillChange();
            RadialValues.Remove(key);
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
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            newValue.Start = EditorGUILayout.Vector3Field(T.始, value.Start);
                            TransformPickerButton(child, title, ref newValue.Start);
                            TransformApplyButton(child, title, newValue.Start);
                        }
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            newValue.End = EditorGUILayout.Vector3Field(T.終, value.End);
                            TransformPickerButton(child, title, ref newValue.End);
                            TransformApplyButton(child, title, newValue.End);
                        }
                        EditorGUIUtility.labelWidth = 70;
                        var initialValue = value.Value(RadialDefaultValue);

                        using (new EditorGUILayout.HorizontalScope())
                        {
                            using (new EditorGUI.DisabledGroupScope(true))
                            {
                                EditorGUILayout.Vector3Field(
                                    T.初期,
                                    initialValue
                                    );
                            }
                            TransformApplyButton(child, title, initialValue);
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

        void PickAll(float radialValue)
        {
            var isStart = radialValue == 0f;
            // blendShapes
            foreach (var (child, name) in RadialBlendShapes.Keys)
            {
                var key = (child, name);
                if (RadialBlendShapes.TryGetValue(key, out var value))
                {
                    if (isStart)
                    {
                        PickBlendShapeWeight(child, name, ref value.Start);
                    }
                    else
                    {
                        PickBlendShapeWeight(child, name, ref value.End);
                    }
                }
            }
            // shader float parameters
            foreach (var (child, name) in RadialShaderParameters.Keys)
            {
                var key = (child, name);
                if (RadialShaderParameters.TryGetValue(key, out var value))
                {
                    if (isStart)
                    {
                        PickShaderFloatParameter(child, name, ref value.Start);
                    }
                    else
                    {
                        PickShaderFloatParameter(child, name, ref value.End);
                    }
                }
            }
            // shader vector parameters
            foreach (var (child, name) in RadialShaderVectorParameters.Keys)
            {
                var key = (child, name);
                if (RadialShaderVectorParameters.TryGetValue(key, out var value))
                {
                    if (isStart)
                    {
                        PickShaderVectorParameter(child, name, ref value.Start);
                    }
                    else
                    {
                        PickShaderVectorParameter(child, name, ref value.End);
                    }
                }
            }
            // values
            foreach (var (child, member) in RadialValues.Keys)
            {
                var key = (child, member);
                if (RadialValues.TryGetValue(key, out var value))
                {
                    if (isStart)
                    {
                        PickValue(child, member, ref value.Start);
                    }
                    else
                    {
                        PickValue(child, member, ref value.End);
                    }
                }
            }
            // positions
            foreach (var child in Positions.Keys)
            {
                var key = child;
                if (Positions.TryGetValue(key, out var value))
                {
                    if (isStart)
                    {
                        PickTransform(child, "Position", ref value.Start);
                    }
                    else
                    {
                        PickTransform(child, "Position", ref value.End);
                    }
                }
            }
            // rotations
            foreach (var child in Rotations.Keys)
            {
                var key = child;
                if (Rotations.TryGetValue(key, out var value))
                {
                    if (isStart)
                    {
                        PickTransform(child, "Rotation", ref value.Start);
                    }
                    else
                    {
                        PickTransform(child, "Rotation", ref value.End);
                    }
                }
            }
            // scales
            foreach (var child in Scales.Keys)
            {
                var key = child;
                if (Scales.TryGetValue(key, out var value))
                {
                    if (isStart)
                    {
                        PickTransform(child, "Scale", ref value.Start);
                    }
                    else
                    {
                        PickTransform(child, "Scale", ref value.End);
                    }
                }
            }
        }

        void ApplyAll(float radialValue)
        {
            // blendShapes
            foreach (var (child, name) in RadialBlendShapes.Keys)
            {
                var key = (child, name);
                if (RadialBlendShapes.TryGetValue(key, out var value))
                {
                    ApplyBlendShapeWeight(child, name, value.Value(radialValue));
                }
            }
            // shader float parameters
            foreach (var (child, name) in RadialShaderParameters.Keys)
            {
                var key = (child, name);
                if (RadialShaderParameters.TryGetValue(key, out var value))
                {
                    ApplyShaderFloatParameter(child, name, value.Value(radialValue));
                }
            }
            // shader vector parameters
            foreach (var (child, name) in RadialShaderVectorParameters.Keys)
            {
                var key = (child, name);
                if (RadialShaderVectorParameters.TryGetValue(key, out var value))
                {
                    ApplyShaderVectorParameter(child, name, value.Value(radialValue));
                }
            }
            // values
            foreach (var (child, member) in RadialValues.Keys)
            {
                var key = (child, member);
                if (RadialValues.TryGetValue(key, out var value))
                {
                    if (member.MemberType == typeof(float))
                    {
                        ApplyValue(child, member, value.FloatValue(radialValue));
                    }
                    else if (member.MemberType == typeof(Vector3))
                    {
                        ApplyValue(child, member, value.Vector3Value(radialValue));
                    }
                    else if (member.MemberType == typeof(Quaternion))
                    {
                        ApplyValue(child, member, value.QuaternionValue(radialValue));
                    }
                    else if (member.MemberType == typeof(Color))
                    {
                        ApplyValue(child, member, value.ColorValue(radialValue));
                    }
                }
            }
            // positions
            foreach (var child in Positions.Keys)
            {
                var key = child;
                if (Positions.TryGetValue(key, out var value))
                {
                    ApplyTransform(child, "Position", value.Value(radialValue));
                }
            }
            // rotations
            foreach (var child in Rotations.Keys)
            {
                var key = child;
                if (Rotations.TryGetValue(key, out var value))
                {
                    ApplyTransform(child, "Rotation", value.Value(radialValue));
                }
            }
            // scales
            foreach (var child in Scales.Keys)
            {
                var key = child;
                if (Scales.TryGetValue(key, out var value))
                {
                    ApplyTransform(child, "Scale", value.Value(radialValue));
                }
            }
        }
#endif
    }
}
