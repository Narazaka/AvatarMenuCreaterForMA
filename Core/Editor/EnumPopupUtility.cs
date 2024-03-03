#if UNITY_EDITOR
using UnityEditor;
using System.Linq;
using UnityEngine;

namespace net.narazaka.avatarmenucreator
{
    public static class EnumPopupUtility
    {
        public static T EnumPopup<T>(GUIContent label, T selected) where T : System.Enum
        {
            var values = (int[])System.Enum.GetValues(typeof(T));
            var names = System.Enum.GetNames(typeof(T)).Select(name => Localization.Get(typeof(T).GetField(name))).ToArray();

            var selectedIndex = System.Array.IndexOf(values, System.Convert.ToInt32(selected));
            var nextSelectedIndex = EditorGUILayout.Popup(label, selectedIndex, names);
            return (T)System.Enum.ToObject(typeof(T), values[nextSelectedIndex]);
        }

        public static T EnumPopup<T>(string label, T selected) where T : System.Enum
        {
            return EnumPopup(new GUIContent(label), selected);
        }

        public static T EnumPopup<T>(T selected) where T : System.Enum
        {
            return EnumPopup(GUIContent.none, selected);
        }
    }
}
#endif
