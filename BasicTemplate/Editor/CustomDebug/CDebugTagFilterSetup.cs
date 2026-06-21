using PJDev.DevelopKit.BasicTemplate.Runtime;
using UnityEditor;

namespace PJDev.DevelopKit.BasicTemplate.Editors
{
    [InitializeOnLoad]
    public static class CDebugTagFilterSetup
    {
        private static bool initialized;

        static CDebugTagFilterSetup()
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
