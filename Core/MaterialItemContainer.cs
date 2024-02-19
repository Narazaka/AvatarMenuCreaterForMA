#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace net.narazaka.avatarmenucreator
{
    class MaterialItemContainer : ListTreeViewItemContainer<int>
    {
        public Material material;

        public MaterialItemContainer(int index, Material material) : base(index)
        {
            this.material = material;
        }

        public override string displayName => material == null ? "" : material.name;
        public override bool Toggle(Rect rect, bool exists)
        {
            var newExists = EditorGUI.ToggleLeft(new Rect(rect.x, rect.y, 45, rect.height), $"[{item}]", exists, exists ? EditorStyles.boldLabel : EditorStyles.label);
            rect.xMin += 45;
            EditorGUI.BeginDisabledGroup(true);
            EditorGUI.ObjectField(rect, material, typeof(Material), false);
            EditorGUI.EndDisabledGroup();
            return newExists;
        }
    }
}
#endif
