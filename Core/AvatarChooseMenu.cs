using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
using net.narazaka.avatarmenucreator.editor.util;
#endif
using VRC.SDK3.Avatars.Components;
using nadena.dev.modular_avatar.core;
using VRC.SDK3.Avatars.ScriptableObjects;
using net.narazaka.avatarmenucreator.collections.instance;

namespace net.narazaka.avatarmenucreator
{
    [Serializable]
    public class AvatarChooseMenu : AvatarMenuBase
    {
        [SerializeField]
        IntHashSetDictionary ChooseObjects = new IntHashSetDictionary();
        [SerializeField]
        ChooseMaterialDictionary ChooseMaterials = new ChooseMaterialDictionary();
        [SerializeField]
        ChooseBlendShapeDictionary ChooseBlendShapes = new ChooseBlendShapeDictionary();
        [SerializeField]
        ChooseBlendShapeDictionary ChooseShaderParameters = new ChooseBlendShapeDictionary();
        [SerializeField]
        int ChooseDefaultValue;
        [SerializeField]
        int ChooseCount = 2;
        [SerializeField]
        IntStringDictionary ChooseNames = new IntStringDictionary();

#if UNITY_EDITOR

        string ChooseName(int index)
        {
            if (ChooseNames.ContainsKey(index)) return ChooseNames[index];
            return $"選択肢{index}";
        }

        public override IEnumerable<string> GetStoredChildren() => ChooseObjects.Keys.Concat(ChooseMaterials.Keys.Select(key => key.Item1)).Concat(ChooseBlendShapes.Keys.Select(key => key.Item1)).Concat(ChooseShaderParameters.Keys.Select(key => key.Item1)).Distinct();

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

        public override void CreateAssets(IncludeAssetType includeAssetType, string baseName, string basePath, string[] children)
        {
            var matchGameObjects = new HashSet<string>(children);
            // clip
            var choices = Enumerable.Range(0, ChooseCount).Select(i => new AnimationClip { name = $"{baseName}_{i}" }).ToList();
            foreach (var child in ChooseObjects.Keys)
            {
                if (!matchGameObjects.Contains(child)) continue;
                var curvePath = child;
                for (var i = 0; i < ChooseCount; ++i)
                {
                    choices[i].SetCurve(curvePath, typeof(GameObject), "m_IsActive", new AnimationCurve(new Keyframe(0, ChooseObjects[child].Contains(i) ? 1 : 0)));
                }
            }
            foreach (var (child, index) in ChooseMaterials.Keys)
            {
                if (!matchGameObjects.Contains(child)) continue;
                var value = ChooseMaterials[(child, index)];
                var curvePath = child;
                var curveName = $"m_Materials.Array.data[{index}]";
                for (var i = 0; i < ChooseCount; ++i)
                {
                    AnimationUtility.SetObjectReferenceCurve(choices[i], EditorCurveBinding.PPtrCurve(curvePath, typeof(SkinnedMeshRenderer), curveName), new ObjectReferenceKeyframe[] { new ObjectReferenceKeyframe { time = 0, value = value.ContainsKey(i) ? value[i] : null } });
                }
            }
            foreach (var (child, name) in ChooseBlendShapes.Keys)
            {
                if (!matchGameObjects.Contains(child)) continue;
                var value = ChooseBlendShapes[(child, name)];
                var curvePath = child;
                var curveName = $"blendShape.{name}";
                for (var i = 0; i < ChooseCount; ++i)
                {
                    choices[i].SetCurve(curvePath, typeof(SkinnedMeshRenderer), curveName, new AnimationCurve(new Keyframe(0, value.ContainsKey(i) ? value[i] : 0)));
                }
            }
            foreach (var (child, name) in ChooseShaderParameters.Keys)
            {
                if (!matchGameObjects.Contains(child)) continue;
                var value = ChooseShaderParameters[(child, name)];
                var curvePath = child;
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
                controls = Enumerable.Range(0, ChooseCount).Select(i => new VRCExpressionsMenu.Control
                {
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
            UnityEngine.Object.DestroyImmediate(prefab);
            SaveAssets(includeAssetType, baseName, basePath, controller, choices, menu, parentMenu);
            prefab = PrefabUtility.LoadPrefabContents(prefabPath);
            var menuInstaller = prefab.GetOrAddComponent<ModularAvatarMenuInstaller>();
            menuInstaller.menuToAppend = parentMenu;
            var parameters = prefab.GetOrAddComponent<ModularAvatarParameters>();
            parameters.parameters.Add(new ParameterConfig
            {
                nameOrPrefix = baseName,
                defaultValue = ChooseDefaultValue,
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
#endif
    }
}
