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
using Object = UnityEngine.Object;

namespace PJDev.DevelopKit.BasicTemplate.Runtime
{
    public struct LoadedResource
    {
        public Object asset;
        public AsyncOperationHandle handle;

        public LoadedResource(Object asset, AsyncOperationHandle handle)
        {
            this.asset = asset;
            this.handle = handle;
        }
    }

    public class AddressableManager : Singleton<AddressableManager>
    {
        private const string SpriteKeySuffix = ".sprite";

        public delegate void OnResourceLoaded(string key, int loadedCount, int totalCount);

        private readonly Dictionary<string, LoadedResource> _resourcesByName =
            new Dictionary<string, LoadedResource>();

        public bool IsDebugging = true;
        public bool IsLoaded;

        #region load resources

        public T Load<T>(string key) where T : Object
        {
            if (!_resourcesByName.TryGetValue(key, out LoadedResource loadedResource))
                return null;

            T result = ResolveAsset<T>(loadedResource);

            if (IsDebugging)
                CDebug.Log(result);

            return result;
        }

        private static T ResolveAsset<T>(LoadedResource loadedResource) where T : Object
        {
            T result = loadedResource.asset as T;
            if (result != null)
                return result;

            if (loadedResource.asset is GameObject go)
                return go.GetComponent<T>();

            return null;
        }

        public T Instantiate<T>(string key, Transform parent = null) where T : Component
        {
            var prefab = Load<GameObject>(key);

            if (!prefab)
            {
                if (IsDebugging)
                    CDebug.LogError($"Failed to load prefab : {key}");
                return null;
            }

            var prefabInstantiate = Object.Instantiate(prefab, parent);
            T comp = prefabInstantiate.GetOrAdd<T>();
            comp.gameObject.name = prefab.name;
            return comp;
        }

        public GameObject Instantiate(string key, Transform parent = null)
        {
            var prefab = Load<GameObject>(key);

            if (!prefab)
            {
                if (IsDebugging)
                    CDebug.LogError($"Failed to load prefab : {key}");
                return null;
            }

            var go = Object.Instantiate(prefab, parent);
            go.name = prefab.name;
            return go;
        }

        #endregion

#if UNITASK_INSTALLED

        #region instantiate async

        public UniTask<T> LoadAssetAsync<T>(string key, CancellationToken cancellationToken = default) where T : Object =>
            LoadAsync<T>(key, cancellationToken);

        public UniTask<GameObject> InstantiateAsync(
            string key,
            Transform parent = null,
            CancellationToken cancellationToken = default)
        {
            return InstantiateInternalAsync(key, parent, cancellationToken);
        }

        public async UniTask<T> InstantiateAsync<T>(
            string key,
            Transform parent = null,
            CancellationToken cancellationToken = default) where T : Component
        {
            GameObject instance = await InstantiateInternalAsync(key, parent, cancellationToken);
            if (instance == null)
                return null;

            return instance.GetOrAdd<T>();
        }

        private async UniTask<GameObject> InstantiateInternalAsync(
            string key,
            Transform parent,
            CancellationToken cancellationToken)
        {
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

                AsyncInstantiateOperation<GameObject> operation = Object.InstantiateAsync(prefab, parent);
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
            key.EndsWith(SpriteKeySuffix, StringComparison.Ordinal);

        private static string ToSpriteLoadKey(string key)
        {
            int baseLength = key.Length - SpriteKeySuffix.Length;
            return string.Concat(key, "[", key.Substring(0, baseLength), "]");
        }

        private async UniTask<T> LoadAsync<T>(string key, CancellationToken cancellationToken = default) where T : Object
        {
            if (_resourcesByName.TryGetValue(key, out LoadedResource loadedResource))
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

            _resourcesByName.TryAdd(key, new LoadedResource(result, asyncOperation));

            return result;
        }

        public async UniTask LoadAllAsync<T>(string label,
            OnResourceLoaded callback = null,
            Action OnResourceAllLoaded = null)
            where T : Object
        {
            if (string.IsNullOrEmpty(label))
            {
                if (IsDebugging)
                    CDebug.LogWarning("Label is null or empty");
                return;
            }

            AsyncOperationHandle<IList<IResourceLocation>> locationsHandle =
                Addressables.LoadResourceLocationsAsync(label, typeof(T));
            await locationsHandle.ToUniTask();

            if (locationsHandle.Status != AsyncOperationStatus.Succeeded ||
                locationsHandle.Result == null ||
                locationsHandle.Result.Count == 0)
            {
                if (IsDebugging)
                    CDebug.LogWarning($"No resources found for label: {label}");

                OnResourceAllLoaded?.Invoke();
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
            int totalCount = locations.Count;
            int loadedCount = 0;
            var loadTasks = new UniTask[totalCount];

            for (int i = 0; i < totalCount; i++)
            {
                IResourceLocation location = locations[i];
                loadTasks[i] = LoadLocationAsync(location);
            }

            await UniTask.WhenAll(loadTasks);

            IsLoaded = true;
            OnResourceAllLoaded?.Invoke();

            async UniTask LoadLocationAsync(IResourceLocation location)
            {
                string primaryKey = location.PrimaryKey;

                if (IsSpriteKey(primaryKey))
                    await LoadAsync<Sprite>(primaryKey);
                else
                    await LoadAsync<T>(primaryKey);

                int count = Interlocked.Increment(ref loadedCount);
                callback?.Invoke(primaryKey, count, totalCount);
            }
        }

        public async UniTask<bool> DownloadDependenciesAsync(object label)
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

        public async UniTask<SceneInstance> LoadSceneAsync(string key,
            LoadSceneMode sceneMode = LoadSceneMode.Single)
        {
            return await Addressables.LoadSceneAsync(key, sceneMode);
        }

        public async UniTask<SceneInstance> UnloadSceneAsync(
            AsyncOperationHandle<SceneInstance> sceneInstanceHandle)
        {
            return await Addressables.UnloadSceneAsync(sceneInstanceHandle);
        }

        #endregion

#endif
    }
}