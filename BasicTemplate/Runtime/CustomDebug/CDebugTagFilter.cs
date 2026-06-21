using System;
using System.Collections.Generic;
#if UNITY_6000_5_OR_NEWER
using Unity.Scripting.LifecycleManagement;
#endif

namespace PJDev.DevelopKit.BasicTemplate.Runtime
{
#if UNITY_6000_5_OR_NEWER
    [AutoStaticsCleanup]
#endif
    public static partial class CDebugTagFilter
    {
        private static readonly HashSet<string> knownTags = new HashSet<string>();

        public static Func<string, bool> ShouldLogFunc { get; set; } = _ => true;

        public static IReadOnlyCollection<string> KnownTags => knownTags;

        public static bool ShouldLog(CDebugTag tag) => ShouldLog(tag.ToString());

        public static bool ShouldLog(string tag)
        {
            return ShouldLogFunc?.Invoke(tag) ?? true;
        }

        public static void LoadKnownTags(IEnumerable<string> tags)
        {
            if (tags == null)
                return;

            knownTags.Clear();
            foreach (var tag in tags)
            {
                if (!string.IsNullOrEmpty(tag))
                    knownTags.Add(tag);
            }
        }
    }
}
