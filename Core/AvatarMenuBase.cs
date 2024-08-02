using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using net.narazaka.avatarmenucreator.util;
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
        public abstract void ReplaceStoredChild(string oldChild, string newChild);
        public abstract void FilterStoredTargets(IEnumerable<string> children);
        public abstract void RemoveStoredChild(string child);
        protected abstract void OnHeaderGUI(IList<string> children);
        protected abstract void OnMainGUI(IList<string> children);
        protected abstract void OnMultiGUI(SerializedProperty serializedProperty);
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

        public void OnMultiAvatarMenuGUI(SerializedProperty serializedProperty)
        {
            using (var scrollView = new EditorGUILayout.ScrollViewScope(ScrollPosition))
            {
                ScrollPosition = scrollView.scrollPosition;
                OnMultiGUI(serializedProperty);
            }
        }

        protected void ShowTransitionSeconds()
        {
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                var newTransitionSeconds = EditorGUILayout.FloatField(T.徐々に変化_start_秒数_end_, TransitionSeconds);
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
                    EditorGUILayout.HelpBox(T.徐々に変化するものの指定が有りません, MessageType.Warning);
                }
                else
                {
                    EditorGUILayout.HelpBox(T.指定時間かけて変化します, MessageType.Info);
                }
            }
        }

        protected void ShowTransitionSecondsMulti(SerializedProperty serializedProperty)
        {
            var transitionSeconds = serializedProperty.FindPropertyRelative(nameof(TransitionSeconds));
            EditorGUILayout.PropertyField(transitionSeconds, new GUIContent(T.徐々に変化_start_秒数_end_));
            if (transitionSeconds.floatValue < 0) transitionSeconds.floatValue = 0;
        }

        protected void ShowSaved()
        {
            Saved = Toggle(T.パラメーター保存, Saved);
        }

        protected void ShowSavedMulti(SerializedProperty serializedProperty)
        {
            EditorGUILayout.PropertyField(serializedProperty.FindPropertyRelative(nameof(Saved)), new GUIContent(T.パラメーター保存));
        }

        protected void ShowDetailMenu()
        {
            ParameterName = TextField(T.パラメーター名_start_オプショナル_end_, ParameterName);
            var internalParameterLabel =
#if UNITY_2022_1_OR_NEWER && !NET_NARAZAKA_VRCHAT_AvatarMenuCreator_HAS_MA_BEFORE_1_8
                T.パラメーター自動リネーム;
#else
                T.パラメーター内部値;
#endif
            InternalParameter = Toggle(internalParameterLabel, InternalParameter);
        }

        protected void ShowDetailMenuMulti(SerializedProperty serializedProperty)
        {
            // EditorGUILayout.PropertyField(serializedProperty.FindPropertyRelative(nameof(ParameterName)), new GUIContent(T.パラメーター名_start_オプショナル_end_));
            var internalParameterLabel =
#if UNITY_2022_1_OR_NEWER && !NET_NARAZAKA_VRCHAT_AvatarMenuCreator_HAS_MA_BEFORE_1_8
                T.パラメーター自動リネーム;
#else
                T.パラメーター内部値;
#endif
            EditorGUILayout.PropertyField(serializedProperty.FindPropertyRelative(nameof(InternalParameter)), new GUIContent(internalParameterLabel));
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

        protected int IntField(int value)
        {
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                var newValue = EditorGUILayout.IntField(value);
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

        protected Texture2D TextureField(Texture2D texture2D)
        {
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                var newValue = EditorGUILayout.ObjectField(texture2D, typeof(Texture2D), false, GUILayout.Height(18)) as Texture2D;
                if (check.changed)
                {
                    WillChange();
                }
                return newValue;
            }
        }

        protected void HorizontalLine()
        {
            var c = GUI.color;
            GUI.color = Color.gray;
            GUILayout.Box(GUIContent.none, new GUIStyle
            {
                normal = { background = EditorGUIUtility.whiteTexture },
                margin = new RectOffset(10, 10, 10, 10),
                fixedHeight = 1
            });
            GUI.color = c;
        }
#endif
    }
}
