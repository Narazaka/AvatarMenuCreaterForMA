using System.Collections.Generic;
using System.Linq;
using System;
using UnityEditor.Animations;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.ScriptableObjects;
using nadena.dev.modular_avatar.core;
using VRC.SDK3.Avatars.Components;
using net.narazaka.avatarmenucreator.editor.util;

namespace net.narazaka.avatarmenucreator.editor
{
    public class CreatedAssets
    {
        string baseName;
        AnimatorController controller;
        IEnumerable<AnimationClip> clips;
        VRCExpressionsMenu menu;
        VRCExpressionsMenu parentMenu;
        IEnumerable<ParameterConfig> parameters;

        public CreatedAssets(string baseName, AnimatorController controller, IEnumerable<AnimationClip> clips, VRCExpressionsMenu menu, VRCExpressionsMenu parentMenu, IEnumerable<ParameterConfig> parameters)
        {
            this.baseName = baseName;
            this.controller = controller;
            this.clips = clips;
            this.menu = menu;
            this.parentMenu = parentMenu;
            this.parameters = parameters;
        }

        public void StoreAssets(GameObject baseObject, bool isMenuInstaller = true)
        {
            StoreToPrefab(baseObject, isMenuInstaller);
        }

        public void SaveAssets(IncludeAssetType includeAssetType, string basePath, Action<GameObject> addifionalModifyPrefab = null, bool isMenuInstaller = true)
        {
            // prefab
            var prefabPath = $"{basePath}.prefab";
            GameObject prefab;
            if (!System.IO.File.Exists(prefabPath))
            {
                prefab = new GameObject(baseName);
                PrefabUtility.SaveAsPrefabAsset(prefab, prefabPath);
                UnityEngine.Object.DestroyImmediate(prefab);
            }
            SaveAssets(includeAssetType, baseName, basePath, controller, clips, menu, parentMenu);
            prefab = PrefabUtility.LoadPrefabContents(prefabPath);

            StoreToPrefab(prefab, isMenuInstaller);

            if (addifionalModifyPrefab != null) addifionalModifyPrefab(prefab);

            PrefabUtility.SaveAsPrefabAsset(prefab, prefabPath);
            PrefabUtility.UnloadPrefabContents(prefab);
            AssetDatabase.SaveAssets();
        }

        void StoreToPrefab(GameObject prefab, bool isMenuInstaller)
        {
            var menuInstaller = prefab.GetComponent<ModularAvatarMenuInstaller>();
            if (menuInstaller == null && isMenuInstaller)
            {
                menuInstaller = prefab.AddComponent<ModularAvatarMenuInstaller>();
            }
            if (menuInstaller == null)
            {
                var menuItem = prefab.GetOrAddComponent<ModularAvatarMenuItem>();
                menuItem.Control = (parentMenu == null ? menu : parentMenu).controls[0].DeepCopy();
                menuItem.MenuSource = SubmenuSource.MenuAsset;
            }
            else
            {
                menuInstaller.menuToAppend = parentMenu == null ? menu : parentMenu;
            }
            var maParameters = prefab.GetOrAddComponent<ModularAvatarParameters>();
            maParameters.parameters.Clear();
            maParameters.parameters.AddRange(parameters);
            var mergeAnimator = prefab.GetOrAddComponent<ModularAvatarMergeAnimator>();
            mergeAnimator.animator = controller;
            mergeAnimator.layerType = VRCAvatarDescriptor.AnimLayerType.FX;
            mergeAnimator.pathMode = MergeAnimatorPathMode.Absolute;
            mergeAnimator.matchAvatarWriteDefaults = true;
        }

        static void SaveAssets(IncludeAssetType includeAssetType, string baseName, string basePath, AnimatorController controller, IEnumerable<AnimationClip> clips, VRCExpressionsMenu menu, VRCExpressionsMenu parentMenu = null)
        {
            var prefabPath = $"{basePath}.prefab";
            var controllerPath = $"{basePath}.controller";
            AssetDatabase.LoadAllAssetsAtPath(prefabPath).Where(a => !(a is GameObject)).ToList().ForEach(AssetDatabase.RemoveObjectFromAsset);
            if (includeAssetType == IncludeAssetType.Include)
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
            else if (includeAssetType == IncludeAssetType.AnimatorAndInclude)
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

        static void SaveAnimator(AnimatorController controller, string path, bool isSubAsset = false)
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

        static void SaveStateMachine(AnimatorStateMachine machine, string path)
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
}
