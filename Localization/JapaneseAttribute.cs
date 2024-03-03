using System;

namespace net.narazaka.avatarmenucreator
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class JapaneseAttribute : LangAttribute
    {
        public JapaneseAttribute(string value) : base(Lang.ja_jp, value) { }
    }
}
