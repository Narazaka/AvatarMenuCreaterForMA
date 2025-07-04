﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using UnityEngine;
using VRC.Dynamics;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
using net.narazaka.avatarmenucreator.util;
using System.Text.RegularExpressions;
#endif
using net.narazaka.avatarmenucreator.collections.instance;
using net.narazaka.avatarmenucreator.value;
using net.narazaka.avatarmenucreator.collections;

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
        public ChooseShaderVectorParameterDictionary ChooseShaderVectorParameters = new ChooseShaderVectorParameterDictionary();
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
        public bool UseCompressed;
        [SerializeField]
        public IntStringDictionary ChooseNames = new IntStringDictionary();
        [SerializeField]
        public bool UseParentMenu = true;
        [SerializeField]
        public Texture2D ChooseParentIcon;
        [SerializeField]
        public IntTexture2DDictionary ChooseIcons = new IntTexture2DDictionary();

        public bool CanUseCompressed => Synced && ChooseCount > 1;

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

        public override IEnumerable<string> GetStoredChildren() => ChooseObjects.Keys.Concat(ChooseMaterials.Keys.Select(key => key.Item1)).Concat(ChooseBlendShapes.Keys.Select(key => key.Item1)).Concat(ChooseShaderParameters.Keys.Select(key => key.Item1)).Concat(ChooseShaderVectorParameters.Keys.Select(key => key.Item1)).Concat(ChooseValues.Keys.Select(key => key.Item1)).Concat(Positions.Keys).Concat(Rotations.Keys).Concat(Scales.Keys).Distinct();

        public override void ReplaceStoredChild(string oldChild, string newChild)
        {
            if (ChooseObjects.ContainsKey(oldChild) || ChooseMaterials.ContainsPrimaryKey(oldChild) || ChooseBlendShapes.ContainsPrimaryKey(oldChild) || ChooseShaderParameters.ContainsPrimaryKey(oldChild) || ChooseShaderVectorParameters.ContainsPrimaryKey(oldChild) || ChooseValues.ContainsPrimaryKey(oldChild) || Positions.ContainsKey(oldChild) || Rotations.ContainsKey(oldChild) || Scales.ContainsKey(oldChild))
            {
                ChooseObjects.ReplaceKey(oldChild, newChild);
                ChooseMaterials.ReplacePrimaryKey(oldChild, newChild);
                ChooseBlendShapes.ReplacePrimaryKey(oldChild, newChild);
                ChooseShaderParameters.ReplacePrimaryKey(oldChild, newChild);
                ChooseShaderVectorParameters.ReplacePrimaryKey(oldChild, newChild);
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
            foreach (var key in ChooseShaderVectorParameters.Keys.Where(key => !filter.Contains(key.Item1)).ToList())
            {
                ChooseShaderVectorParameters.Remove(key);
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
            foreach (var key in ChooseShaderVectorParameters.Keys.Where(key => key.Item1 == child).ToList())
            {
                ChooseShaderVectorParameters.Remove(key);
            }
            foreach (var key in ChooseValues.Keys.Where(key => key.Item1 == child).ToList())
            {
                ChooseValues.Remove(key);
            }
            Positions.Remove(child);
            Rotations.Remove(child);
            Scales.Remove(child);
        }

        protected override bool IsSuitableForTransition() => ChooseBlendShapes.Count > 0 || ChooseShaderParameters.Count > 0 || ChooseShaderVectorParameters.Count > 0 || ChooseValues.Names().Where(t => t.MemberType.IsSuitableForTransition()).Count() > 0 || Positions.Count > 0 || Rotations.Count > 0 || Scales.Count > 0;

        protected override void OnMultiGUI(SerializedProperty serializedProperty)
        {
            var serializedObject = serializedProperty.serializedObject;
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedProperty.FindPropertyRelative(nameof(UseParentMenu)), new GUIContent(T.親メニューを作る));
            EditorGUILayout.PropertyField(serializedProperty.FindPropertyRelative(nameof(ChooseParentIcon)), new GUIContent(T.親メニューアイコン));
            EditorGUILayout.PropertyField(serializedProperty.FindPropertyRelative(nameof(ChooseDefaultValue)), new GUIContent(T.パラメーター初期値));
            ShowDetailMenuMulti(serializedProperty);
#if HAS_COMPRESSED_INT_PARAMETERS
            EditorGUILayout.PropertyField(serializedProperty.FindPropertyRelative(nameof(UseCompressed)), new GUIContent(T.パラメーター圧縮, T.選択肢数に必要最低限なパラメーターbit数にしますゝ同期がほんの少し遅延する可能性がありますゝ));
#endif
            ShowTransitionSecondsMulti(serializedProperty);
            serializedObject.ApplyModifiedProperties();
        }

        bool ChoiceFoldout = true;
        bool ChooseNameFoldout = false;
        (string, int) ChooseNameMaterialSlot;
        bool ChooseNameRenameUseRegexp = false;
        bool ChooseNameRenameIgnoreCase = false;
        string ChooseNameRenameSearch = "";
        string ChooseNameRenameResult = "";
        Vector2 ScrollPositionChoises;
        ReorderableList ChoiceList = null;

        protected override void OnHeaderGUI(IList<string> children)
        {
            UseParentMenu = Toggle(T.親メニューを作る, UseParentMenu);
            if (UseParentMenu)
            {
                ChooseParentIcon = TextureField(T.親メニューアイコン, ChooseParentIcon);
            }
            var labelFontStyle = EditorStyles.label.fontStyle;
            EditorStyles.label.fontStyle = FontStyle.Bold;
            ChooseDefaultValue = IntField(T.パラメーター初期値, ChooseDefaultValue);
            EditorStyles.label.fontStyle = labelFontStyle;
            ShowDetailMenu();
#if HAS_COMPRESSED_INT_PARAMETERS
            EditorGUI.BeginDisabledGroup(!CanUseCompressed);
            EditorGUILayout.BeginHorizontal();
            UseCompressed = Toggle(new GUIContent(T.パラメーター圧縮, T.選択肢数に必要最低限なパラメーターbit数にしますゝ同期がほんの少し遅延する可能性がありますゝ), UseCompressed);
            EditorGUI.BeginDisabledGroup(!UseCompressed);
            EditorGUILayout.LabelField(CanUseCompressed ? $"=> {Narazaka.VRChat.CompressedIntParameters.CompressedParameterConfig.Bits(ChooseCount - 1)} bit" : "");
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
            EditorGUI.EndDisabledGroup();
#endif

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
                EditorGUILayout.BeginHorizontal();
                var dropRect = EditorGUILayout.GetControlRect(false, GUILayout.Height(EditorGUIUtility.singleLineHeight));
                DropAreaGUI(dropRect, T.アイコンをドラッグ__and__ドロップ, (index, icon) => ChooseIcons[index] = icon as Texture2D);
                if (GUILayout.Button(T.選択肢名を変更)) ChooseNameFoldout = !ChooseNameFoldout;
                EditorGUILayout.EndHorizontal();

                EditorGUI.indentLevel++;
                if (ChooseNameFoldout)
                {
                    if (GUILayout.Button(T.アイコンから命名))
                    {
                        WillChange();
                        for (var i = 0; i < ChooseCount; ++i)
                        {
                            var icon = ChooseIcon(i);
                            if (icon != null)
                            {
                                ChooseNames[i] = icon.name;
                            }
                        }
                    }
                    if (ChooseMaterials.Count > 0)
                    {
                        if (GUILayout.Button(T.マテリアルから命名))
                        {
                            if (ChooseMaterials.TryGetValue(ChooseNameMaterialSlot, out var matDic))
                            {
                                WillChange();
                                for (var i = 0; i < ChooseCount; ++i)
                                {
                                    if (matDic.TryGetValue(i, out var mat) && mat != null)
                                    {
                                        ChooseNames[i] = mat.name;
                                    }
                                }
                            }
                        }
                        EditorGUI.indentLevel++;
                        
                        var slots = ChooseMaterials.Keys.ToArray();
                        var slotNames = slots.Select(s => $"{s.Item1.Replace("/", " \u2215 ")}[{s.Item2}]").ToArray();
                        var index = EditorGUILayout.Popup(Array.IndexOf(slots, ChooseNameMaterialSlot), slotNames);
                        if (index >= 0)
                        {
                            ChooseNameMaterialSlot = slots[index];
                        }
                        EditorGUI.indentLevel--;
                    }

                    if (GUILayout.Button(T.一括置換))
                    {
                        WillChange();
                        Regex re;
                        try
                        {
                            var restr = ChooseNameRenameUseRegexp ? ChooseNameRenameSearch : Regex.Escape(ChooseNameRenameSearch);
                            re = new Regex(restr, ChooseNameRenameIgnoreCase ? RegexOptions.IgnoreCase : RegexOptions.None);
                            for (var i = 0; i < ChooseCount; ++i)
                            {
                                var chooseName = ChooseName(i);
                                ChooseNames[i] = re.Replace(chooseName, ChooseNameRenameResult);
                            }
                        }
                        catch (ArgumentException)
                        {
                            EditorUtility.DisplayDialog(T.エラー, T.正規表現が正しくありません, "OK");
                        }
                    }
                    EditorGUI.indentLevel++;
                    ChooseNameRenameSearch = EditorGUILayout.TextField(T.検索文字列, ChooseNameRenameSearch);
                    ChooseNameRenameResult = EditorGUILayout.TextField(T.置換後文字列, ChooseNameRenameResult);
                    ChooseNameRenameIgnoreCase = EditorGUILayout.Toggle(T.大文字小文字を区別しない, ChooseNameRenameIgnoreCase);
                    ChooseNameRenameUseRegexp = EditorGUILayout.Toggle(T.正規表現を使う, ChooseNameRenameUseRegexp);
                    EditorGUI.indentLevel--;
                }
                EditorGUI.indentLevel--;
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
        }

        void HeaderChoises()
        {
            if (ChoiceList == null)
            {
                ChoiceList = new ReorderableList(new CountViewList(() => ChooseCount, (chooseCount) => ChooseCount = chooseCount), typeof(int), true, false, true, true);
                ChoiceList.drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    rect.height = EditorGUIUtility.singleLineHeight;
                    rect.y += 2;
                    rect.width -= EditorGUIUtility.standardVerticalSpacing * 2 + 20;
                    rect.width /= 2;
                    EditorGUIUtility.labelWidth = 50;
                    ChooseNames[index] = TextField(rect, $"{T.選択肢}{index}", ChooseName(index));
                    EditorGUIUtility.labelWidth = 0;
                    rect.x += rect.width + EditorGUIUtility.standardVerticalSpacing;
                    ChooseIcons[index] = TextureField(rect, ChooseIcon(index));
                    rect.x += rect.width + EditorGUIUtility.standardVerticalSpacing;
                    rect.width = 20;
                    if (GUI.Button(rect, "×", EditorStyles.miniButton))
                    {
                        RemoveChoice(index);
                    }
                };
                ChoiceList.onAddCallback = list =>
                {
                    WillChange();
                    ChooseCount++;
                    var index = ChooseCount - 1;
                    ChooseNames[index] = ChooseName(index);
                    ChooseIcons[index] = ChooseIcon(index);
                };
                ChoiceList.onRemoveCallback = list =>
                {
                    RemoveChoice(list.index);
                };
                ChoiceList.onReorderCallbackWithDetails = (list, oldIndex, newIndex) =>
                {
                    var direction = oldIndex < newIndex ? 1 : -1;
                    for (var i = oldIndex; i != newIndex; i = i + direction)
                    {
                        MoveChoice(i, i + direction);
                    }
                };
                ChoiceList.onCanRemoveCallback = list => ChooseCount > 1;
            }
            ChoiceList.DoLayoutList();
        }

        protected override bool HasHeaderBulkGUI => true;

        protected override float HeaderBulkGUIHeight(IList<string> children)
        {
            var allMaterials = GetAllMaterialSlots(children);
            var keyToMaterial = allMaterials.KeyToMaterial();
            var sourceMaterials = DistinctSourceMaterial(keyToMaterial);
            return (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * sourceMaterials.Count() * (ChooseCount + 2) + EditorGUIUtility.standardVerticalSpacing;
        }

        protected override void OnHeaderBulkGUI(IList<string> children)
        {
            var allMaterials = GetAllMaterialSlots(children);
            ShowChooseBulkMaterialControl(allMaterials);
        }

        protected override void OnMainGUI(IList<string> children)
        {
            var allMaterials = GetAllMaterialSlots(children);

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
                var parameters = gameObjectRef == null ? ChooseShaderParameters.Names(child).ToFakeShaderFloatParameters().Concat(ChooseShaderVectorParameters.Names(child).ToFakeShaderVectorParameters()).ToList() : ShaderParametersCache.GetFilteredShaderParameters(gameObjectRef);
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
                        ChooseShaderParameters.HasChild(child) || ChooseShaderVectorParameters.HasChild(child),
                        () => parameters.Select(p => new NameAndDescriptionItemContainer(p) as ListTreeViewItemContainer<string>).ToList(),
                        () => ChooseShaderParameters.Names(child).Concat(ChooseShaderVectorParameters.Names(child)).ToImmutableHashSet(),
                        name =>
                        {
                            if (parameters.Any(p => p.Name == name && p.IsFloatLike))
                            {
                                AddChooseBlendShape(ChooseShaderParameters, children, child, name);
                            }
                            else
                            {
                                AddChooseShaderVectorParameter(children, child, name);
                            }
                        },
                        name =>
                        {
                            if (parameters.Any(p => p.Name == name && p.IsFloatLike))
                            {
                                RemoveChooseBlendShape(ChooseShaderParameters, children, child, name);
                            }
                            else
                            {
                                RemoveChooseShaderVectorParameter(children, child, name);
                            }
                        }
                        ))
                {
                    EditorGUI.indentLevel++;
                    ShowChooseBlendShapeControl(false, children, child, ChooseShaderParameters, parameters, minValue: null, maxValue: null);
                    ShowChooseShaderVectorParameterControl(children, child, parameters);
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
            foreach (var value in ChooseShaderVectorParameters.Values)
            {
                value.SwapKey(from, to);
            }
            foreach (var value in ChooseValues.Values)
            {
                value.SwapKey(from, to);
            }
            foreach (var value in Positions.Values)
            {
                value.SwapKey(from, to);
            }
            foreach (var value in Rotations.Values)
            {
                value.SwapKey(from, to);
            }
            foreach (var value in Scales.Values)
            {
                value.SwapKey(from, to);
            }
        }

        void RemoveChoice(int index)
        {
            WillChange();
            ChooseCount--;
            RemoveAndReorderDictionary(ChooseNames, index);
            RemoveAndReorderDictionary(ChooseIcons, index);
            RemoveAndReorderHashSet(ChooseObjects, index);
            RemoveAndReorderValues<Material, IntMaterialDictionary>(ChooseMaterials, index);
            RemoveAndReorderValues<float, IntFloatDictionary>(ChooseBlendShapes, index);
            RemoveAndReorderValues<float, IntFloatDictionary>(ChooseShaderParameters, index);
            RemoveAndReorderValues<Vector4, IntVector4Dictionary>(ChooseShaderVectorParameters, index);
            RemoveAndReorderValues<value.Value, IntValueDictionary>(ChooseValues, index);
            RemoveAndReorderValues(Positions, index);
            RemoveAndReorderValues(Rotations, index);
            RemoveAndReorderValues(Scales, index);
        }

        void RemoveAndReorderDictionary<T>(Dictionary<int, T> dic, int index)
        {
            dic.Remove(index);
            foreach (var key in dic.Keys.Where(i => i > index).OrderBy(i => i).ToArray())
            {
                dic[key - 1] = dic[key];
                dic.Remove(key);
            }
        }

        void RemoveAndReorderHashSet(IntHashSetDictionary dic, int index)
        {
            foreach (var value in dic.Values)
            {
                value.Remove(index);
                foreach (var key in value.Where(i => i > index).OrderBy(i => i).ToArray())
                {
                    value.Add(key - 1);
                    value.Remove(key);
                }
            }
        }

        void RemoveAndReorderValues<V, D>(SerializedTwoTupleDictionary<string, int, D> dic, int index) where D : Dictionary<int, V>
        {
            RemoveAndReorderValues<(string, int), V, D>(dic, index);
        }

        void RemoveAndReorderValues<V, D>(SerializedTwoTupleDictionary<string, string, D> dic, int index) where D : Dictionary<int, V>
        {
            RemoveAndReorderValues<(string, string), V, D>(dic, index);
        }

        void RemoveAndReorderValues<V, D>(SerializedTwoTupleDictionary<string, TypeMember, D> dic, int index) where D : Dictionary<int, V>
        {
            RemoveAndReorderValues<(string, TypeMember), V, D>(dic, index);
        }

        void RemoveAndReorderValues(Dictionary<string, IntVector3Dictionary> dic, int index)
        {
            RemoveAndReorderValues<string, Vector3, IntVector3Dictionary>(dic, index);
        }

        void RemoveAndReorderValues<K, V, D>(Dictionary<K, D> dic, int index) where D : Dictionary<int, V>
        {
            foreach (var value in dic.Values)
            {
                RemoveAndReorderDictionary(value, index);
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
                        var rect = EditorGUILayout.GetControlRect(false, 0, GUILayout.Height(EditorGUIUtility.singleLineHeight));
                        DropAreaGUI(rect, T.マテリアルをドラッグ__and__ドロップ, (index, mat) =>
                        {
                            values[index] = mat as Material;
                            if (BulkSet)
                            {
                                SetBulkChooseMaterial(materials[i], index, mat as Material);
                            }
                        });
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

        IEnumerable<Material> DistinctSourceMaterial(Func<(string, int), Material> keyToMaterial)
        {
            return ChooseMaterials.Keys.Select(keyToMaterial).Where(m => m != null).Distinct();
        }

        void ShowChooseBulkMaterialControl(Dictionary<string, Material[]> allMaterials)
        {
            var keyToMaterial = allMaterials.KeyToMaterial();
            var sourceMaterials = DistinctSourceMaterial(keyToMaterial);
            var chooseMaterialGroups = ChooseMaterials.Keys.GroupBy(keyToMaterial).Where(m => m.Key != null).ToDictionary(group => group.Key, group => group.ToList());
            foreach (var sourceMaterial in sourceMaterials)
            {
                using (new EditorGUI.DisabledGroupScope(true))
                {
                    EditorGUILayout.ObjectField(sourceMaterial, typeof(Material), false);
                }
                EditorGUI.indentLevel++;
                var rect = EditorGUILayout.GetControlRect(false, 0, GUILayout.Height(EditorGUIUtility.singleLineHeight));
                DropAreaGUI(rect, T.マテリアルをドラッグ__and__ドロップ, (index, mat) =>
                {
                    SetBulkChooseMaterial(sourceMaterial, index, mat as Material);
                });
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

        void ShowChooseShaderVectorParameterControl(
            IList<string> children,
            string child,
            IEnumerable<INameAndDescription> names
            )
        {
            foreach (var name in names)
            {
                var key = (child, name.Name);
                IntVector4Dictionary values;
                if (ChooseShaderVectorParameters.TryGetValue(key, out values))
                {
                    if (EditorGUILayout.ToggleLeft(name.Description, true))
                    {
                        EditorGUI.indentLevel++;
                        for (var i = 0; i < ChooseCount; ++i)
                        {
                            var value = values.ContainsKey(i) ? values[i] : Vector4.zero;
                            EditorGUILayout.BeginHorizontal();
                            var newValue = EditorGUILayout.Vector4Field(ChooseName(i), value);
                            ShaderVectorParameterPickerButton(child, name.Name, ref newValue);
                            EditorGUILayout.EndHorizontal();

                            if (value != newValue)
                            {
                                WillChange();
                                values[i] = newValue;
                                if (BulkSet)
                                {
                                    BulkSetChooseShaderVectorParameter(name.Name, i, newValue);
                                }
                            }
                        }
                        EditorGUI.indentLevel--;
                    }
                    else
                    {
                        RemoveChooseShaderVectorParameter(children, child, name.Name);
                    }
                }
            }
        }

        void BulkSetChooseShaderVectorParameter(string toggleName, int choiseIndex, Vector4 choiceValue)
        {
            foreach (var (child, name) in ChooseShaderVectorParameters.Keys)
            {
                if (name == toggleName)
                {
                    ChooseShaderVectorParameters[(child, name)][choiseIndex] = choiceValue;
                }
            }
        }

        void AddChooseShaderVectorParameter(IList<string> children, string child, string name)
        {
            if (BulkSet)
            {
                foreach (var c in children)
                {
                    AddChooseShaderVectorParameterSingle(c, name);
                }
            }
            else
            {
                AddChooseShaderVectorParameterSingle(child, name);
            }
        }

        void AddChooseShaderVectorParameterSingle(string child, string name)
        {
            var key = (child, name);
            if (ChooseShaderVectorParameters.ContainsKey(key)) return;
            WillChange();
            ChooseShaderVectorParameters[key] = new IntVector4Dictionary();
        }

        void RemoveChooseShaderVectorParameter(IList<string> children, string child, string name)
        {
            if (BulkSet)
            {
                foreach (var c in children)
                {
                    RemoveChooseShaderVectorParameterSingle(c, name);
                }
            }
            else
            {
                RemoveChooseShaderVectorParameterSingle(child, name);
            }
        }

        void RemoveChooseShaderVectorParameterSingle(string child, string name)
        {
            var key = (child, name);
            if (!ChooseShaderVectorParameters.ContainsKey(key)) return;
            WillChange();
            ChooseShaderVectorParameters.Remove(key);
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
                            else if (member.MemberType == typeof(Quaternion))
                            {
                                var widemode = EditorGUIUtility.wideMode;
                                EditorGUIUtility.wideMode = true;
                                newValue = EditorGUILayout.Vector4Field(ChooseName(i), ((Quaternion)value).ToVector4()).ToQuaternion();
                                ValuePickerButton(child, member, p => newValue = p.quaternionValue);
                                EditorGUIUtility.wideMode = widemode;
                            }
                            else if (member.MemberType == typeof(Color))
                            {
                                var widemode = EditorGUIUtility.wideMode;
                                EditorGUIUtility.wideMode = true;
                                newValue = EditorGUILayout.ColorField(ChooseName(i), (Color)value);
                                ValuePickerButton(child, member, p => newValue = p.colorValue);
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

        UnityEngine.Object[] DropAreaGUI(Rect rect, string label)
        {
            var evt = Event.current;
            GUI.Box(rect, label);

            switch (evt.type)
            {
                case EventType.DragUpdated:
                    if (!rect.Contains(evt.mousePosition))
                        return null;
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    return null;
                case EventType.DragPerform:
                    if (!rect.Contains(evt.mousePosition))
                        return null;
                    DragAndDrop.AcceptDrag();
                    return DragAndDrop.objectReferences;
                default:
                    return null;
            }
        }

        void DropAreaGUI(Rect rect, string label, Action<int, UnityEngine.Object> onEachDrop)
        {
            var objects = DropAreaGUI(rect, label);
            if (objects != null)
            {
                WillChange();
                var len = Mathf.Min(objects.Length, ChooseCount);
                for (var i = 0; i < len; ++i)
                {
                    onEachDrop(i, objects[i]);
                }
            }
        }

        Dictionary<string, Material[]> GetAllMaterialSlots(IList<string> children) => children.ToDictionary(child => child, child => GetMaterialSlots(child));

        // with prefab shim
        protected override Material[] GetMaterialSlots(string child)
        {
            var go = GetGameObject(child);
            Material[] slots = null;
            if (go != null) slots = go.GetMaterialSlots();
            if (slots == null) slots = ChooseMaterials.MaterialSlots(child);
            return slots;
        }
#endif
    }
}
