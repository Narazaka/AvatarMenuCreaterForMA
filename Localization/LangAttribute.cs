using System;

namespace net.narazaka.avatarmenucreator
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class LangAttribute : Attribute
    {
        public Lang Lang { get; }
        public string Value { get; }

        public LangAttribute(Lang lang, string value)
        {
            Lang = lang;
            Value = value;
        }
    }
}
