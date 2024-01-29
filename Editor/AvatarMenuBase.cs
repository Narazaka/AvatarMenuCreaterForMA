using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using VRC.SDK3.Avatars.ScriptableObjects;
using UnityEditor.Animations;

namespace net.narazaka.avatarmenucreator
{
    public abstract class AvatarMenuBase
    {
        protected float TransitionSeconds;

        protected bool BulkSet;

        HashSet<GameObject> FoldoutGameObjects = new HashSet<GameObject>();
        HashSet<GameObject> FoldoutMaterials = new HashSet<GameObject>();
        HashSet<GameObject> FoldoutBlendShapes = new HashSet<GameObject>();
        HashSet<GameObject> FoldoutShaderParameters = new HashSet<GameObject>();
        Vector2 ScrollPosition;

        protected Util.ShaderParametersCache ShaderParametersCache = new Util.ShaderParametersCache();

        public abstract void CreateAssets(IncludeAssetType includeAssetType, GameObject baseObject, string baseName, string basePath, GameObject[] gameObjects);
        protected abstract void OnHeaderGUI(GameObject baseObject, GameObject[] gameObjects);
        protected abstract void OnMainGUI(GameObject baseObject, GameObject[] gameObjects);
        protected abstract bool IsSuitableForTransition();

        public void OnAvatarMenuGUI(GameObject baseObject, GameObject[] gameObjects)
        {
            OnHeaderGUI(baseObject, gameObjects);

            using (var scrollView = new EditorGUILayout.ScrollViewScope(ScrollPosition))
            {
                ScrollPosition = scrollView.scrollPosition;
                OnMainGUI(baseObject, gameObjects);
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

        protected void ShowBulkSet()
        {
            BulkSet = EditorGUILayout.ToggleLeft("同名パラメーターや同マテリアルスロットを一括設定", BulkSet);
        }

        protected bool FoldoutGameObjectHeader(GameObject gameObject, string title)
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

        protected bool FoldoutMaterialHeader(GameObject gameObject, string title)
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

        protected bool FoldoutBlendShapeHeader(GameObject gameObject, string title)
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

        protected bool FoldoutShaderParameterHeader(GameObject gameObject, string title)
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

        protected void SaveAssets(IncludeAssetType IncludeAssetType, string baseName, string basePath, AnimatorController controller, IEnumerable<AnimationClip> clips, VRCExpressionsMenu menu, VRCExpressionsMenu parentMenu = null)
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
}
