using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace PJDev.DevelopKit.BasicTemplate.Runtime.PoolSystem.Addressables
{
    [Serializable]
    public sealed class AddressablePoolConfig
    {
        [SerializeField] private AssetReferenceGameObject prefab = null;
        [SerializeField, Min(0)] private int prewarmCount = 0;
        [SerializeField, Min(1)] private int maxSize = 128;

        public AssetReferenceGameObject Prefab => prefab;
        public int PrewarmCount => prewarmCount;
        public int MaxSize => maxSize;
        public bool IsValid => prefab != null && prefab.RuntimeKeyIsValid();
    }

    [CreateAssetMenu(
        fileName = "SO_AddressablePoolSettings",
        menuName = "PJDev/Pool System/Addressable Pool Settings")]
    public sealed class AddressablePoolSettingsSO : ScriptableObject
    {
        [SerializeField] private List<AddressablePoolConfig> pools = new();

        public IReadOnlyList<AddressablePoolConfig> Pools => pools;

        public async Task PrewarmAsync()
        {
            var tasks = new List<Task>(pools.Count);
            for (int i = 0; i < pools.Count; i++)
            {
                AddressablePoolConfig config = pools[i];
                if (config != null && config.IsValid)
                {
                    tasks.Add(AddressablePrefabPool.PrewarmAsync(
                        config.Prefab,
                        config.PrewarmCount,
                        config.MaxSize));
                }
            }

            if (tasks.Count > 0)
                await Task.WhenAll(tasks);
        }
    }
}
