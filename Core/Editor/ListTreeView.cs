#if UNITY_EDITOR
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace net.narazaka.avatarmenucreator
{
    public class ListTreeView : TreeView
    {
        class Item : TreeViewItem { }

        public Action<string> OnAdd;
        public Action<string> OnRemove;

        IList<string> Items;
        Func<IEnumerable<string>> GetExistItems;
        IEnumerable<string> ExistItemsCache;
        TreeViewItem Root;

        public ListTreeView(TreeViewState state, IList<string> items, Func<IEnumerable<string>> getExistItems) : base(state)
        {
            Items = items;
            GetExistItems = getExistItems;
            Root = new TreeViewItem { id = -1, depth = -1, displayName = "Root" };
            SetupParentsAndChildrenFromDepths(Root, Items.Select((item, index) => new Item() { id = index, depth = 0, displayName = item } as TreeViewItem).ToList());
            Reload();
        }

        protected override TreeViewItem BuildRoot()
        {
            ExistItemsCache = GetExistItems();
            return Root;
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            if (args.item is Item source)
            {
                var exists = ExistItemsCache.Contains(source.displayName);

                var rect = args.rowRect;
                rect.xMin += GetContentIndent(args.item) + extraSpaceBeforeIconAndLabel;
                var width = rect.width;

                var newExists = EditorGUI.ToggleLeft(rect, source.displayName, exists, exists ? EditorStyles.boldLabel : EditorStyles.label);
                if (newExists != exists)
                {
                    if (newExists)
                    {
                        if (OnAdd != null) OnAdd(source.displayName);
                    }
                    else
                    {
                        if (OnRemove != null) OnRemove(source.displayName);
                    }
                    Reload();
                }
            }
            else
            {
                base.RowGUI(args);
            }
        }

        protected override void SelectionChanged(IList<int> selectedIds)
        {
            SetSelection(new List<int> { });
        }

        protected override void DoubleClickedItem(int id)
        {
            var item = Items[id];
            // if (item != null && OnCommit != null) OnCommit(item);
            Reload();
        }
    }
}
#endif
