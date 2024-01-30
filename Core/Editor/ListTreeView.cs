#if UNITY_EDITOR
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;

namespace net.narazaka.avatarmenucreator
{
    public class ListTreeView<T> : TreeView
    {
        class Item : TreeViewItem
        {
            public ListTreeViewItemContainer<T> source;
        }

        public Action<T> OnAdd;
        public Action<T> OnRemove;

        IList<ListTreeViewItemContainer<T>> Items;
        Func<IEnumerable<T>> GetExistItems;
        IEnumerable<T> ExistItemsCache;
        TreeViewItem Root;

        public ListTreeView(TreeViewState state, IEnumerable<T> items, Func<IEnumerable<T>> getExistItems) : this(state, items.Select(item => new ListTreeViewItemContainer<T>(item)).ToList(), getExistItems)
        {
        }

        public ListTreeView(TreeViewState state, IList<ListTreeViewItemContainer<T>> items, Func<IEnumerable<T>> getExistItems) : base(state)
        {
            Items = items;
            GetExistItems = getExistItems;
            Root = new TreeViewItem { id = -1, depth = -1, displayName = "Root" };
            SetupParentsAndChildrenFromDepths(Root, Items.Select((item, index) => new Item() { id = index, depth = 0, displayName = item.displayName, source = item } as TreeViewItem).ToList());
            Reload();
        }

        protected override TreeViewItem BuildRoot()
        {
            ExistItemsCache = GetExistItems();
            return Root;
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            if (args.item is Item item)
            {
                var exists = ExistItemsCache.Contains(item.source.item);

                var rect = args.rowRect;
                rect.xMin += GetContentIndent(item) + extraSpaceBeforeIconAndLabel;
                var width = rect.width;

                var newExists = item.source.Toggle(rect, exists);
                if (newExists != exists)
                {
                    if (newExists)
                    {
                        if (OnAdd != null) OnAdd(item.source.item);
                    }
                    else
                    {
                        if (OnRemove != null) OnRemove(item.source.item);
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
    }
}
#endif
