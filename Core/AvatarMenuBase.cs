using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using net.narazaka.avatarmenucreator.editor.util;
#endif

namespace net.narazaka.avatarmenucreator
{
    [Serializable]
    public abstract class AvatarMenuBase
    {
        [NonSerialized]
        public bool BulkSet;

        [SerializeField]
        public float TransitionSeconds;
        [SerializeField]
        public bool Saved = true;

#if UNITY_EDITOR
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
        public UnityEngine.Object UndoObject;

        HashSet<string> FoldoutGameObjects = new HashSet<string>();
        HashSet<string> FoldoutMaterials = new HashSet<string>();
        HashSet<string> FoldoutBlendShapes = new HashSet<string>();
        HashSet<string> FoldoutShaderParameters = new HashSet<string>();
        Vector2 ScrollPosition;

        protected Util.ShaderParametersCache ShaderParametersCache = new Util.ShaderParametersCache();
        Dictionary<string, GameObject> GameObjectCache = new Dictionary<string, GameObject>();

        public abstract IEnumerable<string> GetStoredChildren();
        public abstract void RemoveStoredChild(string child);
        protected abstract void OnHeaderGUI(IList<string> children);
        protected abstract void OnMainGUI(IList<string> children);
        protected abstract bool IsSuitableForTransition();

        public void OnAvatarMenuGUI(IList<string> children)
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
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                var newTransitionSeconds = EditorGUILayout.FloatField("徐々に変化（秒数）", TransitionSeconds);
                if (check.changed)
                {
                    WillChange();
                    TransitionSeconds = newTransitionSeconds;
                }
            }
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

        protected void ShowSaved()
        {
            Saved = Toggle("Saved", Saved);
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

        protected void WillChange(string message = null)
        {
            UndoUtility.RecordObject(UndoObject, message ?? UndoMessage);
        }

        string _UndoMessage;
        string UndoMessage
        {
            get
            {
                if (_UndoMessage == null)
                {
                    _UndoMessage = $"{GetType().Name} changed";
                }
                return _UndoMessage;
            }
        }

        protected bool Toggle(string label, bool value)
        {
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                var newValue = EditorGUILayout.Toggle(label, value);
                if (check.changed)
                {
                    WillChange();
                }
                return newValue;
            }
        }

        protected string TextField(string label, string value)
        {
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                var newValue = EditorGUILayout.TextField(label, value);
                if (check.changed)
                {
                    WillChange();
                }
                return newValue;
            }
        }

        protected int IntField(string label, int value)
        {
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                var newValue = EditorGUILayout.IntField(label, value);
                if (check.changed)
                {
                    WillChange();
                }
                return newValue;
            }
        }

        protected float FloatField(string label, float value)
        {
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                var newValue = EditorGUILayout.FloatField(label, value);
                if (check.changed)
                {
                    WillChange();
                }
                return newValue;
            }
        }

        protected float FloatField(float value)
        {
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                var newValue = EditorGUILayout.FloatField(value);
                if (check.changed)
                {
                    WillChange();
                }
                return newValue;
            }
        }
#endif
    }
}
