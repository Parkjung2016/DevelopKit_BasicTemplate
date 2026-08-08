using System;
using System.Collections.Generic;
#if UNITASK_INSTALLED
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.ResourceManagement.ResourceLocations;
#endif
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using PJDev.DevelopKit.BasicTemplate.Runtime.PoolSystem;
using Object = UnityEngine.Object;

namespace PJDev.DevelopKit.BasicTemplate.Runtime
{
    public struct LoadedResource
    {
        public readonly Object Asset;
        public readonly AsyncOperationHandle Handle;

        public LoadedResource(Object asset, AsyncOperationHandle handle)
        {
            this.Asset = asset;
            this.Handle = handle;
        }
    }

    public class AddressableManager : Singleton<AddressableManager>
    {
        private const string SUFFIX_SPRITE_KEY = ".sprite";

        public delegate void OnResourceLoaded(string key, int loadedCount, int totalCount);

        private readonly Dictionary<string, LoadedResource> resourcesByName =
            new Dictionary<string, LoadedResource>();

        public bool IsDebugging = true;
        public bool IsLoaded;

        #region load resources

        public T Load<T>(string key) where T : Object
        {
            if (!resourcesByName.TryGetValue(key, out LoadedResource loadedResource))
                return null;

            T result = ResolveAsset<T>(loadedResource);

            if (IsDebugging)
                CDebug.Log(result);

            return result;
        }

        private static T ResolveAsset<T>(LoadedResource loadedResource) where T : Object
        {
            T result = loadedResource.Asset as T;
            if (result != null)
                return result;

            if (loadedResource.Asset is GameObject go)
                return go.GetComponent<T>();

            return null;
        }

        public T Instantiate<T>(string key, Transform parent = null, bool usePool = false) where T : Component
        {
            var prefab = Load<GameObject>(key);

            if (!prefab)
            {
                if (IsDebugging)
                    CDebug.LogError($"Failed to load prefab : {key}");
                return null;
            }

            GameObject prefabInstantiate = usePool
                ? PrefabPool.Spawn(prefab, parent)
                : Object.Instantiate(prefab, parent);
            T comp = prefabInstantiate.GetOrAddComponent<T>();
            comp.gameObject.name = prefab.name;
            return comp;
        }

        public GameObject Instantiate(string key, Transform parent = null, bool usePool = false)
        {
            var prefab = Load<GameObject>(key);

            if (!prefab)
            {
                if (IsDebugging)
                    CDebug.LogError($"Failed to load prefab : {key}");
                return null;
            }

            GameObject go = usePool
                ? PrefabPool.Spawn(prefab, parent)
                : Object.Instantiate(prefab, parent);
            go.name = prefab.name;
            return go;
        }

        /// <summary>
        /// 풀에서 생성한 인스턴스는 반환하고, 일반 인스턴스는 제거합니다.
        /// 반환값은 풀로 돌아갔는지를 나타냅니다.
        /// </summary>
        public bool ReleaseInstance(GameObject instance)
        {
            if (instance == null)
                return false;

            if (PrefabPool.Release(instance))
                return true;

            Object.Destroy(instance);
            return false;
        }

        public bool ReleaseInstance(Component instance) =>
            instance != null && ReleaseInstance(instance.gameObject);

        #endregion

#if UNITASK_INSTALLED

        #region instantiate async

        public AddressableAsyncRequest<T> LoadAssetAsync<T>(string key) where T : Object =>
            new AddressableAsyncRequest<T>(
                key,
                ct => LoadAsync<T>(key, ct),
                static result => result != null);

        public AddressableAsyncRequest<GameObject> InstantiateAsync(
            string key,
            Transform parent = null,
            bool usePool = false) =>
            new AddressableAsyncRequest<GameObject>(
                key,
                ct => InstantiateInternalAsync(key, parent, usePool, ct),
                static result => result != null);

        public AddressableAsyncRequest<T> InstantiateAsync<T>(
            string key,
            Transform parent = null,
            bool usePool = false)
            where T : Component =>
            new AddressableAsyncRequest<T>(
                key,
                async ct =>
                {
                    GameObject instance = await InstantiateInternalAsync(key, parent, usePool, ct);
                    return instance != null ? instance.GetOrAddComponent<T>() : null;
                },
                static result => result != null);

        private async UniTask<GameObject> InstantiateInternalAsync(
            string key,
            Transform parent,
            bool usePool,
            CancellationToken cancellationToken)
        {
            AsyncInstantiateOperation<GameObject> operation = null;
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                GameObject prefab = Load<GameObject>(key);
                if (prefab == null)
                    prefab = await LoadAsync<GameObject>(key, cancellationToken);

                if (prefab == null)
                {
                    if (IsDebugging)
                        CDebug.LogError($"Failed to load prefab: {key}");
                    return null;
                }

                cancellationToken.ThrowIfCancellationRequested();

                if (usePool)
                    return PrefabPool.Spawn(prefab, parent);

                operation = Object.InstantiateAsync(prefab, parent);
                await operation.ToUniTask(cancellationToken: cancellationToken);

                if (operation.Result == null || operation.Result.Length == 0)
                {
                    if (IsDebugging)
                        CDebug.LogError($"Failed to instantiate prefab: {key}");
                    return null;
                }

                GameObject instance = operation.Result[0];
                instance.name = prefab.name;
                return instance;
            }
            catch (OperationCanceledException)
            {
                if (operation != null)
                {
                    if (!operation.isDone)
                    {
                        operation.Cancel();
                    }
                    else if (operation.Result != null)
                    {
                        for (int i = 0; i < operation.Result.Length; i++)
                        {
                            if (operation.Result[i] != null)
                                Object.Destroy(operation.Result[i]);
                        }
                    }
                }
                if (IsDebugging)
                    CDebug.Log($"InstantiateAsync cancelled: {key}");
                throw;
            }
            catch (Exception e)
            {
                if (IsDebugging)
                    CDebug.LogError($"InstantiateAsync failed for '{key}': {e.Message}");
                return null;
            }
        }

        #endregion

        #region addressable

        private static bool IsSpriteKey(string key) =>
            key.EndsWith(SUFFIX_SPRITE_KEY, StringComparison.Ordinal);

        private static string ToSpriteLoadKey(string key)
        {
            int baseLength = key.Length - SUFFIX_SPRITE_KEY.Length;
            return string.Concat(key, "[", key.Substring(0, baseLength), "]");
        }

        private async UniTask<T> LoadAsync<T>(string key, CancellationToken cancellationToken = default)
            where T : Object
        {
            if (resourcesByName.TryGetValue(key, out LoadedResource loadedResource))
                return ResolveAsset<T>(loadedResource);

            string loadKey = IsSpriteKey(key) ? ToSpriteLoadKey(key) : key;

            AsyncOperationHandle<T> asyncOperation = Addressables.LoadAssetAsync<T>(loadKey);
            await asyncOperation.ToUniTask(cancellationToken: cancellationToken);

            if (asyncOperation.Status != AsyncOperationStatus.Succeeded)
            {
                if (IsDebugging)
                    CDebug.LogError($"Failed to load asset: {key}");
                return null;
            }

            T result = asyncOperation.Result;

            resourcesByName.TryAdd(key, new LoadedResource(result, asyncOperation));

            return result;
        }

        public AddressableLoadAllRequest<T> LoadAllAsync<T>(string label) where T : Object =>
            new AddressableLoadAllRequest<T>(this, label);

        internal async UniTask LoadAllInternalAsync<T>(
            string label,
            OnResourceLoaded onResourceLoaded,
            Action onAllLoaded,
            AddressableFailedHandler onFailed) where T : Object
        {
            if (string.IsNullOrEmpty(label))
            {
                if (IsDebugging)
                    CDebug.LogWarning("Label is null or empty");
                onFailed?.Invoke(label, new ArgumentException("Label is null or empty.", nameof(label)));
                return;
            }

            int totalCount = 0;
            int loadedCount = 0;

            try
            {
                AsyncOperationHandle<IList<IResourceLocation>> locationsHandle =
                    Addressables.LoadResourceLocationsAsync(label, typeof(T));
                await locationsHandle.ToUniTask();

                if (locationsHandle.Status != AsyncOperationStatus.Succeeded ||
                    locationsHandle.Result == null ||
                    locationsHandle.Result.Count == 0)
                {
                    if (IsDebugging)
                        CDebug.LogWarning($"No resources found for label: {label}");

                    onAllLoaded?.Invoke();
                    return;
                }

                try
                {
                    await EnsureDependenciesDownloadedAsync(label);
                }
                catch (Exception e)
                {
                    if (IsDebugging)
                        CDebug.LogError($"DownloadDependencies failed: {e.Message}");
                }

                IList<IResourceLocation> locations = locationsHandle.Result;
                totalCount = locations.Count;
                var loadTasks = new UniTask[totalCount];

                for (int i = 0; i < totalCount; i++)
                {
                    IResourceLocation location = locations[i];
                    loadTasks[i] = LoadLocationAsync(location);
                }

                await UniTask.WhenAll(loadTasks);

                IsLoaded = true;
                onAllLoaded?.Invoke();
            }
            catch (Exception exception)
            {
                onFailed?.Invoke(label, exception);
                throw;
            }

            async UniTask LoadLocationAsync(IResourceLocation location)
            {
                string primaryKey = location.PrimaryKey;

                if (IsSpriteKey(primaryKey))
                    await LoadAsync<Sprite>(primaryKey);
                else
                    await LoadAsync<T>(primaryKey);

                int count = Interlocked.Increment(ref loadedCount);
                onResourceLoaded?.Invoke(primaryKey, count, totalCount);
            }
        }

        public AddressableAsyncRequest<bool> DownloadDependenciesAsync(object label) =>
            new AddressableAsyncRequest<bool>(
                label?.ToString() ?? string.Empty,
                _ => DownloadDependenciesInternalAsync(label),
                static result => result);

        private async UniTask<bool> DownloadDependenciesInternalAsync(object label)
        {
            if (label == null)
                return false;

            try
            {
                AsyncOperationHandle<IList<IResourceLocation>> locationsHandle =
                    Addressables.LoadResourceLocationsAsync(label);
                await locationsHandle.ToUniTask();

                if (locationsHandle.Status != AsyncOperationStatus.Succeeded ||
                    locationsHandle.Result == null ||
                    locationsHandle.Result.Count == 0)
                {
                    if (IsDebugging)
                        CDebug.LogWarning($"No addressable locations found for label: {label}");
                    return false;
                }

                return await EnsureDependenciesDownloadedAsync(label);
            }
            catch (Exception e)
            {
                if (IsDebugging)
                    CDebug.LogWarning($"Skipping dependency download for invalid label '{label}': {e.Message}");
                return false;
            }
        }

        private static async UniTask<bool> EnsureDependenciesDownloadedAsync(object label)
        {
            AsyncOperationHandle<long> sizeHandle = Addressables.GetDownloadSizeAsync(label);
            await sizeHandle.ToUniTask();

            if (sizeHandle.Status != AsyncOperationStatus.Succeeded)
                return false;

            if (sizeHandle.Result <= 0)
                return true;

            AsyncOperationHandle downloadHandle = Addressables.DownloadDependenciesAsync(label, true);
            await downloadHandle.ToUniTask();

            return downloadHandle.Status == AsyncOperationStatus.Succeeded;
        }

        #endregion

        #region scene

        public AddressableAsyncRequest<SceneInstance> LoadSceneAsync(
            string key,
            LoadSceneMode sceneMode = LoadSceneMode.Single) =>
            new AddressableAsyncRequest<SceneInstance>(
                key,
                async ct =>
                {
                    AsyncOperationHandle<SceneInstance> handle =
                        Addressables.LoadSceneAsync(key, sceneMode);
                    await handle.ToUniTask(cancellationToken: ct);
                    return handle.Status == AsyncOperationStatus.Succeeded ? handle.Result : default;
                },
                static result => result.Scene.IsValid());

        public AddressableAsyncRequest<SceneInstance> UnloadSceneAsync(
            AsyncOperationHandle<SceneInstance> sceneInstanceHandle) =>
            new AddressableAsyncRequest<SceneInstance>(
                "UnloadScene",
                async ct =>
                {
                    AsyncOperationHandle<SceneInstance> handle =
                        Addressables.UnloadSceneAsync(sceneInstanceHandle);
                    await handle.ToUniTask(cancellationToken: ct);
                    return handle.Status == AsyncOperationStatus.Succeeded ? handle.Result : default;
                },
                static result => result.Scene.IsValid());

        #endregion

#endif
    }
}