#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;

using net.narazaka.avatarmenucreator.components;

namespace Narazaka.Unity.DisableGizmoIcons
{
    [InitializeOnLoad]
    class DisableGizmoIcons
    {
        static IEnumerable<Type> Types() => new Type[] { typeof(AvatarToggleMenuCreator), typeof(AvatarChooseMenuCreator), typeof(AvatarRadialMenuCreator) };

        static DisableGizmoIcons()
        {
            EditorApplication.delayCall += Next;
        }

        static void Next()
        {
            foreach (var type in Types())
            {
                SetGizmoIconEnabled(type, false);
            }
            EditorApplication.delayCall -= Next;
        }

#if UNITY_2022_1_OR_NEWER
        static void SetGizmoIconEnabled(Type type, bool enabled)
        {
            GizmoUtility.SetIconEnabled(type, enabled);
        }
#else
        // cf. https://stackoverflow.com/questions/74930784/hide-custom-script-gizmo-in-scene-unity
        static MethodInfo _setIconEnabled;
        static MethodInfo SetIconEnabled => _setIconEnabled = _setIconEnabled ?? Assembly.GetAssembly(typeof(Editor))
            ?.GetType("UnityEditor.AnnotationUtility")
            ?.GetMethod("SetIconEnabled", BindingFlags.Static | BindingFlags.NonPublic);

        static void SetGizmoIconEnabled(Type type, bool enabled)
        {
            if (SetIconEnabled == null) return;
            const int MONO_BEHAVIOR_CLASS_ID = 114; // https://docs.unity3d.com/Manual/ClassIDReference.html
            SetIconEnabled.Invoke(null, new object[] { MONO_BEHAVIOR_CLASS_ID, type.Name, enabled ? 1 : 0 });
        }
#endif
    }
}
#endif
