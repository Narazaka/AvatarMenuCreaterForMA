using System;

namespace net.narazaka.avatarmenucreator
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class EnglishAttribute : LangAttribute
    {
        public EnglishAttribute(string value) : base(Lang.en_us, value) { }
    }
}
