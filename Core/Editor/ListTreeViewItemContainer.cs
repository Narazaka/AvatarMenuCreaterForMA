#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace net.narazaka.avatarmenucreator
{
    public class ListTreeViewItemContainer<T>
    {
        public T item;

        public ListTreeViewItemContainer() { }
        public ListTreeViewItemContainer(T item)
        {
            this.item = item;
        }

        public virtual string displayName => item.ToString();

        public virtual bool Toggle(Rect rect, bool exists)
        {
            return EditorGUI.ToggleLeft(rect, displayName, exists, exists ? EditorStyles.boldLabel : EditorStyles.label);
        }
    }
}
#endif
