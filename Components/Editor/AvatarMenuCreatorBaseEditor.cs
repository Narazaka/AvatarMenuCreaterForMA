using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using VRC.SDK3.Avatars.Components;
using net.narazaka.avatarmenucreator.util;
using nadena.dev.modular_avatar.core;
using net.narazaka.avatarmenucreator.editor;
using System;

namespace net.narazaka.avatarmenucreator.components.editor
{
    [CanEditMultipleObjects]
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
        string EditChild;
        [NonSerialized]
        string EditChildNew;

        void OnEnable()
        {
            Creator = target as AvatarMenuCreatorBase;
            Creator.AvatarMenu.UndoObject = Creator;
            UpdateChildren();
        }

        void UpdateChildren()
        {
            Children = Creator.AvatarMenu.GetStoredChildren().ToList();
        }

        public override void OnInspectorGUI()
        {
            if (targets.Length == 1)
            {
                OnInspectorGUISingle();
            }
            else
            {
                OnInspectorGUIMultiple();
            }
        }

        void OnInspectorGUISingle()
        {
            var baseObject = GetParentAvatar();

            var hasAssets = !Creator.IsEffective;
            if (hasAssets)
            {
                EditorGUILayout.HelpBox(T.MA_Merge_AnimatorまたはMA_Parametersがある場合ヽ_このコンポーネントは影響せずそれらの設定がそのまま使われますゝ, MessageType.Info);
                if (GUILayout.Button(T.コンポーネントの設定を優先する) && EditorUtility.DisplayDialog(T.本当に削除しますか_Q_, T.MA_Merge_AnimatorとMA_Parametersを削除しますゝ_n_MA_Menu_Installerのインストールされるメニューをリセットしますゝ, "OK", "Cancel"))
                {
                    var mergeAnimator = Creator.GetComponent<ModularAvatarMergeAnimator>();
                    if (mergeAnimator != null)
                    {
                        Undo.DestroyObjectImmediate(mergeAnimator);
                    }
                    var parameters = Creator.GetComponent<ModularAvatarParameters>();
                    if (parameters != null)
                    {
                        Undo.DestroyObjectImmediate(parameters);
                    }
                    var maMenuInstaller = Creator.GetComponent<ModularAvatarMenuInstaller>();
                    if (maMenuInstaller != null)
                    {
                        Undo.RecordObject(maMenuInstaller, "Remove MA Components");
                        maMenuInstaller.menuToAppend = null;
                    }
                }
            }
            else
            {
                var maMenuInstaller = Creator.GetComponent<ModularAvatarMenuInstaller>();
                if (maMenuInstaller?.menuToAppend != null)
                {
                    EditorGUILayout.HelpBox(T.MA_Menu_Installerのプレハブ開発者向け設定_sl_インストールされるメニューが設定されていますがヽ無視されますゝ, MessageType.Warning);
                }

                EditorGUILayout.HelpBox(T.MA_Menu_Installerのインストール先にインストールされますゝ_n_MA_Menu_Installer_が無い場合は_MA_Menu_Item_のように振る舞いますゝ__start_ネストしたメニューなどに便利_end_, MessageType.Info);
                if (baseObject != null)
                {
                    var parentMenuItem = Creator.transform.parent.GetComponent<ModularAvatarMenuItem>();
                    var parentMenuGroup = Creator.transform.parent.GetComponent<ModularAvatarMenuGroup>();
                    var parentChooseMenu = Creator.transform.parent.GetComponent<AvatarChooseMenuCreator>();
                    if (parentChooseMenu != null || parentMenuGroup != null || (parentMenuItem != null && parentMenuItem.Control.type == VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control.ControlType.SubMenu && parentMenuItem.MenuSource == SubmenuSource.Children))
                    {
                        if (maMenuInstaller != null)
                        {
                            if (parentMenuGroup != null)
                            {
                                EditorGUILayout.HelpBox(T.MA_Menu_Groupの配下にありますゝ_n_MA_Menu_Installerがなくてもインストールできますゝ, MessageType.Info);
                            }
                            else
                            {
                                EditorGUILayout.HelpBox(T.MA_Menu_Itemのサブメニュー配下にありますゝ_n_MA_Menu_Installerがなくてもインストールできますゝ, MessageType.Info);
                            }
                            if (GUILayout.Button(T.MA_Menu_Installerを削除) && EditorUtility.DisplayDialog(T.本当に削除しますか_Q_, T.MA_Menu_Installerを削除します, "OK", "Cancel"))
                            {
                                Undo.DestroyObjectImmediate(maMenuInstaller);
                            }
                        }
                    }
                    else
                    {
                        if (maMenuInstaller == null)
                        {
                            EditorGUILayout.HelpBox(T.MA_Menu_Itemのサブメニュー配下にありませんゝ_n_メニューの生成にはMA_Menu_Installerが必要ですゝ_n__start_メニュー生成無しでも他の手段でパラメーターを変更すれば動作します_end_, MessageType.Warning);
                            if (GUILayout.Button(T.MA_Menu_Installerを追加))
                            {
                                var installer = Creator.gameObject.AddComponent<ModularAvatarMenuInstaller>();
                                Undo.RegisterCreatedObjectUndo(installer, "Remove MA Menu Installer");
                            }
                        }
                    }
                }
            }

            if (PrefabUtility.GetOutermostPrefabInstanceRoot(Creator.gameObject) == Creator.gameObject)
            {
                if (FoldoutSave = EditorGUILayout.Foldout(FoldoutSave, hasAssets ? T.アセット生成_復元_オプショナル : T.アセット生成_オプショナル))
                {
                    using (new EditorGUI.IndentLevelScope())
                    {
                        var newIncludeAssetType = (IncludeAssetType)EnumPopupUtility.EnumPopup(T.保存形式, Creator.AvatarMenu.IncludeAssetType);
                        if (newIncludeAssetType != Creator.AvatarMenu.IncludeAssetType)
                        {
                            UndoUtility.RecordObject(Creator, "change IncludeAssetType");
                            Creator.AvatarMenu.IncludeAssetType = newIncludeAssetType;
                        }
                        if (Creator.AvatarMenu is AvatarToggleMenu toggleMenu && toggleMenu.UseAdvanced)
                        {
                            EditorGUILayout.HelpBox(T.アセットからの復元機能は高度な設定に対応していません_n_生成後のアセットから設定を復元することは出来ません, MessageType.Warning);
                        }
                        EditorGUI.BeginDisabledGroup(Creator.AvatarMenu.IncludeAssetType == IncludeAssetType.Component);
                        if (GUILayout.Button(hasAssets ? T.この設定でアセットを再生成 : T.この設定でアセットを生成))
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
                            createAvatarMenu.CreateAssets(baseName).SaveAssets(Creator.AvatarMenu.IncludeAssetType, basePath, (prefab) => CreateAvatarMenuBase.GetOrAddMenuCreatorComponent(prefab, Creator.AvatarMenu, true));
                        }
                        EditorGUI.EndDisabledGroup();
                        if (hasAssets)
                        {
                            EditorGUILayout.HelpBox(T.設定を変えてアセットを再生成出来ます, MessageType.Info);
                        }
                        else
                        {
                            EditorGUILayout.HelpBox(T.手動編集用にアセットを生成できます, MessageType.Info);
                        }
                        if (GUILayout.Button(T.アセット内容から設定を復元) && Restore(Creator.gameObject))
                        {
                            return;
                        }
                        EditorGUILayout.HelpBox(T.アセットを手動で編集していた場合などは正確な復元にならない可能性があります, MessageType.Warning);
                    }
                }
                EditorGUILayout.Space();
            }
            else if (baseObject != null)
            {
                if (FoldoutSave = EditorGUILayout.Foldout(FoldoutSave, T.アセット生成_オプショナル))
                {
                    EditorGUILayout.HelpBox(T.このオブジェクトをprefabにすると手動編集用にアセットを生成できます, MessageType.Info);
                }
                EditorGUILayout.Space();
            }

            if (baseObject == null)
            {
                EditorGUILayout.HelpBox(T.このコンポーネントが正しく動作するにはヽアバター内に配置する必要がありますゝ, MessageType.Warning);
            }

            if (baseObject != null)
            {
                var child = EditorGUILayout.ObjectField(T.オブジェクトを追加, null, typeof(GameObject), true) as GameObject;
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
                            if (EditorUtility.DisplayDialog(T.本当に削除しますか_Q_, c, "OK", "Cancel")) toRemoves.Add(c);
                            break;
                        }
                        if (GUILayout.Button(EditorGUIUtility.IconContent("editicon.sml"), GUILayout.Width(20)))
                        {
                            if (EditChild == null)
                            {
                                EditChild = c;
                                EditChildNew = c;
                            }
                            else
                            {
                                EditChild = null;
                                EditChildNew = null;
                            }
                        }
                        if (EditChild == c)
                        {
                            EditChildNew = EditorGUILayout.TextField(EditChildNew);
                            var newObj = EditorGUILayout.ObjectField(null, typeof(GameObject), true);
                            if (newObj != null)
                            {
                                var newPath = Util.ChildPath(baseObject, newObj as GameObject);
                                if (EditorUtility.DisplayDialog(T.本当に変更しますか_Q_, $"{c} -> {newPath}", "OK", "Cancel"))
                                {
                                    Creator.AvatarMenu.ReplaceStoredChild(EditChild, newPath);
                                    UpdateChildren();
                                    EditChild = null;
                                    EditChildNew = null;
                                }
                            }
                            using (new EditorGUI.DisabledScope(c == EditChildNew))
                            {
                                if (GUILayout.Button("OK"))
                                {
                                    if (EditorUtility.DisplayDialog(T.本当に変更しますか_Q_, $"{c} -> {EditChildNew}", "OK", "Cancel"))
                                    {
                                        Creator.AvatarMenu.ReplaceStoredChild(EditChild, EditChildNew);
                                        UpdateChildren();
                                        EditChild = null;
                                        EditChildNew = null;
                                    }
                                }
                            }
                        }
                        if (baseObject != null && baseObject.transform.Find(c) == null)
                        {
                            EditorGUILayout.LabelField(EditorGUIUtility.IconContent("Warning"), GUILayout.Width(35));
                            toFilters.Add(c);
                        }
                        if (EditChild != c)
                        {
                            EditorGUILayout.LabelField(c);
                        }
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
                EditorGUILayout.HelpBox(T.アバターに存在しないオブジェクト名が指定されています, MessageType.Warning);
            }

            var newBulkSet = EditorGUILayout.ToggleLeft(T.同名パラメーターや同マテリアルスロットを一括設定, BulkSet, BulkSet ? EditorStyles.boldLabel : EditorStyles.label);
            if (newBulkSet != BulkSet)
            {
                UndoUtility.RecordObject(this, "BulkSet");
                BulkSet = newBulkSet;
            }
            Creator.AvatarMenu.BulkSet = BulkSet;

            Creator.AvatarMenu.BaseObject = baseObject;
            Creator.AvatarMenu.OnAvatarMenuGUI(children);
        }

        void OnInspectorGUIMultiple()
        {
            Creator.AvatarMenu.OnMultiAvatarMenuGUI(Creator.AvatarMenuProperty(serializedObject));
        }

        GameObject GetParentAvatar()
        {
            if (Creator == null) return null;
            return Creator.GetComponentInParent<VRCAvatarDescriptor>()?.gameObject;
        }

        [MenuItem("CONTEXT/ModularAvatarParameters/AvatarMenuCreator for MA/アセット内容から設定を復元 (Restore)")]
        [MenuItem("CONTEXT/ModularAvatarMergeAnimator/AvatarMenuCreator for MA/アセット内容から設定を復元 (Restore)")]
        static void RestoreFromMenu(MenuCommand command)
        {
            var creator = command.context as Component;
            if (creator == null) return;
            Restore(creator.gameObject);
        }

        static bool Restore(GameObject gameObject)
        {
            try
            {
                var avatarMenuBase = RestoreAvatarMenuBase<AvatarMenuBase>.RestoreAssets(gameObject);
                if (avatarMenuBase == null)
                {
                    EditorUtility.DisplayDialog(T.復元に失敗しました, T.適切な復元法が見つかりません, "OK");
                    return false;
                }
                var creator = gameObject.GetComponent<AvatarMenuCreatorBase>();
                if (creator == null)
                {
                    creator = CreateAvatarMenuBase.GetOrAddMenuCreatorComponentOnly(gameObject, avatarMenuBase);
                }

                if (avatarMenuBase != null)
                {
                    UndoUtility.RecordObject(creator, "Restore Assets");
                    if (avatarMenuBase is AvatarToggleMenu avatarMenu)
                    {
                        if (creator is AvatarToggleMenuCreator toggleMenuCreator)
                        {
                            UndoUtility.RecordObject(creator, "Restore Assets");
                            toggleMenuCreator.AvatarToggleMenu = avatarMenu;
                            EditorUtility.DisplayDialog(T.成功, T.復元に成功しました, "OK");
                            return true;
                        }
                        else
                        {
                            if (EditorUtility.DisplayDialog(T.警告, T.今のコンポーネントと別のコンポーネントに置き換わりますゝ_n_続行しますか_Q_, "OK", "Cancel"))
                            {
                                UndoUtility.RecordObject(creator, "Restore Assets destroy component");
                                var go = creator.gameObject;
                                DestroyImmediate(creator);
                                var component = go.AddComponent<AvatarToggleMenuCreator>();
                                component.AvatarToggleMenu = avatarMenu;
                                Undo.RegisterCreatedObjectUndo(component, "Restore Assets create component");
                                EditorUtility.DisplayDialog(T.成功, T.復元に成功しました, "OK");
                                return true;
                            }
                        }
                    }
                    else if (avatarMenuBase is AvatarChooseMenu avatarChooseMenu)
                    {
                        if (creator is AvatarChooseMenuCreator chooseMenuCreator)
                        {
                            UndoUtility.RecordObject(creator, "Restore Assets");
                            chooseMenuCreator.AvatarChooseMenu = avatarChooseMenu;
                            EditorUtility.DisplayDialog(T.成功, T.復元に成功しました, "OK");
                            return true;
                        }
                        else
                        {
                            if (EditorUtility.DisplayDialog(T.警告, T.今のコンポーネントと別のコンポーネントに置き換わりますゝ_n_続行しますか_Q_, "OK", "Cancel"))
                            {
                                UndoUtility.RecordObject(creator, "Restore Assets destroy component");
                                var go = creator.gameObject;
                                DestroyImmediate(creator);
                                var component = go.AddComponent<AvatarChooseMenuCreator>();
                                component.AvatarChooseMenu = avatarChooseMenu;
                                Undo.RegisterCreatedObjectUndo(component, "Restore Assets create component");
                                EditorUtility.DisplayDialog(T.成功, T.復元に成功しました, "OK");
                                return true;
                            }
                        }
                    }
                    else if (avatarMenuBase is AvatarRadialMenu avatarRadialMenu)
                    {
                        if (creator is AvatarRadialMenuCreator radialMenuCreator)
                        {
                            UndoUtility.RecordObject(creator, "Restore Assets");
                            radialMenuCreator.AvatarRadialMenu = avatarRadialMenu;
                            EditorUtility.DisplayDialog(T.成功, T.復元に成功しました, "OK");
                            return true;
                        }
                        else
                        {
                            if (EditorUtility.DisplayDialog(T.警告, T.今のコンポーネントと別のコンポーネントに置き換わりますゝ_n_続行しますか_Q_, "OK", "Cancel"))
                            {
                                UndoUtility.RecordObject(creator, "Restore Assets destroy component");
                                var go = creator.gameObject;
                                DestroyImmediate(creator);
                                var component = go.AddComponent<AvatarRadialMenuCreator>();
                                component.AvatarRadialMenu = avatarRadialMenu;
                                Undo.RegisterCreatedObjectUndo(component, "Restore Assets create component");
                                EditorUtility.DisplayDialog(T.成功, T.復元に成功しました, "OK");
                                return true;
                            }
                        }
                    }
                }
                else
                {
                    EditorUtility.DisplayDialog(T.復元に失敗しました, T.適切な復元法が見つかりません, "OK");
                }
            }
            catch (AssertException e)
            {
                EditorUtility.DisplayDialog(T.復元に失敗しました, e.Message, "OK");
            }
            catch (Exception e)
            {
                EditorUtility.DisplayDialog(T.復元に失敗しました, $"{T.想定外エラーです_ツール作者_narazaka_にスクリーンショットを添えて連絡して下さい}\n\n{e}", "OK");
                Debug.LogError(e);
            }
            return false;
        }
    }
}
