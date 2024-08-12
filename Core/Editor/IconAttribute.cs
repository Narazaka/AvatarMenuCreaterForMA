using System;

namespace net.narazaka.avatarmenucreator
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class IconAttribute : Attribute
    {
        public string Path { get; }

        public IconAttribute(string path)
        {
            Path = path;
        }
    }
}
