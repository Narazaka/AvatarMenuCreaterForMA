﻿#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace net.narazaka.avatarmenucreator
{
    public class ListPopupWindow<T> : PopupWindowContent
    {
        public Action<T> OnAdd;
        public Action<T> OnRemove;
        IList<T> Items = null;
        IList<ListTreeViewItemContainer<T>> ItemsContainers = null;
        Func<IEnumerable<T>> GetExistItems;
        SearchField SearchField;
        string SearchQuery;
        ListTreeView<T> TreeView;

        public ListPopupWindow(IList<T> items, Func<IEnumerable<T>> getExistItems)
        {
            Items = items;
            GetExistItems = getExistItems;
        }

        public ListPopupWindow(IList<ListTreeViewItemContainer<T>> items, Func<IEnumerable<T>> getExistItems)
        {
            ItemsContainers = items;
            GetExistItems = getExistItems;
        }

        public override Vector2 GetWindowSize()
        {
            return new Vector2(300f, 200f);
        }

        public override void OnGUI(Rect rect)
        {
            if (SearchField == null) SearchField = new SearchField();
            SearchQuery = SearchField.OnGUI(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), SearchQuery);

            rect.y += EditorGUIUtility.singleLineHeight;
            rect.height -= EditorGUIUtility.singleLineHeight;

            if (TreeView == null)
            {
                TreeView = Items == null
                    ? new ListTreeView<T>(new TreeViewState(), ItemsContainers, GetExistItems)
                    {
                        OnAdd = OnAdd,
                        OnRemove = OnRemove,
                    }
                    : new ListTreeView<T>(new TreeViewState(), Items, GetExistItems)
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
