﻿using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using net.narazaka.avatarmenucreator.collections.instance;

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
        public bool Synced = true;
        [SerializeField]
        public string ParameterName;
        [SerializeField]
        public bool InternalParameter = false;
        [SerializeField]
        public StringHashSet PhysBoneAutoResetDisabledObjects = new StringHashSet();

        protected bool PhysBoneAutoResetEnabled(string child) => !PhysBoneAutoResetDisabledObjects.Contains(child);
        protected void EnablePhysBoneAutoReset(string child) => PhysBoneAutoResetDisabledObjects.Remove(child);
        protected void DisablePhysBoneAutoReset(string child) => PhysBoneAutoResetDisabledObjects.Add(child);

        public virtual void Reset() { }

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
        [NonSerialized]
        public bool ShowMultiSelectInfo;

        bool? _FoldoutDetails;
        bool FoldoutDetails
        {

            get
            {
                if (_FoldoutDetails == null)
                {
                    _FoldoutDetails = !Synced || InternalParameter || !string.IsNullOrEmpty(ParameterName);
                }
                return (bool)_FoldoutDetails;
            }
            set => _FoldoutDetails = value;
        }

        bool? _FoldoutDetailsMulti;
        bool GetFoldoutDetailsMulti(SerializedProperty serializedProperty)
        {
            if (_FoldoutDetailsMulti == null)
            {
                var synced = serializedProperty.FindPropertyRelative(nameof(Synced));
                var internalParameter = serializedProperty.FindPropertyRelative(nameof(InternalParameter));
                _FoldoutDetailsMulti = synced.hasMultipleDifferentValues || !synced.boolValue || internalParameter.hasMultipleDifferentValues || internalParameter.boolValue;
            }
            return (bool)_FoldoutDetailsMulti;
        }
        bool SetFoldoutDetailsMulti(bool value)
        {
            _FoldoutDetailsMulti = value;
            return value;
        }

        HashSet<string> FoldoutGameObjects = new HashSet<string>();
        Dictionary<string, HashSet<string>> FoldoutGroups = new Dictionary<string, HashSet<string>>();
        Vector2 ScrollPosition;
        Vector2 BulkScrollPosition;
        float BulkHeight = 220;

        protected Util.ShaderParametersCache ShaderParametersCache = new Util.ShaderParametersCache();
        Dictionary<string, GameObject> GameObjectCache = new Dictionary<string, GameObject>();

        public abstract IEnumerable<string> GetStoredChildren();
        public abstract void ReplaceStoredChild(string oldChild, string newChild);
        public abstract void FilterStoredTargets(IEnumerable<string> children);
        public abstract void RemoveStoredChild(string child);
        protected abstract void OnHeaderGUI(IList<string> children);
        protected abstract bool HasHeaderBulkGUI { get; }
        protected virtual float HeaderBulkGUIHeight(IList<string> children) => 0;
        protected virtual void OnHeaderBulkGUI(IList<string> children) { }
        protected abstract void OnMainGUI(IList<string> children);
        protected abstract void OnMultiGUI(SerializedProperty serializedProperty);
        protected abstract bool IsSuitableForTransition();

        public void OnAvatarMenuGUI(IList<string> children)
        {
            OnHeaderGUI(children);

            if (HasHeaderBulkGUI && BulkSet)
            {
                if (FoldoutHeader("", T.一括設定, true))
                {
                    var controlHeight = HeaderBulkGUIHeight(children);
                    var clampedHeight = Mathf.Min(controlHeight, BulkHeight);
                    var indivisualFoldout = FoldoutGroupIsFoldout("", T.個別設定);
                    var guiLayoutOptions = indivisualFoldout ? new GUILayoutOption[] { GUILayout.Height(clampedHeight) } : new GUILayoutOption[0];
                    using (var scrollView = new EditorGUILayout.ScrollViewScope(BulkScrollPosition, guiLayoutOptions))
                    {
                        BulkScrollPosition = scrollView.scrollPosition;
                        OnHeaderBulkGUI(children);
                    }
                    if (indivisualFoldout)
                    {
                        var delta = DraggableHorizontalLine();
                        if (delta < 0 || controlHeight > BulkHeight)
                        {
                            BulkHeight += delta;
                            if (BulkHeight < 20) BulkHeight = 20;
                        }
                    }
                }
            }

            if (!HasHeaderBulkGUI || !BulkSet || FoldoutHeader("", T.個別設定, true))
            {
                using (var scrollView = new EditorGUILayout.ScrollViewScope(ScrollPosition))
                {
                    ScrollPosition = scrollView.scrollPosition;
                    OnMainGUI(children);
                    if (ShowMultiSelectInfo && children.Count == 1)
                    {
                        EditorGUILayout.HelpBox(T.ヒント_colon__複数のオブジェクトを選択して一緒に設定出来ます, MessageType.Info);
                    }
                }
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

        protected void ShowDetailMenu()
        {
            Saved = Toggle(T.パラメーター保存, Saved);
            FoldoutDetails = EditorGUILayout.Foldout(FoldoutDetails, T.オプション);
            if (!FoldoutDetails) return;
            EditorGUI.indentLevel++;
#if !NET_NARAZAKA_VRCHAT_AvatarMenuCreator_HAS_NO_MENU_MA
            Synced = Toggle(T.パラメーター同期, Synced);
#endif
            ParameterName = TextField(T.パラメーター名, ParameterName);
            var internalParameterLabel =
#if UNITY_2022_1_OR_NEWER && !NET_NARAZAKA_VRCHAT_AvatarMenuCreator_HAS_MA_BEFORE_1_8
                T.パラメーター自動リネーム;
#else
                T.パラメーター内部値;
#endif
            InternalParameter = Toggle(internalParameterLabel, InternalParameter);
            EditorGUI.indentLevel--;
        }

        protected void ShowDetailMenuMulti(SerializedProperty serializedProperty)
        {
            EditorGUILayout.PropertyField(serializedProperty.FindPropertyRelative(nameof(Saved)), new GUIContent(T.パラメーター保存));
            EditorGUI.indentLevel++;
            var foldout = SetFoldoutDetailsMulti(EditorGUILayout.Foldout(GetFoldoutDetailsMulti(serializedProperty), T.オプション));
            EditorGUI.indentLevel--;
            if (!foldout) return;
            EditorGUI.indentLevel++;
#if !NET_NARAZAKA_VRCHAT_AvatarMenuCreator_HAS_NO_MENU_MA
            EditorGUILayout.PropertyField(serializedProperty.FindPropertyRelative(nameof(Synced)), new GUIContent(T.パラメーター同期));
#endif
            // EditorGUILayout.PropertyField(serializedProperty.FindPropertyRelative(nameof(ParameterName)), new GUIContent(T.パラメーター名_start_オプショナル_end_));
            var internalParameterLabel =
#if UNITY_2022_1_OR_NEWER && !NET_NARAZAKA_VRCHAT_AvatarMenuCreator_HAS_MA_BEFORE_1_8
                T.パラメーター自動リネーム;
#else
                T.パラメーター内部値;
#endif
            EditorGUILayout.PropertyField(serializedProperty.FindPropertyRelative(nameof(InternalParameter)), new GUIContent(internalParameterLabel));
            EditorGUI.indentLevel--;
        }

        protected void ShowPhysBoneAutoResetMenu(string child, TypeMember[] childMembers)
        {
            var needResetPhysBoneField = VRCPhysBoneUtil.IsNeedResetPhysBoneField(childMembers);
            if (needResetPhysBoneField)
            {
                var hasPhysBoneField = VRCPhysBoneUtil.HasPhysBoneEnabled(childMembers);
                var physBoneAutoResetEnabled = PhysBoneAutoResetEnabled(child);
                if (hasPhysBoneField)
                {
                    EditorGUILayout.HelpBox(T.PhysBone自動リセットを有効にするには_PhysBone_dot_enabled_設定を削除して下さいゝ, MessageType.Info);
                }
                else
                {
                    EditorGUI.BeginChangeCheck();
                    physBoneAutoResetEnabled = EditorGUILayout.Toggle(T.PhysBone自動リセット, physBoneAutoResetEnabled);
                    if (EditorGUI.EndChangeCheck())
                    {
                        WillChange();
                        if (physBoneAutoResetEnabled)
                        {
                            EnablePhysBoneAutoReset(child);
                        }
                        else
                        {
                            DisablePhysBoneAutoReset(child);
                        }
                    }
                }
                if (hasPhysBoneField || !physBoneAutoResetEnabled)
                {
                    EditorGUILayout.HelpBox(T.PhysBoneをリセットしないと反映されない値が設定されていますゝPhysBone自動リセットを有効にするとリセット_start_OFF_sl_ON_end_処理を挿入しますゝ, MessageType.Warning);
                }
            }
        }

        public IEnumerable<string> GetPhysBoneAutoResetEffectiveObjects(IEnumerable<string> children, IEnumerable<(string, TypeMember)> valueKeys) =>
            valueKeys.Where(k => children.Contains(k.Item1)).GroupBy(k => k.Item1).Where(g =>
            {
                var tms = g.Select(k => k.Item2).ToArray();
                return PhysBoneAutoResetEnabled(g.Key) && !VRCPhysBoneUtil.HasPhysBoneEnabled(tms) && VRCPhysBoneUtil.IsNeedResetPhysBoneField(tms);
            }).Select(g => g.Key);

        protected GameObject GetGameObject(string child)
        {
            if (BaseObject == null) return null;
            if (GameObjectCache == null) GameObjectCache = new Dictionary<string, GameObject>();
            if (!GameObjectCache.TryGetValue(child, out var gameObjectRef) || gameObjectRef == null)
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

        protected bool FoldoutGroupIsFoldout(string child, string title)
        {
            return !FoldoutGroups.TryGetValue(title, out var foldoutGroup) || !foldoutGroup.Contains(child);
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

        protected bool Toggle(GUIContent label, bool value)
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

        protected bool ToggleLeft(string label, bool value)
        {
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                var newValue = EditorGUILayout.ToggleLeft(label, value);
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

        protected string TextField(Rect rect, string label, string value)
        {
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                var newValue = EditorGUI.TextField(rect, label, value);
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

        protected int IntField(int value, Action<int> onChange = null)
        {
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                var newValue = EditorGUILayout.IntField(value);
                if (check.changed)
                {
                    WillChange();
                    onChange?.Invoke(newValue);
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

        protected Texture2D TextureField(Rect rect, Texture2D texture2D)
        {
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                var newValue = EditorGUI.ObjectField(rect, texture2D, typeof(Texture2D), false) as Texture2D;
                if (check.changed)
                {
                    WillChange();
                }
                return newValue;
            }
        }

        // for MaterialPickerButton
        protected virtual Material[] GetMaterialSlots(string child) { throw new NotImplementedException(); }

        protected bool PickerButton()
        {
            return GUILayout.Button(EditorGUIUtility.IconContent("Grid.PickingTool"), GUILayout.Width(20), GUILayout.Height(18));
        }

        protected void MaterialPickerButton(string child, int index, ref Material value)
        {
            var go = GetGameObject(child);
            if (go == null || !PickerButton()) return;
            value = GetMaterialSlots(child)[index];
        }

        protected void BlendShapeLikePickerButton(bool isBlendShape, string child, string name, ref float value)
        {
            if (isBlendShape)
            {
                BlendShapePickerButton(child, name, ref value);
            }
            else
            {
                ShaderParameterPickerButton(child, name, ref value);
            }
        }

        void BlendShapePickerButton(string child, string name, ref float value)
        {
            var go = GetGameObject(child);
            if (go == null || !PickerButton()) return;
            var mesh = go.GetComponent<SkinnedMeshRenderer>();
            value = mesh.GetBlendShapeWeight(mesh.sharedMesh.GetBlendShapeIndex(name));
        }

        void ShaderParameterPickerButton(string child, string name, ref float value)
        {
            var go = GetGameObject(child);
            if (go == null || !PickerButton()) return;
            var mesh = go.GetComponent<Renderer>();
            foreach (var mat in mesh.sharedMaterials)
            {
                if (mat.shader.FindPropertyIndex(name) != -1)
                {
                    value = mat.GetFloat(name);
                    return;
                }
            }
        }

        protected void ShaderVectorParameterPickerButton(string child, string name, ref Vector4 value)
        {
            var go = GetGameObject(child);
            if (go == null || !PickerButton()) return;
            var mesh = go.GetComponent<Renderer>();
            foreach (var mat in mesh.sharedMaterials)
            {
                if (mat.shader.FindPropertyIndex(name) != -1)
                {
                    value = mat.GetColor(name);
                    return;
                }
            }
        }

        protected void TransformPickerButton(string child, string transformComponentName, ref Vector3 value)
        {
            var go = GetGameObject(child);
            if (go == null || !PickerButton()) return;
            var component = go.GetComponent<Transform>();
            var memberInfo = typeof(Transform).GetProperty(TransformPropertyName(transformComponentName), System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (memberInfo is System.Reflection.PropertyInfo propertyInfo)
            {
                value = (Vector3)propertyInfo.GetValue(component);
            }
        }

        string TransformPropertyName(string transformComponentName)
        {
            switch (transformComponentName)
            {
                case "Position": return "localPosition";
                case "Rotation": return "localEulerAngles";
                case "Scale": return "localScale";
                default: throw new ArgumentException();
            }
        }

        protected void ValuePickerButton(string child, TypeMember member, Action<SerializedProperty> setValue)
        {
            var go = GetGameObject(child);
            if (go == null || !PickerButton()) return;
            setValue(new SerializedObject(go.GetComponent(member.Type)).FindProperty(member.AnimationMemberName));
        }

        protected Rect HorizontalLine()
        {
            var c = GUI.color;
            GUI.color = Color.gray;
            var rect = EditorGUILayout.GetControlRect(false);
            var boxRect = rect;
            boxRect.y += boxRect.height / 2;
            boxRect.height = 1;
            GUI.Box(boxRect, GUIContent.none, new GUIStyle
            {
                normal = { background = EditorGUIUtility.whiteTexture },
                margin = new RectOffset(10, 10, 10, 10),
                fixedHeight = 1
            });
            GUI.color = c;
            return rect;
        }

        protected float DraggableHorizontalLine()
        {
            var rect = HorizontalLine();
            EditorGUIUtility.AddCursorRect(rect, MouseCursor.ResizeVertical);
            var e = Event.current;
            var controlId = GUIUtility.GetControlID(FocusType.Passive);
            float delta = 0;
            switch (e.type)
            {
                case EventType.MouseDown:
                    if (rect.Contains(e.mousePosition))
                    {
                        GUIUtility.hotControl = controlId;
                        e.Use();
                    }
                    break;
                case EventType.MouseDrag:
                    if (GUIUtility.hotControl == controlId)
                    {
                        delta = e.delta.y;
                        e.Use();
                    }
                    break;
                case EventType.MouseUp:
                    if (GUIUtility.hotControl == controlId)
                    {
                        GUIUtility.hotControl = 0;
                        e.Use();
                    }
                    break;
            }
            return delta;
        }
#endif
    }
}
