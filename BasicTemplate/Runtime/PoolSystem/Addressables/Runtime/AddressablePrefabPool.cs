using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PJDev.DevelopKit.BasicTemplate.Runtime.PoolSystem;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityAddressables = UnityEngine.AddressableAssets.Addressables;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace PJDev.DevelopKit.BasicTemplate.Runtime.PoolSystem.Addressables
{
    public readonly struct AddressablePrefabPoolStats
    {
        public string Address { get; }
        public GameObject Prefab { get; }
        public int CountAll { get; }
        public int CountActive { get; }
        public int CountInactive { get; }
        public int MaxSize { get; }

        internal AddressablePrefabPoolStats(string address, GameObjectPool pool)
        {
            Address = address;
            Prefab = pool.Prefab;
            CountAll = pool.CountAll;
            CountActive = pool.CountActive;
            CountInactive = pool.CountInactive;
            MaxSize = pool.MaxSize;
        }
    }

    /// <summary>
    /// Addressable Prefab을 한 번 로드한 뒤 인스턴스를 재사용합니다.
    /// 모든 API는 Unity 메인 스레드에서 사용해야 합니다.
    /// </summary>
    public static class AddressablePrefabPool
    {
        private const int DefaultMaxSize = 128;

        private static readonly Dictionary<string, AddressablePoolEntry> Entries =
            new(StringComparer.Ordinal);

        private static Transform root;

        public static int PoolCount => Entries.Count;

        public static async Task<GameObject> SpawnAsync(
            string address,
            Transform parent = null,
            int maxSize = DefaultMaxSize)
        {
            GameObjectPool pool = await GetOrCreateAsync(address, 0, maxSize);
            return pool.Spawn(parent);
        }

        public static async Task<GameObject> SpawnAsync(
            string address,
            Vector3 position,
            Quaternion rotation,
            Transform parent = null,
            int maxSize = DefaultMaxSize)
        {
            GameObjectPool pool = await GetOrCreateAsync(address, 0, maxSize);
            return pool.Spawn(position, rotation, parent);
        }

        public static Task<GameObject> SpawnAsync(
            AssetReferenceGameObject prefab,
            Transform parent = null,
            int maxSize = DefaultMaxSize) =>
            SpawnAsync(GetAddress(prefab), parent, maxSize);

        public static Task<GameObject> SpawnAsync(
            AssetReferenceGameObject prefab,
            Vector3 position,
            Quaternion rotation,
            Transform parent = null,
            int maxSize = DefaultMaxSize) =>
            SpawnAsync(GetAddress(prefab), position, rotation, parent, maxSize);

        public static async Task PrewarmAsync(
            string address,
            int count,
            int maxSize = DefaultMaxSize)
        {
            ValidateSize(count, maxSize);
            GameObjectPool pool = await GetOrCreateAsync(address, count, maxSize);
            pool.Prewarm(count);
        }

        public static Task PrewarmAsync(
            AssetReferenceGameObject prefab,
            int count,
            int maxSize = DefaultMaxSize) =>
            PrewarmAsync(GetAddress(prefab), count, maxSize);

        public static bool Release(GameObject instance) => PrefabPool.Release(instance);

        public static bool Remove(string address)
        {
            address = ValidateAddress(address);
            if (!Entries.Remove(address, out AddressablePoolEntry entry))
                return false;

            entry.Dispose();
            DestroyRootIfEmpty();
            return true;
        }

        public static bool Remove(AssetReferenceGameObject prefab) =>
            Remove(GetAddress(prefab));

        public static void ClearInactive()
        {
            foreach (AddressablePoolEntry entry in Entries.Values)
                entry.ClearInactive();
        }

        public static void GetStats(List<AddressablePrefabPoolStats> destination)
        {
            if (destination == null)
                throw new ArgumentNullException(nameof(destination));

            destination.Clear();
            if (destination.Capacity < Entries.Count)
                destination.Capacity = Entries.Count;

            foreach (KeyValuePair<string, AddressablePoolEntry> pair in Entries)
            {
                if (pair.Value.TryGetPool(out GameObjectPool pool))
                    destination.Add(new AddressablePrefabPoolStats(pair.Key, pool));
            }
        }

        public static void Clear()
        {
            foreach (AddressablePoolEntry entry in Entries.Values)
                entry.Dispose();

            Entries.Clear();

            if (root != null)
                DestroyObject(root.gameObject);

            root = null;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Clear();

        private static async Task<GameObjectPool> GetOrCreateAsync(
            string address,
            int initialCapacity,
            int maxSize)
        {
            address = ValidateAddress(address);
            ValidateSize(initialCapacity, maxSize);

            if (!Entries.TryGetValue(address, out AddressablePoolEntry entry))
            {
                entry = new AddressablePoolEntry(address, initialCapacity, maxSize);
                Entries.Add(address, entry);
            }

            try
            {
                return await entry.GetPoolAsync(GetRoot());
            }
            catch
            {
                if (Entries.TryGetValue(address, out AddressablePoolEntry current) &&
                    ReferenceEquals(current, entry))
                {
                    Entries.Remove(address);
                    entry.Dispose();
                    DestroyRootIfEmpty();
                }

                throw;
            }
        }

        private static string GetAddress(AssetReferenceGameObject prefab)
        {
            if (prefab == null)
                throw new ArgumentNullException(nameof(prefab));
            if (!prefab.RuntimeKeyIsValid())
                throw new ArgumentException("Addressable Prefab reference is not valid.", nameof(prefab));

            return ValidateAddress(prefab.RuntimeKey.ToString());
        }

        private static string ValidateAddress(string address)
        {
            if (address == null)
                throw new ArgumentNullException(nameof(address));

            string trimmed = address.Trim();
            if (trimmed.Length == 0)
                throw new ArgumentException("Address cannot be empty.", nameof(address));

            return trimmed;
        }

        private static void ValidateSize(int initialCapacity, int maxSize)
        {
            if (initialCapacity < 0)
                throw new ArgumentOutOfRangeException(nameof(initialCapacity));
            if (maxSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxSize));
            if (initialCapacity > maxSize)
                throw new ArgumentOutOfRangeException(nameof(initialCapacity));
        }

        private static Transform GetRoot()
        {
            if (root != null)
                return root;

            var rootObject = new GameObject("[Addressable Prefab Pool]");
            root = rootObject.transform;
            rootObject.SetActive(false);

            if (Application.isPlaying)
                UnityEngine.Object.DontDestroyOnLoad(rootObject);

            return root;
        }

        private static void DestroyRootIfEmpty()
        {
            if (Entries.Count == 0 && root != null)
            {
                DestroyObject(root.gameObject);
                root = null;
            }
        }

        private static void DestroyObject(UnityEngine.Object target)
        {
            if (target == null)
                return;

            if (Application.isPlaying)
                UnityEngine.Object.Destroy(target);
            else
                UnityEngine.Object.DestroyImmediate(target);
        }

        private sealed class AddressablePoolEntry : IDisposable
        {
            private readonly string address;
            private readonly int initialCapacity;
            private readonly int maxSize;

            private AsyncOperationHandle<GameObject> loadHandle;
            private Task<GameObjectPool> loadTask;
            private GameObjectPool pool;
            private bool ownsHandle;
            private bool disposed;

            public AddressablePoolEntry(string address, int initialCapacity, int maxSize)
            {
                this.address = address;
                this.initialCapacity = initialCapacity;
                this.maxSize = maxSize;
            }

            public Task<GameObjectPool> GetPoolAsync(Transform storageParent)
            {
                if (disposed)
                    throw new ObjectDisposedException(nameof(AddressablePoolEntry));

                return pool != null
                    ? Task.FromResult(pool)
                    : loadTask ??= LoadAsync(storageParent);
            }

            public bool TryGetPool(out GameObjectPool result)
            {
                result = pool;
                return result != null && !result.IsDisposed;
            }

            public void ClearInactive()
            {
                if (TryGetPool(out GameObjectPool loadedPool))
                    loadedPool.Clear();
            }

            public void Dispose()
            {
                if (disposed)
                    return;

                disposed = true;
                pool?.Dispose();
                pool = null;

                if (ownsHandle && loadHandle.IsValid())
                    UnityAddressables.Release(loadHandle);

                ownsHandle = false;
            }

            private async Task<GameObjectPool> LoadAsync(Transform storageParent)
            {
                loadHandle = UnityAddressables.LoadAssetAsync<GameObject>(address);
                ownsHandle = true;

                GameObject prefab;
                try
                {
                    prefab = await loadHandle.Task;
                }
                catch
                {
                    ReleaseHandle();
                    throw;
                }

                if (disposed)
                    throw new ObjectDisposedException(nameof(AddressablePoolEntry));

                if (loadHandle.Status != AsyncOperationStatus.Succeeded || prefab == null)
                {
                    Exception cause = loadHandle.OperationException;
                    ReleaseHandle();
                    throw new InvalidOperationException(
                        $"Addressable Prefab '{address}' could not be loaded.",
                        cause);
                }

                pool = new GameObjectPool(
                    prefab,
                    initialCapacity,
                    maxSize,
                    storageParent);
                return pool;
            }

            private void ReleaseHandle()
            {
                if (ownsHandle && loadHandle.IsValid())
                    UnityAddressables.Release(loadHandle);

                ownsHandle = false;
            }
        }
    }
}
