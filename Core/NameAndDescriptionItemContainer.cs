#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using net.narazaka.avatarmenucreator.util;

namespace net.narazaka.avatarmenucreator
{
    class NameAndDescriptionItemContainer : ListTreeViewItemContainer<string>
    {
        public INameAndDescription nameAndDescription;

        public NameAndDescriptionItemContainer(INameAndDescription nameAndDescription) : base(nameAndDescription.Name)
        {
            this.nameAndDescription = nameAndDescription;
        }

        public override string displayName => nameAndDescription.Description;
    }
}
#endif
