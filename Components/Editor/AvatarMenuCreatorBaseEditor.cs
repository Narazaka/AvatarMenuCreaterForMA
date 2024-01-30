using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using VRC.SDK3.Avatars.Components;
using net.narazaka.avatarmenucreator.editor.util;

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

        void OnEnable()
        {
            Creator = target as AvatarMenuCreatorBase;
            Creator.AvatarMenu.UndoObject = Creator;
            Children = Creator.AvatarMenu.GetStoredChildren().ToList();
        }

        public override void OnInspectorGUI()
        {
            var baseObject = GetParentAvatar();
            var child = EditorGUILayout.ObjectField("Add Children", null, typeof(GameObject), true) as GameObject;
            if (child != null)
            {
                UndoUtility.RecordObject(this, "Add Children");
                Children.Add(Util.ChildPath(baseObject, child));
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
                        if (baseObject.transform.Find(c) == null)
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
            return Creator.GetComponentInParent<VRCAvatarDescriptor>().gameObject;
        }
    }
}

