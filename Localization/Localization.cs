using System.Linq;
using System.Reflection;

namespace net.narazaka.avatarmenucreator
{
    public class Localization
    {
        static Lang Lang
        {
            get {
                switch (LanguageName)
                {
                    case "ja-jp":
                        return Lang.ja_jp;
                    default:
                        return Lang.en_us;
                }
            }
        }

#if HAS_NDMF && UNITY_EDITOR
        static string LanguageName => nadena.dev.ndmf.localization.LanguagePrefs.Language;
#else
        static string LanguageName => "ja-jp";
#endif

        public static string Get(MemberInfo info)
        {
            var lang = Lang;
            var langAttributes = info.GetCustomAttributes(typeof(LangAttribute), true);
            if (langAttributes.Length == 0)
            {
                return Replaced(info.Name);
            }
            var langAttribute = langAttributes.FirstOrDefault(a => ((LangAttribute)a).Lang == lang);
            if (langAttribute != null)
            {
                return ((LangAttribute)langAttribute).Value;
            }
            return Replaced(info.Name);
        }

        static string Replaced(string str)
        {
            return str
                .Replace("_Q_", "？")
                .Replace("_n_", "\n")
                .Replace("_eq_", "=")
                .Replace("_per_", "%")
                .Replace("_colon_", ":")
                .Replace("_sl_", "/")
                .Replace("_start_", "（")
                .Replace("_end_", "）")
                .Replace("_dot_", ".")
                .Replace("_", " ")
                .Replace("ヽ", "、")
                .Replace("ゝ", "。");
        }
    }
}
