using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace net.narazaka.avatarmenucreator.util
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

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = assembly.GetType(typeName);
                if (type != null)
                {
                    TypeCache[typeName] = type;
                    return type;
                }
            }

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
            if (!type.IsSubclassOf(typeof(Behaviour)))
            {
                var field = type.GetField(nameof(Behaviour.enabled), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                var property = type.GetProperty(nameof(Behaviour.enabled), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (field != null || property != null)
                {
                    return HasEnabledCache[type] = true;
                }
            }
            var baseType = type;
            // private methodは継承されないのでbaseTypeで探す
            while (baseType != null)
            {
                if (baseType == typeof(Component))
                {
                    break;
                }
                var methods = baseType.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                if (methods.Any(m => HasEnabledMethodNames.Contains(m.Name)))
                {
                    return HasEnabledCache[type] = true;
                }
                baseType = baseType.BaseType;
            }
            return HasEnabledCache[type] = false;
        }

        public static IEnumerable<Type> HasEnabled(this IEnumerable<Type> types)
        {
            return types.Where(type => GetHasEnabled(type));
        }
    }
}
