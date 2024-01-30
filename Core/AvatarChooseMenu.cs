using System;
using System.Collections.Generic;
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
        public int ChooseDefaultValue;
        [SerializeField]
        public int ChooseCount = 2;
        [SerializeField]
        public IntStringDictionary ChooseNames = new IntStringDictionary();

#if UNITY_EDITOR

        public string ChooseName(int index)
        {
            if (ChooseNames.ContainsKey(index)) return ChooseNames[index];
            return $"選択肢{index}";
        }

        public override IEnumerable<string> GetStoredChildren() => ChooseObjects.Keys.Concat(ChooseMaterials.Keys.Select(key => key.Item1)).Concat(ChooseBlendShapes.Keys.Select(key => key.Item1)).Concat(ChooseShaderParameters.Keys.Select(key => key.Item1)).Distinct();

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
        }

        protected override bool IsSuitableForTransition() => ChooseBlendShapes.Count > 0 || ChooseShaderParameters.Count > 0;

        protected override void OnHeaderGUI(IList<string> children)
        {
            ShowTransitionSeconds();

            ChooseDefaultValue = IntField("パラメーター初期値", ChooseDefaultValue);
            ShowSaved();

            ChooseCount = IntField("選択肢の数", ChooseCount);

            if (ChooseDefaultValue < 0 && ChooseCount > 0) ChooseDefaultValue = 0;
            if (ChooseDefaultValue >= ChooseCount) ChooseDefaultValue = ChooseCount - 1;

            EditorGUI.indentLevel++;
            for (var i = 0; i < ChooseCount; ++i)
            {
                ChooseNames[i] = TextField($"選択肢{i}", ChooseName(i));
            }
            EditorGUI.indentLevel--;

            var allMaterials = children.ToDictionary(child => child, child => Util.GetMaterialSlots(GetGameObject(child)));

            if (BulkSet)
            {
                ShowChooseBulkMaterialControl(allMaterials);
            }
        }

        protected override void OnMainGUI(IList<string> children)
        {
            var allMaterials = children.ToDictionary(child => child, child => Util.GetMaterialSlots(GetGameObject(child)));

            foreach (var child in children)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField(child);
                EditorGUI.indentLevel++;
                if (FoldoutGameObjectHeader(child, "GameObject"))
                {
                    EditorGUI.indentLevel++;
                    ShowChooseObjectControl(child);
                    EditorGUI.indentLevel--;
                }

                var materials = allMaterials[child];
                if (materials.Length > 0 && FoldoutMaterialHeader(child, "Materials"))
                {
                    EditorGUI.indentLevel++;
                    ShowChooseMaterialControl(child, materials);
                    EditorGUI.indentLevel--;
                }

                var gameObjectRef = GetGameObject(child);
                var names = Util.GetBlendShapeNames(gameObjectRef);
                var parameters = ShaderParametersCache.GetFilteredShaderParameters(gameObjectRef);
                if (names.Count > 0 && FoldoutBlendShapeHeader(child, "BlendShapes"))
                {
                    EditorGUI.indentLevel++;
                    ShowChooseBlendShapeControl(child, ChooseBlendShapes, names.ToNames());
                    EditorGUI.indentLevel--;
                }
                if (parameters.Count > 0 && FoldoutShaderParameterHeader(child, "Shader Parameters"))
                {
                    EditorGUI.indentLevel++;
                    ShowChooseBlendShapeControl(child, ChooseShaderParameters, parameters, minValue: null, maxValue: null);
                    EditorGUI.indentLevel--;
                }
                EditorGUI.indentLevel--;
            }
        }

        void ShowChooseObjectControl(string child)
        {
            IntHashSet indexes;
            if (!ChooseObjects.TryGetValue(child, out indexes))
            {
                indexes = new IntHashSet();
            }
            var changed = false;
            var isEmpty = indexes.Count == 0;
            if (EditorGUILayout.ToggleLeft($"制御しない", isEmpty) && !isEmpty)
            {
                indexes = new IntHashSet();
                changed = true;
            }
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

        void ShowChooseMaterialControl(string child, Material[] materials)
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
                            var newValue = EditorGUILayout.ObjectField(ChooseName(j), value, typeof(Material), false) as Material;
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
                        WillChange();
                        ChooseMaterials.Remove(key);
                    }
                }
                else
                {
                    if (ShowChooseMaterialToggle(i, materials[i], false))
                    {
                        WillChange();
                        ChooseMaterials[key] = new IntMaterialDictionary();
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
                var materials = Util.GetMaterialSlots(GetGameObject(child));
                if (index < materials.Length && materials[index] == sourceMaterial)
                {
                    var values = ChooseMaterials[(child, index)];
                    values[choiseIndex] = choiceMaterial;
                }
            }
        }

        void ShowChooseBulkMaterialControl(Dictionary<string, Material[]> allMaterials)
        {
            Func<(string, int), Material> keyToMaterial = ((string, int) key) => allMaterials.TryGetValue(key.Item1, out var m) && m != null && m.Length > key.Item2 ? m[key.Item2] : null;
            var sourceMaterials = ChooseMaterials.Keys.Select(keyToMaterial).Distinct();
            var chooseMaterialGroups = ChooseMaterials.Keys.GroupBy(keyToMaterial).ToDictionary(group => group.Key, group => group.ToList());
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
                        EditorGUILayout.HelpBox("複数のマテリアルが選択されています", MessageType.Warning);
                    }
                }
                EditorGUI.indentLevel--;
            }
        }

        void ShowChooseBlendShapeControl(
            string child,
            ChooseBlendShapeDictionary choices,
            IEnumerable<Util.INameAndDescription> names,
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
                            var newValue = EditorGUILayout.FloatField(ChooseName(i), value);

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
                        WillChange();
                        choices.Remove(key);
                    }
                }
                else
                {
                    if (EditorGUILayout.ToggleLeft(name.Description, false))
                    {
                        WillChange();
                        choices[key] = new IntFloatDictionary();
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
#endif
    }
}
