using System.Collections.Generic;
using UnityEngine;

namespace PJDev.DevelopKit.BasicTemplate.Runtime
{
    [CreateAssetMenu(fileName = "CDebugTagDefinitions", menuName = "PJDev/CDebug/CDebug Tag Definitions")]
    public class CDebugTagDefinitions : ScriptableObject
    {
        private const string DefaultTagName = "Default";

        [SerializeField] private List<string> tags = new List<string> { DefaultTagName };

        public IReadOnlyList<string> Tags => tags;

        public bool ContainsTag(string tag) => tags.Contains(tag);

        public bool TryAddTag(string tag)
        {
            if (tags.Contains(tag))
                return false;

            tags.Add(tag);
            return true;
        }

        public bool RemoveTag(string tag) => tags.Remove(tag);

        public void EnsureDefaultTagFirst()
        {
            if (tags.Count == 0)
            {
                tags.Add(DefaultTagName);
                return;
            }

            if (tags[0] == DefaultTagName)
                return;

            tags.Remove(DefaultTagName);
            tags.Insert(0, DefaultTagName);
        }

        public void SetTags(IEnumerable<string> newTags)
        {
            tags.Clear();

            if (newTags == null)
            {
                EnsureDefaultTagFirst();
                return;
            }

            foreach (var tag in newTags)
            {
                if (string.IsNullOrWhiteSpace(tag))
                    continue;

                var trimmed = tag.Trim();
                if (!tags.Contains(trimmed))
                    tags.Add(trimmed);
            }

            EnsureDefaultTagFirst();
        }
    }
}
