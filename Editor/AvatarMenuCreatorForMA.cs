using UnityEngine;
using UnityEditor;
using VRC.SDK3.Avatars.Components;
using net.narazaka.avatarmenucreator.editor.util;
#if NET_NARAZAKA_VRCHAT_AvatarMenuCreator_HAS_NDMF
using net.narazaka.avatarmenucreator.components;
#endif

namespace net.narazaka.avatarmenucreator.editor
{
    public class AvatarMenuCreatorForMA : EditorWindow
    {
        [SerializeField]
        VRCAvatarDescriptor VRCAvatarDescriptor;
        [SerializeField]
        MenuType MenuType = MenuType.Toggle;
        [SerializeField]
        IncludeAssetType IncludeAssetType =
#if NET_NARAZAKA_VRCHAT_AvatarMenuCreator_HAS_NDMF
            IncludeAssetType.Component;
#else
            IncludeAssetType.AnimatorAndInclude;
#endif

        [SerializeField]
        AvatarToggleMenu AvatarToggleMenu = new AvatarToggleMenu();
        [SerializeField]
        AvatarChooseMenu AvatarChooseMenu = new AvatarChooseMenu();
        [SerializeField]
        AvatarRadialMenu AvatarRadialMenu = new AvatarRadialMenu();

        [SerializeField]
        bool BulkSet;

        [SerializeField]
        string BaseName;
        [SerializeField]
        bool MakeMultipleObjects;

        string SaveFolder = "Assets";

        [MenuItem("Tools/Modular Avatar/AvatarMenuCreator for Modular Avatar")]
        static void CreateWindow()
        {
            GetWindow<AvatarMenuCreatorForMA>("AvatarMenuCreator for Modular Avatar");
        }

        void OnEnable()
        {
            AvatarToggleMenu.UndoObject = this;
            AvatarChooseMenu.UndoObject = this;
            AvatarRadialMenu.UndoObject = this;
        }

        void Update()
        {
            Repaint();
        }

        void ShowBulkSet()
        {
            var newBulkSet = EditorGUILayout.ToggleLeft(T.同名パラメーターや同マテリアルスロットを一括設定, BulkSet, BulkSet ? EditorStyles.boldLabel : EditorStyles.label);
            if (newBulkSet != BulkSet)
            {
                UndoUtility.RecordObject(this, "BulkSet");
                BulkSet = newBulkSet;
            }
        }

        GameObject[] selectedGameObjects;
        string[] children;
        string[] GetChildren()
        {
            if (selectedGameObjects == Selection.gameObjects && children != null)
            {
                return children;
            }
            AvatarToggleMenu.ClearGameObjectCache();
            AvatarChooseMenu.ClearGameObjectCache();
            AvatarRadialMenu.ClearGameObjectCache();
            selectedGameObjects = Selection.gameObjects;
            children = new string[selectedGameObjects.Length];
            for (int i = 0; i < selectedGameObjects.Length; i++)
            {
                children[i] = util.Util.ChildPath(VRCAvatarDescriptor.gameObject, selectedGameObjects[i]);
            }
            return children;
        }

        void OnGUI()
        {

            var newVRCAvatarDescriptor = EditorGUILayout.ObjectField("Avatar", VRCAvatarDescriptor, typeof(VRCAvatarDescriptor), true) as VRCAvatarDescriptor;
            if (newVRCAvatarDescriptor != VRCAvatarDescriptor)
            {
                UndoUtility.RecordObject(this, "VRCAvatarDescriptor");
                VRCAvatarDescriptor = newVRCAvatarDescriptor;
            }

            if (VRCAvatarDescriptor == null)
            {
                VRCAvatarDescriptor = null;
                EditorGUILayout.LabelField(T.対象のアバターを選択して下さい);
                return;
            }

            var children = GetChildren();

            if (children.Length == 0 || (children.Length == 1 && selectedGameObjects[0] == VRCAvatarDescriptor.gameObject))
            {
                EditorGUILayout.LabelField(T.対象のオブジェクトを選択して下さい);
                return;
            }

            var newMenuType = (MenuType)ToolbarUtility.Toolbar(MenuType);
            if (newMenuType != MenuType)
            {
                UndoUtility.RecordObject(this, "MenuType");
                MenuType = newMenuType;
            }

            ShowBulkSet();
            AvatarToggleMenu.BulkSet = BulkSet;
            AvatarChooseMenu.BulkSet = BulkSet;
            AvatarRadialMenu.BulkSet = BulkSet;
            AvatarToggleMenu.BaseObject = VRCAvatarDescriptor.gameObject;
            AvatarChooseMenu.BaseObject = VRCAvatarDescriptor.gameObject;
            AvatarRadialMenu.BaseObject = VRCAvatarDescriptor.gameObject;

            if (MenuType == MenuType.Toggle)
            {
                AvatarToggleMenu.OnAvatarMenuGUI(children);
            }
            else if (MenuType == MenuType.Choose)
            {
                AvatarChooseMenu.OnAvatarMenuGUI(children);
            }
            else
            {
                AvatarRadialMenu.OnAvatarMenuGUI(children);
            }

            var newIncludeAssetType = (IncludeAssetType)EnumPopupUtility.EnumPopup(T.保存形式, IncludeAssetType);
            if (newIncludeAssetType != IncludeAssetType)
            {
                UndoUtility.RecordObject(this, "IncludeAssetType");
                IncludeAssetType = newIncludeAssetType;
            }
            var isComponent =
#if NET_NARAZAKA_VRCHAT_AvatarMenuCreator_HAS_NDMF
                IncludeAssetType == IncludeAssetType.Component;
#else
                false;
#endif
            if (isComponent)
            {
                if (MenuType != MenuType.Choose)
                {
                    var newMakeMultipleObjects = EditorGUILayout.ToggleLeft(T.選択オブジェクト一つごとにメニューを作成, MakeMultipleObjects);
                    if (newMakeMultipleObjects != MakeMultipleObjects)
                    {
                        UndoUtility.RecordObject(this, "MakeMultipleObjects");
                        MakeMultipleObjects = newMakeMultipleObjects;
                    }
                }
                if (!MakeMultipleObjectsEffective)
                {
                    var newBaseName = EditorGUILayout.TextField(T.名前, BaseName);
                    if (newBaseName != BaseName)
                    {
                        UndoUtility.RecordObject(this, "BaseName");
                        BaseName = newBaseName;
                    }
                }
            }
            using (new EditorGUI.DisabledScope(isComponent && !MakeMultipleObjects && string.IsNullOrEmpty(BaseName)))
            {
                if (GUILayout.Button("Create!"))
                {
                    AvatarMenuBase avatarMenu = MenuType == MenuType.Toggle ? AvatarToggleMenu as AvatarMenuBase : MenuType == MenuType.Choose ? AvatarChooseMenu as AvatarMenuBase : AvatarRadialMenu as AvatarMenuBase;
                    avatarMenu.IncludeAssetType = IncludeAssetType;

                    if (isComponent)
                    {
#if NET_NARAZAKA_VRCHAT_AvatarMenuCreator_HAS_NDMF
                        if (MakeMultipleObjectsEffective)
                        {
                            foreach (var child in children)
                            {
                                SaveAsComponent(avatarMenu, System.IO.Path.GetFileName(child), new string[] { child });
                            }
                        }
                        else
                        {
                            SaveAsComponent(avatarMenu, BaseName, children);
                        }
#endif
                    }
                    else
                    {
                        System.Action<GameObject> modifyPrefab =
#if NET_NARAZAKA_VRCHAT_AvatarMenuCreator_HAS_NDMF
                            (GameObject prefab) =>
                            {
                                var creator = CreateAvatarMenuBase.GetOrAddMenuCreatorComponent(prefab, avatarMenu);
                                creator.AvatarMenu.FilterStoredTargets(children);
                            };
#else
                            (GameObject prefab) => { };
#endif
                        var prefabPath = EditorUtility.SaveFilePanelInProject(T.保存場所, "New Menu", "prefab", T.保存場所, SaveFolder);
                        if (string.IsNullOrEmpty(prefabPath)) return;
                        var (basePath, baseName) = Util.GetBasePathAndNameFromPrefabPath(prefabPath);
                        var createAvatarMenu = CreateAvatarMenuBase.GetCreateAvatarMenu(avatarMenu);
                        createAvatarMenu.CreateAssets(baseName, children).SaveAssets(IncludeAssetType, basePath, modifyPrefab);
                        SaveFolder = System.IO.Path.GetDirectoryName(basePath);
                    }
                }
            }
        }

        public bool MakeMultipleObjectsEffective => MakeMultipleObjects && MenuType != MenuType.Choose;

#if NET_NARAZAKA_VRCHAT_AvatarMenuCreator_HAS_NDMF
        public void SaveAsComponent(AvatarMenuBase avatarMenu, string baseName, string[] children)
        {
            var obj = new GameObject(baseName);
            var creator = CreateAvatarMenuBase.GetOrAddMenuCreatorComponent(obj, avatarMenu);
            creator.AvatarMenu.FilterStoredTargets(children);
            obj.transform.SetParent(VRCAvatarDescriptor.transform, false);
            Undo.RegisterCreatedObjectUndo(obj, "Create Component");
        }
#endif
    }
}
