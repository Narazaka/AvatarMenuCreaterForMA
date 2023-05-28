using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using VRC.Core;
using nadena.dev.modular_avatar.core;

namespace net.narazaka.avatarmenucreater
{
    enum MenuType
    {
        [InspectorName("ON／OFF")]
        Toggle,
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

        public bool Equals(ToggleBlendShape other)
        {
            return Inactive == other.Inactive && Active == other.Active;
        }
    }

    struct RadialBlendShape : System.IEquatable<RadialBlendShape>
    {
        public float Start;
        public float End;

        public bool Equals(RadialBlendShape other)
        {
            return Start == other.Start && End == other.End;
        }
    }

    public class AvatarMenuCreaterForMA : EditorWindow
    {
        VRCAvatarDescriptor VRCAvatarDescriptor;
        MenuType MenuType = MenuType.Toggle;
        Dictionary<GameObject, ToggleType> ToggleObjects = new Dictionary<GameObject, ToggleType>();
        Dictionary<(GameObject, string), ToggleBlendShape> ToggleBlendShapes = new Dictionary<(GameObject, string), ToggleBlendShape>();
        Dictionary<(GameObject, string), ToggleBlendShape> ToggleShaderParameters = new Dictionary<(GameObject, string), ToggleBlendShape>();
        Dictionary<(GameObject, string), RadialBlendShape> RadialBlendShapes = new Dictionary<(GameObject, string), RadialBlendShape>();
        Dictionary<(GameObject, string), RadialBlendShape> RadialShaderParameters = new Dictionary<(GameObject, string), RadialBlendShape>();
        float RadialDefaultValue;

        HashSet<GameObject> Foldouts = new HashSet<GameObject>();
        HashSet<GameObject> FoldoutBlendShapes = new HashSet<GameObject>();
        HashSet<GameObject> FoldoutShaderParameters = new HashSet<GameObject>();
        Vector2 ScrollPosition;

        [MenuItem("Tools/Modular Avatar/AvatarMenuCreater for Modular Avatar")]
        static void CreateWindow()
        {
            GetWindow<AvatarMenuCreaterForMA>("AvatarMenuCreater for Modular Avatar");
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

            if (MenuType == MenuType.Toggle)
            {
                using (var scrollView = new EditorGUILayout.ScrollViewScope(ScrollPosition))
                {
                    ScrollPosition = scrollView.scrollPosition;

                    foreach (var gameObject in gameObjects)
                    {
                        EditorGUILayout.LabelField(Util.ChildPath(VRCAvatarDescriptor.gameObject, gameObject));
                        EditorGUI.indentLevel++;
                        ShowToggleObjectControl(gameObject);

                        var names = Util.GetBlendShapeNames(gameObject);
                        var parameters = Util.GetShaderParameters(gameObject).ToFlatUniqueShaderParameterValues().OnlyFloatLike().OrderDefault().ToList();
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
            else
            {
                RadialDefaultValue = EditorGUILayout.FloatField("パラメーター初期値", RadialDefaultValue);
                using (var scrollView = new EditorGUILayout.ScrollViewScope(ScrollPosition))
                {
                    ScrollPosition = scrollView.scrollPosition;

                    if (RadialDefaultValue < 0) RadialDefaultValue = 0;
                    if (RadialDefaultValue > 1) RadialDefaultValue = 1;
                    foreach (var gameObject in gameObjects)
                    {
                        var names = Util.GetBlendShapeNames(gameObject);
                        var parameters = Util.GetShaderParameters(gameObject).ToFlatUniqueShaderParameterValues().OnlyFloatLike().OrderDefault().ToList();
                        var path = Util.ChildPath(VRCAvatarDescriptor.gameObject, gameObject);
                        if (names.Count > 0 || parameters.Count > 0)
                        {
                            if (FoldoutHeader(gameObject, path))
                            {
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
                        }
                        else
                        {
                            EditorGUILayout.LabelField($"{path} (BlendShapeなし)");
                        }
                    }
                }
            }

            if (GUILayout.Button("Create!"))
            {
                var basePath = EditorUtility.SaveFilePanelInProject("保存場所", "New Menu", "asset", "アセットの保存場所");
                if (basePath == null) return;
                basePath = new System.Text.RegularExpressions.Regex(@"\.asset").Replace(basePath, "");
                var baseName = System.IO.Path.GetFileNameWithoutExtension(basePath);
                if (MenuType == MenuType.Toggle)
                {
                    CreateToggleAssets(baseName, basePath, gameObjects);
                }
                else
                {
                    CreateRadialAssets(baseName, basePath, gameObjects);
                }
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
                        var newValue = new ToggleBlendShape();
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            EditorGUI.indentLevel++;
                            EditorGUIUtility.labelWidth = 70;
                            newValue.Inactive = EditorGUILayout.FloatField("OFF", value.Inactive, GUILayout.Width(100));
                            newValue.Active = EditorGUILayout.FloatField("ON", value.Active, GUILayout.Width(100));
                            EditorGUIUtility.labelWidth = 0;
                            EditorGUI.indentLevel--;
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

                            toggles[key] = newValue;
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
                        toggles[key] = new ToggleBlendShape { Inactive = 0, Active = defaultActiveValue };
                    }
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

        void CreateToggleAssets(string baseName, string basePath, GameObject[] gameObjects)
        {
            var matchGameObjects = new HashSet<GameObject>(gameObjects);
            // clip
            var active = new AnimationClip();
            var inactive = new AnimationClip();
            foreach (var gameObject in ToggleObjects.Keys)
            {
                if (!matchGameObjects.Contains(gameObject)) continue;
                var activeValue = ToggleObjects[gameObject] == ToggleType.ON;
                var curvePath = Util.ChildPath(VRCAvatarDescriptor.gameObject, gameObject);
                active.SetCurve(curvePath, typeof(GameObject), "m_IsActive", new AnimationCurve(new Keyframe(0 / 60.0f, activeValue ? 1 : 0)));
                inactive.SetCurve(curvePath, typeof(GameObject), "m_IsActive", new AnimationCurve(new Keyframe(0 / 60.0f, activeValue ? 0 : 1)));
            }
            foreach (var (gameObject, name) in ToggleBlendShapes.Keys)
            {
                if (!matchGameObjects.Contains(gameObject)) continue;
                var value = ToggleBlendShapes[(gameObject, name)];
                var curvePath = Util.ChildPath(VRCAvatarDescriptor.gameObject, gameObject);
                var curveName = $"blendShape.{name}";
                active.SetCurve(curvePath, typeof(SkinnedMeshRenderer), curveName, new AnimationCurve(new Keyframe(0 / 60.0f, value.Active)));
                inactive.SetCurve(curvePath, typeof(SkinnedMeshRenderer), curveName, new AnimationCurve(new Keyframe(0 / 60.0f, value.Inactive)));
            }
            foreach (var (gameObject, name) in ToggleShaderParameters.Keys)
            {
                if (!matchGameObjects.Contains(gameObject)) continue;
                var value = ToggleShaderParameters[(gameObject, name)];
                var curvePath = Util.ChildPath(VRCAvatarDescriptor.gameObject, gameObject);
                var curveName = $"material.{name}";
                active.SetCurve(curvePath, typeof(SkinnedMeshRenderer), curveName, new AnimationCurve(new Keyframe(0 / 60.0f, value.Active)));
                inactive.SetCurve(curvePath, typeof(SkinnedMeshRenderer), curveName, new AnimationCurve(new Keyframe(0 / 60.0f, value.Inactive)));
            }
            AssetDatabase.CreateAsset(active, $"{basePath}_active.anim");
            AssetDatabase.CreateAsset(inactive, $"{basePath}_inactive.anim");
            // controller
            var controller = AnimatorController.CreateAnimatorControllerAtPath($"{basePath}.controller");
            controller.parameters = new AnimatorControllerParameter[]{
                new AnimatorControllerParameter { name = baseName, type = AnimatorControllerParameterType.Bool, defaultBool = false },
            };
            var layer = controller.layers[0];
            layer.name = baseName;
            layer.stateMachine.name = baseName;
            var inactiveState = layer.stateMachine.AddState($"{baseName}_inactive", new Vector3(300, 100));
            inactiveState.motion = inactive;
            inactiveState.writeDefaultValues = false;
            var activeState = layer.stateMachine.AddState($"{baseName}_active", new Vector3(300, -100));
            activeState.motion = active;
            activeState.writeDefaultValues = false;
            layer.stateMachine.defaultState = inactiveState;
            var toActive = inactiveState.AddTransition(activeState);
            toActive.exitTime = 0;
            toActive.duration = 0;
            toActive.hasExitTime = false;
            toActive.conditions = new AnimatorCondition[] {
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
            toInactive.conditions = new AnimatorCondition[] {
                new AnimatorCondition
                {
                    mode = AnimatorConditionMode.IfNot,
                    parameter = baseName,
                    threshold = 1,
                },
            };
            // AssetDatabase.AddObjectToAsset(toActive, controller);
            // AssetDatabase.AddObjectToAsset(toInactive, controller);
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
            AssetDatabase.CreateAsset(menu, $"{basePath}.asset");
            // prefab
            var prefab = new GameObject(baseName);
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
            PrefabUtility.SaveAsPrefabAsset(prefab, $"{basePath}.prefab");
            DestroyImmediate(prefab);
        }

        void CreateRadialAssets(string baseName, string basePath, GameObject[] gameObjects)
        {
            var matchGameObjects = new HashSet<GameObject>(gameObjects);
            // clip
            var clip = new AnimationClip();
            foreach (var (gameObject, name) in RadialBlendShapes.Keys)
            {
                if (!matchGameObjects.Contains(gameObject)) continue;
                var value = RadialBlendShapes[(gameObject, name)];
                clip.SetCurve(Util.ChildPath(VRCAvatarDescriptor.gameObject, gameObject), typeof(SkinnedMeshRenderer), $"blendShape.{name}", new AnimationCurve(new Keyframe(0 / 60.0f, value.Start), new Keyframe(1 / 60.0f, value.End)));
            }
            foreach (var (gameObject, name) in RadialShaderParameters.Keys)
            {
                if (!matchGameObjects.Contains(gameObject)) continue;
                var value = RadialShaderParameters[(gameObject, name)];
                clip.SetCurve(Util.ChildPath(VRCAvatarDescriptor.gameObject, gameObject), typeof(SkinnedMeshRenderer), $"material.{name}", new AnimationCurve(new Keyframe(0 / 60.0f, value.Start), new Keyframe(1 / 60.0f, value.End)));
            }
            AssetDatabase.CreateAsset(clip, $"{basePath}.anim");
            // controller
            var controller = AnimatorController.CreateAnimatorControllerAtPath($"{basePath}.controller");
            controller.parameters = new AnimatorControllerParameter[]{
                new AnimatorControllerParameter { name = baseName, type = AnimatorControllerParameterType.Float, defaultFloat = RadialDefaultValue },
            };
            var layer = controller.layers[0];
            layer.name = baseName;
            layer.stateMachine.name = baseName;
            var state = layer.stateMachine.AddState(baseName, new Vector3(300, 0));
            state.timeParameterActive = true;
            state.timeParameter = baseName;
            state.motion = clip;
            state.writeDefaultValues = false;
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
            AssetDatabase.CreateAsset(menu, $"{basePath}.asset");
            // prefab
            var prefab = new GameObject(baseName);
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
            PrefabUtility.SaveAsPrefabAsset(prefab, $"{basePath}.prefab");
            DestroyImmediate(prefab);
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
            public List<ShaderParameter> ShaderParameters;
        }

        public static IEnumerable<MaterialShaderDescription> GetShaderParameters(GameObject gameObject)
        {
            var renderer = gameObject.GetComponent<Renderer>();
            if (renderer == null) return Enumerable.Empty<MaterialShaderDescription>();
            return renderer.sharedMaterials.Select((mat, i) => new MaterialShaderDescription
            {
                Index = i,
                Material = mat,
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
