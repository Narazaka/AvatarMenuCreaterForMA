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
using net.narazaka.avatarmenucreator.value;

namespace net.narazaka.avatarmenucreator
{
    [Serializable]
    public class AvatarChooseMenu : AvatarMenuBase
    {
        [SerializeField]
        public IntHashSetDictionary ChooseObjects = new IntHashSetDictionary();
        [SerializeField]
        public ChooseMaterialDictionary ChooseMaterials = new ChooseMaterialDictionary();
        [SerializeField]
        public ChooseBlendShapeDictionary ChooseBlendShapes = new ChooseBlendShapeDictionary();
        [SerializeField]
        public ChooseBlendShapeDictionary ChooseShaderParameters = new ChooseBlendShapeDictionary();
        [SerializeField]
        public ChooseValueDictionary ChooseValues = new ChooseValueDictionary();
        [SerializeField]
        public ChooseVector3Dictionary Positions = new ChooseVector3Dictionary();
        [SerializeField]
        public ChooseVector3Dictionary Rotations = new ChooseVector3Dictionary();
        [SerializeField]
        public ChooseVector3Dictionary Scales = new ChooseVector3Dictionary();
        [SerializeField]
        public int ChooseDefaultValue;
        [SerializeField]
        public int ChooseCount = 2;
        [SerializeField]
        public IntStringDictionary ChooseNames = new IntStringDictionary();
        [SerializeField]
        public Texture2D ChooseParentIcon;
        [SerializeField]
        public IntTexture2DDictionary ChooseIcons = new IntTexture2DDictionary();

#if UNITY_EDITOR

        static readonly string[] TransformComponentNames = new[] { "Position", "Rotation", "Scale" };
        ChooseVector3Dictionary TransformComponent(string transformComponentName)
        {
            switch (transformComponentName)
            {
                case "Position": return Positions;
                case "Rotation": return Rotations;
                case "Scale": return Scales;
                default: throw new ArgumentException();
            }
        }

        public string ChooseName(int index)
        {
            if (ChooseNames.ContainsKey(index)) return ChooseNames[index];
            return $"{T.選択肢}{index}";
        }

        public Texture2D ChooseIcon(int index)
        {
            if (ChooseIcons.ContainsKey(index)) return ChooseIcons[index];
            return null;
        }

        public override IEnumerable<string> GetStoredChildren() => ChooseObjects.Keys.Concat(ChooseMaterials.Keys.Select(key => key.Item1)).Concat(ChooseBlendShapes.Keys.Select(key => key.Item1)).Concat(ChooseShaderParameters.Keys.Select(key => key.Item1)).Concat(ChooseValues.Keys.Select(key => key.Item1)).Concat(Positions.Keys).Concat(Rotations.Keys).Concat(Scales.Keys).Distinct();

        public override void ReplaceStoredChild(string oldChild, string newChild)
        {
            if (ChooseObjects.ContainsKey(oldChild) || ChooseMaterials.ContainsPrimaryKey(oldChild) || ChooseBlendShapes.ContainsPrimaryKey(oldChild) || ChooseShaderParameters.ContainsPrimaryKey(oldChild) || ChooseValues.ContainsPrimaryKey(oldChild) || Positions.ContainsKey(oldChild) || Rotations.ContainsKey(oldChild) || Scales.ContainsKey(oldChild))
            {
                ChooseObjects.ReplaceKey(oldChild, newChild);
                ChooseMaterials.ReplacePrimaryKey(oldChild, newChild);
                ChooseBlendShapes.ReplacePrimaryKey(oldChild, newChild);
                ChooseShaderParameters.ReplacePrimaryKey(oldChild, newChild);
                ChooseValues.ReplacePrimaryKey(oldChild, newChild);
                Positions.ReplaceKey(oldChild, newChild);
                Rotations.ReplaceKey(oldChild, newChild);
                Scales.ReplaceKey(oldChild, newChild);
            }
        }

        public override void FilterStoredTargets(IEnumerable<string> children)
        {
            var filter = new HashSet<string>(children);
            foreach (var child in ChooseObjects.Keys.Where(child => !filter.Contains(child)).ToList())
            {
                ChooseObjects.Remove(child);
            }
            foreach (var key in ChooseMaterials.Keys.Where(key => !filter.Contains(key.Item1)).ToList())
            {
                ChooseMaterials.Remove(key);
            }
            foreach (var key in ChooseBlendShapes.Keys.Where(key => !filter.Contains(key.Item1)).ToList())
            {
                ChooseBlendShapes.Remove(key);
            }
            foreach (var key in ChooseShaderParameters.Keys.Where(key => !filter.Contains(key.Item1)).ToList())
            {
                ChooseShaderParameters.Remove(key);
            }
            foreach (var key in ChooseValues.Keys.Where(key => !filter.Contains(key.Item1)).ToList())
            {
                ChooseValues.Remove(key);
            }
            foreach (var child in Positions.Keys.Where(child => !filter.Contains(child)).ToList())
            {
                Positions.Remove(child);
            }
            foreach (var child in Rotations.Keys.Where(child => !filter.Contains(child)).ToList())
            {
                Rotations.Remove(child);
            }
            foreach (var child in Scales.Keys.Where(child => !filter.Contains(child)).ToList())
            {
                Scales.Remove(child);
            }
        }

        public override void RemoveStoredChild(string child)
        {
            WillChange();
            ChooseObjects.Remove(child);
            foreach (var key in ChooseMaterials.Keys.Where(key => key.Item1 == child).ToList())
            {
                ChooseMaterials.Remove(key);
            }
            foreach (var key in ChooseBlendShapes.Keys.Where(key => key.Item1 == child).ToList())
            {
                ChooseBlendShapes.Remove(key);
            }
            foreach (var key in ChooseShaderParameters.Keys.Where(key => key.Item1 == child).ToList())
            {
                ChooseShaderParameters.Remove(key);
            }
            foreach (var key in ChooseValues.Keys.Where(key => key.Item1 == child).ToList())
            {
                ChooseValues.Remove(key);
            }
            Positions.Remove(child);
            Rotations.Remove(child);
            Scales.Remove(child);
        }

        protected override bool IsSuitableForTransition() => ChooseBlendShapes.Count > 0 || ChooseShaderParameters.Count > 0 || ChooseValues.Names().Where(t => t.MemberType.IsSuitableForTransition()).Count() > 0 || Positions.Count > 0 || Rotations.Count > 0 || Scales.Count > 0;

        protected override void OnMultiGUI(SerializedProperty serializedProperty)
        {
            var serializedObject = serializedProperty.serializedObject;
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedProperty.FindPropertyRelative(nameof(ChooseParentIcon)), new GUIContent(T.親メニューアイコン));
            EditorGUILayout.PropertyField(serializedProperty.FindPropertyRelative(nameof(ChooseDefaultValue)), new GUIContent(T.パラメーター初期値));
            ShowSavedMulti(serializedProperty);
            ShowDetailMenuMulti(serializedProperty);
            ShowTransitionSecondsMulti(serializedProperty);
            serializedObject.ApplyModifiedProperties();
        }

        bool ChoiceFoldout = true;
        Vector2 ScrollPositionChoises;

        protected override void OnHeaderGUI(IList<string> children)
        {
            ChooseParentIcon = TextureField(T.親メニューアイコン, ChooseParentIcon);
            var labelFontStyle = EditorStyles.label.fontStyle;
            EditorStyles.label.fontStyle = FontStyle.Bold;
            ChooseDefaultValue = IntField(T.パラメーター初期値, ChooseDefaultValue);
            EditorStyles.label.fontStyle = labelFontStyle;
            ShowSaved();
            ShowDetailMenu();

            EditorGUILayout.Space();

            ShowTransitionSeconds();

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            var rect = EditorGUILayout.GetControlRect(false, GUILayout.Width(EditorGUIUtility.labelWidth));
            ChoiceFoldout = EditorGUI.Foldout(rect, ChoiceFoldout, T.選択肢の数);
            ChooseCount = IntField(ChooseCount);
            EditorGUILayout.EndHorizontal();

            if (ChooseCount < 1) ChooseCount = 1;
            if (ChooseDefaultValue < 0) ChooseDefaultValue = 0;
            if (ChooseDefaultValue >= ChooseCount) ChooseDefaultValue = ChooseCount - 1;

            if (ChoiceFoldout)
            {
                if (ChooseCount > 10)
                {
                    using (var scrollView = new EditorGUILayout.ScrollViewScope(ScrollPositionChoises, GUILayout.Height(220)))
                    {
                        ScrollPositionChoises = scrollView.scrollPosition;
                        HeaderChoises();
                    }
                }
                else
                {
                    HeaderChoises();
                }
            }
            
            HeaderBulkChoises(children);

            HorizontalLine();
        }

        void HeaderChoises()
        {
            EditorGUI.indentLevel++;
            EditorGUIUtility.labelWidth = 65;
            for (var i = 0; i < ChooseCount; ++i)
            {
                EditorGUILayout.BeginHorizontal();
                ChooseNames[i] = TextField($"{T.選択肢}{i}", ChooseName(i));
                ChooseIcons[i] = TextureField(ChooseIcon(i));
                if (GUILayout.Button("↑", GUILayout.Width(19)))
                {
                    MoveChoice(i, i > 0 ? i - 1 : ChooseCount - 1);
                }
                if (GUILayout.Button("↓", GUILayout.Width(19)))
                {
                    MoveChoice(i, i < ChooseCount - 1 ? i + 1 : 0);
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUIUtility.labelWidth = 0;
            EditorGUI.indentLevel--;
        }

        void HeaderBulkChoises(IList<string> children)
        {
            if (BulkSet)
            {
                if (FoldoutHeader("", T.一括設定, true))
                {
                    var allMaterials = children.ToDictionary(child => child, child => GetMaterialSlots(child));
                    ShowChooseBulkMaterialControl(allMaterials);
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

                if (!ChooseObjects.TryGetValue(child, out var indexes))
                {
                    indexes = new IntHashSet();
                }
                EditorGUILayout.BeginHorizontal();
                var hasSetting = indexes.Count > 0;
                var foldoutGameObject = FoldoutHeader(child, "GameObject", hasSetting);
                EditorGUIUtility.labelWidth = 40;
                var newHasSetting = EditorGUILayout.Toggle(T.制御, hasSetting, GUILayout.Width(57));
                EditorGUIUtility.labelWidth = 0;
                if (hasSetting != newHasSetting)
                {
                    WillChange();
                    if (newHasSetting)
                    {
                        indexes = new IntHashSet();
                        indexes.Add(0);
                        ChooseObjects[child] = indexes;
                    }
                    else
                    {
                        indexes = new IntHashSet();
                        ChooseObjects.Remove(child);
                    }
                }
                EditorGUILayout.EndHorizontal();
                if (foldoutGameObject)
                {
                    EditorGUI.indentLevel++;
                    ShowChooseObjectControl(child, indexes);
                    EditorGUI.indentLevel--;
                }

                var materials = allMaterials[child];
                if (materials.Length > 0 &&
                    FoldoutHeaderWithAddItemButton(
                        child,
                        "Materials",
                        ChooseMaterials.HasChild(child),
                        () => materials.Select((material, index) => new MaterialItemContainer(index, material) as ListTreeViewItemContainer<int>).Where(m => (m as MaterialItemContainer).material != null).ToList(),
                        () => ChooseMaterials.Indexes(child).ToImmutableHashSet(),
                        index => AddChooseMaterial(children, child, index),
                        index => RemoveChooseMaterial(children, child, index)
                        ))
                {
                    EditorGUI.indentLevel++;
                    ShowChooseMaterialControl(children, child, materials);
                    EditorGUI.indentLevel--;
                }

                var gameObjectRef = GetGameObject(child);
                var names = gameObjectRef == null ? ChooseBlendShapes.Names(child).ToList() : Util.GetBlendShapeNames(gameObjectRef);
                var parameters = gameObjectRef == null ? ChooseShaderParameters.Names(child).ToFakeShaderParameters().ToList() : ShaderParametersCache.GetFilteredShaderParameters(gameObjectRef);
                var components = gameObjectRef == null ? ChooseValues.Names(child).Select(n => n.Type) : gameObjectRef.GetAllComponents().Select(c => TypeUtil.GetType(c)).FilterByVRCWhitelist();
                var members = components.GetAvailableMembers();
                if (names.Count > 0 &&
                    FoldoutHeaderWithAddItemButton(
                        child,
                        "BlendShapes",
                        ChooseBlendShapes.HasChild(child),
                        () => names,
                        () => ChooseBlendShapes.Names(child).ToImmutableHashSet(),
                        name => AddChooseBlendShape(ChooseBlendShapes, children, child, name),
                        name => RemoveChooseBlendShape(ChooseBlendShapes, children, child, name)
                        ))
                {
                    EditorGUI.indentLevel++;
                    ShowChooseBlendShapeControl(true, children, child, ChooseBlendShapes, names.ToNames());
                    EditorGUI.indentLevel--;
                }
                if (parameters.Count > 0 &&
                    FoldoutHeaderWithAddItemButton(
                        child,
                        "Shader Parameters",
                        ChooseShaderParameters.HasChild(child),
                        () => parameters.Select(p => new NameAndDescriptionItemContainer(p) as ListTreeViewItemContainer<string>).ToList(),
                        () => ChooseShaderParameters.Names(child).ToImmutableHashSet(),
                        name => AddChooseBlendShape(ChooseShaderParameters, children, child, name),
                        name => RemoveChooseBlendShape(ChooseShaderParameters, children, child, name)
                        ))
                {
                    EditorGUI.indentLevel++;
                    ShowChooseBlendShapeControl(false, children, child, ChooseShaderParameters, parameters, minValue: null, maxValue: null);
                    EditorGUI.indentLevel--;
                }
                if (members.Count > 0 && FoldoutHeaderWithAddItemButton(
                    child,
                    "Components",
                    ChooseValues.HasChild(child),
                    () => members.Select(c => new NameAndDescriptionItemContainer(c) as ListTreeViewItemContainer<string>).ToList(),
                    () => ChooseValues.Names(child).Select(n => n.Name).ToImmutableHashSet(),
                    name => AddChooseValue(children, child, TypeMember.FromName(name)),
                    name => RemoveChooseValue(children, child, TypeMember.FromName(name))
                    ))
                {
                    EditorGUI.indentLevel++;
                    ShowChooseValueControl(children, child, members);
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

        void MoveChoice(int from, int to)
        {
            WillChange();
            ChooseNames.SwapKey(from, to);
            ChooseIcons.SwapKey(from, to);
            foreach (var indexes in ChooseObjects.Values)
            {
                var hasFrom = indexes.Contains(from);
                var hasTo = indexes.Contains(to);
                if (hasFrom != hasTo)
                {
                    if (hasFrom)
                    {
                        indexes.Remove(from);
                        indexes.Add(to);
                    }
                    else
                    {
                        indexes.Remove(to);
                        indexes.Add(from);
                    }
                }
            }
            foreach (var value in ChooseMaterials.Values)
            {
                value.SwapKey(from, to);
            }
            foreach (var value in ChooseBlendShapes.Values)
            {
                value.SwapKey(from, to);
            }
            foreach (var value in ChooseShaderParameters.Values)
            {
                value.SwapKey(from, to);
            }
        }

        void ShowChooseObjectControl(string child, IntHashSet indexes)
        {
            var changed = false;
            for (var i = 0; i < ChooseCount; i++)
            {
                var active = indexes.Contains(i);
                var newActive = EditorGUILayout.ToggleLeft(ChooseName(i), active);
                if (active != newActive)
                {
                    if (newActive)
                    {
                        indexes.Add(i);
                    }
                    else
                    {
                        indexes.Remove(i);
                    }
                    changed = true;
                }
            }
            if (changed)
            {
                WillChange();
                if (indexes.Count == 0)
                {
                    ChooseObjects.Remove(child);
                }
                else
                {
                    ChooseObjects[child] = indexes;
                }
            }
        }

        void ShowChooseMaterialControl(IList<string> children, string child, Material[] materials)
        {
            for (var i = 0; i < materials.Length; ++i)
            {
                var key = (child, i);
                IntMaterialDictionary values;
                if (ChooseMaterials.TryGetValue(key, out values))
                {
                    if (ShowChooseMaterialToggle(i, materials[i], true))
                    {
                        EditorGUI.indentLevel++;
                        for (var j = 0; j < ChooseCount; ++j)
                        {
                            var value = values.ContainsKey(j) ? values[j] : null;
                            EditorGUILayout.BeginHorizontal();
                            var newValue = EditorGUILayout.ObjectField(ChooseName(j), value, typeof(Material), false) as Material;
                            MaterialPickerButton(child, i, ref newValue);
                            EditorGUILayout.EndHorizontal();
                            if (value != newValue)
                            {
                                WillChange();
                                values[j] = newValue;
                                if (BulkSet)
                                {
                                    SetBulkChooseMaterial(materials[i], j, newValue);
                                }
                            }
                        }
                        EditorGUI.indentLevel--;
                    }
                    else
                    {
                        RemoveChooseMaterial(children, child, i);
                    }
                }
            }
        }

        bool ShowChooseMaterialToggle(int index, Material material, bool value)
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

        void SetBulkChooseMaterial(Material sourceMaterial, int choiseIndex, Material choiceMaterial)
        {
            foreach (var (child, index) in ChooseMaterials.Keys)
            {
                var materials = GetMaterialSlots(child);
                if (index < materials.Length && materials[index] == sourceMaterial)
                {
                    var values = ChooseMaterials[(child, index)];
                    values[choiseIndex] = choiceMaterial;
                }
            }
        }

        void AddChooseMaterial(IList<string> children, string child, int index)
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
                            AddChooseMaterialSingle(c, i);
                        }
                    }
                }
            }
            else
            {
                AddChooseMaterialSingle(child, index);
            }
        }

        void AddChooseMaterialSingle(string child, int index)
        {
            var key = (child, index);
            if (ChooseMaterials.ContainsKey(key)) return;
            WillChange();
            ChooseMaterials[key] = new IntMaterialDictionary();
            ChooseMaterials[key][0] = GetMaterialSlots(child)[index];
        }

        void RemoveChooseMaterial(IList<string> children, string child, int index)
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
                            RemoveChooseMaterialSingle(c, i);
                        }
                    }
                }
            }
            else
            {
                RemoveChooseMaterialSingle(child, index);
            }
        }

        void RemoveChooseMaterialSingle(string child, int index)
        {
            var key = (child, index);
            if (!ChooseMaterials.ContainsKey(key)) return;
            WillChange();
            ChooseMaterials.Remove(key);
        }

        void ShowChooseBulkMaterialControl(Dictionary<string, Material[]> allMaterials)
        {
            Func<(string, int), Material> keyToMaterial = ((string, int) key) => allMaterials.TryGetValue(key.Item1, out var m) && m != null && m.Length > key.Item2 ? m[key.Item2] : null;
            var sourceMaterials = ChooseMaterials.Keys.Select(keyToMaterial).Where(m => m != null).Distinct();
            var chooseMaterialGroups = ChooseMaterials.Keys.GroupBy(keyToMaterial).Where(m => m.Key != null).ToDictionary(group => group.Key, group => group.ToList());
            foreach (var sourceMaterial in sourceMaterials)
            {
                using (new EditorGUI.DisabledGroupScope(true))
                {
                    EditorGUILayout.ObjectField(sourceMaterial, typeof(Material), false);
                }
                EditorGUI.indentLevel++;
                for (var j = 0; j < ChooseCount; ++j)
                {
                    var materials = chooseMaterialGroups[sourceMaterial].Select(key => ChooseMaterials.TryGetValue(key, out var ms) && ms.TryGetValue(j, out var m) ? m : null).Distinct().ToList();
                    var value = materials[0];
                    var newValue = EditorGUILayout.ObjectField(ChooseName(j), value, typeof(Material), false) as Material;
                    if (value != newValue)
                    {
                        WillChange();
                        SetBulkChooseMaterial(sourceMaterial, j, newValue);
                    }
                    if (materials.Count != 1)
                    {
                        EditorGUILayout.HelpBox(T.複数のマテリアルが選択されています, MessageType.Warning);
                    }
                }
                EditorGUI.indentLevel--;
            }
        }

        void ShowChooseBlendShapeControl(
            bool isBlendShape,
            IList<string> children,
            string child,
            ChooseBlendShapeDictionary choices,
            IEnumerable<INameAndDescription> names,
            float? minValue = 0,
            float? maxValue = 100
            )
        {
            foreach (var name in names)
            {
                var key = (child, name.Name);
                IntFloatDictionary values;
                if (choices.TryGetValue(key, out values))
                {
                    if (EditorGUILayout.ToggleLeft(name.Description, true))
                    {
                        EditorGUI.indentLevel++;
                        for (var i = 0; i < ChooseCount; ++i)
                        {
                            var value = values.ContainsKey(i) ? values[i] : 0;
                            EditorGUILayout.BeginHorizontal();
                            var newValue = EditorGUILayout.FloatField(ChooseName(i), value);
                            BlendShapeLikePickerButton(isBlendShape, child, name.Name, ref newValue);
                            EditorGUILayout.EndHorizontal();

                            if (value != newValue)
                            {
                                WillChange();
                                if (minValue != null)
                                {
                                    if (newValue < (float)minValue) newValue = (float)minValue;
                                }
                                if (maxValue != null)
                                {
                                    if (newValue > (float)maxValue) newValue = (float)maxValue;
                                }
                                values[i] = newValue;
                                if (BulkSet)
                                {
                                    BulkSetChooseBlendShape(choices, name.Name, i, newValue);
                                }
                            }
                        }
                        EditorGUI.indentLevel--;
                    }
                    else
                    {
                        RemoveChooseBlendShape(choices, children, child, name.Name);
                    }
                }
            }
        }

        void BulkSetChooseBlendShape(ChooseBlendShapeDictionary choices, string toggleName, int choiseIndex, float choiceValue)
        {
            foreach (var (child, name) in choices.Keys)
            {
                if (name == toggleName)
                {
                    choices[(child, name)][choiseIndex] = choiceValue;
                }
            }
        }

        void AddChooseBlendShape(ChooseBlendShapeDictionary choices, IList<string> children, string child, string name)
        {
            if (BulkSet)
            {
                foreach (var c in children)
                {
                    AddChooseBlendShapeSingle(choices, c, name);
                }
            }
            else
            {
                AddChooseBlendShapeSingle(choices, child, name);
            }
        }

        void AddChooseBlendShapeSingle(ChooseBlendShapeDictionary choices, string child, string name)
        {
            var key = (child, name);
            if (choices.ContainsKey(key)) return;
            WillChange();
            choices[key] = new IntFloatDictionary();
        }

        void RemoveChooseBlendShape(ChooseBlendShapeDictionary choices, IList<string> children, string child, string name)
        {
            if (BulkSet)
            {
                foreach (var c in children)
                {
                    RemoveChooseBlendShapeSingle(choices, c, name);
                }
            }
            else
            {
                RemoveChooseBlendShapeSingle(choices, child, name);
            }
        }

        void RemoveChooseBlendShapeSingle(ChooseBlendShapeDictionary choices, string child, string name)
        {
            var key = (child, name);
            if (!choices.ContainsKey(key)) return;
            WillChange();
            choices.Remove(key);
        }

        void ShowChooseValueControl(
            IList<string> children,
            string child,
            IEnumerable<TypeMember> members
            )
        {
            ShowPhysBoneAutoResetMenu(child, ChooseValues.Names(child).ToArray());
            foreach (var member in members)
            {
                var key = (child, member);
                if (ChooseValues.TryGetValue(key, out var values))
                {
                    if (EditorGUILayout.ToggleLeft(member.Description, true))
                    {
                        EditorGUI.indentLevel++;
                        for (var i = 0; i < ChooseCount; ++i)
                        {
                            var value = values.ContainsKey(i) ? values[i] : 0;
                            Value newValue = null;
                            EditorGUILayout.BeginHorizontal();
                            if (member.MemberType == typeof(float))
                            {
                                newValue = EditorGUILayout.FloatField(ChooseName(i), (float)value);
                                ValuePickerButton(child, member, p => newValue = p.floatValue);
                            }
                            else if (member.MemberType == typeof(int))
                            {
                                newValue = EditorGUILayout.IntField(ChooseName(i), (int)value);
                                ValuePickerButton(child, member, p => newValue = p.intValue);
                            }
                            else if (member.MemberType == typeof(bool))
                            {
                                newValue = EditorGUILayout.Toggle(ChooseName(i), (bool)value);
                                ValuePickerButton(child, member, p => newValue = p.boolValue);
                            }
                            else if (member.MemberType.IsSubclassOf(typeof(Enum)))
                            {
                                var enumNames = member.MemberType.GetEnumNamesCached();
                                var enumValues = member.MemberType.GetEnumValuesCached();
                                newValue = EditorGUILayout.IntPopup(ChooseName(i), (int)value, enumNames, enumValues);
                                ValuePickerButton(child, member, p => newValue = enumValues[p.enumValueIndex]);
                            }
                            else if (member.MemberType == typeof(VRCPhysBoneBase.PermissionFilter))
                            {
                                EditorGUILayout.LabelField(ChooseName(i), GUILayout.Width(EditorGUIUtility.labelWidth));
                                EditorGUIUtility.labelWidth = 85;
                                var filter = (VRCPhysBoneBase.PermissionFilter)value;
                                newValue = new VRCPhysBoneBase.PermissionFilter
                                {
                                    allowSelf = EditorGUILayout.ToggleLeft($"allowSelf", filter.allowSelf, GUILayout.Width(115)),
                                    allowOthers = EditorGUILayout.ToggleLeft("allowOthers", filter.allowOthers, GUILayout.Width(130)),
                                };
                                EditorGUIUtility.labelWidth = 0;
                            }
                            else if (member.MemberType == typeof(Vector3))
                            {
                                var widemode = EditorGUIUtility.wideMode;
                                EditorGUIUtility.wideMode = true;
                                newValue = EditorGUILayout.Vector3Field(ChooseName(i), (Vector3)value);
                                ValuePickerButton(child, member, p => newValue = p.vector3Value);
                                EditorGUIUtility.wideMode = widemode;
                            }
                            EditorGUILayout.EndHorizontal();

                            if (value != newValue)
                            {
                                WillChange();
                                values[i] = newValue;
                                if (BulkSet)
                                {
                                    BulkSetChooseValue(member, i, newValue);
                                }
                            }
                        }
                        EditorGUI.indentLevel--;
                    }
                    else
                    {
                        RemoveChooseValue(children, child, member);
                    }
                }
            }
        }

        void BulkSetChooseValue(TypeMember sourceMember, int choiseIndex, Value choiceValue)
        {
            foreach (var (child, member) in ChooseValues.Keys.ToArray())
            {
                if (member == sourceMember)
                {
                    ChooseValues[(child, member)][choiseIndex] = choiceValue;
                }
            }
        }

        void AddChooseValue(IList<string> children, string child, TypeMember member)
        {
            if (BulkSet)
            {
                foreach (var c in children)
                {
                    AddChooseValueSingle(c, member);
                }
            }
            else
            {
                AddChooseValueSingle(child, member);
            }
        }

        void AddChooseValueSingle(string child, TypeMember member)
        {
            var key = (child, member);
            if (ChooseValues.ContainsKey(key)) return;
            WillChange();
            ChooseValues[key] = new IntValueDictionary();
        }

        void RemoveChooseValue(IList<string> children, string child, TypeMember member)
        {
            if (BulkSet)
            {
                foreach (var c in children)
                {
                    RemoveChooseValueSingle(c, member);
                }
            }
            else
            {
                RemoveChooseValueSingle(child, member);
            }
        }

        void RemoveChooseValueSingle(string child, TypeMember member)
        {
            var key = (child, member);
            if (!ChooseValues.ContainsKey(key)) return;
            WillChange();
            ChooseValues.Remove(key);
        }

        void ShowTransformComponentControl(IList<string> children, string child, ChooseVector3Dictionary choices, string title)
        {
            if (choices.TryGetValue(child, out var values))
            {
                if (EditorGUILayout.ToggleLeft(title, true))
                {
                    EditorGUI.indentLevel++;
                    var widemode = EditorGUIUtility.wideMode;
                    EditorGUIUtility.wideMode = true;
                    for (var i = 0; i < ChooseCount; ++i)
                    {
                        var value = values.ContainsKey(i) ? values[i] : Vector3.zero;
                        EditorGUILayout.BeginHorizontal();
                        var newValue = EditorGUILayout.Vector3Field(ChooseName(i), value);
                        TransformPickerButton(child, title, ref newValue);
                        EditorGUILayout.EndHorizontal();

                        if (value != newValue)
                        {
                            WillChange();
                            values[i] = newValue;
                            if (BulkSet)
                            {
                                BulkSetTransformComponent(choices, i, newValue);
                            }
                        }
                    }
                    EditorGUIUtility.wideMode = widemode;
                    EditorGUI.indentLevel--;
                }
                else
                {
                    RemoveTransformComponent(choices, children, child);
                }
            }
        }

        void BulkSetTransformComponent(ChooseVector3Dictionary values, int choiseIndex, Vector3 choiceValue)
        {
            foreach (var key in values.Keys.ToArray())
            {
                values[key][choiseIndex] = choiceValue;
            }
        }

        void AddTransformComponent(ChooseVector3Dictionary values, IList<string> children, string child)
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

        void AddTransformComponentSingle(ChooseVector3Dictionary values, string child)
        {
            if (values.ContainsKey(child)) return;
            WillChange();
            values[child] = new IntVector3Dictionary();
        }

        void RemoveTransformComponent(ChooseVector3Dictionary values, IList<string> children, string child)
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

        void RemoveTransformComponentSingle(ChooseVector3Dictionary values, string child)
        {
            if (!values.ContainsKey(child)) return;
            WillChange();
            values.Remove(child);
        }

        // with prefab shim
        protected override Material[] GetMaterialSlots(string child) => GetGameObject(child)?.GetMaterialSlots() ?? ChooseMaterials.MaterialSlots(child);
#endif
    }
}
