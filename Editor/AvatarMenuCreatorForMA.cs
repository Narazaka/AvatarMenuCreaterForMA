using UnityEngine;
using UnityEditor;
using VRC.SDK3.Avatars.Components;
using net.narazaka.avatarmenucreator.editor.util;

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
            var newBulkSet = EditorGUILayout.ToggleLeft("同名パラメーターや同マテリアルスロットを一括設定", BulkSet);
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
            if (GUILayout.Button("Create!"))
            {
                var basePath = EditorUtility.SaveFilePanelInProject("保存場所", "New Menu", "prefab", "アセットの保存場所", SaveFolder);
                if (string.IsNullOrEmpty(basePath)) return;
                basePath = new System.Text.RegularExpressions.Regex(@"\.prefab").Replace(basePath, "");
                var baseName = System.IO.Path.GetFileNameWithoutExtension(basePath);
                AvatarMenuBase avatarMenu = MenuType == MenuType.Toggle ? AvatarToggleMenu : MenuType == MenuType.Choose ? AvatarChooseMenu : AvatarRadialMenu;
                var createAvatarMenu = CreateAvatarMenuBase.GetCreateAvatarMenu(avatarMenu);
                createAvatarMenu.CreateAssets(baseName, children).SaveAssets(IncludeAssetType, basePath);
                SaveFolder = System.IO.Path.GetDirectoryName(basePath);
            }
        }
    }
}
