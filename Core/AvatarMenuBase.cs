using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
using net.narazaka.avatarmenucreator.editor.util;
#endif
using VRC.SDK3.Avatars.ScriptableObjects;

namespace net.narazaka.avatarmenucreator
{
    [Serializable]
    public abstract class AvatarMenuBase
    {
        [NonSerialized]
        GameObject _BaseObject;
        public GameObject BaseObject
        {
            get => _BaseObject;
            set
            {
                if (_BaseObject == value) return;
                _BaseObject = value;
                ClearGameObjectCache();
            }
        }
        [NonSerialized]
        public bool BulkSet;

        [SerializeField]
        protected float TransitionSeconds;

#if UNITY_EDITOR

        HashSet<string> FoldoutGameObjects = new HashSet<string>();
        HashSet<string> FoldoutMaterials = new HashSet<string>();
        HashSet<string> FoldoutBlendShapes = new HashSet<string>();
        HashSet<string> FoldoutShaderParameters = new HashSet<string>();
        Vector2 ScrollPosition;

        protected Util.ShaderParametersCache ShaderParametersCache = new Util.ShaderParametersCache();
        Dictionary<string, GameObject> GameObjectCache = new Dictionary<string, GameObject>();

        public abstract void CreateAssets(IncludeAssetType includeAssetType, string baseName, string basePath, string[] children);
        protected abstract void OnHeaderGUI(string[] children);
        protected abstract void OnMainGUI(string[] children);
        protected abstract bool IsSuitableForTransition();

        public void OnAvatarMenuGUI(string[] children)
        {
            OnHeaderGUI(children);

            using (var scrollView = new EditorGUILayout.ScrollViewScope(ScrollPosition))
            {
                ScrollPosition = scrollView.scrollPosition;
                OnMainGUI(children);
            }
        }

        protected void ShowTransitionSeconds()
        {
            TransitionSeconds = EditorGUILayout.FloatField("徐々に変化（秒数）", TransitionSeconds);
            if (TransitionSeconds < 0) TransitionSeconds = 0;
            if (TransitionSeconds > 0)
            {
                if (!IsSuitableForTransition())
                {
                    EditorGUILayout.HelpBox("徐々に変化するものの指定が有りません", MessageType.Warning);
                }
                else
                {
                    EditorGUILayout.HelpBox("指定時間かけて変化します", MessageType.Info);
                }
            }
        }

        protected GameObject GetGameObject(string child)
        {
            if (GameObjectCache == null) GameObjectCache = new Dictionary<string, GameObject>();
            if (!GameObjectCache.TryGetValue(child, out var gameObjectRef))
            {
                GameObjectCache[child] = gameObjectRef = BaseObject.transform.Find(child).gameObject;
            }
            return gameObjectRef;
        }

        public void ClearGameObjectCache()
        {
            GameObjectCache = null;
        }

        protected bool FoldoutGameObjectHeader(string child, string title)
        {
            var foldout = FoldoutGameObjects.Contains(child);
            var newFoldout = EditorGUILayout.Foldout(foldout, title);
            if (newFoldout != foldout)
            {
                if (newFoldout)
                {
                    FoldoutGameObjects.Add(child);
                }
                else
                {
                    FoldoutGameObjects.Remove(child);
                }
            }
            return newFoldout;
        }

        protected bool FoldoutMaterialHeader(string child, string title)
        {
            var foldout = FoldoutMaterials.Contains(child);
            var newFoldout = EditorGUILayout.Foldout(foldout, title);
            if (newFoldout != foldout)
            {
                if (newFoldout)
                {
                    FoldoutMaterials.Add(child);
                }
                else
                {
                    FoldoutMaterials.Remove(child);
                }
            }
            return newFoldout;
        }

        protected bool FoldoutBlendShapeHeader(string child, string title)
        {
            var foldout = FoldoutBlendShapes.Contains(child);
            var newFoldout = EditorGUILayout.Foldout(foldout, title);
            if (newFoldout != foldout)
            {
                if (newFoldout)
                {
                    FoldoutBlendShapes.Add(child);
                }
                else
                {
                    FoldoutBlendShapes.Remove(child);
                }
            }
            return newFoldout;
        }

        protected bool FoldoutShaderParameterHeader(string child, string title)
        {
            var foldout = FoldoutShaderParameters.Contains(child);
            var newFoldout = EditorGUILayout.Foldout(foldout, title);
            if (newFoldout != foldout)
            {
                if (newFoldout)
                {
                    FoldoutShaderParameters.Add(child);
                }
                else
                {
                    FoldoutShaderParameters.Remove(child);
                }
            }
            return newFoldout;
        }

        protected void SaveAssets(IncludeAssetType includeAssetType, string baseName, string basePath, AnimatorController controller, IEnumerable<AnimationClip> clips, VRCExpressionsMenu menu, VRCExpressionsMenu parentMenu = null)
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
#endif
    }
}
