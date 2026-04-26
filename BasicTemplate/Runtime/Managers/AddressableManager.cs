using System;
using System.Collections.Generic;
#if UNITASK_INSTALLED
using Cysharp.Threading.Tasks;
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
        public delegate void OnResourceLoaded(string key, int loadedCount, int totalCount);

        private readonly Dictionary<string, LoadedResource> _resourcesByName =
            new Dictionary<string, LoadedResource>();

        public bool IsDebugging = true;
        public bool IsLoaded;

        #region load resources

        public T Load<T>(string key) where T : Object
        {
            if (_resourcesByName.TryGetValue(key, out LoadedResource loadedResource))
            {
                var result = loadedResource.asset as T;

                if (IsDebugging)
                    CDebug.Log(result);

                if (result == null && loadedResource.asset is GameObject go)
                {
                    return go.GetComponent<T>();
                }

                return result;
            }

            return null;
        }

        public T Instantiate<T>(string key, Transform parent = null) where T : Component
        {
            GameObject prefab = Load<GameObject>(key);

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
            GameObject prefab = Load<GameObject>(key);

            if (!prefab)
            {
                if (IsDebugging)
                    CDebug.LogError($"Failed to load prefab : {key}");
                return null;
            }

            GameObject go = Object.Instantiate(prefab, parent);
            go.gameObject.name = prefab.name;
            return go;
        }

        #endregion

#if UNITASK_INSTALLED

        #region addressable

        private async UniTask<T> LoadAsync<T>(string key) where T : Object
        {
            if (_resourcesByName.TryGetValue(key, out LoadedResource loadedResource))
            {
                return loadedResource.asset as T;
            }

            string loadKey = key;

            if (key.Contains(".sprite"))
                loadKey = $"{key}[{key.Replace(".sprite", "")}]";

            var asyncOperation = Addressables.LoadAssetAsync<T>(loadKey);
            await asyncOperation;

            if (asyncOperation.Status != AsyncOperationStatus.Succeeded)
            {
                if (IsDebugging)
                    CDebug.LogError($"Failed to load asset: {key}");
                return null;
            }

            T result = asyncOperation.Result;

            loadedResource = new LoadedResource(result, asyncOperation);
            _resourcesByName.TryAdd(key, loadedResource);

            return result;
        }

        public async UniTask LoadALlAsync<T>(string label,
            OnResourceLoaded callBack = null,
            Action OnResourceAllLoaded = null)
            where T : Object
        {
            if (string.IsNullOrEmpty(label))
            {
                if (IsDebugging)
                    CDebug.LogWarning("Label is null or empty");
                return;
            }

            try
            {
                await DownloadDependenciesAsync(label);
            }
            catch (Exception e)
            {
                if (IsDebugging)
                    CDebug.LogError($"DownloadDependencies failed: {e.Message}");
            }

            var opHandle = Addressables.LoadResourceLocationsAsync(label, typeof(T));
            await opHandle;

            if (opHandle.Status != AsyncOperationStatus.Succeeded ||
                opHandle.Result == null ||
                opHandle.Result.Count == 0)
            {
                if (IsDebugging)
                    CDebug.LogWarning($"No resources found for label: {label}");

                OnResourceAllLoaded?.Invoke();
                return;
            }

            int loadCount = 0;
            int totalCount = opHandle.Result.Count;

            foreach (var result in opHandle.Result)
            {
                bool isSprite = result.PrimaryKey.Contains(".sprite");

                if (isSprite)
                    await LoadAsync<Sprite>(result.PrimaryKey);
                else
                    await LoadAsync<T>(result.PrimaryKey);

                loadCount++;
                callBack?.Invoke(result.PrimaryKey, loadCount, totalCount);
            }

            IsLoaded = true;
            OnResourceAllLoaded?.Invoke();
        }

        public async UniTask<bool> DownloadDependenciesAsync(object label)
        {
            if (label == null)
                return false;

            try
            {
                var locationsHandle = Addressables.LoadResourceLocationsAsync(label);
                await locationsHandle.Task;

                if (locationsHandle.Status != AsyncOperationStatus.Succeeded ||
                    locationsHandle.Result == null ||
                    locationsHandle.Result.Count == 0)
                {
                    if (IsDebugging)
                        CDebug.LogWarning($"No addressable locations found for label: {label}");
                    return false;
                }

                var sizeHandle = Addressables.GetDownloadSizeAsync(label);
                await sizeHandle.Task;

                if (sizeHandle.Status != AsyncOperationStatus.Succeeded)
                    return false;

                if (sizeHandle.Result > 0)
                {
                    var downloadHandle = Addressables.DownloadDependenciesAsync(label, true);
                    await downloadHandle.Task;

                    if (downloadHandle.Status != AsyncOperationStatus.Succeeded)
                        return false;
                }

                return true;
            }
            catch (Exception e)
            {
                if (IsDebugging)
                    CDebug.LogWarning($"Skipping dependency download for invalid label '{label}': {e.Message}");
                return false;
            }
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