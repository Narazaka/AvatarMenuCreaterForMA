#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using net.narazaka.avatarmenucreator.editor.util;

namespace net.narazaka.avatarmenucreator
{
    class NameAndDescriptionItemContainer : ListTreeViewItemContainer<string>
    {
        public Util.INameAndDescription nameAndDescription;

        public NameAndDescriptionItemContainer(Util.INameAndDescription nameAndDescription) : base(nameAndDescription.Name)
        {
            this.nameAndDescription = nameAndDescription;
        }

        public override string displayName => nameAndDescription.Description;
    }
}
#endif
