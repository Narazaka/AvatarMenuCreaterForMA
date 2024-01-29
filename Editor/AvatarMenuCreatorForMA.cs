using UnityEngine;
using UnityEditor;
using VRC.SDK3.Avatars.Components;

namespace net.narazaka.avatarmenucreator.editor
{
    public class AvatarMenuCreatorForMA : EditorWindow
    {
        VRCAvatarDescriptor VRCAvatarDescriptor;
        MenuType MenuType = MenuType.Toggle;
        IncludeAssetType IncludeAssetType = IncludeAssetType.AnimatorAndInclude;

        AvatarToggleMenu AvatarToggleMenu = new AvatarToggleMenu();
        AvatarChooseMenu AvatarChooseMenu = new AvatarChooseMenu();
        AvatarRadialMenu AvatarRadialMenu = new AvatarRadialMenu();

        bool BulkSet;

        string SaveFolder = "Assets";

        [MenuItem("Tools/Modular Avatar/AvatarMenuCreator for Modular Avatar")]
        static void CreateWindow()
        {
            GetWindow<AvatarMenuCreatorForMA>("AvatarMenuCreator for Modular Avatar");
        }

        void Update()
        {
            Repaint();
        }

        void ShowBulkSet()
        {
            BulkSet = EditorGUILayout.ToggleLeft("同名パラメーターや同マテリアルスロットを一括設定", BulkSet);
            AvatarToggleMenu.BulkSet = BulkSet;
            AvatarChooseMenu.BulkSet = BulkSet;
            AvatarRadialMenu.BulkSet = BulkSet;
        }

        void OnGUI()
        {
            VRCAvatarDescriptor = EditorGUILayout.ObjectField("Avatar", VRCAvatarDescriptor, typeof(VRCAvatarDescriptor), true) as VRCAvatarDescriptor;

            if (VRCAvatarDescriptor == null)
            {
                EditorGUILayout.LabelField("対象のアバターを選択して下さい");
                return;
            }

            var gameObjects = Selection.gameObjects;

            if (gameObjects.Length == 0 || (gameObjects.Length == 1 && gameObjects[0] == VRCAvatarDescriptor.gameObject))
            {
                EditorGUILayout.LabelField("対象のオブジェクトを選択して下さい");
                return;
            }

            MenuType = (MenuType)EditorGUILayout.EnumPopup(MenuType);

            ShowBulkSet();
            AvatarToggleMenu.BaseObject = VRCAvatarDescriptor.gameObject;
            AvatarChooseMenu.BaseObject = VRCAvatarDescriptor.gameObject;
            AvatarRadialMenu.BaseObject = VRCAvatarDescriptor.gameObject;

            if (MenuType == MenuType.Toggle)
            {
                AvatarToggleMenu.OnAvatarMenuGUI(gameObjects);
            }
            else if (MenuType == MenuType.Choose)
            {
                AvatarChooseMenu.OnAvatarMenuGUI(gameObjects);
            }
            else
            {
                AvatarRadialMenu.OnAvatarMenuGUI(gameObjects);
            }

            IncludeAssetType = (IncludeAssetType)EditorGUILayout.EnumPopup("保存形式", IncludeAssetType);
            if (GUILayout.Button("Create!"))
            {
                var basePath = EditorUtility.SaveFilePanelInProject("保存場所", "New Menu", "prefab", "アセットの保存場所", SaveFolder);
                if (string.IsNullOrEmpty(basePath)) return;
                basePath = new System.Text.RegularExpressions.Regex(@"\.prefab").Replace(basePath, "");
                var baseName = System.IO.Path.GetFileNameWithoutExtension(basePath);
                if (MenuType == MenuType.Toggle)
                {
                    AvatarToggleMenu.CreateAssets(IncludeAssetType, baseName, basePath, gameObjects);
                }
                else if (MenuType == MenuType.Choose)
                {
                    AvatarChooseMenu.CreateAssets(IncludeAssetType, baseName, basePath, gameObjects);
                }
                else
                {
                    AvatarRadialMenu.CreateAssets(IncludeAssetType, baseName, basePath, gameObjects);
                }
                SaveFolder = System.IO.Path.GetDirectoryName(basePath);
            }
        }
    }
}
