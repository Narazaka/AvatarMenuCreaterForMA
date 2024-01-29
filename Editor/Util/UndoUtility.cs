using UnityEditor;
using UnityObject = UnityEngine.Object;

namespace net.narazaka.avatarmenucreator.editor.util
{
    public static class UndoUtility
    {
        public static void RecordObject(UnityObject uo, string name)
        {
            if (uo == null)
            {
                return;
            }

            Undo.RegisterCompleteObjectUndo(uo, name);

            if (!uo.IsSceneBound())
            {
                EditorUtility.SetDirty(uo);
            }

            if (uo.IsPrefabInstance())
            {
                EditorApplication.delayCall += () =>
                {
                    PrefabUtility.RecordPrefabInstancePropertyModifications(uo);
                };
            }
        }
    }
}
