using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRC.SDK3.Avatars.Components;
#if UNITY_2021_2_OR_NEWER
using UnityEditor.SceneManagement;
#else
using UnityEditor.Experimental.SceneManagement;
#endif

namespace net.narazaka.avatarmenucreator.components.editor
{
    // cf. MA: ObjectReferenceFixer
    [InitializeOnLoad]
    static class ChildReferenceFixer
    {
        static bool scheduled;

        static ChildReferenceFixer()
        {
#if UNITY_2021_2_OR_NEWER
            ObjectChangeEvents.changesPublished += OnChangesPublished;
#else
            EditorApplication.hierarchyChanged += Schedule;
#endif
            Schedule();
        }

#if UNITY_2021_2_OR_NEWER
        // パスに影響する変更の時だけ同期する
        static void OnChangesPublished(ref ObjectChangeEventStream stream)
        {
            if (scheduled) return;
            for (var i = 0; i < stream.length; i++)
            {
                switch (stream.GetEventType(i))
                {
                    case ObjectChangeKind.CreateGameObjectHierarchy:
                    case ObjectChangeKind.DestroyGameObjectHierarchy:
                    case ObjectChangeKind.ChangeGameObjectParent:
                    case ObjectChangeKind.ChangeGameObjectStructure:
                    case ObjectChangeKind.ChangeGameObjectStructureHierarchy:
                    case ObjectChangeKind.UpdatePrefabInstances:
                        Schedule();
                        return;
                    case ObjectChangeKind.ChangeGameObjectOrComponentProperties:
                        stream.GetChangeGameObjectOrComponentPropertiesEvent(i, out var change);
                        if (EditorUtility.InstanceIDToObject(change.instanceId) is GameObject)
                        {
                            Schedule();
                            return;
                        }
                        break;
                }
            }
        }
#endif

        static void Schedule()
        {
            if (scheduled) return;
            scheduled = true;
            EditorApplication.delayCall += Sync;
        }

        static void Sync()
        {
            scheduled = false;
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            foreach (var creator in EnumerateCreators())
            {
#if UNITY_2021_3_OR_NEWER
                var avatar = creator.GetComponentInParent<VRCAvatarDescriptor>(true);
#else
                var avatar = creator.GetComponentsInParent<VRCAvatarDescriptor>(true).FirstOrDefault();
#endif
                if (avatar == null) continue;
                creator.AvatarMenu.UndoObject = creator;
                creator.AvatarMenu.ClearGameObjectCache();
                creator.AvatarMenu.SyncChildReferences(avatar.gameObject);
            }
        }

        static IEnumerable<AvatarMenuCreatorBase> EnumerateCreators()
        {
            for (var i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (!scene.isLoaded) continue;
                foreach (var root in scene.GetRootGameObjects())
                {
                    foreach (var creator in root.GetComponentsInChildren<AvatarMenuCreatorBase>(true)) yield return creator;
                }
            }
            var stage = PrefabStageUtility.GetCurrentPrefabStage();
            if (stage != null && stage.prefabContentsRoot != null)
            {
                foreach (var creator in stage.prefabContentsRoot.GetComponentsInChildren<AvatarMenuCreatorBase>(true)) yield return creator;
            }
        }
    }
}
