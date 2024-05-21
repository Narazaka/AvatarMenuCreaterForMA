#if UNITY_EDITOR
using UnityEditor;
using System.Linq;
using UnityEngine;

namespace net.narazaka.avatarmenucreator
{
    public static class ToolbarUtility
    {
        public static T Toolbar<T>(T selected) where T : System.Enum
        {
            var values = (int[])System.Enum.GetValues(typeof(T));
            var names = System.Enum.GetNames(typeof(T)).Select(name => Localization.Get(typeof(T).GetField(name))).ToArray();

            var selectedIndex = System.Array.IndexOf(values, System.Convert.ToInt32(selected));
            var nextSelectedIndex = GUILayout.Toolbar(selectedIndex, names);
            return (T)System.Enum.ToObject(typeof(T), values[nextSelectedIndex]);
        }
    }
}
#endif
