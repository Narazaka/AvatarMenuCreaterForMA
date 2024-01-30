#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace net.narazaka.avatarmenucreator
{
    public class ListPopupWindow : PopupWindowContent
    {
        public Action<string> OnAdd;
        public Action<string> OnRemove;
        IList<string> Items;
        Func<IEnumerable<string>> GetExistItems;
        SearchField SearchField;
        string SearchQuery;
        ListTreeView TreeView;

        public ListPopupWindow(IList<string> items, Func<IEnumerable<string>> getExistItems)
        {
            Items = items;
            GetExistItems = getExistItems;
        }

        public override void OnGUI(Rect rect)
        {
            if (SearchField == null) SearchField = new SearchField();
            SearchQuery = SearchField.OnGUI(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), SearchQuery);

            rect.y += EditorGUIUtility.singleLineHeight;
            rect.height -= EditorGUIUtility.singleLineHeight;

            if (TreeView == null)
            {
                TreeView = new ListTreeView(new TreeViewState(), Items, GetExistItems)
                {
                    OnAdd = OnAdd,
                    OnRemove = OnRemove,
                };
            }
            TreeView.searchString = SearchQuery;
            TreeView.OnGUI(rect);
        }
    }
}
#endif
