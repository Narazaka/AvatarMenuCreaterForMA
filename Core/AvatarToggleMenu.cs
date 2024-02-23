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
        public ToggleUsingDictionary ToggleObjectUsings = new ToggleUsingDictionary();
        [SerializeField]
        public ToggleMaterialDictionary ToggleMaterials = new ToggleMaterialDictionary();
        [SerializeField]
        public ToggleBlendShapeDictionary ToggleBlendShapes = new ToggleBlendShapeDictionary();
        [SerializeField]
        public ToggleBlendShapeDictionary ToggleShaderParameters = new ToggleBlendShapeDictionary();
        [SerializeField]
        public bool ToggleDefaultValue;
        [SerializeField]
        public Texture2D ToggleIcon;

#if UNITY_EDITOR

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
                        if (EditorUtility.DisplayDialog("高度な設定の削除", "高度な設定がリセットされます\n本当に高度な設定を無効にしますか？", "OK", "Cancel"))
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
                                ToggleBlendShapes[key] = ToggleBlendShapes[key].ResetAdvanced();
                            }
                            foreach (var key in ToggleShaderParameters.Keys.ToList())
                            {
                                ToggleShaderParameters[key] = ToggleShaderParameters[key].ResetAdvanced();
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
        bool HasAdvanced => ToggleObjectUsings.Count > 0 || ToggleMaterials.Any(t => t.Value.HasAdvanced) || ToggleBlendShapes.Any(t => t.Value.HasAdvanced) || ToggleShaderParameters.Any(t => t.Value.HasAdvanced);

        public override IEnumerable<string> GetStoredChildren() => ToggleObjects.Keys.Concat(ToggleMaterials.Keys.Select(k => k.Item1)).Concat(ToggleBlendShapes.Keys.Select(k => k.Item1)).Concat(ToggleShaderParameters.Keys.Select(k => k.Item1)).Distinct();
        public override void ReplaceStoredChild(string oldChild, string newChild)
        {
            if (ToggleObjects.ContainsKey(oldChild) || ToggleMaterials.ContainsPrimaryKey(oldChild) || ToggleBlendShapes.ContainsPrimaryKey(oldChild) || ToggleShaderParameters.ContainsPrimaryKey(oldChild))
            {
                WillChange();
                ToggleObjects.ReplaceKey(oldChild, newChild);
                ToggleMaterials.ReplacePrimaryKey(oldChild, newChild);
                ToggleBlendShapes.ReplacePrimaryKey(oldChild, newChild);
                ToggleShaderParameters.ReplacePrimaryKey(oldChild, newChild);
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
        }
        public override void RemoveStoredChild(string child)
        {
            WillChange();
            ToggleObjects.Remove(child);
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
        }
        protected override bool IsSuitableForTransition() => ToggleBlendShapes.Count > 0 || ToggleShaderParameters.Count > 0;

        protected override void OnHeaderGUI(IList<string> children)
        {
            ToggleIcon = TextureField("アイコン", ToggleIcon);
            ToggleDefaultValue = Toggle("パラメーター初期値", ToggleDefaultValue);
            ShowSaved();
            ShowDetailMenu();
            UseAdvanced = EditorGUILayout.Toggle("高度な設定", UseAdvanced);
            if (UseAdvanced)
            {
                EditorGUILayout.HelpBox("高度な設定を有効にすると、ONまたはOFF片方だけ制御する設定ができます。", MessageType.Info);
            }

            EditorGUILayout.Space();

            ShowTransitionSeconds();

            var allMaterials = children.ToDictionary(child => child, child => GetMaterialSlots(child));

            if (BulkSet)
            {
                if (FoldoutHeader("", "一括設定", true))
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
                var parameters = gameObjectRef == null ? ToggleShaderParameters.Names(child).ToFakeShaderParameters().ToList() : ShaderParametersCache.GetFilteredShaderParameters(gameObjectRef);

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
                    ShowToggleBlendShapeControl(children, child, ToggleBlendShapes, names.ToNames());
                    EditorGUI.indentLevel--;
                }
                if (parameters.Count > 0 &&
                    FoldoutHeaderWithAddItemButton(
                        child,
                        "Shader Parameters",
                        ToggleShaderParameters.HasChild(child),
                        () => parameters.Select(p => new NameAndDescriptionItemContainer(p) as ListTreeViewItemContainer<string>).ToList(),
                        () => ToggleShaderParameters.Names(child).ToImmutableHashSet(),
                        (name) => AddToggleBlendShape(ToggleShaderParameters, children, child, name, 1),
                        (name) => RemoveToggleBlendShape(ToggleShaderParameters, children, child, name)
                        ))
                {
                    EditorGUI.indentLevel++;
                    ShowToggleBlendShapeControl(children, child, ToggleShaderParameters, parameters, minValue: null, maxValue: null);
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
                    if (EditorGUILayout.Toggle("両方制御", use == ToggleUsing.Both) && use != ToggleUsing.Both) newUse = ToggleUsing.Both;
                    EditorGUIUtility.labelWidth = 90;
                    if (EditorGUILayout.Toggle("ONのみ制御", use == ToggleUsing.ON) && use != ToggleUsing.ON) newUse = ToggleUsing.ON;
                    if (EditorGUILayout.Toggle("OFFのみ制御", use == ToggleUsing.OFF) && use != ToggleUsing.OFF) newUse = ToggleUsing.OFF;
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
                            newValue.Active = EditorGUILayout.ObjectField("ON", value.Active, typeof(Material), false) as Material;
                            EditorGUIUtility.labelWidth = 0;
                        }
                        if (TransitionSeconds > 0)
                        {
                            EditorGUIUtility.labelWidth = 110;
                            newValue.TransitionOffsetPercent = EditorGUILayout.FloatField("変化待機%", value.TransitionOffsetPercent, GUILayout.Width(140));
                            EditorGUIUtility.labelWidth = 0;
                        }
                        if (UseAdvanced)
                        {
                            using (new EditorGUILayout.HorizontalScope())
                            {
                                newValue.UseActive = EditorGUILayout.Toggle("ONのみ制御", value.UseActive);
                                newValue.UseInactive = EditorGUILayout.Toggle("OFFのみ制御", value.UseInactive);
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
                    newValue.TransitionOffsetPercent = EditorGUILayout.FloatField("変化待機%", value.TransitionOffsetPercent, GUILayout.Width(140));
                    EditorGUIUtility.labelWidth = 0;
                }
                if (UseAdvanced)
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        newValue.UseActive = EditorGUILayout.Toggle("ONを制御", value.UseActive);
                        newValue.UseInactive = EditorGUILayout.Toggle("OFFを制御", value.UseInactive);
                    }
                }
                if (!value.Equals(newValue))
                {
                    WillChange();
                    BulkSetToggleMaterial(sourceMaterial, newValue, value.ChangedProp(newValue));
                }
                if (inactiveMaterials.Count != 1)
                {
                    EditorGUILayout.HelpBox("OFFに複数のマテリアルが選択されています", MessageType.Warning);
                }
                if (activeMaterials.Count != 1)
                {
                    EditorGUILayout.HelpBox("ONに複数のマテリアルが選択されています", MessageType.Warning);
                }
                if (TransitionSeconds > 0)
                {
                    if (transitionOffsetPercents.Count != 1)
                    {
                        EditorGUILayout.HelpBox("複数の変化待機%が設定されています", MessageType.Warning);
                    }
                }
                if (UseAdvanced)
                {
                    if (useActives.Count != 1)
                    {
                        EditorGUILayout.HelpBox("ONを制御に複数の設定がされています", MessageType.Warning);
                    }
                    if (useInactives.Count != 1)
                    {
                        EditorGUILayout.HelpBox("OFFを制御に複数の設定がされています", MessageType.Warning);
                    }
                }
                EditorGUI.indentLevel--;
            }
        }

        void ShowToggleBlendShapeControl(
            IList<string> children,
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
                            // 過去互換性
                            if (newValue.TransitionDurationPercent <= 0)
                            {
                                EditorGUILayout.HelpBox("変化時間%は0より大きく設定して下さい", MessageType.Error);
                            }
                        }
                        if (UseAdvanced)
                        {
                            using (new EditorGUILayout.HorizontalScope())
                            {
                                EditorGUI.indentLevel++;
                                newValue.UseActive = EditorGUILayout.Toggle("ONを制御", value.UseActive);
                                newValue.UseInactive = EditorGUILayout.Toggle("OFFを制御", value.UseInactive);
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
                            if (newValue.TransitionDurationPercent <= 0) newValue.TransitionDurationPercent = 1;
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
                        RemoveToggleBlendShape(toggles, children, child, name.Name);
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

        // with prefab shim
        Material[] GetMaterialSlots(string child) => GetGameObject(child)?.GetMaterialSlots() ?? ToggleMaterials.MaterialSlots(child);
#endif
    }
}
