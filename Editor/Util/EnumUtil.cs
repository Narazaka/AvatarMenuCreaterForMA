using System;
using System.Linq;
using System.Collections.Generic;

namespace net.narazaka.avatarmenucreator.util
{
    public static class EnumUtil
    {
        static Dictionary<Type, string[]> EnumCache = new Dictionary<Type, string[]>();

        public static string[] GetEnumNamesCached(this Type enumType)
        {
            if (EnumCache.TryGetValue(enumType, out var names))
            {
                return names;
            }
            return EnumCache[enumType] = enumType.GetEnumNames();
        }

        static Dictionary<Type, int[]> EnumValuesCache = new Dictionary<Type, int[]>();

        public static int[] GetEnumValuesCached(this Type enumType)
        {
            if (EnumValuesCache.TryGetValue(enumType, out var values))
            {
                return values;
            }
            return EnumValuesCache[enumType] = enumType.GetEnumValues().Cast<int>().ToArray();
        }
    }
}
