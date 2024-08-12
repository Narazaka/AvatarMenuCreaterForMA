#if UNITY_EDITOR
using UnityEditor;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;

namespace net.narazaka.avatarmenucreator
{
    public static class ToolbarUtility
    {
        static Dictionary<System.Type, int[]> CachedValues = new Dictionary<System.Type, int[]>();
        static Dictionary<(System.Type, Lang), GUIContent[]> CachedContents = new Dictionary<(System.Type, Lang), GUIContent[]>();

        public static T Toolbar<T>(T selected) where T : System.Enum
        {
            var values = ToolbarValues<T>();
            var selectedIndex = System.Array.IndexOf(values, System.Convert.ToInt32(selected));
            var nextSelectedIndex = GUILayout.Toolbar(selectedIndex, ToolbarLabels<T>());
            return (T)System.Enum.ToObject(typeof(T), values[nextSelectedIndex]);
        }

        static int[] ToolbarValues<T>() where T : System.Enum
        {
            if (CachedValues.TryGetValue(typeof(T), out var values)) return values;
            return CachedValues[typeof(T)] = (int[])System.Enum.GetValues(typeof(T));
        }

        static GUIContent[] ToolbarLabels<T>() where T : System.Enum
        {
            if (CachedContents.TryGetValue((typeof(T), Localization.Lang), out var contents)) return contents;
            return CachedContents[(typeof(T), Localization.Lang)] =
                System.Enum.GetNames(typeof(T))
                .Select(name => typeof(T).GetField(name))
                .Select(field =>
                {
                    var label = Localization.Get(field);
                    var iconAttr = field.GetCustomAttributes(typeof(IconAttribute), true).FirstOrDefault();
                    return iconAttr == null ? new GUIContent(label) : new GUIContent(label, AssetDatabase.LoadAssetAtPath<Texture2D>(System.IO.Path.Combine("Packages/net.narazaka.vrchat.avatar-menu-creater-for-ma", (iconAttr as IconAttribute).Path)));
                }).ToArray();
        }
    }
}
#endif
