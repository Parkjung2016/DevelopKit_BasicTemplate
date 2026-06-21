using PJDev.DevelopKit.BasicTemplate.Runtime;
using UnityEditor;
#if UNITY_6000_5_OR_NEWER
using Unity.Scripting.LifecycleManagement;
#endif

namespace PJDev.DevelopKit.BasicTemplate.Editors
{
    [InitializeOnLoad]
#if UNITY_6000_5_OR_NEWER
    [AutoStaticsCleanup]
#endif
    public static partial class CDebugTagFilterSetup
    {
        private static bool initialized;

        static CDebugTagFilterSetup()
        {
            EnsureInitialized();
        }

        [InitializeOnEnterPlayMode]
        private static void OnEnterPlayMode()
        {
            EnsureInitialized();
        }

        internal static void EnsureInitialized()
        {
            if (initialized)
                return;

            var definitions = CDebugTagDefinitionsProvider.GetOrCreate();
            LoadPersistedTags();
            CDebugTagFilter.ShouldLogFunc = ShouldLogTag;
            initialized = true;
        }

        internal static void LoadPersistedTags()
        {
            var definitions = CDebugTagDefinitionsProvider.GetOrCreate();
            CDebugTagFilter.LoadKnownTags(definitions.Tags);
        }

        private static bool ShouldLogTag(string tag)
        {
            if (!CDebugTagFilterWindow.IsFilterEnabled())
                return true;

            return CDebugTagFilterWindow.IsTagEnabled(tag);
        }
    }
}
