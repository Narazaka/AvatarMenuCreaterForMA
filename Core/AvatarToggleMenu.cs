using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using UnityEngine;
using VRC.Dynamics;

#if UNITY_EDITOR
using UnityEditor;
using net.narazaka.avatarmenucreator.util;
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
        public ToggleUsingDictionary ToggleObjectUsings = new ToggleUsingDictionary();
        [SerializeField]
        public ToggleMaterialDictionary ToggleMaterials = new ToggleMaterialDictionary();
        [SerializeField]
        public ToggleBlendShapeDictionary ToggleBlendShapes = new ToggleBlendShapeDictionary();
        [SerializeField]
        public ToggleBlendShapeDictionary ToggleShaderParameters = new ToggleBlendShapeDictionary();
        [SerializeField]
        public ToggleShaderVectorParameterDictionary ToggleShaderVectorParameters = new ToggleShaderVectorParameterDictionary();
        [SerializeField]
        public ToggleValueDictionary ToggleValues = new ToggleValueDictionary();
        [SerializeField]
        public ToggleVector3Dictionary Positions = new ToggleVector3Dictionary();
        [SerializeField]
        public ToggleVector3Dictionary Rotations = new ToggleVector3Dictionary();
        [SerializeField]
        public ToggleVector3Dictionary Scales = new ToggleVector3Dictionary();
        [SerializeField]
        public bool ToggleDefaultValue;
        [SerializeField]
        public Texture2D ToggleIcon;

#if UNITY_EDITOR

        static readonly string[] TransformComponentNames = new[] { "Position", "Rotation", "Scale" };
        ToggleVector3Dictionary TransformComponent(string transformComponentName)
        {
            switch (transformComponentName)
            {
                case "Position": return Positions;
                case "Rotation": return Rotations;
                case "Scale": return Scales;
                default: throw new ArgumentException();
            }
        }

        [NonSerialized]
        bool _UseAdvanced;
        public bool UseAdvanced
        {
            get
            {
                if (_UseAdvanced) return true;
                if (HasAdvanced)
                {
                    _UseAdvanced = true;
                }
                return _UseAdvanced;
            }
            set
            {
                if (_UseAdvanced != value)
                {
                    if (!value && HasAdvanced)
                    {
                        if (EditorUtility.DisplayDialog(T.高度な設定の削除, T.高度な設定がリセットされます_n_本当に高度な設定を無効にしますか_Q_, "OK", "Cancel"))
                        {
                            _UseAdvanced = value;
                            WillChange();
                            ToggleObjectUsings.Clear();
                            foreach (var key in ToggleMaterials.Keys.ToList())
                            {
                                ToggleMaterials[key] = ToggleMaterials[key].ResetAdvanced();
                            }
                            foreach (var key in ToggleBlendShapes.Keys.ToList())
                            {
                                ToggleBlendShapes[key].ResetAdvanced();
                            }
                            foreach (var key in ToggleShaderParameters.Keys.ToList())
                            {
                                ToggleShaderParameters[key].ResetAdvanced();
                            }
                            foreach (var key in ToggleShaderVectorParameters.Keys.ToList())
                            {
                                ToggleShaderVectorParameters[key].ResetAdvanced();
                            }
                            foreach (var key in ToggleValues.Keys.ToList())
                            {
                                ToggleValues[key].ResetAdvanced();
                            }
                            foreach (var key in Positions.Keys.ToList())
                            {
                                Positions[key].ResetAdvanced();
                            }
                            foreach (var key in Rotations.Keys.ToList())
                            {
                                Rotations[key].ResetAdvanced();
                            }
                            foreach (var key in Scales.Keys.ToList())
                            {
                                Scales[key].ResetAdvanced();
                            }
                        }
                    }
                    else
                    {
                        _UseAdvanced = value;
                    }
                }
            }
        }
        bool HasAdvanced => ToggleObjectUsings.Count > 0 || ToggleMaterials.Any(t => t.Value.HasAdvanced) || ToggleBlendShapes.Any(t => t.Value.HasAdvanced) || ToggleShaderParameters.Any(t => t.Value.HasAdvanced) || ToggleShaderVectorParameters.Any(t => t.Value.HasAdvanced) || ToggleValues.Any(t => t.Value.HasAdvanced) || Positions.Any(t => t.Value.HasAdvanced) || Rotations.Any(t => t.Value.HasAdvanced) || Scales.Any(t => t.Value.HasAdvanced);

        public override IEnumerable<string> GetStoredChildren() => ToggleObjects.Keys.Concat(ToggleMaterials.Keys.Select(k => k.Item1)).Concat(ToggleBlendShapes.Keys.Select(k => k.Item1)).Concat(ToggleShaderParameters.Keys.Select(k => k.Item1)).Concat(ToggleShaderVectorParameters.Keys.Select(k => k.Item1)).Concat(ToggleValues.Keys.Select(k => k.Item1)).Concat(Positions.Keys).Concat(Rotations.Keys).Concat(Scales.Keys).Distinct();
        public override void ReplaceStoredChild(string oldChild, string newChild)
        {
            if (ToggleObjects.ContainsKey(oldChild) || ToggleMaterials.ContainsPrimaryKey(oldChild) || ToggleBlendShapes.ContainsPrimaryKey(oldChild) || ToggleShaderParameters.ContainsPrimaryKey(oldChild) || ToggleShaderVectorParameters.ContainsPrimaryKey(oldChild) || ToggleValues.ContainsPrimaryKey(oldChild) || Positions.ContainsKey(oldChild) || Rotations.ContainsKey(oldChild) || Scales.ContainsKey(oldChild))
            {
                WillChange();
                ToggleObjects.ReplaceKey(oldChild, newChild);
                ToggleObjectUsings.ReplaceKey(oldChild, newChild);
                ToggleMaterials.ReplacePrimaryKey(oldChild, newChild);
                ToggleBlendShapes.ReplacePrimaryKey(oldChild, newChild);
                ToggleShaderParameters.ReplacePrimaryKey(oldChild, newChild);
                ToggleShaderVectorParameters.ReplacePrimaryKey(oldChild, newChild);
                ToggleValues.ReplacePrimaryKey(oldChild, newChild);
                Positions.ReplaceKey(oldChild, newChild);
                Rotations.ReplaceKey(oldChild, newChild);
                Scales.ReplaceKey(oldChild, newChild);
            }
        }
        public override void FilterStoredTargets(IEnumerable<string> children)
        {
            WillChange();
            var filter = new HashSet<string>(children);
            foreach (var key in ToggleObjects.Keys.Where(k => !filter.Contains(k)).ToList())
            {
                ToggleObjects.Remove(key);
            }
            foreach (var key in ToggleObjectUsings.Keys.Where(k => !filter.Contains(k)).ToList())
            {
                ToggleObjectUsings.Remove(key);
            }
            foreach (var key in ToggleMaterials.Keys.Where(k => !filter.Contains(k.Item1)).ToList())
            {
                ToggleMaterials.Remove(key);
            }
            foreach (var key in ToggleBlendShapes.Keys.Where(k => !filter.Contains(k.Item1)).ToList())
            {
                ToggleBlendShapes.Remove(key);
            }
            foreach (var key in ToggleShaderParameters.Keys.Where(k => !filter.Contains(k.Item1)).ToList())
            {
                ToggleShaderParameters.Remove(key);
            }
            foreach (var key in ToggleShaderVectorParameters.Keys.Where(k => !filter.Contains(k.Item1)).ToList())
            {
                ToggleShaderVectorParameters.Remove(key);
            }
            foreach (var key in ToggleValues.Keys.Where(k => !filter.Contains(k.Item1)).ToList())
            {
                ToggleValues.Remove(key);
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
            ToggleObjects.Remove(child);
            ToggleObjectUsings.Remove(child);
            foreach (var key in ToggleMaterials.Keys.Where(k => k.Item1 == child).ToList())
            {
                ToggleMaterials.Remove(key);
            }
            foreach (var key in ToggleBlendShapes.Keys.Where(k => k.Item1 == child).ToList())
            {
                ToggleBlendShapes.Remove(key);
            }
            foreach (var key in ToggleShaderParameters.Keys.Where(k => k.Item1 == child).ToList())
            {
                ToggleShaderParameters.Remove(key);
            }
            foreach (var key in ToggleShaderVectorParameters.Keys.Where(k => k.Item1 == child).ToList())
            {
                ToggleShaderVectorParameters.Remove(key);
            }
            foreach (var key in ToggleValues.Keys.Where(k => k.Item1 == child).ToList())
            {
                ToggleValues.Remove(key);
            }
            Positions.Remove(child);
            Rotations.Remove(child);
            Scales.Remove(child);
        }
        // TODO: ToggleValues
        protected override bool IsSuitableForTransition() => ToggleBlendShapes.Count > 0 || ToggleShaderParameters.Count > 0 || ToggleShaderVectorParameters.Count > 0 || ToggleValues.Names().Where(t => t.MemberType.IsSuitableForTransition()).Count() > 0 || Positions.Count > 0 || Rotations.Count > 0 || Scales.Count > 0;

        protected override void OnMultiGUI(SerializedProperty serializedProperty)
        {
            var serializedObject = serializedProperty.serializedObject;
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedProperty.FindPropertyRelative(nameof(ToggleIcon)), new GUIContent(T.アイコン));
            EditorGUILayout.PropertyField(serializedProperty.FindPropertyRelative(nameof(ToggleDefaultValue)), new GUIContent(T.パラメーター初期値));
            ShowDetailMenuMulti(serializedProperty);
            ShowTransitionSecondsMulti(serializedProperty);
            serializedObject.ApplyModifiedProperties();
        }

        protected override void OnHeaderGUI(IList<string> children)
        {
            ToggleIcon = TextureField(T.アイコン, ToggleIcon);
            var labelFontStyle = EditorStyles.label.fontStyle;
            EditorStyles.label.fontStyle = FontStyle.Bold;
            ToggleDefaultValue = Toggle(T.パラメーター初期値, ToggleDefaultValue);
            EditorStyles.label.fontStyle = labelFontStyle;
            ShowDetailMenu();
            UseAdvanced = EditorGUILayout.Toggle(T.高度な設定, UseAdvanced);
            if (UseAdvanced)
            {
                EditorGUILayout.HelpBox(T.高度な設定を有効にするとヽONまたはOFF片方だけ制御する設定ができますゝ, MessageType.Info);
            }

            EditorGUILayout.Space();

            ShowTransitionSeconds();

            var allMaterials = children.ToDictionary(child => child, child => GetMaterialSlots(child));

            if (BulkSet)
            {
                if (FoldoutHeader("", T.一括設定, true))
                {
                    ShowToggleBulkMaterialControl(allMaterials);
                }
            }
        }

        protected override void OnMainGUI(IList<string> children)
        {
            var allMaterials = children.ToDictionary(child => child, child => GetMaterialSlots(child));

            foreach (var child in children)
            {
                EditorGUILayout.Space();
                GameObjectHeader(child);
                EditorGUI.indentLevel++;
                ShowToggleObjectControl(children, child);

                var gameObjectRef = GetGameObject(child);
                var names = gameObjectRef == null ? ToggleBlendShapes.Names(child).ToList() : Util.GetBlendShapeNames(gameObjectRef);
                var parameters = gameObjectRef == null ? ToggleShaderParameters.Names(child).ToFakeShaderFloatParameters().Concat(ToggleShaderVectorParameters.Names(child).ToFakeShaderVectorParameters()).ToList() : ShaderParametersCache.GetFilteredShaderParameters(gameObjectRef);
                var components = gameObjectRef == null ? ToggleValues.Names(child).Select(n => n.Type) : gameObjectRef.GetAllComponents().Select(c => TypeUtil.GetType(c)).FilterByVRCWhitelist();
                var members = components.GetAvailableMembers();

                var materials = allMaterials[child];
                if (materials.Length > 0 &&
                    FoldoutHeaderWithAddItemButton(
                        child,
                        "Materials",
                        ToggleMaterials.HasChild(child),
                        () => materials.Select((material, index) => new MaterialItemContainer(index, material) as ListTreeViewItemContainer<int>).Where(m => (m as MaterialItemContainer).material != null).ToList(),
                        () => ToggleMaterials.Indexes(child).ToImmutableHashSet(),
                        index => AddToggleMaterial(children, child, index),
                        index => RemoveToggleMaterial(children, child, index)
                        ))
                {
                    EditorGUI.indentLevel++;
                    ShowToggleMaterialControl(children, child, materials);
                    EditorGUI.indentLevel--;
                }

                if (names.Count > 0 &&
                    FoldoutHeaderWithAddItemButton(
                        child,
                        "BlendShapes",
                        ToggleBlendShapes.HasChild(child),
                        () => names,
                        () => ToggleBlendShapes.Names(child).ToImmutableHashSet(),
                        (name) => AddToggleBlendShape(ToggleBlendShapes, children, child, name),
                        (name) => RemoveToggleBlendShape(ToggleBlendShapes, children, child, name)
                        ))
                {
                    EditorGUI.indentLevel++;
                    ShowToggleBlendShapeControl(true, children, child, ToggleBlendShapes, names.ToNames());
                    EditorGUI.indentLevel--;
                }
                if (parameters.Count > 0 &&
                    FoldoutHeaderWithAddItemButton(
                        child,
                        "Shader Parameters",
                        ToggleShaderParameters.HasChild(child) || ToggleShaderVectorParameters.HasChild(child),
                        () => parameters.Select(p => new NameAndDescriptionItemContainer(p) as ListTreeViewItemContainer<string>).ToList(),
                        () => ToggleShaderParameters.Names(child).Concat(ToggleShaderVectorParameters.Names(child)).ToImmutableHashSet(),
                        (name) =>
                        {
                            if (parameters.Find(p => p.Name == name).IsFloatLike) AddToggleBlendShape(ToggleShaderParameters, children, child, name, 1);
                            else AddToggleShaderVectorParameter(children, child, name, 1);
                        },
                        (name) =>
                        {
                            if (parameters.Find(p => p.Name == name).IsFloatLike)
                            {
                                RemoveToggleBlendShape(ToggleShaderParameters, children, child, name);
                            }
                            else
                            {
                                RemoveToggleShaderVectorParameter(children, child, name);
                            }
                        }
                        ))
                {
                    EditorGUI.indentLevel++;
                    ShowToggleBlendShapeControl(false, children, child, ToggleShaderParameters, parameters, minValue: null, maxValue: null);
                    ShowToggleShaderVectorParameterControl(children, child, parameters);
                    EditorGUI.indentLevel--;
                }
                if (members.Count > 0 && FoldoutHeaderWithAddItemButton(
                    child,
                    "Components",
                    ToggleValues.HasChild(child),
                    () => members.Select(c => new NameAndDescriptionItemContainer(c) as ListTreeViewItemContainer<string>).ToList(),
                    () => ToggleValues.Names(child).Select(n => n.Name).ToImmutableHashSet(),
                    name => AddToggleValue(children, child, TypeMember.FromName(name)),
                    name => RemoveToggleValue(children, child, TypeMember.FromName(name))
                    ))
                {
                    EditorGUI.indentLevel++;
                    ShowToggleValueControl(children, child, members);
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

        void ShowToggleObjectControl(IList<string> children, string child)
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
                if (EditorGUILayout.Toggle(T.制御しない, type == ToggleType.None) && type != ToggleType.None) newType = ToggleType.None;
                if (EditorGUILayout.Toggle(T.ON_eq_表示, type == ToggleType.ON) && type != ToggleType.ON) newType = ToggleType.ON;
                EditorGUIUtility.labelWidth = 80;
                if (EditorGUILayout.Toggle(T.ON_eq_非表示, type == ToggleType.OFF) && type != ToggleType.OFF) newType = ToggleType.OFF;
                EditorGUIUtility.labelWidth = 0;
            }
            // Debug.Log($"{type} {newType}");
            if (type != newType)
            {
                WillChange();
                if (newType == ToggleType.None)
                {
                    ToggleObjects.Remove(child);
                    if (BulkSet)
                    {
                        foreach (var c in children)
                        {
                            ToggleObjects.Remove(c);
                        }
                    }
                }
                else
                {
                    ToggleObjects[child] = newType;
                    if (BulkSet)
                    {
                        foreach (var c in children)
                        {
                            ToggleObjects[c] = newType;
                        }
                    }
                }
            }
            if (UseAdvanced && newType != ToggleType.None)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (!ToggleObjectUsings.TryGetValue(child, out var use))
                    {
                        use = ToggleUsing.Both;
                    }
                    var newUse = use;
                    EditorGUIUtility.labelWidth = 70;
                    if (EditorGUILayout.Toggle(T.両方制御, use == ToggleUsing.Both) && use != ToggleUsing.Both) newUse = ToggleUsing.Both;
                    EditorGUIUtility.labelWidth = 90;
                    if (EditorGUILayout.Toggle(T.ONのみ制御, use == ToggleUsing.ON) && use != ToggleUsing.ON) newUse = ToggleUsing.ON;
                    if (EditorGUILayout.Toggle(T.OFFのみ制御, use == ToggleUsing.OFF) && use != ToggleUsing.OFF) newUse = ToggleUsing.OFF;
                    EditorGUIUtility.labelWidth = 0;
                    if (use != newUse)
                    {
                        WillChange();
                        if (newUse == ToggleUsing.Both)
                        {
                            ToggleObjectUsings.Remove(child);
                            if (BulkSet)
                            {
                                foreach (var c in children)
                                {
                                    ToggleObjectUsings.Remove(c);
                                }
                            }
                        }
                        else
                        {
                            ToggleObjectUsings[child] = newUse;
                            if (BulkSet)
                            {
                                foreach (var c in children)
                                {
                                    ToggleObjectUsings[c] = newUse;
                                }
                            }
                        }
                    }
                }
            }
        }

        void ShowToggleMaterialControl(IList<string> children, string child, Material[] materials)
        {
            for (var i = 0; i < materials.Length; ++i)
            {
                var key = (child, i);
                ToggleMaterial value;
                if (ToggleMaterials.TryGetValue(key, out value))
                {
                    if (ShowToggleMaterialToggle(i, materials[i], true))
                    {
                        var newValue = new ToggleMaterial();
                        EditorGUI.indentLevel++;
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            EditorGUIUtility.labelWidth = 70;
                            newValue.Inactive = EditorGUILayout.ObjectField("OFF", value.Inactive, typeof(Material), false) as Material;
                            MaterialPickerButton(child, i, ref newValue.Inactive);
                            newValue.Active = EditorGUILayout.ObjectField("ON", value.Active, typeof(Material), false) as Material;
                            MaterialPickerButton(child, i, ref newValue.Active);
                            EditorGUIUtility.labelWidth = 0;
                        }
                        if (TransitionSeconds > 0)
                        {
                            EditorGUIUtility.labelWidth = 110;
                            newValue.TransitionOffsetPercent = EditorGUILayout.FloatField(T.変化待機_per_, value.TransitionOffsetPercent, GUILayout.Width(140));
                            EditorGUIUtility.labelWidth = 0;
                        }
                        if (UseAdvanced)
                        {
                            using (new EditorGUILayout.HorizontalScope())
                            {
                                newValue.UseActive = EditorGUILayout.Toggle(T.ONを制御, value.UseActive);
                                newValue.UseInactive = EditorGUILayout.Toggle(T.OFFを制御, value.UseInactive);
                            }
                        }
                        if (!value.Equals(newValue))
                        {
                            WillChange();
                            ToggleMaterials[key] = newValue;
                            if (BulkSet)
                            {
                                BulkSetToggleMaterial(materials[i], newValue, value.ChangedProp(newValue));
                            }
                        }
                        EditorGUI.indentLevel--;
                    }
                    else
                    {
                        RemoveToggleMaterial(children, child, i);
                    }
                }
            }
        }

        bool ShowToggleMaterialToggle(int index, Material material, bool value)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUIUtility.labelWidth = 40;
                var result = EditorGUILayout.ToggleLeft($"[{index}]", value, GUILayout.Width(80));
                EditorGUIUtility.labelWidth = 0;
                using (new EditorGUI.DisabledGroupScope(true))
                {
                    EditorGUILayout.ObjectField(material, typeof(Material), false);
                }
                return result;
            }
        }

        void BulkSetToggleMaterial(Material sourceMaterial, ToggleMaterial toggleMaterial, string changedProp)
        {
            var matches = new List<(string, int)>();
            foreach (var (child, index) in ToggleMaterials.Keys)
            {
                var materials = GetMaterialSlots(child);
                if (index < materials.Length && materials[index] == sourceMaterial)
                {
                    matches.Add((child, index));
                }
            }
            foreach (var key in matches)
            {
                ToggleMaterials[key] = ToggleMaterials[key].SetProp(changedProp, toggleMaterial.GetProp(changedProp));
            }
        }

        void AddToggleMaterial(IList<string> children, string child, int index)
        {
            if (BulkSet)
            {
                var mat = GetMaterialSlots(child)[index];
                foreach (var c in children)
                {
                    var materials = GetMaterialSlots(c);
                    for (var i = 0; i < materials.Length; ++i)
                    {
                        if (materials[i] == mat)
                        {
                            AddToggleMaterialSingle(c, i);
                        }
                    }
                }
            }
            else
            {
                AddToggleMaterialSingle(child, index);
            }
        }

        void AddToggleMaterialSingle(string child, int index)
        {
            var key = (child, index);
            if (ToggleMaterials.ContainsKey(key)) return;
            WillChange();
            var toggleMaterial = new ToggleMaterial();
            toggleMaterial.Inactive = GetMaterialSlots(child)[index];
            ToggleMaterials[key] = toggleMaterial;
        }

        void RemoveToggleMaterial(IList<string> children, string child, int index)
        {
            if (BulkSet)
            {
                var mat = GetMaterialSlots(child)[index];
                foreach (var c in children)
                {
                    var materials = GetMaterialSlots(c);
                    for (var i = 0; i < materials.Length; ++i)
                    {
                        if (materials[i] == mat)
                        {
                            RemoveToggleMaterialSingle(c, i);
                        }
                    }
                }
            }
            else
            {
                RemoveToggleMaterialSingle(child, index);
            }
        }

        void RemoveToggleMaterialSingle(string child, int index)
        {
            var key = (child, index);
            if (!ToggleMaterials.ContainsKey(key)) return;
            WillChange();
            ToggleMaterials.Remove(key);
        }

        void ShowToggleBulkMaterialControl(Dictionary<string, Material[]> allMaterials)
        {
            Func<(string, int), Material> keyToMaterial = ((string, int) key) => allMaterials.TryGetValue(key.Item1, out var m) && m != null && m.Length > key.Item2 ? m[key.Item2] : null;
            var sourceMaterials = ToggleMaterials.Keys.Select(keyToMaterial).Where(m => m != null).Distinct().ToList();
            var chooseMaterialGroups = ToggleMaterials.Keys.GroupBy(keyToMaterial).Where(m => m.Key != null).ToDictionary(group => group.Key, group => group.ToList());
            foreach (var sourceMaterial in sourceMaterials)
            {
                using (new EditorGUI.DisabledGroupScope(true))
                {
                    EditorGUILayout.ObjectField(sourceMaterial, typeof(Material), false);
                }
                EditorGUI.indentLevel++;
                var inactiveMaterials = chooseMaterialGroups[sourceMaterial].Select(key => ToggleMaterials.TryGetValue(key, out var tm) ? tm.Inactive : null).Distinct().ToList();
                var activeMaterials = chooseMaterialGroups[sourceMaterial].Select(key => ToggleMaterials.TryGetValue(key, out var tm) ? tm.Active : null).Distinct().ToList();
                var transitionOffsetPercents = chooseMaterialGroups[sourceMaterial].Select(key => ToggleMaterials.TryGetValue(key, out var tm) ? tm.TransitionOffsetPercent : 0).Distinct().ToList();
                var useActives = chooseMaterialGroups[sourceMaterial].Select(key => ToggleMaterials.TryGetValue(key, out var tm) ? tm.UseActive : false).Distinct().ToList();
                var useInactives = chooseMaterialGroups[sourceMaterial].Select(key => ToggleMaterials.TryGetValue(key, out var tm) ? tm.UseInactive : false).Distinct().ToList();
                var value = new ToggleMaterial { Inactive = inactiveMaterials[0], Active = activeMaterials[0], TransitionOffsetPercent = transitionOffsetPercents[0], UseActive = useActives[0], UseInactive = useInactives[0] };
                var newValue = new ToggleMaterial();
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUIUtility.labelWidth = 70;
                    newValue.Inactive = EditorGUILayout.ObjectField("OFF", value.Inactive, typeof(Material), false) as Material;
                    newValue.Active = EditorGUILayout.ObjectField("ON", value.Active, typeof(Material), false) as Material;
                    EditorGUIUtility.labelWidth = 0;
                }
                if (TransitionSeconds > 0)
                {
                    EditorGUIUtility.labelWidth = 110;
                    newValue.TransitionOffsetPercent = EditorGUILayout.FloatField(T.変化待機_per_, value.TransitionOffsetPercent, GUILayout.Width(140));
                    EditorGUIUtility.labelWidth = 0;
                }
                if (UseAdvanced)
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        newValue.UseActive = EditorGUILayout.Toggle(T.ONを制御, value.UseActive);
                        newValue.UseInactive = EditorGUILayout.Toggle(T.OFFを制御, value.UseInactive);
                    }
                }
                if (!value.Equals(newValue))
                {
                    WillChange();
                    BulkSetToggleMaterial(sourceMaterial, newValue, value.ChangedProp(newValue));
                }
                if (inactiveMaterials.Count != 1)
                {
                    EditorGUILayout.HelpBox(T.OFFに複数のマテリアルが選択されています, MessageType.Warning);
                }
                if (activeMaterials.Count != 1)
                {
                    EditorGUILayout.HelpBox(T.ONに複数のマテリアルが選択されています, MessageType.Warning);
                }
                if (TransitionSeconds > 0)
                {
                    if (transitionOffsetPercents.Count != 1)
                    {
                        EditorGUILayout.HelpBox(T.複数の変化待機_per_が設定されています, MessageType.Warning);
                    }
                }
                if (UseAdvanced)
                {
                    if (useActives.Count != 1)
                    {
                        EditorGUILayout.HelpBox(T.ONを制御に複数の設定がされています, MessageType.Warning);
                    }
                    if (useInactives.Count != 1)
                    {
                        EditorGUILayout.HelpBox(T.OFFを制御に複数の設定がされています, MessageType.Warning);
                    }
                }
                EditorGUI.indentLevel--;
            }
        }

        void ShowToggleBlendShapeControl(
            bool isBlendShape,
            IList<string> children,
            string child,
            ToggleBlendShapeDictionary toggles,
            IEnumerable<INameAndDescription> names,
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
                            newValue.Inactive = EditorGUILayout.FloatField("OFF", value.Inactive, GUILayout.Width(110));
                            BlendShapeLikePickerButton(isBlendShape, child, name.Name, ref newValue.Inactive);
                            newValue.Active = EditorGUILayout.FloatField("ON", value.Active, GUILayout.Width(110));
                            BlendShapeLikePickerButton(isBlendShape, child, name.Name, ref newValue.Active);
                            EditorGUIUtility.labelWidth = 0;
                            EditorGUI.indentLevel--;
                        }
                        if (TransitionSeconds > 0)
                        {
                            using (new EditorGUILayout.HorizontalScope())
                            {
                                EditorGUI.indentLevel++;
                                EditorGUIUtility.labelWidth = 110;
                                newValue.TransitionOffsetPercent = EditorGUILayout.FloatField(T.変化待機_per_, value.TransitionOffsetPercent, GUILayout.Width(140));
                                newValue.TransitionDurationPercent = EditorGUILayout.FloatField(T.変化時間_per_, value.TransitionDurationPercent, GUILayout.Width(140));
                                EditorGUIUtility.labelWidth = 0;
                                EditorGUI.indentLevel--;
                            }
                            // 過去互換性
                            if (newValue.TransitionDurationPercent <= 0)
                            {
                                EditorGUILayout.HelpBox(T.変化時間_per_は0より大きく設定して下さい, MessageType.Error);
                            }
                        }
                        if (UseAdvanced)
                        {
                            using (new EditorGUILayout.HorizontalScope())
                            {
                                EditorGUI.indentLevel++;
                                newValue.UseActive = EditorGUILayout.Toggle(T.ONを制御, value.UseActive);
                                newValue.UseInactive = EditorGUILayout.Toggle(T.OFFを制御, value.UseInactive);
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
                            newValue.AdjustTransitionValues();

                            toggles[key] = newValue;
                            if (BulkSet)
                            {
                                BulkSetToggleBlendShape(toggles, name.Name, newValue, value.ChangedProps(newValue));
                            }
                        }
                    }
                    else
                    {
                        RemoveToggleBlendShape(toggles, children, child, name.Name);
                    }
                }
            }
        }

        void BulkSetToggleBlendShape(ToggleBlendShapeDictionary toggles, string toggleName, ToggleBlendShape toggleBlendShape, IEnumerable<string> changedProps)
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
                foreach (var changedProp in changedProps)
                {
                    toggles[key].SetProp(changedProp, toggleBlendShape.GetProp(changedProp));
                }
            }
        }

        void AddToggleBlendShape(ToggleBlendShapeDictionary toggles, IList<string> children, string child, string name, float defaultActiveValue = 100)
        {
            if (BulkSet)
            {
                foreach (var c in children)
                {
                    AddToggleBlendShapeSingle(toggles, c, name, defaultActiveValue);
                }
            }
            else
            {
                AddToggleBlendShapeSingle(toggles, child, name, defaultActiveValue);
            }
        }

        void AddToggleBlendShapeSingle(ToggleBlendShapeDictionary toggles, string child, string name, float defaultActiveValue = 100)
        {
            var key = (child, name);
            if (toggles.ContainsKey(key)) return;
            WillChange();
            toggles[key] = new ToggleBlendShape { Inactive = 0, Active = defaultActiveValue, TransitionDurationPercent = 100 };
        }

        void RemoveToggleBlendShape(ToggleBlendShapeDictionary toggles, IList<string> children, string child, string name)
        {
            if (BulkSet)
            {
                foreach (var c in children)
                {
                    RemoveToggleBlendShapeSingle(toggles, c, name);
                }
            }
            else
            {
                RemoveToggleBlendShapeSingle(toggles, child, name);
            }
        }

        void RemoveToggleBlendShapeSingle(ToggleBlendShapeDictionary toggles, string child, string name)
        {
            var key = (child, name);
            if (!toggles.ContainsKey(key)) return;
            WillChange();
            toggles.Remove(key);
        }

        void ShowToggleShaderVectorParameterControl(
            IList<string> children,
            string child,
            IEnumerable<INameAndDescription> names
            )
        {
            foreach (var name in names)
            {
                var key = (child, name.Name);
                ToggleVector4 value;
                if (ToggleShaderVectorParameters.TryGetValue(key, out value))
                {
                    if (EditorGUILayout.ToggleLeft(name.Description, true))
                    {
                        var newValue = new ToggleVector4();
                        EditorGUI.indentLevel++;
                        var widemode = EditorGUIUtility.wideMode;
                        EditorGUIUtility.wideMode = true;
                        EditorGUIUtility.labelWidth = 70;
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            newValue.Inactive = EditorGUILayout.Vector4Field("OFF", value.Inactive);
                            ShaderVectorParameterPickerButton(child, name.Name, ref newValue.Inactive);
                        }
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            newValue.Active = EditorGUILayout.Vector4Field("ON", value.Active);
                            ShaderVectorParameterPickerButton(child, name.Name, ref newValue.Active);
                        }
                        EditorGUIUtility.labelWidth = 0;
                        EditorGUIUtility.wideMode = widemode;
                        EditorGUI.indentLevel--;
                        if (TransitionSeconds > 0)
                        {
                            using (new EditorGUILayout.HorizontalScope())
                            {
                                EditorGUI.indentLevel++;
                                EditorGUIUtility.labelWidth = 110;
                                newValue.TransitionOffsetPercent = EditorGUILayout.FloatField(T.変化待機_per_, value.TransitionOffsetPercent, GUILayout.Width(140));
                                newValue.TransitionDurationPercent = EditorGUILayout.FloatField(T.変化時間_per_, value.TransitionDurationPercent, GUILayout.Width(140));
                                EditorGUIUtility.labelWidth = 0;
                                EditorGUI.indentLevel--;
                            }
                            // 過去互換性
                            if (newValue.TransitionDurationPercent <= 0)
                            {
                                EditorGUILayout.HelpBox(T.変化時間_per_は0より大きく設定して下さい, MessageType.Error);
                            }
                        }
                        if (UseAdvanced)
                        {
                            using (new EditorGUILayout.HorizontalScope())
                            {
                                EditorGUI.indentLevel++;
                                newValue.UseActive = EditorGUILayout.Toggle(T.ONを制御, value.UseActive);
                                newValue.UseInactive = EditorGUILayout.Toggle(T.OFFを制御, value.UseInactive);
                                EditorGUI.indentLevel--;
                            }
                        }
                        if (!value.Equals(newValue))
                        {
                            WillChange();
                            newValue.AdjustTransitionValues();

                            ToggleShaderVectorParameters[key] = newValue;
                            if (BulkSet)
                            {
                                BulkSetToggleShaderVectorParameter(name.Name, newValue, value.ChangedProps(newValue));
                            }
                        }
                    }
                    else
                    {
                        RemoveToggleShaderVectorParameter(children, child, name.Name);
                    }
                }
            }
        }

        void BulkSetToggleShaderVectorParameter(string toggleName, ToggleVector4 toggleBlendShape, IEnumerable<string> changedProps)
        {
            var matches = new List<(string, string)>();
            foreach (var (child, name) in ToggleShaderVectorParameters.Keys)
            {
                if (name == toggleName)
                {
                    matches.Add((child, name));
                }
            }
            foreach (var key in matches)
            {
                foreach (var changedProp in changedProps)
                {
                    ToggleShaderVectorParameters[key].SetProp(changedProp, toggleBlendShape.GetProp(changedProp));
                }
            }
        }

        void AddToggleShaderVectorParameter(IList<string> children, string child, string name, float defaultActiveValue = 100)
        {
            if (BulkSet)
            {
                foreach (var c in children)
                {
                    AddToggleShaderVectorParameter(c, name, defaultActiveValue);
                }
            }
            else
            {
                AddToggleShaderVectorParameter(child, name, defaultActiveValue);
            }
        }

        void AddToggleShaderVectorParameter(string child, string name, float defaultActiveValue = 100)
        {
            var key = (child, name);
            if (ToggleShaderVectorParameters.ContainsKey(key)) return;
            WillChange();
            ToggleShaderVectorParameters[key] = new ToggleVector4();
        }

        void RemoveToggleShaderVectorParameter(IList<string> children, string child, string name)
        {
            if (BulkSet)
            {
                foreach (var c in children)
                {
                    RemoveToggleShaderVectorParameterSingle(c, name);
                }
            }
            else
            {
                RemoveToggleShaderVectorParameterSingle(child, name);
            }
        }

        void RemoveToggleShaderVectorParameterSingle(string child, string name)
        {
            var key = (child, name);
            if (!ToggleShaderVectorParameters.ContainsKey(key)) return;
            WillChange();
            ToggleShaderVectorParameters.Remove(key);
        }

        void ShowToggleValueControl(
            IList<string> children,
            string child,
            IEnumerable<TypeMember> members
            )
        {
            ShowPhysBoneAutoResetMenu(child, ToggleValues.Names(child).ToArray());
            foreach (var member in members)
            {
                var key = (child, member);
                if (ToggleValues.TryGetValue(key, out var value))
                {
                    if (EditorGUILayout.ToggleLeft(member.Description, true))
                    {
                        var newValue = new ToggleValue { TransitionDurationPercent = 100 };
                        EditorGUI.indentLevel++;
                        EditorGUIUtility.labelWidth = 70;
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            if (member.MemberType == typeof(float))
                            {
                                newValue.Inactive = EditorGUILayout.FloatField("OFF", (float)value.Inactive, GUILayout.Width(110));
                                ValuePickerButton(child, member, p => newValue.Inactive = p.floatValue);
                                newValue.Active = EditorGUILayout.FloatField("ON", (float)value.Active, GUILayout.Width(110));
                                ValuePickerButton(child, member, p => newValue.Active = p.floatValue);
                            }
                            else if (member.MemberType == typeof(int))
                            {
                                newValue.Inactive = EditorGUILayout.IntField("OFF", (int)value.Inactive, GUILayout.Width(100));
                                ValuePickerButton(child, member, p => newValue.Inactive = p.intValue);
                                newValue.Active = EditorGUILayout.IntField("ON", (int)value.Active, GUILayout.Width(100));
                                ValuePickerButton(child, member, p => newValue.Active = p.intValue);
                            }
                            else if (member.MemberType == typeof(bool))
                            {
                                if (member.Member == VRCPhysBoneUtil.PhysBoneEnabled)
                                {

                                }
                                EditorGUIUtility.labelWidth = 75;
                                var activeOn = EditorGUILayout.Toggle(T.ON_eq_, (bool)value.Active == true);
                                newValue.Active = activeOn;
                                newValue.Inactive = !activeOn;
                                EditorGUIUtility.labelWidth = 0;
                            }
                            else if (member.MemberType.IsSubclassOf(typeof(Enum)))
                            {
                                var enumNames = member.MemberType.GetEnumNamesCached();
                                var enumValues = member.MemberType.GetEnumValuesCached();
                                newValue.Inactive = EditorGUILayout.IntPopup("OFF", (int)value.Inactive, enumNames, enumValues);
                                ValuePickerButton(child, member, p => newValue.Inactive = enumValues[p.enumValueIndex]);
                                newValue.Active = EditorGUILayout.IntPopup("ON", (int)value.Active, enumNames, enumValues);
                                ValuePickerButton(child, member, p => newValue.Active = enumValues[p.enumValueIndex]);
                            }
                        }
                        if (member.MemberType == typeof(VRCPhysBoneBase.PermissionFilter))
                        {
                            EditorGUIUtility.labelWidth = 90;
                            var inactive = (VRCPhysBoneBase.PermissionFilter)value.Inactive;
                            var active = (VRCPhysBoneBase.PermissionFilter)value.Active;
                            bool inactiveAllowSelf, activeAllowSelf, inactiveAllowOthers, activeAllowOthers;
                            using (new EditorGUILayout.HorizontalScope())
                            {
                                inactiveAllowSelf = EditorGUILayout.ToggleLeft("OFF allowSelf", inactive.allowSelf);
                                activeAllowSelf = EditorGUILayout.ToggleLeft("ON allowSelf", active.allowSelf);
                            }
                            using (new EditorGUILayout.HorizontalScope())
                            {
                                inactiveAllowOthers = EditorGUILayout.ToggleLeft("OFF allowOthers", inactive.allowOthers);
                                activeAllowOthers = EditorGUILayout.ToggleLeft("ON allowOthers", active.allowOthers);
                            }
                            newValue.Inactive = new VRCPhysBoneBase.PermissionFilter { allowSelf = inactiveAllowSelf, allowOthers = inactiveAllowOthers };
                            newValue.Active = new VRCPhysBoneBase.PermissionFilter { allowSelf = activeAllowSelf, allowOthers = activeAllowOthers };
                        }
                        else if (member.MemberType == typeof(Vector3))
                        {
                            var widemode = EditorGUIUtility.wideMode;
                            EditorGUIUtility.wideMode = true;
                            EditorGUIUtility.labelWidth = 70;
                            using (new EditorGUILayout.HorizontalScope())
                            {
                                newValue.Inactive = EditorGUILayout.Vector3Field("OFF", (Vector3)value.Inactive);
                                ValuePickerButton(child, member, p => newValue.Inactive = p.vector3Value);
                            }
                            using (new EditorGUILayout.HorizontalScope())
                            {
                                newValue.Active = EditorGUILayout.Vector3Field("ON", (Vector3)value.Active);
                                ValuePickerButton(child, member, p => newValue.Active = p.vector3Value);
                            }
                            EditorGUIUtility.labelWidth = 0;
                            EditorGUIUtility.wideMode = widemode;
                        }
                        EditorGUIUtility.labelWidth = 0;
                        EditorGUI.indentLevel--;
                        if (TransitionSeconds > 0)
                        {
                            using (new EditorGUILayout.HorizontalScope())
                            {
                                EditorGUI.indentLevel++;
                                EditorGUIUtility.labelWidth = 110;
                                newValue.TransitionOffsetPercent = EditorGUILayout.FloatField(T.変化待機_per_, value.TransitionOffsetPercent, GUILayout.Width(140));
                                if (member.MemberType.IsSuitableForTransition())
                                {
                                    newValue.TransitionDurationPercent = EditorGUILayout.FloatField(T.変化時間_per_, value.TransitionDurationPercent, GUILayout.Width(140));
                                }
                                EditorGUIUtility.labelWidth = 0;
                                EditorGUI.indentLevel--;
                            }
                            // 過去互換性
                            if (newValue.TransitionDurationPercent <= 0)
                            {
                                EditorGUILayout.HelpBox(T.変化時間_per_は0より大きく設定して下さい, MessageType.Error);
                            }
                        }
                        if (UseAdvanced)
                        {
                            using (new EditorGUILayout.HorizontalScope())
                            {
                                EditorGUI.indentLevel++;
                                newValue.UseActive = EditorGUILayout.Toggle(T.ONを制御, value.UseActive);
                                newValue.UseInactive = EditorGUILayout.Toggle(T.OFFを制御, value.UseInactive);
                                EditorGUI.indentLevel--;
                            }
                        }
                        if (!value.Equals(newValue))
                        {
                            WillChange();
                            newValue.AdjustTransitionValues();

                            ToggleValues[key] = newValue;
                            if (BulkSet)
                            {
                                BulkSetToggleValue(member, newValue, value.ChangedProps(newValue));
                            }
                        }
                    }
                    else
                    {
                        RemoveToggleValue(children, child, member);
                    }
                }
            }
        }

        void BulkSetToggleValue(TypeMember toggleTypeMember, ToggleValue toggleValue, IEnumerable<string> changedProps)
        {
            var matches = new List<(string, TypeMember)>();
            foreach (var (child, typeMember) in ToggleValues.Keys)
            {
                if (typeMember == toggleTypeMember)
                {
                    matches.Add((child, typeMember));
                }
            }
            foreach (var key in matches)
            {
                foreach (var changedProp in changedProps)
                {
                    ToggleValues[key].SetProp(changedProp, toggleValue.GetProp(changedProp));
                }
            }
        }

        void BulkAddToggleValue(IList<string> children, string child, TypeMember member)
        {
            if (BulkSet)
            {
                foreach (var c in children)
                {
                    AddToggleValueSingle(c, member);
                }
            }
            else
            {
                AddToggleValueSingle(child, member);
            }
        }

        void AddToggleValue(IList<string> children, string child, TypeMember member)
        {
            if (BulkSet)
            {
                foreach (var c in children)
                {
                    AddToggleValueSingle(c, member);
                }
            }
            else
            {
                AddToggleValueSingle(child, member);
            }
        }

        void AddToggleValueSingle(string child, TypeMember member)
        {
            var key = (child, member);
            if (ToggleValues.ContainsKey(key)) return;
            WillChange();
            ToggleValues[key] = member.MemberType == typeof(bool) ? new ToggleValue { Active = true, TransitionDurationPercent = 100 } : new ToggleValue { TransitionDurationPercent = 100 };
        }

        void BulkRemoveToggleValue(IList<string> children, string child, TypeMember member)
        {
            if (BulkSet)
            {
                foreach (var c in children)
                {
                    RemoveToggleValueSingle(c, member);
                }
            }
            else
            {
                RemoveToggleValueSingle(child, member);
            }
        }

        void RemoveToggleValue(IList<string> children, string child, TypeMember member)
        {
            if (BulkSet)
            {
                foreach (var c in children)
                {
                    RemoveToggleValueSingle(c, member);
                }
            }
            else
            {
                RemoveToggleValueSingle(child, member);
            }
        }

        void RemoveToggleValueSingle(string child, TypeMember member)
        {
            var key = (child, member);
            if (!ToggleValues.ContainsKey(key)) return;
            WillChange();
            ToggleValues.Remove(key);
        }

        void ShowTransformComponentControl(IList<string> children, string child, ToggleVector3Dictionary values, string title)
        {
            if (values.TryGetValue(child, out var value))
            {
                if (EditorGUILayout.ToggleLeft(title, true))
                {
                    var newValue = new ToggleVector3();
                    EditorGUI.indentLevel++;
                    var widemode = EditorGUIUtility.wideMode;
                    EditorGUIUtility.wideMode = true;
                    EditorGUIUtility.labelWidth = 70;
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        newValue.Inactive = EditorGUILayout.Vector3Field("OFF", value.Inactive);
                        TransformPickerButton(child, title, ref newValue.Inactive);
                    }
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        newValue.Active = EditorGUILayout.Vector3Field("ON", value.Active);
                        TransformPickerButton(child, title, ref newValue.Active);
                    }
                    EditorGUIUtility.labelWidth = 0;
                    EditorGUIUtility.wideMode = widemode;
                    EditorGUI.indentLevel--;
                    if (TransitionSeconds > 0)
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            EditorGUI.indentLevel++;
                            EditorGUIUtility.labelWidth = 110;
                            newValue.TransitionOffsetPercent = EditorGUILayout.FloatField(T.変化待機_per_, value.TransitionOffsetPercent, GUILayout.Width(140));
                            newValue.TransitionDurationPercent = EditorGUILayout.FloatField(T.変化時間_per_, value.TransitionDurationPercent, GUILayout.Width(140));
                            EditorGUIUtility.labelWidth = 0;
                            EditorGUI.indentLevel--;
                        }
                    }
                    if (UseAdvanced)
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            EditorGUI.indentLevel++;
                            newValue.UseActive = EditorGUILayout.Toggle(T.ONを制御, value.UseActive);
                            newValue.UseInactive = EditorGUILayout.Toggle(T.OFFを制御, value.UseInactive);
                            EditorGUI.indentLevel--;
                        }
                    }
                    if (!value.Equals(newValue))
                    {
                        WillChange();
                        newValue.AdjustTransitionValues();

                        values[child] = newValue;
                        if (BulkSet)
                        {
                            BulkSetTransformComponent(values, newValue, value.ChangedProps(newValue));
                        }
                    }
                }
                else
                {
                    RemoveTransformComponent(values, children, child);
                }
            }
        }

        void BulkSetTransformComponent(ToggleVector3Dictionary values, ToggleVector3 value, IEnumerable<string> changedProps)
        {
            foreach (var key in values.Keys.ToArray())
            {
                foreach (var changedProp in changedProps)
                {
                    values[key].SetProp(changedProp, value.GetProp(changedProp));
                }
            }
        }

        void AddTransformComponent(ToggleVector3Dictionary values, IList<string> children, string child)
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

        void AddTransformComponentSingle(ToggleVector3Dictionary values, string child)
        {
            if (values.ContainsKey(child)) return;
            WillChange();
            values[child] = new ToggleVector3();
        }

        void RemoveTransformComponent(ToggleVector3Dictionary values, IList<string> children, string child)
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

        void RemoveTransformComponentSingle(ToggleVector3Dictionary values, string child)
        {
            if (!values.ContainsKey(child)) return;
            WillChange();
            values.Remove(child);
        }

        // with prefab shim
        protected override Material[] GetMaterialSlots(string child) => GetGameObject(child)?.GetMaterialSlots() ?? ToggleMaterials.MaterialSlots(child);
#endif
    }
}
