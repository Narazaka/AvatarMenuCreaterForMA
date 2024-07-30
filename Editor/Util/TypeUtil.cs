using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace net.narazaka.avatarmenucreator.editor.util
{
    public static class TypeUtil
    {
        static Dictionary<string, Type> TypeCache = new Dictionary<string, Type>();

        public static Type GetType(string typeName)
        {
            if (TypeCache.ContainsKey(typeName))
            {
                return TypeCache[typeName];
            }

            Type type = Type.GetType(typeName);
            if (type != null)
            {
                TypeCache[typeName] = type;
                return type;
            }

            /*
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = assembly.GetType(typeName);
                if (type != null)
                {
                    TypeCache[typeName] = type;
                    return type;
                }
            }
            */

            return null;
        }

        public static Type GetType(Component component) => component.GetType();

        public static Type GetType(Type type) => type;

        public static string GetTypeName(Type type)
        {
            if (type == null)
            {
                return null;
            }

            return type.FullName;
        }


        // cf. https://docs.unity3d.com/ja/2022.3/ScriptReference/MonoBehaviour.html
        static HashSet<string> HasEnabledMethodNames = new HashSet<string> { "Start", "Update", "FixedUpdate", "LateUpdate", "OnGUI", "OnDisable", "OnEnable" };

        // VRCに許可されているコンポーネントはユーザーの手元で変わることがなくキャッシュしても問題ないはず
        static Dictionary<Type, bool> HasEnabledCache = new Dictionary<Type, bool>();

        public static bool GetHasEnabled(Type type)
        {
            if (HasEnabledCache.TryGetValue(type, out var hasEnabled))
            {
                return hasEnabled;
            }
            var field = type.GetField("enabled", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            var property = type.GetProperty("enabled", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (field == null && property == null)
            {
                return HasEnabledCache[type] = false;
            }
            var methods = type.GetMethods(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
            return HasEnabledCache[type] = methods.Any(m => HasEnabledMethodNames.Contains(m.Name));
        }

        public static IEnumerable<Type> HasEnabled(this IEnumerable<Type> types)
        {
            return types.Where(type => GetHasEnabled(type));
        }
    }
}
