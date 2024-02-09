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
        public IncludeAssetType IncludeAssetType = IncludeAssetType.AnimatorAndInclude;
        [SerializeField]
        public float TransitionSeconds;
        [SerializeField]
        public bool Saved = true;
        [SerializeField]
        public string ParameterName;
        [SerializeField]
        public bool InternalParameter = false;

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
        Dictionary<string, HashSet<string>> FoldoutGroups = new Dictionary<string, HashSet<string>>();
        Vector2 ScrollPosition;

        protected Util.ShaderParametersCache ShaderParametersCache = new Util.ShaderParametersCache();
        Dictionary<string, GameObject> GameObjectCache = new Dictionary<string, GameObject>();

        public abstract IEnumerable<string> GetStoredChildren();
        public abstract void FilterStoredTargets(IEnumerable<string> children);
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
            Saved = Toggle("パラメーター保存", Saved);
        }

        protected void ShowDetailMenu()
        {
            ParameterName = TextField("パラメーター名(オプショナル)", ParameterName);
            InternalParameter = Toggle("パラメーター内部値", InternalParameter);
        }

        protected GameObject GetGameObject(string child)
        {
            if (BaseObject == null) return null;
            if (GameObjectCache == null) GameObjectCache = new Dictionary<string, GameObject>();
            if (!GameObjectCache.TryGetValue(child, out var gameObjectRef))
            {
                var transform = BaseObject.transform.Find(child);
                GameObjectCache[child] = gameObjectRef = transform == null ? null : transform.gameObject;
            }
            return gameObjectRef;
        }

        public void ClearGameObjectCache()
        {
            GameObjectCache = null;
        }

        protected void GameObjectHeader(string child)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(child, EditorStyles.boldLabel);
            var gameObjectRef = GetGameObject(child);
            if (gameObjectRef == null)
            {
                EditorGUILayout.LabelField(EditorGUIUtility.IconContent("Warning"), GUILayout.Width(35));
            }
            else
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.ObjectField(gameObjectRef, typeof(GameObject), true);
                EditorGUI.EndDisabledGroup();
            }
            EditorGUILayout.EndHorizontal();
        }

        protected bool FoldoutHeader(string child, string title, bool hasChildren = true)
        {
            if (!hasChildren)
            {
                EditorGUILayout.LabelField(title);
                return false;
            }
            if (!FoldoutGroups.TryGetValue(title, out var foldoutGroup))
            {
                FoldoutGroups[title] = foldoutGroup = new HashSet<string>();
            }
            var notFoldout = foldoutGroup.Contains(child);
            var newNotFoldout = !EditorGUILayout.Foldout(!notFoldout, title); // default open
            if (newNotFoldout != notFoldout)
            {
                if (newNotFoldout)
                {
                    foldoutGroup.Add(child);
                }
                else
                {
                    foldoutGroup.Remove(child);
                }
            }
            return !newNotFoldout;
        }

        protected void AddItemButton<T>(Func<IList<T>> getChildren, Func<IEnumerable<T>> getExistChildren, Action<T> onAdd, Action<T> onRemove)
        {
            var rect = EditorGUILayout.GetControlRect(GUILayout.Width(20));
            if (GUI.Button(rect, "+"))
            {
                PopupWindow.Show(rect, new ListPopupWindow<T>(getChildren(), getExistChildren) { OnAdd = onAdd, OnRemove = onRemove });
            }
        }

        protected void AddItemButton<T>(Func<IList<ListTreeViewItemContainer<T>>> getChildren, Func<IEnumerable<T>> getExistChildren, Action<T> onAdd, Action<T> onRemove)
        {
            var rect = EditorGUILayout.GetControlRect(GUILayout.Width(20));
            if (GUI.Button(rect, "+"))
            {
                PopupWindow.Show(rect, new ListPopupWindow<T>(getChildren(), getExistChildren) { OnAdd = onAdd, OnRemove = onRemove });
            }
        }

        protected bool FoldoutHeaderWithAddItemButton<T>(string child, string title, bool hasChildren, Func<IList<T>> getChildren, Func<IEnumerable<T>> getExistChildren, Action<T> onAdd, Action<T> onRemove)
        {
            EditorGUILayout.BeginHorizontal();
            var foldout = FoldoutHeader(child, title, hasChildren);
            AddItemButton(getChildren, getExistChildren, onAdd, onRemove);
            EditorGUILayout.EndHorizontal();
            return foldout;
        }

        protected bool FoldoutHeaderWithAddItemButton<T>(string child, string title, bool hasChildren, Func<IList<ListTreeViewItemContainer<T>>> getChildren, Func<IEnumerable<T>> getExistChildren, Action<T> onAdd, Action<T> onRemove)
        {
            EditorGUILayout.BeginHorizontal();
            var foldout = FoldoutHeader(child, title, hasChildren);
            AddItemButton(getChildren, getExistChildren, onAdd, onRemove);
            EditorGUILayout.EndHorizontal();
            return foldout;
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

        protected Texture2D TextureField(string label, Texture2D texture2D)
        {
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                var newValue = EditorGUILayout.ObjectField(label, texture2D, typeof(Texture2D), false, GUILayout.Height(18)) as Texture2D;
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
