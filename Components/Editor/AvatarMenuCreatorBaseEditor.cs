using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using VRC.SDK3.Avatars.Components;
using net.narazaka.avatarmenucreator.editor.util;
using nadena.dev.modular_avatar.core;
using net.narazaka.avatarmenucreator.editor;
using System;
using System.ComponentModel;

namespace net.narazaka.avatarmenucreator.components.editor
{
    [CustomEditor(typeof(AvatarMenuCreatorBase), true)]
    public class AvatarMenuCreatorBaseEditor : Editor
    {
        AvatarMenuCreatorBase Creator;

        [SerializeField]
        bool BulkSet;
        [SerializeField]
        List<string> Children;
        [NonSerialized]
        bool FoldoutSave;
        [NonSerialized]
        bool FoldoutRestore;

        void OnEnable()
        {
            Creator = target as AvatarMenuCreatorBase;
            Creator.AvatarMenu.UndoObject = Creator;
            Children = Creator.AvatarMenu.GetStoredChildren().ToList();
        }

        public override void OnInspectorGUI()
        {
            var baseObject = GetParentAvatar();

            var hasAssets = !Creator.IsEffective;
            if (hasAssets)
            {
                EditorGUILayout.HelpBox("MA Merge AnimatorまたはMA Parametersがある場合、 このコンポーネントは影響せずそれらの設定がそのまま使われます。", MessageType.Info);
            }
            else
            {
                var maMenuInstaller = Creator.GetComponent<ModularAvatarMenuInstaller>();
                if (maMenuInstaller?.menuToAppend != null)
                {
                    EditorGUILayout.HelpBox("MA Menu Installerのプレハブ開発者向け設定/インストールされるメニューが設定されていますが、無視されます。", MessageType.Warning);
                }

                EditorGUILayout.HelpBox("MA Menu Installerのインストール先にインストールされます。\nMA Menu Installer が無い場合は MA Menu Item のように振る舞います。 （ネストしたメニューなどに便利）", MessageType.Info);
            }

            if (PrefabUtility.GetOutermostPrefabInstanceRoot(Creator.gameObject) == Creator.gameObject)
            {
                if (FoldoutSave = EditorGUILayout.Foldout(FoldoutSave, hasAssets ? "アセット生成/復元（オプショナル）" : "アセット生成（オプショナル）"))
                {
                    using (new EditorGUI.IndentLevelScope())
                    {
                        var newIncludeAssetType = (IncludeAssetType)EditorGUILayout.EnumPopup("保存形式", Creator.AvatarMenu.IncludeAssetType);
                        if (newIncludeAssetType != Creator.AvatarMenu.IncludeAssetType)
                        {
                            UndoUtility.RecordObject(Creator, "change IncludeAssetType");
                            Creator.AvatarMenu.IncludeAssetType = newIncludeAssetType;
                        }
                        EditorGUI.BeginDisabledGroup(Creator.AvatarMenu.IncludeAssetType == IncludeAssetType.Component);
                        if (GUILayout.Button(hasAssets ? "この設定でアセットを再生成" : "この設定でアセットを生成"))
                        {
                            var prefabPath = AssetDatabase.GetAssetPath(Creator.gameObject);
                            if (string.IsNullOrEmpty(prefabPath))
                            {
                                var prefab = PrefabUtility.GetCorrespondingObjectFromSource(Creator.gameObject);
                                prefabPath = AssetDatabase.GetAssetPath(prefab);
                            }
                            if (string.IsNullOrEmpty(prefabPath))
                            {
                                return;
                            }
                            var (basePath, baseName) = Util.GetBasePathAndNameFromPrefabPath(prefabPath);
                            var createAvatarMenu = CreateAvatarMenuBase.GetCreateAvatarMenu(Creator.AvatarMenu);
                            createAvatarMenu.CreateAssets(baseName).SaveAssets(Creator.AvatarMenu.IncludeAssetType, basePath, (prefab) => CreateAvatarMenuBase.GetOrAddMenuCreatorComponent(prefab, Creator.AvatarMenu));
                        }
                        EditorGUI.EndDisabledGroup();
                        if (hasAssets)
                        {
                            EditorGUILayout.HelpBox("設定を変えてアセットを再生成出来ます", MessageType.Info);
                        }
                        else
                        {
                            EditorGUILayout.HelpBox("手動編集用にアセットを生成できます", MessageType.Info);
                        }
                        if (FoldoutRestore = EditorGUILayout.Foldout(FoldoutRestore, "アセットから設定を復元（オプショナル）"))
                        {
                            using (new EditorGUI.IndentLevelScope())
                            {
                                if (GUILayout.Button("アセット内容から設定を復元") && Restore())
                                {
                                    return;
                                }
                                EditorGUILayout.HelpBox("アセットを手動で編集していた場合などは正確な復元にならない可能性があります。", MessageType.Warning);
                            }
                        }
                    }
                }
                EditorGUILayout.Space();
            }
            else if (baseObject != null)
            {
                if (FoldoutSave = EditorGUILayout.Foldout(FoldoutSave, "アセット生成（オプショナル）"))
                {
                    EditorGUILayout.HelpBox("このオブジェクトをprefabにすると手動編集用にアセットを生成できます", MessageType.Info);
                }
                EditorGUILayout.Space();
            }

            if (baseObject == null)
            {
                EditorGUILayout.HelpBox("このコンポーネントが正しく動作するには、アバター内に配置する必要があります。", MessageType.Warning);
            }

            if (baseObject != null)
            {
                var child = EditorGUILayout.ObjectField("オブジェクトを追加", null, typeof(GameObject), true) as GameObject;
                if (child != null)
                {
                    UndoUtility.RecordObject(this, "Add Children");
                    Children.Add(Util.ChildPath(baseObject, child));
                }
            }

            var children = Children;
            var toRemoves = new List<string>();
            var toFilters = new HashSet<string>();
            using (new EditorGUI.IndentLevelScope())
            {
                foreach (var c in Children)
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button("×", GUILayout.Width(20)))
                        {
                            toRemoves.Add(c);
                            break;
                        }
                        if (baseObject != null && baseObject.transform.Find(c) == null)
                        {
                            EditorGUILayout.LabelField(EditorGUIUtility.IconContent("Warning"), GUILayout.Width(35));
                            toFilters.Add(c);
                        }
                        EditorGUILayout.LabelField(c);
                    }
                }
            }
            foreach (var c in toRemoves)
            {
                UndoUtility.RecordObject(this, "Remove Children");
                Children.Remove(c);
                Creator.AvatarMenu.RemoveStoredChild(c);
            }
            if (toFilters.Count > 0)
            {
                EditorGUILayout.HelpBox("Some children are not found in the avatar.", MessageType.Warning);
                children = children.Where(c => !toFilters.Contains(c)).ToList();
            }

            var newBulkSet = EditorGUILayout.ToggleLeft("同名パラメーターや同マテリアルスロットを一括設定", BulkSet, BulkSet ? EditorStyles.boldLabel : EditorStyles.label);
            if (newBulkSet != BulkSet)
            {
                UndoUtility.RecordObject(this, "BulkSet");
                BulkSet = newBulkSet;
                Creator.AvatarMenu.BulkSet = BulkSet;
            }

            Creator.AvatarMenu.BaseObject = baseObject;
            Creator.AvatarMenu.OnAvatarMenuGUI(children);
        }

        GameObject GetParentAvatar()
        {
            if (Creator == null) return null;
            return Creator.GetComponentInParent<VRCAvatarDescriptor>()?.gameObject;
        }

        bool Restore()
        {
            try
            {
                var avatarMenuBase = RestoreAvatarMenuBase<AvatarMenuBase>.RestoreAssets(Creator.gameObject);
                if (avatarMenuBase != null)
                {
                    UndoUtility.RecordObject(Creator, "Restore Assets");
                    if (avatarMenuBase is AvatarToggleMenu avatarMenu)
                    {
                        if (Creator is AvatarToggleMenuCreator toggleMenuCreator)
                        {
                            UndoUtility.RecordObject(Creator, "Restore Assets");
                            toggleMenuCreator.AvatarToggleMenu = avatarMenu;
                            return true;
                        }
                        else
                        {
                            if (EditorUtility.DisplayDialog("警告", "今のコンポーネントと別のコンポーネントに置き換わります。\n続行しますか？", "OK", "Cancel"))
                            {
                                UndoUtility.RecordObject(Creator, "Restore Assets destroy component");
                                var go = Creator.gameObject;
                                DestroyImmediate(Creator);
                                var component = go.AddComponent<AvatarToggleMenuCreator>();
                                component.AvatarToggleMenu = avatarMenu;
                                Undo.RegisterCreatedObjectUndo(component, "Restore Assets create component");
                                return true;
                            }
                        }
                    }
                    else if (avatarMenuBase is AvatarChooseMenu avatarChooseMenu)
                    {
                        if (Creator is AvatarChooseMenuCreator chooseMenuCreator)
                        {
                            UndoUtility.RecordObject(Creator, "Restore Assets");
                            chooseMenuCreator.AvatarChooseMenu = avatarChooseMenu;
                            return true;
                        }
                        else
                        {
                            if (EditorUtility.DisplayDialog("警告", "今のコンポーネントと別のコンポーネントに置き換わります。\n続行しますか？", "OK", "Cancel"))
                            {
                                UndoUtility.RecordObject(Creator, "Restore Assets destroy component");
                                var go = Creator.gameObject;
                                DestroyImmediate(Creator);
                                var component = go.AddComponent<AvatarChooseMenuCreator>();
                                component.AvatarChooseMenu = avatarChooseMenu;
                                Undo.RegisterCreatedObjectUndo(component, "Restore Assets create component");
                                return true;
                            }
                        }
                    }
                    else if (avatarMenuBase is AvatarRadialMenu avatarRadialMenu)
                    {
                        if (Creator is AvatarRadialMenuCreator radialMenuCreator)
                        {
                            UndoUtility.RecordObject(Creator, "Restore Assets");
                            radialMenuCreator.AvatarRadialMenu = avatarRadialMenu;
                        }
                        else
                        {
                            if (EditorUtility.DisplayDialog("警告", "今のコンポーネントと別のコンポーネントに置き換わります。\n続行しますか？", "OK", "Cancel"))
                            {
                                UndoUtility.RecordObject(Creator, "Restore Assets destroy component");
                                var go = Creator.gameObject;
                                DestroyImmediate(Creator);
                                var component = go.AddComponent<AvatarRadialMenuCreator>();
                                component.AvatarRadialMenu = avatarRadialMenu;
                                Undo.RegisterCreatedObjectUndo(component, "Restore Assets create component");
                                return true;
                            }
                        }
                    }
                }
                else
                {
                    EditorUtility.DisplayDialog("復元に失敗しました", "適切な復元法が見つかりません", "OK");
                }
            }
            catch (Exception e)
            {
                EditorUtility.DisplayDialog("復元に失敗しました", e.ToString(), "OK");
            }
            return false;
        }
    }
}
