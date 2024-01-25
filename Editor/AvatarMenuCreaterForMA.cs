using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using VRC.Core;
using nadena.dev.modular_avatar.core;
using System;

namespace net.narazaka.avatarmenucreater
{
    enum IncludeAssetType
    {
        [InspectorName("全て個別に保存")]
        Extract,
        [InspectorName("prefabとanimator")]
        AnimatorAndInclude,
        [InspectorName("全てprefabに含める")]
        Include,
    }

    enum MenuType
    {
        [InspectorName("ON／OFF")]
        Toggle,
        [InspectorName("選択式")]
        Choose,
        [InspectorName("無段階制御")]
        Slider,
    }

    enum ToggleType
    {
        [InspectorName("制御しない")]
        None,
        [InspectorName("ON")]
        ON,
        [InspectorName("OFF")]
        OFF,
    }

    struct ToggleBlendShape : System.IEquatable<ToggleBlendShape>
    {
        public float Inactive;
        public float Active;
        public float TransitionOffsetPercent;
        public float TransitionDurationPercent;

        public bool Equals(ToggleBlendShape other)
        {
            return Inactive == other.Inactive && Active == other.Active && TransitionOffsetPercent == other.TransitionOffsetPercent && TransitionDurationPercent == other.TransitionDurationPercent;
        }

        public string ChangedProp(ToggleBlendShape other)
        {
            if (Inactive != other.Inactive) return nameof(Inactive);
            if (Active != other.Active) return nameof(Active);
            if (TransitionOffsetPercent != other.TransitionOffsetPercent) return nameof(TransitionOffsetPercent);
            if (TransitionDurationPercent != other.TransitionDurationPercent) return nameof(TransitionDurationPercent);
            return "";
        }

        public float GetProp(string name)
        {
            if (name == nameof(Inactive)) return Inactive;
            if (name == nameof(Active)) return Active;
            if (name == nameof(TransitionOffsetPercent)) return TransitionOffsetPercent;
            if (name == nameof(TransitionDurationPercent)) return TransitionDurationPercent;
            return 0;
        }

        public ToggleBlendShape SetProp(string name, float value)
        {
            if (name == nameof(Inactive)) Inactive = value;
            if (name == nameof(Active)) Active = value;
            if (name == nameof(TransitionOffsetPercent)) TransitionOffsetPercent = value;
            if (name == nameof(TransitionDurationPercent)) TransitionDurationPercent = value;
            return this;
        }

        public float TransitionOffsetRate { get => TransitionOffsetPercent / 100f; }
        public float TransitionDurationRate { get => TransitionDurationPercent / 100f; }
        public float ActivateStartRate { get => TransitionOffsetRate; }
        public float ActivateEndRate { get => TransitionOffsetRate + TransitionDurationRate; }
        public float InactivateStartRate { get => 1f - ActivateEndRate; }
        public float InactivateEndRate { get => 1f - ActivateStartRate; }
    }

    struct RadialBlendShape : System.IEquatable<RadialBlendShape>
    {
        public float Start;
        public float End;

        public bool Equals(RadialBlendShape other)
        {
            return Start == other.Start && End == other.End;
        }

        public string ChangedProp(RadialBlendShape other)
        {
            if (Start != other.Start) return nameof(Start);
            if (End != other.End) return nameof(End);
            return "";
        }

        public float GetProp(string name)
        {
            if (name == nameof(Start)) return Start;
            if (name == nameof(End)) return End;
            return 0;
        }

        public RadialBlendShape SetProp(string name, float value)
        {
            if (name == nameof(Start)) Start = value;
            if (name == nameof(End)) End = value;
            return this;
        }
    }

    public class AvatarMenuCreaterForMA : EditorWindow
    {
        VRCAvatarDescriptor VRCAvatarDescriptor;
        MenuType MenuType = MenuType.Toggle;
        IncludeAssetType IncludeAssetType = IncludeAssetType.AnimatorAndInclude;
        Dictionary<GameObject, ToggleType> ToggleObjects = new Dictionary<GameObject, ToggleType>();
        Dictionary<(GameObject, string), ToggleBlendShape> ToggleBlendShapes = new Dictionary<(GameObject, string), ToggleBlendShape>();
        Dictionary<(GameObject, string), ToggleBlendShape> ToggleShaderParameters = new Dictionary<(GameObject, string), ToggleBlendShape>();
        Dictionary<GameObject, HashSet<int>> ChooseObjects = new Dictionary<GameObject, HashSet<int>>();
        Dictionary<(GameObject, int), Dictionary<int, Material>> ChooseMaterials = new Dictionary<(GameObject, int), Dictionary<int, Material>>();
        Dictionary<(GameObject, string), Dictionary<int, float>> ChooseBlendShapes = new Dictionary<(GameObject, string), Dictionary<int, float>>();
        Dictionary<(GameObject, string), Dictionary<int, float>> ChooseShaderParameters = new Dictionary<(GameObject, string), Dictionary<int, float>>();
        Dictionary<(GameObject, string), RadialBlendShape> RadialBlendShapes = new Dictionary<(GameObject, string), RadialBlendShape>();
        Dictionary<(GameObject, string), RadialBlendShape> RadialShaderParameters = new Dictionary<(GameObject, string), RadialBlendShape>();
        float RadialDefaultValue;
        bool RadialInactiveRange;
        float RadialInactiveRangeMin = float.NaN;
        float RadialInactiveRangeMax = float.NaN;
        float TransitionSeconds;
        int ChooseCount = 2;
        Dictionary<int, string> ChooseNames = new Dictionary<int, string>();
        string ChooseName(int index)
        {
            if (ChooseNames.ContainsKey(index)) return ChooseNames[index];
            return $"選択肢{index}";
        }

        bool BulkSet;

        HashSet<GameObject> Foldouts = new HashSet<GameObject>();
        HashSet<GameObject> FoldoutGameObjects = new HashSet<GameObject>();
        HashSet<GameObject> FoldoutMaterials = new HashSet<GameObject>();
        HashSet<GameObject> FoldoutBlendShapes = new HashSet<GameObject>();
        HashSet<GameObject> FoldoutShaderParameters = new HashSet<GameObject>();
        Vector2 ScrollPosition;

        Util.ShaderParametersCache ShaderParametersCache = new Util.ShaderParametersCache();

        string SaveFolder = "Assets";

        [MenuItem("Tools/Modular Avatar/AvatarMenuCreater for Modular Avatar")]
        static void CreateWindow()
        {
            GetWindow<AvatarMenuCreaterForMA>("AvatarMenuCreater for Modular Avatar");
        }

        void Update()
        {
            Repaint();
        }

        void OnGUI()
        {
            VRCAvatarDescriptor = EditorGUILayout.ObjectField("Avatar", VRCAvatarDescriptor, typeof(VRCAvatarDescriptor), true) as VRCAvatarDescriptor;

            if (VRCAvatarDescriptor == null)
            {
                EditorGUILayout.LabelField("対象のアバターを選択して下さい");
                return;
            }

            var gameObjects = Selection.gameObjects;

            if (gameObjects.Length == 0 || (gameObjects.Length == 1 && gameObjects[0] == VRCAvatarDescriptor.gameObject))
            {
                EditorGUILayout.LabelField("対象のオブジェクトを選択して下さい");
                return;
            }

            MenuType = (MenuType)EditorGUILayout.EnumPopup(MenuType);
            BulkSet = EditorGUILayout.ToggleLeft("同名パラメーターや同マテリアルスロットを一括設定", BulkSet);

            if (MenuType == MenuType.Toggle)
            {
                using (var scrollView = new EditorGUILayout.ScrollViewScope(ScrollPosition))
                {
                    ScrollPosition = scrollView.scrollPosition;

                    TransitionSeconds = EditorGUILayout.FloatField("徐々に変化（秒数）", TransitionSeconds);
                    if (TransitionSeconds < 0) TransitionSeconds = 0;
                    if (TransitionSeconds > 0)
                    {
                        if (ToggleBlendShapes.Count == 0 && ToggleShaderParameters.Count == 0)
                        {
                            EditorGUILayout.HelpBox("徐々に変化するものの指定が有りません", MessageType.Warning);
                        }
                        else
                        {
                            EditorGUILayout.HelpBox("指定時間かけて変化します", MessageType.Info);
                        }
                    }

                    foreach (var gameObject in gameObjects)
                    {
                        EditorGUILayout.Space();
                        EditorGUILayout.LabelField(Util.ChildPath(VRCAvatarDescriptor.gameObject, gameObject));
                        EditorGUI.indentLevel++;
                        ShowToggleObjectControl(gameObject);

                        var names = Util.GetBlendShapeNames(gameObject);
                        var parameters = ShaderParametersCache.GetFilteredShaderParameters(gameObject);
                        if (names.Count > 0 && FoldoutBlendShapeHeader(gameObject, "BlendShapes"))
                        {
                            EditorGUI.indentLevel++;
                            ShowToggleBlendShapeControl(gameObject, ToggleBlendShapes, names.ToNames());
                            EditorGUI.indentLevel--;
                        }
                        if (parameters.Count > 0 && FoldoutShaderParameterHeader(gameObject, "Shader Parameters"))
                        {
                            EditorGUI.indentLevel++;
                            ShowToggleBlendShapeControl(gameObject, ToggleShaderParameters, parameters, 1, minValue: null, maxValue: null);
                            EditorGUI.indentLevel--;
                        }
                        EditorGUI.indentLevel--;
                    }
                }
            }
            else if (MenuType == MenuType.Choose)
            {
                using (var scrollView = new EditorGUILayout.ScrollViewScope(ScrollPosition))
                {
                    ScrollPosition = scrollView.scrollPosition;

                    ChooseCount = EditorGUILayout.IntField("選択肢の数", ChooseCount);
                    EditorGUI.indentLevel++;
                    for (var i = 0; i < ChooseCount; ++i)
                    {
                        ChooseNames[i] = EditorGUILayout.TextField($"選択肢{i}", ChooseName(i));
                    }
                    EditorGUI.indentLevel--;
                    TransitionSeconds = EditorGUILayout.FloatField("徐々に変化（秒数）", TransitionSeconds);
                    if (TransitionSeconds < 0) TransitionSeconds = 0;
                    if (TransitionSeconds > 0)
                    {
                        if (ChooseBlendShapes.Count == 0 && ChooseShaderParameters.Count == 0)
                        {
                            EditorGUILayout.HelpBox("徐々に変化するものの指定が有りません", MessageType.Warning);
                        }
                        else
                        {
                            EditorGUILayout.HelpBox("指定時間かけて変化します", MessageType.Info);
                        }
                    }

                    var allMaterials = gameObjects.ToDictionary(gameObject => gameObject, gameObject => Util.GetMaterialSlots(gameObject));

                    if (BulkSet)
                    {
                        ShowChooseBulkMaterialControl(allMaterials);
                    }

                    foreach (var gameObject in gameObjects)
                    {
                        EditorGUILayout.Space();
                        EditorGUILayout.LabelField(Util.ChildPath(VRCAvatarDescriptor.gameObject, gameObject));
                        EditorGUI.indentLevel++;
                        if (FoldoutGameObjectHeader(gameObject, "GameObject"))
                        {
                            EditorGUI.indentLevel++;
                            ShowChooseObjectControl(gameObject);
                            EditorGUI.indentLevel--;
                        }

                        var materials = allMaterials[gameObject];
                        if (materials.Length > 0 && FoldoutMaterialHeader(gameObject, "Materials"))
                        {
                            EditorGUI.indentLevel++;
                            ShowChooseMaterialControl(gameObject, materials);
                            EditorGUI.indentLevel--;
                        }

                        var names = Util.GetBlendShapeNames(gameObject);
                        var parameters = ShaderParametersCache.GetFilteredShaderParameters(gameObject);
                        if (names.Count > 0 && FoldoutBlendShapeHeader(gameObject, "BlendShapes"))
                        {
                            EditorGUI.indentLevel++;
                            ShowChooseBlendShapeControl(gameObject, ChooseBlendShapes, names.ToNames());
                            EditorGUI.indentLevel--;
                        }
                        if (parameters.Count > 0 && FoldoutShaderParameterHeader(gameObject, "Shader Parameters"))
                        {
                            EditorGUI.indentLevel++;
                            ShowChooseBlendShapeControl(gameObject, ChooseShaderParameters, parameters, minValue: null, maxValue: null);
                            EditorGUI.indentLevel--;
                        }
                        EditorGUI.indentLevel--;
                    }
                }
            }
            else
            {
                RadialDefaultValue = EditorGUILayout.FloatField("パラメーター初期値", RadialDefaultValue);
                if (RadialInactiveRange)
                {
                    if (!EditorGUILayout.Toggle("無効領域を設定", true))
                    {
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
                                    RadialInactiveRangeMin = 0.49f;
                                }
                            }
                            else
                            {
                                var active = EditorGUILayout.ToggleLeft("これより大きい場合", true);
                                if (active)
                                {
                                    RadialInactiveRangeMin = EditorGUILayout.FloatField(RadialInactiveRangeMin);
                                }
                                else
                                {
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
                                    RadialInactiveRangeMax = 0.01f;
                                }
                            }
                            else
                            {
                                var active = EditorGUILayout.ToggleLeft("これより小さい場合", true);
                                if (active)
                                {
                                    RadialInactiveRangeMax = EditorGUILayout.FloatField(RadialInactiveRangeMax);
                                }
                                else
                                {
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
                        RadialInactiveRange = true;
                    }
                }
                using (var scrollView = new EditorGUILayout.ScrollViewScope(ScrollPosition))
                {
                    ScrollPosition = scrollView.scrollPosition;

                    if (RadialDefaultValue < 0) RadialDefaultValue = 0;
                    if (RadialDefaultValue > 1) RadialDefaultValue = 1;
                    foreach (var gameObject in gameObjects)
                    {
                        EditorGUILayout.Space();
                        var names = Util.GetBlendShapeNames(gameObject);
                        var parameters = ShaderParametersCache.GetFilteredShaderParameters(gameObject);
                        var path = Util.ChildPath(VRCAvatarDescriptor.gameObject, gameObject);
                        if (names.Count > 0 || parameters.Count > 0)
                        {
                            EditorGUILayout.LabelField(path);
                            EditorGUI.indentLevel++;
                            if (names.Count > 0 && FoldoutBlendShapeHeader(gameObject, "BlendShapes"))
                            {
                                EditorGUI.indentLevel++;
                                ShowRadialBlendShapeControl(gameObject, RadialBlendShapes, names.ToNames());
                                EditorGUI.indentLevel--;
                            }
                            if (parameters.Count > 0 && FoldoutShaderParameterHeader(gameObject, "Shader Parameters"))
                            {
                                EditorGUI.indentLevel++;
                                ShowRadialBlendShapeControl(gameObject, RadialShaderParameters, parameters, 1, minValue: null, maxValue: null);
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
            }

            IncludeAssetType = (IncludeAssetType)EditorGUILayout.EnumPopup("保存形式", IncludeAssetType);
            if (GUILayout.Button("Create!"))
            {
                var basePath = EditorUtility.SaveFilePanelInProject("保存場所", "New Menu", "prefab", "アセットの保存場所", SaveFolder);
                if (string.IsNullOrEmpty(basePath)) return;
                basePath = new System.Text.RegularExpressions.Regex(@"\.prefab").Replace(basePath, "");
                var baseName = System.IO.Path.GetFileNameWithoutExtension(basePath);
                if (MenuType == MenuType.Toggle)
                {
                    CreateToggleAssets(baseName, basePath, gameObjects);
                }
                else if (MenuType == MenuType.Choose)
                {
                    CreateChooseAssets(baseName, basePath, gameObjects);
                }
                else
                {
                    CreateRadialAssets(baseName, basePath, gameObjects);
                }
                SaveFolder = System.IO.Path.GetDirectoryName(basePath);
            }
        }

        bool FoldoutHeader(GameObject gameObject, string title)
        {
            var foldout = Foldouts.Contains(gameObject);
            var newFoldout = EditorGUILayout.Foldout(foldout, title);
            if (newFoldout != foldout)
            {
                if (newFoldout)
                {
                    Foldouts.Add(gameObject);
                }
                else
                {
                    Foldouts.Remove(gameObject);
                }
            }
            return newFoldout;
        }

        bool FoldoutGameObjectHeader(GameObject gameObject, string title)
        {
            var foldout = FoldoutGameObjects.Contains(gameObject);
            var newFoldout = EditorGUILayout.Foldout(foldout, title);
            if (newFoldout != foldout)
            {
                if (newFoldout)
                {
                    FoldoutGameObjects.Add(gameObject);
                }
                else
                {
                    FoldoutGameObjects.Remove(gameObject);
                }
            }
            return newFoldout;
        }

        bool FoldoutMaterialHeader(GameObject gameObject, string title)
        {
            var foldout = FoldoutMaterials.Contains(gameObject);
            var newFoldout = EditorGUILayout.Foldout(foldout, title);
            if (newFoldout != foldout)
            {
                if (newFoldout)
                {
                    FoldoutMaterials.Add(gameObject);
                }
                else
                {
                    FoldoutMaterials.Remove(gameObject);
                }
            }
            return newFoldout;
        }

        bool FoldoutBlendShapeHeader(GameObject gameObject, string title)
        {
            var foldout = FoldoutBlendShapes.Contains(gameObject);
            var newFoldout = EditorGUILayout.Foldout(foldout, title);
            if (newFoldout != foldout)
            {
                if (newFoldout)
                {
                    FoldoutBlendShapes.Add(gameObject);
                }
                else
                {
                    FoldoutBlendShapes.Remove(gameObject);
                }
            }
            return newFoldout;
        }

        bool FoldoutShaderParameterHeader(GameObject gameObject, string title)
        {
            var foldout = FoldoutShaderParameters.Contains(gameObject);
            var newFoldout = EditorGUILayout.Foldout(foldout, title);
            if (newFoldout != foldout)
            {
                if (newFoldout)
                {
                    FoldoutShaderParameters.Add(gameObject);
                }
                else
                {
                    FoldoutShaderParameters.Remove(gameObject);
                }
            }
            return newFoldout;
        }

        void ShowToggleObjectControl(GameObject gameObject)
        {
            ToggleType type;
            if (!ToggleObjects.TryGetValue(gameObject, out type))
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
                if (newType == ToggleType.None)
                {
                    ToggleObjects.Remove(gameObject);
                }
                else
                {
                    ToggleObjects[gameObject] = newType;
                }
            }

        }

        void ShowToggleBlendShapeControl(
            GameObject gameObject,
            Dictionary<(GameObject, string), ToggleBlendShape> toggles,
            IEnumerable<Util.INameAndDescription> names,
            float defaultActiveValue = 100,
            float? minValue = 0,
            float? maxValue = 100
            )
        {
            foreach (var name in names)
            {
                var key = (gameObject, name.Name);
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
                        toggles.Remove(key);
                    }
                }
                else
                {
                    if (EditorGUILayout.ToggleLeft(name.Description, false))
                    {
                        toggles[key] = new ToggleBlendShape { Inactive = 0, Active = defaultActiveValue, TransitionDurationPercent = 100 };
                    }
                }
            }
        }

        void BulkSetToggleBlendShape(Dictionary<(GameObject, string), ToggleBlendShape> toggles, string toggleName, ToggleBlendShape toggleBlendShape, string changedProp)
        {
            var matches = new List<(GameObject, string)>();
            foreach (var (gameObject, name) in toggles.Keys)
            {
                if (name == toggleName)
                {
                    matches.Add((gameObject, name));
                }
            }
            foreach (var key in matches)
            {
                toggles[key] = toggles[key].SetProp(changedProp, toggleBlendShape.GetProp(changedProp));
            }
        }

        void ShowChooseObjectControl(GameObject gameObject)
        {
            HashSet<int> indexes;
            if (!ChooseObjects.TryGetValue(gameObject, out indexes))
            {
                indexes = new HashSet<int>();
            }
            var changed = false;
            var isEmpty = indexes.Count == 0;
            if (EditorGUILayout.ToggleLeft($"制御しない", isEmpty) && !isEmpty)
            {
                indexes = new HashSet<int>();
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
                if (indexes.Count == 0)
                {
                    ChooseObjects.Remove(gameObject);
                }
                else
                {
                    ChooseObjects[gameObject] = indexes;
                }
            }
        }

        void ShowChooseMaterialControl(GameObject gameObject, Material[] materials)
        {
            for (var i = 0; i < materials.Length; ++i)
            {
                var key = (gameObject, i);
                Dictionary<int, Material> values;
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
                        ChooseMaterials.Remove(key);
                    }
                }
                else
                {
                    if (ShowChooseMaterialToggle(i, materials[i], false))
                    {
                        ChooseMaterials[key] = new Dictionary<int, Material>();
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
            foreach (var (gameObject, index) in ChooseMaterials.Keys)
            {
                var materials = Util.GetMaterialSlots(gameObject);
                if (index < materials.Length && materials[index] == sourceMaterial)
                {
                    var values = ChooseMaterials[(gameObject, index)];
                    values[choiseIndex] = choiceMaterial;
                }
            }
        }

        void ShowChooseBulkMaterialControl(Dictionary<GameObject, Material[]> allMaterials)
        {
            Func<(GameObject, int), Material> keyToMaterial = ((GameObject, int) key) => allMaterials.TryGetValue(key.Item1, out var m) && m != null && m.Length > key.Item2 ? m[key.Item2] : null;
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
            GameObject gameObject,
            Dictionary<(GameObject, string), Dictionary<int, float>> choices,
            IEnumerable<Util.INameAndDescription> names,
            float? minValue = 0,
            float? maxValue = 100
            )
        {
            foreach (var name in names)
            {
                var key = (gameObject, name.Name);
                Dictionary<int, float> values;
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
                        choices.Remove(key);
                    }
                }
                else
                {
                    if (EditorGUILayout.ToggleLeft(name.Description, false))
                    {
                        choices[key] = new Dictionary<int, float>();
                    }
                }
            }
        }

        void BulkSetChooseBlendShape(Dictionary<(GameObject, string), Dictionary<int, float>> choices, string toggleName, int choiseIndex, float choiceValue)
        {
            foreach (var (gameObject, name) in choices.Keys)
            {
                if (name == toggleName)
                {
                    choices[(gameObject, name)][choiseIndex] = choiceValue; 
                }
            }
        }

        void ShowRadialBlendShapeControl(
            GameObject gameObject,
            Dictionary<(GameObject, string), RadialBlendShape> radials,
            IEnumerable<Util.INameAndDescription> names,
            float defaultEndValue = 100,
            float? minValue = 0,
            float? maxValue = 100
            )
        {
            foreach (var name in names)
            {
                var key = (gameObject, name.Name);
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
                        radials.Remove(key);
                    }
                }
                else
                {
                    if (EditorGUILayout.ToggleLeft(name.Description, false))
                    {
                        radials[key] = new RadialBlendShape { Start = 0, End = defaultEndValue };
                    }
                }
            }
        }

        void BulkSetRadialBlendShape(Dictionary<(GameObject, string), RadialBlendShape> radials, string radialName, RadialBlendShape radialBlendShape, string changedProp)
        {
            var matches = new List<(GameObject, string)>();
            foreach (var (gameObject, name) in radials.Keys)
            {
                if (name == radialName)
                {
                    matches.Add((gameObject, name));
                }
            }
            foreach (var key in matches)
            {
                radials[key] = radials[key].SetProp(changedProp, radialBlendShape.GetProp(changedProp));
            }
        }

        void CreateToggleAssets(string baseName, string basePath, GameObject[] gameObjects)
        {
            var matchGameObjects = new HashSet<GameObject>(gameObjects);
            // clip
            var active = new AnimationClip();
            active.name = $"{baseName}_active";
            var activate = new AnimationClip();
            activate.name = $"{baseName}_activate";
            var inactive = new AnimationClip();
            inactive.name = $"{baseName}_inactive";
            var inactivate = new AnimationClip();
            inactivate.name = $"{baseName}_inactivate";
            foreach (var gameObject in ToggleObjects.Keys)
            {
                if (!matchGameObjects.Contains(gameObject)) continue;
                var activeValue = ToggleObjects[gameObject] == ToggleType.ON;
                var curvePath = Util.ChildPath(VRCAvatarDescriptor.gameObject, gameObject);
                active.SetCurve(curvePath, typeof(GameObject), "m_IsActive", new AnimationCurve(new Keyframe(0 / 60.0f, activeValue ? 1 : 0)));
                inactive.SetCurve(curvePath, typeof(GameObject), "m_IsActive", new AnimationCurve(new Keyframe(0 / 60.0f, activeValue ? 0 : 1)));
                if (TransitionSeconds > 0)
                {
                    activate.SetCurve(curvePath, typeof(GameObject), "m_IsActive", new AnimationCurve(new Keyframe(0 / 60.0f, 1), new Keyframe(TransitionSeconds, activeValue ? 1 : 0)));
                    inactivate.SetCurve(curvePath, typeof(GameObject), "m_IsActive", new AnimationCurve(new Keyframe(0 / 60.0f, 1), new Keyframe(TransitionSeconds, activeValue ? 0 : 1)));
                }
            }
            foreach (var (gameObject, name) in ToggleBlendShapes.Keys)
            {
                if (!matchGameObjects.Contains(gameObject)) continue;
                var value = ToggleBlendShapes[(gameObject, name)];
                var curvePath = Util.ChildPath(VRCAvatarDescriptor.gameObject, gameObject);
                var curveName = $"blendShape.{name}";
                active.SetCurve(curvePath, typeof(SkinnedMeshRenderer), curveName, new AnimationCurve(new Keyframe(0 / 60.0f, value.Active)));
                inactive.SetCurve(curvePath, typeof(SkinnedMeshRenderer), curveName, new AnimationCurve(new Keyframe(0 / 60.0f, value.Inactive)));
                if (TransitionSeconds > 0)
                {
                    activate.SetCurve(curvePath, typeof(SkinnedMeshRenderer), curveName, new AnimationCurve(new Keyframe(TransitionSeconds * value.ActivateStartRate, value.Inactive), new Keyframe(TransitionSeconds * value.ActivateEndRate, value.Active)));
                    inactivate.SetCurve(curvePath, typeof(SkinnedMeshRenderer), curveName, new AnimationCurve(new Keyframe(TransitionSeconds * value.InactivateStartRate, value.Active), new Keyframe(TransitionSeconds * value.InactivateEndRate, value.Inactive)));
                }
            }
            foreach (var (gameObject, name) in ToggleShaderParameters.Keys)
            {
                if (!matchGameObjects.Contains(gameObject)) continue;
                var value = ToggleShaderParameters[(gameObject, name)];
                var curvePath = Util.ChildPath(VRCAvatarDescriptor.gameObject, gameObject);
                var curveName = $"material.{name}";
                active.SetCurve(curvePath, typeof(Renderer), curveName, new AnimationCurve(new Keyframe(0 / 60.0f, value.Active)));
                inactive.SetCurve(curvePath, typeof(Renderer), curveName, new AnimationCurve(new Keyframe(0 / 60.0f, value.Inactive)));
                if (TransitionSeconds > 0)
                {
                    activate.SetCurve(curvePath, typeof(Renderer), curveName, new AnimationCurve(new Keyframe(TransitionSeconds * value.ActivateStartRate, value.Inactive), new Keyframe(TransitionSeconds * value.ActivateEndRate, value.Active)));
                    inactivate.SetCurve(curvePath, typeof(Renderer), curveName, new AnimationCurve(new Keyframe(TransitionSeconds * value.InactivateStartRate, value.Active), new Keyframe(TransitionSeconds * value.InactivateEndRate, value.Inactive)));
                }
            }
            // controller
            var controller = new AnimatorController();
            controller.AddParameter(new AnimatorControllerParameter { name = baseName, type = AnimatorControllerParameterType.Bool, defaultBool = false });
            if (controller.layers.Length == 0) controller.AddLayer(baseName);
            var layer = controller.layers[0];
            layer.name = baseName;
            layer.stateMachine.name = baseName;
            layer.stateMachine.entryPosition = new Vector3(-600, 0);
            layer.stateMachine.anyStatePosition = new Vector3(800, 0);
            var idleState = layer.stateMachine.AddState($"{baseName}_idle", new Vector3(-300, 0));
            idleState.motion = inactive;
            idleState.writeDefaultValues = false;
            var inactiveState = layer.stateMachine.AddState($"{baseName}_inactive", new Vector3(300, 100));
            inactiveState.motion = inactive;
            inactiveState.writeDefaultValues = false;
            var activeState = layer.stateMachine.AddState($"{baseName}_active", new Vector3(300, -100));
            activeState.motion = active;
            activeState.writeDefaultValues = false;
            layer.stateMachine.defaultState = idleState;
            var idleToActive = idleState.AddTransition(activeState);
            idleToActive.exitTime = 0;
            idleToActive.duration = 0;
            idleToActive.hasExitTime = false;
            idleToActive.conditions = new AnimatorCondition[]
            {
                new AnimatorCondition
                {
                    mode = AnimatorConditionMode.If,
                    parameter = baseName,
                    threshold = 1,
                },
            };
            var idleToInactive = idleState.AddTransition(inactiveState);
            idleToInactive.exitTime = 0;
            idleToInactive.duration = 0;
            idleToInactive.hasExitTime = false;
            idleToInactive.conditions = new AnimatorCondition[]
            {
                new AnimatorCondition
                {
                    mode = AnimatorConditionMode.IfNot,
                    parameter = baseName,
                    threshold = 1,
                },
            };
            if (TransitionSeconds > 0)
            {
                var inactivateState = layer.stateMachine.AddState($"{baseName}_inactivate", new Vector3(500, 0));
                inactivateState.motion = inactivate;
                inactivateState.writeDefaultValues = false;
                var activateState = layer.stateMachine.AddState($"{baseName}_activate", new Vector3(100, 0));
                activateState.motion = activate;
                activateState.writeDefaultValues = false;
                var toActivate = inactiveState.AddTransition(activateState);
                toActivate.exitTime = 0;
                toActivate.duration = 0;
                toActivate.hasExitTime = false;
                toActivate.conditions = new AnimatorCondition[]
                {
                    new AnimatorCondition
                    {
                        mode = AnimatorConditionMode.If,
                        parameter = baseName,
                        threshold = 1,
                    },
                };
                var toActive = activateState.AddTransition(activeState);
                toActive.exitTime = 1;
                toActive.duration = 0;
                toActive.hasExitTime = true;
                var toInactivate = activeState.AddTransition(inactivateState);
                toInactivate.exitTime = 0;
                toInactivate.duration = 0;
                toInactivate.hasExitTime = false;
                toInactivate.conditions = new AnimatorCondition[]
                {
                    new AnimatorCondition
                    {
                        mode = AnimatorConditionMode.IfNot,
                        parameter = baseName,
                        threshold = 1,
                    },
                };
                var toInactive = inactivateState.AddTransition(inactiveState);
                toInactive.exitTime = 1;
                toInactive.duration = 0;
                toInactive.hasExitTime = true;
            }
            else
            {
                var toActive = inactiveState.AddTransition(activeState);
                toActive.exitTime = 0;
                toActive.duration = 0;
                toActive.hasExitTime = false;
                toActive.conditions = new AnimatorCondition[]
                {
                    new AnimatorCondition
                    {
                        mode = AnimatorConditionMode.If,
                        parameter = baseName,
                        threshold = 1,
                    },
                };
                var toInactive = activeState.AddTransition(inactiveState);
                toInactive.exitTime = 0;
                toInactive.duration = 0;
                toInactive.hasExitTime = false;
                toInactive.conditions = new AnimatorCondition[]
                {
                    new AnimatorCondition
                    {
                        mode = AnimatorConditionMode.IfNot,
                        parameter = baseName,
                        threshold = 1,
                    },
                };
            }
            // menu
            var menu = new VRCExpressionsMenu
            {
                controls = new List<VRCExpressionsMenu.Control>
                {
                    new VRCExpressionsMenu.Control {
                        name = baseName,
                        type = VRCExpressionsMenu.Control.ControlType.Toggle,
                        parameter = new VRCExpressionsMenu.Control.Parameter
                        {
                            name = baseName,
                        },
                        subParameters = new VRCExpressionsMenu.Control.Parameter[] { },
                        value = 1,
                        labels = new VRCExpressionsMenu.Control.Label[] { },
                    },
                },
            };
            menu.name = baseName;
            // prefab
            var prefabPath = $"{basePath}.prefab";
            var prefab = new GameObject(baseName);
            PrefabUtility.SaveAsPrefabAsset(prefab, prefabPath);
            DestroyImmediate(prefab);
            SaveAssets(baseName, basePath, controller, TransitionSeconds > 0 ? new AnimationClip[] { active, inactive, activate, inactivate } : new AnimationClip[] { active, inactive }, menu);
            prefab = PrefabUtility.LoadPrefabContents(prefabPath);
            var menuInstaller = prefab.GetOrAddComponent<ModularAvatarMenuInstaller>();
            menuInstaller.menuToAppend = menu;
            var parameters = prefab.GetOrAddComponent<ModularAvatarParameters>();
            parameters.parameters.Add(new ParameterConfig
            {
                nameOrPrefix = baseName,
                defaultValue = 0,
                syncType = ParameterSyncType.Bool,
                saved = true,
            });
            var mergeAnimator = prefab.GetOrAddComponent<ModularAvatarMergeAnimator>();
            mergeAnimator.animator = controller;
            mergeAnimator.layerType = VRCAvatarDescriptor.AnimLayerType.FX;
            mergeAnimator.pathMode = MergeAnimatorPathMode.Absolute;
            mergeAnimator.matchAvatarWriteDefaults = true;

            PrefabUtility.SaveAsPrefabAsset(prefab, prefabPath);
            PrefabUtility.UnloadPrefabContents(prefab);
            AssetDatabase.SaveAssets();
        }


        void CreateChooseAssets(string baseName, string basePath, GameObject[] gameObjects)
        {
            var matchGameObjects = new HashSet<GameObject>(gameObjects);
            // clip
            var choices = Enumerable.Range(0, ChooseCount).Select(i => new AnimationClip { name = $"{baseName}_{i}" }).ToList();
            foreach (var gameObject in ChooseObjects.Keys)
            {
                if (!matchGameObjects.Contains(gameObject)) continue;
                var curvePath = Util.ChildPath(VRCAvatarDescriptor.gameObject, gameObject);
                for (var i = 0; i < ChooseCount; ++i)
                {
                    choices[i].SetCurve(curvePath, typeof(GameObject), "m_IsActive", new AnimationCurve(new Keyframe(0, ChooseObjects[gameObject].Contains(i) ? 1 : 0)));
                }
            }
            foreach (var (gameObject, index) in ChooseMaterials.Keys)
            {
                if (!matchGameObjects.Contains(gameObject)) continue;
                var value = ChooseMaterials[(gameObject, index)];
                var curvePath = Util.ChildPath(VRCAvatarDescriptor.gameObject, gameObject);
                var curveName = $"m_Materials.Array.data[{index}]";
                for (var i = 0; i < ChooseCount; ++i)
                {
                    AnimationUtility.SetObjectReferenceCurve(choices[i], EditorCurveBinding.PPtrCurve(curvePath, typeof(SkinnedMeshRenderer), curveName), new ObjectReferenceKeyframe[] { new ObjectReferenceKeyframe { time = 0, value = value.ContainsKey(i) ? value[i] : null } });
                }
            }
            foreach (var (gameObject, name) in ChooseBlendShapes.Keys)
            {
                if (!matchGameObjects.Contains(gameObject)) continue;
                var value = ChooseBlendShapes[(gameObject, name)];
                var curvePath = Util.ChildPath(VRCAvatarDescriptor.gameObject, gameObject);
                var curveName = $"blendShape.{name}";
                for (var i = 0; i < ChooseCount; ++i)
                {
                    choices[i].SetCurve(curvePath, typeof(SkinnedMeshRenderer), curveName, new AnimationCurve(new Keyframe(0, value.ContainsKey(i) ? value[i] : 0)));
                }
            }
            foreach (var (gameObject, name) in ChooseShaderParameters.Keys)
            {
                if (!matchGameObjects.Contains(gameObject)) continue;
                var value = ChooseShaderParameters[(gameObject, name)];
                var curvePath = Util.ChildPath(VRCAvatarDescriptor.gameObject, gameObject);
                var curveName = $"material.{name}";
                for (var i = 0; i < ChooseCount; ++i)
                {
                    choices[i].SetCurve(curvePath, typeof(Renderer), curveName, new AnimationCurve(new Keyframe(0, value.ContainsKey(i) ? value[i] : 0)));
                }
            }
            // controller
            var controller = new AnimatorController();
            controller.AddParameter(new AnimatorControllerParameter { name = baseName, type = AnimatorControllerParameterType.Int, defaultInt = 0 });
            if (controller.layers.Length == 0) controller.AddLayer(baseName);
            var layer = controller.layers[0];
            layer.name = baseName;
            layer.stateMachine.name = baseName;
            layer.stateMachine.entryPosition = new Vector3(-600, 0);
            layer.stateMachine.anyStatePosition = new Vector3(800, 0);
            var idleState = layer.stateMachine.AddState($"{baseName}_idle", new Vector3(-300, 0));
            idleState.motion = choices[0];
            idleState.writeDefaultValues = false;
            layer.stateMachine.defaultState = idleState;
            var states = choices.Select((clip, i) =>
            {
                var state = layer.stateMachine.AddState($"{baseName}_{i}", new Vector3(300, 50 * i));
                state.motion = clip;
                state.writeDefaultValues = false;
                return state;
            }).ToList();
            for (var i = 0; i < ChooseCount; ++i)
            {
                var state = states[i];
                var toNext = layer.stateMachine.AddAnyStateTransition(state);
                toNext.exitTime = 0;
                toNext.duration = TransitionSeconds;
                toNext.hasExitTime = false;
                toNext.conditions = new AnimatorCondition[]
                {
                    new AnimatorCondition
                    {
                        mode = AnimatorConditionMode.Equals,
                        parameter = baseName,
                        threshold = i,
                    },
                };
                toNext.canTransitionToSelf = false;
                var fromIdle = idleState.AddTransition(state);
                fromIdle.exitTime = 0;
                fromIdle.duration = 0;
                fromIdle.hasExitTime = false;
                fromIdle.conditions = new AnimatorCondition[]
                {
                    new AnimatorCondition
                    {
                        mode = AnimatorConditionMode.Equals,
                        parameter = baseName,
                        threshold = i,
                    },
                };
            }
            // menu
            var menu = new VRCExpressionsMenu
            {
                controls = Enumerable.Range(0, ChooseCount).Select(i => new VRCExpressionsMenu.Control {
                    name = ChooseName(i),
                    type = VRCExpressionsMenu.Control.ControlType.Toggle,
                    parameter = new VRCExpressionsMenu.Control.Parameter
                    {
                        name = baseName,
                    },
                    subParameters = new VRCExpressionsMenu.Control.Parameter[] { },
                    value = i,
                    labels = new VRCExpressionsMenu.Control.Label[] { },
                }).ToList(),
            };
            menu.name = baseName;
            var parentMenu = new VRCExpressionsMenu
            {
                controls = new List<VRCExpressionsMenu.Control>
                {
                    new VRCExpressionsMenu.Control {
                        name = baseName,
                        type = VRCExpressionsMenu.Control.ControlType.SubMenu,
                        parameter = new VRCExpressionsMenu.Control.Parameter
                        {
                            name = "",
                        },
                        subParameters = new VRCExpressionsMenu.Control.Parameter[] { },
                        value = 1,
                        labels = new VRCExpressionsMenu.Control.Label[] { },
                        subMenu = menu,
                    },
                },
            };
            parentMenu.name = $"{baseName}_parent";
            // prefab
            var prefabPath = $"{basePath}.prefab";
            var prefab = new GameObject(baseName);
            PrefabUtility.SaveAsPrefabAsset(prefab, prefabPath);
            DestroyImmediate(prefab);
            SaveAssets(baseName, basePath, controller, choices, menu, parentMenu);
            prefab = PrefabUtility.LoadPrefabContents(prefabPath);
            var menuInstaller = prefab.GetOrAddComponent<ModularAvatarMenuInstaller>();
            menuInstaller.menuToAppend = parentMenu;
            var parameters = prefab.GetOrAddComponent<ModularAvatarParameters>();
            parameters.parameters.Add(new ParameterConfig
            {
                nameOrPrefix = baseName,
                defaultValue = 0,
                syncType = ParameterSyncType.Int,
                saved = true,
            });
            var mergeAnimator = prefab.GetOrAddComponent<ModularAvatarMergeAnimator>();
            mergeAnimator.animator = controller;
            mergeAnimator.layerType = VRCAvatarDescriptor.AnimLayerType.FX;
            mergeAnimator.pathMode = MergeAnimatorPathMode.Absolute;
            mergeAnimator.matchAvatarWriteDefaults = true;

            PrefabUtility.SaveAsPrefabAsset(prefab, prefabPath);
            PrefabUtility.UnloadPrefabContents(prefab);
            AssetDatabase.SaveAssets();
        }

        void CreateRadialAssets(string baseName, string basePath, GameObject[] gameObjects)
        {
            var matchGameObjects = new HashSet<GameObject>(gameObjects);
            // clip
            var clip = new AnimationClip();
            clip.name = baseName;
            foreach (var (gameObject, name) in RadialBlendShapes.Keys)
            {
                if (!matchGameObjects.Contains(gameObject)) continue;
                var value = RadialBlendShapes[(gameObject, name)];
                clip.SetCurve(Util.ChildPath(VRCAvatarDescriptor.gameObject, gameObject), typeof(SkinnedMeshRenderer), $"blendShape.{name}", SetAutoTangentMode(new AnimationCurve(new Keyframe(0 / 60.0f, value.Start), new Keyframe(1 / 60.0f, value.End))));
            }
            foreach (var (gameObject, name) in RadialShaderParameters.Keys)
            {
                if (!matchGameObjects.Contains(gameObject)) continue;
                var value = RadialShaderParameters[(gameObject, name)];
                clip.SetCurve(Util.ChildPath(VRCAvatarDescriptor.gameObject, gameObject), typeof(Renderer), $"material.{name}", SetAutoTangentMode(new AnimationCurve(new Keyframe(0 / 60.0f, value.Start), new Keyframe(1 / 60.0f, value.End))));
            }
            // controller
            var controller = new AnimatorController();
            controller.AddParameter(new AnimatorControllerParameter { name = baseName, type = AnimatorControllerParameterType.Float, defaultFloat = RadialDefaultValue });
            if (controller.layers.Length == 0) controller.AddLayer(baseName);
            var layer = controller.layers[0];
            layer.name = baseName;
            layer.stateMachine.name = baseName;
            var state = layer.stateMachine.AddState(baseName, new Vector3(300, 0));
            state.timeParameterActive = true;
            state.timeParameter = baseName;
            state.motion = clip;
            state.writeDefaultValues = false;
            AnimationClip emptyClip = null;
            if (RadialInactiveRange && !(float.IsNaN(RadialInactiveRangeMin) && float.IsNaN(RadialInactiveRangeMax)))
            {
                emptyClip = new AnimationClip();
                emptyClip.name = $"{baseName}_empty";
                var inactiveState = layer.stateMachine.AddState($"{baseName}_inactive", new Vector3(300, 100));
                inactiveState.motion = emptyClip;
                inactiveState.writeDefaultValues = false;
                layer.stateMachine.defaultState = inactiveState;
                var toInactive = state.AddTransition(inactiveState);
                toInactive.exitTime = 0;
                toInactive.duration = 0;
                toInactive.hasExitTime = false;
                var toInactiveConditions = new List<AnimatorCondition>();
                if (!float.IsNaN(RadialInactiveRangeMin))
                {
                    toInactiveConditions.Add(new AnimatorCondition
                    {
                        mode = AnimatorConditionMode.Greater,
                        parameter = baseName,
                        threshold = RadialInactiveRangeMin,
                    });
                }
                if (!float.IsNaN(RadialInactiveRangeMax))
                {
                    toInactiveConditions.Add(new AnimatorCondition
                    {
                        mode = AnimatorConditionMode.Less,
                        parameter = baseName,
                        threshold = RadialInactiveRangeMax,
                    });
                }
                toInactive.conditions = toInactiveConditions.ToArray();

                if (!float.IsNaN(RadialInactiveRangeMin))
                {
                    var toActiveMin = inactiveState.AddTransition(state);
                    toActiveMin.exitTime = 0;
                    toActiveMin.duration = 0;
                    toActiveMin.hasExitTime = false;
                    toActiveMin.conditions = new AnimatorCondition[]
                    {
                        new AnimatorCondition
                        {
                            mode = AnimatorConditionMode.Less,
                            parameter = baseName,
                            threshold = RadialInactiveRangeMin,
                        },
                    };
                }
                if (!float.IsNaN(RadialInactiveRangeMax))
                {
                    var toActiveMax = inactiveState.AddTransition(state);
                    toActiveMax.exitTime = 0;
                    toActiveMax.duration = 0;
                    toActiveMax.hasExitTime = false;
                    toActiveMax.conditions = new AnimatorCondition[]
                    {
                        new AnimatorCondition
                        {
                            mode = AnimatorConditionMode.Greater,
                            parameter = baseName,
                            threshold = RadialInactiveRangeMax,
                        },
                    };
                }
            }
            // menu
            var menu = new VRCExpressionsMenu
            {
                controls = new List<VRCExpressionsMenu.Control>
                {
                    new VRCExpressionsMenu.Control {
                        name = baseName,
                        type = VRCExpressionsMenu.Control.ControlType.RadialPuppet,
                        subParameters = new VRCExpressionsMenu.Control.Parameter[] {
                            new VRCExpressionsMenu.Control.Parameter
                            {
                                name = baseName,
                            }
                        },
                        value = RadialDefaultValue,
                        labels = new VRCExpressionsMenu.Control.Label[] { },
                    },
                },
            };
            menu.name = baseName;
            // prefab
            var prefabPath = $"{basePath}.prefab";
            var prefab = new GameObject(baseName);
            PrefabUtility.SaveAsPrefabAsset(prefab, prefabPath);
            DestroyImmediate(prefab);
            SaveAssets(baseName, basePath, controller, emptyClip == null ? new AnimationClip[] { clip } : new AnimationClip[] { clip, emptyClip }, menu);
            prefab = PrefabUtility.LoadPrefabContents(prefabPath);
            var menuInstaller = prefab.GetOrAddComponent<ModularAvatarMenuInstaller>();
            menuInstaller.menuToAppend = menu;
            var parameters = prefab.GetOrAddComponent<ModularAvatarParameters>();
            parameters.parameters.Add(new ParameterConfig {
                nameOrPrefix = baseName,
                defaultValue = RadialDefaultValue,
                syncType = ParameterSyncType.Float,
                saved = true,
            });
            var mergeAnimator = prefab.GetOrAddComponent<ModularAvatarMergeAnimator>();
            mergeAnimator.animator = controller;
            mergeAnimator.layerType = VRCAvatarDescriptor.AnimLayerType.FX;
            mergeAnimator.pathMode = MergeAnimatorPathMode.Absolute;
            mergeAnimator.matchAvatarWriteDefaults = true;

            PrefabUtility.SaveAsPrefabAsset(prefab, prefabPath);
            PrefabUtility.UnloadPrefabContents(prefab);
            AssetDatabase.SaveAssets();
        }

        AnimationCurve SetAutoTangentMode(AnimationCurve curve)
        {
            for (var i = 0; i < curve.length; ++i)
            {
                AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.Auto);
                AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.Auto);
            }
            return curve;
        }

        void SaveAssets(string baseName, string basePath, AnimatorController controller, IEnumerable<AnimationClip> clips, VRCExpressionsMenu menu, VRCExpressionsMenu parentMenu = null)
        {
            var prefabPath = $"{basePath}.prefab";
            var controllerPath = $"{basePath}.controller";
            AssetDatabase.LoadAllAssetsAtPath(prefabPath).Where(a => !(a is GameObject)).ToList().ForEach(AssetDatabase.RemoveObjectFromAsset);
            if (IncludeAssetType == IncludeAssetType.Include)
            {
                AssetDatabase.AddObjectToAsset(menu, prefabPath);
                if (parentMenu != null) AssetDatabase.AddObjectToAsset(parentMenu, prefabPath);
                foreach (var clip in clips)
                {
                    AssetDatabase.AddObjectToAsset(clip, prefabPath);
                }
                controller.name = baseName;
                SaveAnimator(controller, prefabPath, true);
            }
            else if (IncludeAssetType == IncludeAssetType.AnimatorAndInclude)
            {
                AssetDatabase.AddObjectToAsset(menu, prefabPath);
                if (parentMenu != null) AssetDatabase.AddObjectToAsset(parentMenu, prefabPath);

                SaveAnimator(controller, controllerPath);
                foreach (var clip in clips)
                {
                    AssetDatabase.AddObjectToAsset(clip, controllerPath);
                }
            }
            else
            {
                var basePathDir = System.IO.Path.GetDirectoryName(basePath);
                AssetDatabase.CreateAsset(menu, $"{basePathDir}/{menu.name}.asset");
                if (parentMenu != null) AssetDatabase.CreateAsset(parentMenu, $"{basePathDir}/{parentMenu.name}.asset");
                foreach (var clip in clips)
                {
                    AssetDatabase.CreateAsset(clip, $"{basePathDir}/{clip.name}.anim");
                }
                SaveAnimator(controller, controllerPath);
            }
        }

        void SaveAnimator(AnimatorController controller, string path, bool isSubAsset = false)
        {
            if (isSubAsset)
            {
                AssetDatabase.AddObjectToAsset(controller, path);
            }
            else
            {
                AssetDatabase.CreateAsset(controller, path);
            }

            foreach (var l in controller.layers)
            {
                SaveStateMachine(l.stateMachine, path);
            }
        }

        void SaveStateMachine(AnimatorStateMachine machine, string path)
        {
            machine.hideFlags = HideFlags.HideInHierarchy;
            AssetDatabase.AddObjectToAsset(machine, path);
            foreach (var s in machine.states)
            {
                AssetDatabase.AddObjectToAsset(s.state, path);
                foreach (var t in s.state.transitions)
                {
                    t.hideFlags = HideFlags.HideInHierarchy;
                    AssetDatabase.AddObjectToAsset(t, path);
                }
            }
            foreach (var t in machine.entryTransitions)
            {
                t.hideFlags = HideFlags.HideInHierarchy;
                AssetDatabase.AddObjectToAsset(t, path);
            }
            foreach (var t in machine.anyStateTransitions)
            {
                t.hideFlags = HideFlags.HideInHierarchy;
                AssetDatabase.AddObjectToAsset(t, path);
            }
            foreach (var m in machine.stateMachines)
            {
                SaveStateMachine(m.stateMachine, path);
            }
        }
    }

    public static class Util
    {
        /// <summary>
        /// 子GameObjectの相対パスを返す (親がnullなら絶対パス)
        /// </summary>
        /// <param name="baseObject">親 (nullなら絶対パス)</param>
        /// <param name="targetObject">子</param>
        /// <returns>パス</returns>
        public static string ChildPath(GameObject baseObject, GameObject targetObject)
        {
            var paths = new List<string>();
            var transform = targetObject.transform;
            var baseObjectTransform = baseObject == null ? null : baseObject.transform;
            while (baseObjectTransform != transform && transform != null)
            {
                paths.Add(transform.gameObject.name);
                transform = transform.parent;
            }
            paths.Reverse();
            return string.Join("/", paths.ToArray());
        }

        public static Material[] GetMaterialSlots(GameObject gameObject)
        {
            var renderer = gameObject.GetComponent<Renderer>();
            if (renderer == null) return new Material[0];
            return renderer.sharedMaterials;
        }

        public static List<string> GetBlendShapeNames(GameObject gameObject)
        {
            var shapekeyNames = new List<string>();
            var mesh = gameObject.GetComponent<SkinnedMeshRenderer>();
            if (mesh == null) return shapekeyNames;
            for (var i = 0; i < mesh.sharedMesh.blendShapeCount; ++i)
            {
                var name = mesh.sharedMesh.GetBlendShapeName(i);
                if (shapekeyNames.Contains(name)) continue;
                shapekeyNames.Add(name);
            }
            return shapekeyNames;
        }

        public interface INameAndDescription
        {
            string Name { get; }
            string Description { get; }
        }

        public class NameWithDescription : INameAndDescription
        {
            public string Name { get; set; }
            public string Description { get => Name; }
        }

        public static IEnumerable<NameWithDescription> ToNames(this IEnumerable<string> names) =>
            names.Select(name => new NameWithDescription { Name = name });

        public class MaterialShaderDescription
        {
            public int Index;
            public Material Material;
            public Shader Shader;
            public List<ShaderParameter> ShaderParameters;
        }

        public class ShaderParametersCache
        {
            Dictionary<GameObject, List<MaterialShaderDescription>> All = new Dictionary<GameObject, List<MaterialShaderDescription>>();
            Dictionary<GameObject, List<ShaderParameter>> Filtered = new Dictionary<GameObject, List<ShaderParameter>>();

            public void Clear()
            {
                All.Clear();
                Filtered.Clear();
            }

            public List<MaterialShaderDescription> GetShaderParameters(GameObject gameObject)
            {
                var renderer = gameObject.GetComponent<Renderer>();
                if (renderer == null) return new List<MaterialShaderDescription>();
                if (All.TryGetValue(gameObject, out var descriptions))
                {
                    if (Valid(renderer, descriptions))
                    {
                        return descriptions;
                    }
                }
                descriptions = Util.GetShaderParameters(gameObject).ToList();
                All[gameObject] = descriptions;
                return descriptions;
            }

            public List<ShaderParameter> GetFilteredShaderParameters(GameObject gameObject)
            {
                var renderer = gameObject.GetComponent<Renderer>();
                if (renderer == null) return new List<ShaderParameter>();
                if (All.TryGetValue(gameObject, out var descriptions))
                {
                    if (Valid(renderer, descriptions))
                    {
                        if (Filtered.TryGetValue(gameObject, out var filtered))
                        {
                            return filtered;
                        }
                    }
                }
                return (Filtered[gameObject] = GetShaderParameters(gameObject).ToFlatUniqueShaderParameterValues().OnlyFloatLike().OrderDefault().ToList());
            }

            bool Valid(Renderer renderer, List<MaterialShaderDescription> descriptions)
            {
                var materials = renderer.sharedMaterials;
                if (descriptions.Count != materials.Length) return false;
                for (var i = 0; i < descriptions.Count; ++i)
                {
                    if (descriptions[i].Material != materials[i]) return false;
                    if (descriptions[i].Shader != materials[i].shader) return false;
                }
                return true;
            }
        }

        public static IEnumerable<MaterialShaderDescription> GetShaderParameters(GameObject gameObject)
        {
            var renderer = gameObject.GetComponent<Renderer>();
            if (renderer == null) return Enumerable.Empty<MaterialShaderDescription>();
            return renderer.sharedMaterials.Select((mat, i) => new MaterialShaderDescription
            {
                Index = i,
                Material = mat,
                Shader = mat.shader,
                ShaderParameters = GetShaderParameters(mat.shader).ToList(),
            });
        }

        public class ShaderParameter : ShaderParameterValue
        {
            public int Index { get; set; }
        }

        public class ShaderParameterValue : INameAndDescription, System.IEquatable<ShaderParameterValue>
        {
            static Dictionary<string, string> CommonNames = new Dictionary<string, string>
            {
                { "_Tweak_transparency", "Transparency Level(UTS)" },
                { "_AlphaMaskValue", "Transparency(lilToon)" },
            };

            public UnityEngine.Rendering.ShaderPropertyType Type { get; set; }
            public string Name { get; set; }
            public string Description {
                get {
                    if (CommonNames.TryGetValue(Name, out var description)) return $"{Name} [{description}]";
                    if (string.IsNullOrEmpty(PropertyDescription) || Name == PropertyDescription) return Name;
                    return $"{Name} [{PropertyDescription}]";
                }
            }
            public string PropertyDescription { get; set; }
            public bool IsCommon { get => CommonNames.ContainsKey(Name); }

            public override bool Equals(object obj)
            {
                return Equals(obj as ShaderParameterValue);
            }

            public bool Equals(ShaderParameterValue other)
            {
                return Name == other.Name && Type == other.Type;
            }

            public override int GetHashCode()
            {
                return Name.GetHashCode() ^ Type.GetHashCode() * 17;
            }
        }

        public class ShaderParameterComparator : IEqualityComparer<ShaderParameter>
        {
            public bool Equals(ShaderParameter x, ShaderParameter y)
            {
                return (x as ShaderParameterValue).Equals(y);
            }

            public int GetHashCode(ShaderParameter obj)
            {
                return (obj as ShaderParameterValue).GetHashCode();
            }
        }

        public static IEnumerable<ShaderParameter> GetShaderParameters(Shader shader)
        {
            return Enumerable.Range(0, shader.GetPropertyCount()).Select(i => new ShaderParameter
            {
                Index = i,
                Type = shader.GetPropertyType(i),
                Name = shader.GetPropertyName(i),
                PropertyDescription = shader.GetPropertyDescription(i),
            });
        }

        static HashSet<UnityEngine.Rendering.ShaderPropertyType> FloatLikeShaderPropertyType = new HashSet<UnityEngine.Rendering.ShaderPropertyType> {
            UnityEngine.Rendering.ShaderPropertyType.Range,
            UnityEngine.Rendering.ShaderPropertyType.Float,
        };

        public static IEnumerable<ShaderParameter> OnlyFloatLike(this IEnumerable<ShaderParameter> values) =>
                values.Where(p => FloatLikeShaderPropertyType.Contains(p.Type));

        public static IEnumerable<ShaderParameter> OrderDefault(this IEnumerable<ShaderParameter> values) =>
            values.OrderBy(p => !p.IsCommon).ThenBy(p => p.Name);

        public static IEnumerable<ShaderParameter> ToFlatUniqueShaderParameterValues(this IEnumerable<MaterialShaderDescription> descriptions) =>
            descriptions
                .SelectMany(desc => desc.ShaderParameters)
                .Distinct(new ShaderParameterComparator());
    }
}
