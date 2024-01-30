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
        IncludeAssetType IncludeAssetType = IncludeAssetType.AnimatorAndInclude;

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
            var newBulkSet = EditorGUILayout.ToggleLeft("同名パラメーターや同マテリアルスロットを一括設定", BulkSet, BulkSet ? EditorStyles.boldLabel : EditorStyles.label);
            if (newBulkSet != BulkSet)
            {
                UndoUtility.RecordObject(this, "BulkSet");
                BulkSet = newBulkSet;
                AvatarToggleMenu.BulkSet = BulkSet;
                AvatarChooseMenu.BulkSet = BulkSet;
                AvatarRadialMenu.BulkSet = BulkSet;
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
                EditorGUILayout.LabelField("対象のアバターを選択して下さい");
                return;
            }

            var children = GetChildren();

            if (children.Length == 0 || (children.Length == 1 && selectedGameObjects[0] == VRCAvatarDescriptor.gameObject))
            {
                EditorGUILayout.LabelField("対象のオブジェクトを選択して下さい");
                return;
            }

            var newMenuType = (MenuType)EditorGUILayout.EnumPopup(MenuType);
            if (newMenuType != MenuType)
            {
                UndoUtility.RecordObject(this, "MenuType");
                MenuType = newMenuType;
            }

            ShowBulkSet();
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

            var newIncludeAssetType = (IncludeAssetType)EditorGUILayout.EnumPopup("保存形式", IncludeAssetType);
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
                var newBaseName = EditorGUILayout.TextField("名前", BaseName);
                if (newBaseName != BaseName)
                {
                    UndoUtility.RecordObject(this, "BaseName");
                    BaseName = newBaseName;
                }
            }
            using (new EditorGUI.DisabledScope(isComponent && string.IsNullOrEmpty(BaseName)))
            {
                if (GUILayout.Button("Create!"))
                {
                    AvatarMenuBase avatarMenu = MenuType == MenuType.Toggle ? AvatarToggleMenu as AvatarMenuBase : MenuType == MenuType.Choose ? AvatarChooseMenu as AvatarMenuBase : AvatarRadialMenu as AvatarMenuBase;

                    if (isComponent)
                    {
#if NET_NARAZAKA_VRCHAT_AvatarMenuCreator_HAS_NDMF
                        SaveAsComponent(avatarMenu, children);
#endif
                    }
                    else
                    {
                        var basePath = EditorUtility.SaveFilePanelInProject("保存場所", "New Menu", "prefab", "アセットの保存場所", SaveFolder);
                        if (string.IsNullOrEmpty(basePath)) return;
                        basePath = new System.Text.RegularExpressions.Regex(@"\.prefab").Replace(basePath, "");
                        var baseName = System.IO.Path.GetFileNameWithoutExtension(basePath);
                        var createAvatarMenu = CreateAvatarMenuBase.GetCreateAvatarMenu(avatarMenu);
                        createAvatarMenu.CreateAssets(baseName, children).SaveAssets(IncludeAssetType, basePath);
                        SaveFolder = System.IO.Path.GetDirectoryName(basePath);
                    }
                }
            }
        }

#if NET_NARAZAKA_VRCHAT_AvatarMenuCreator_HAS_NDMF
        public void SaveAsComponent(AvatarMenuBase avatarMenu, string[] children)
        {
            var obj = new GameObject(BaseName);
            var creator = AddMenuCreatorComponent(obj, avatarMenu);
            creator.AvatarMenu.FilterStoredTargets(children);
            obj.transform.SetParent(VRCAvatarDescriptor.transform, false);
            Undo.RegisterCreatedObjectUndo(obj, "Create Component");
        }

        AvatarMenuCreatorBase AddMenuCreatorComponent(GameObject obj, AvatarMenuBase avatarMenu)
        {
            switch (avatarMenu)
            {
                case AvatarToggleMenu a:
                    {
                        var creator = obj.AddComponent<AvatarToggleMenuCreator>();
                        creator.AvatarToggleMenu = a.DeepCopy();
                        return creator;
                    }
                case AvatarChooseMenu a:
                    {
                        var creator = obj.AddComponent<AvatarChooseMenuCreator>();
                        creator.AvatarChooseMenu = a.DeepCopy();
                        return creator;
                    }
                case AvatarRadialMenu a:
                    {
                        var creator = obj.AddComponent<AvatarRadialMenuCreator>();
                        creator.AvatarRadialMenu = a.DeepCopy();
                        return creator;
                    }
                default:
                    throw new System.ArgumentException($"unknown menu type");
            }
        }
#endif
    }
}
