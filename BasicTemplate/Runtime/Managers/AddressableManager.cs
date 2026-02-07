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

namespace Skddkkkk.DevelopKit.BasicTemplate.Runtime
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

        #region load reousources

        public T Load<T>(string key) where T : Object
        {
            if (_resourcesByName.TryGetValue(key, out LoadedResource loadedResource))
            {
                var result = loadedResource.asset as T;
                if (IsDebugging)
                    CDebug.Log(result);
                if (result == null)
                    if (loadedResource.asset is GameObject go)
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
            T go = prefabInstantiate.GetOrAdd<T>();
            go.gameObject.name = prefab.name;
            return go;
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
            LoadedResource loadedResource;
            if (_resourcesByName.TryGetValue(key, out loadedResource))
            {
                return loadedResource.asset as T;
            }

            string loadKey = key;
            if (key.Contains(".sprite"))
                loadKey = $"{key}[{key.Replace(".sprite", "")}]";

            var asyncOperation = Addressables.LoadAssetAsync<T>(loadKey);
            await asyncOperation;

            T result = asyncOperation.Result;
            loadedResource = new LoadedResource(result, asyncOperation);
            _resourcesByName.TryAdd(key, loadedResource);
            return result;
        }

        public async UniTask LoadALlAsync<T>(string label, OnResourceLoaded callBack = null,
            Action OnResourceAllLoaded = null)
            where T : Object
        {
            float timeoutSeconds = 5f;
            try
            {
                await DownloadDependenciesAsync(label).Timeout(TimeSpan.FromSeconds(timeoutSeconds));
            }
            catch (TimeoutException)
            {
                CDebug.LogError($"DownloadDependenciesAsync timeout ({{timeoutSeconds}}s for label: {label}");
            }

            var opHandle = Addressables.LoadResourceLocationsAsync(label, typeof(T));
            await opHandle;

            int loadCount = 0;
            int totalCount = opHandle.Result.Count;

            foreach (var result in opHandle.Result)
            {
                bool isContainsDotSprite = result.PrimaryKey.Contains(".sprite");
                if (isContainsDotSprite)
                {
                    await LoadAsync<Sprite>(result.PrimaryKey);
                    loadCount++;
                    callBack?.Invoke(result.PrimaryKey, loadCount, totalCount);
                }
                else
                {
                    await LoadAsync<T>(result.PrimaryKey);
                    loadCount++;
                    callBack?.Invoke(result.PrimaryKey, loadCount, totalCount);
                }
            }

            IsLoaded = true;
            OnResourceAllLoaded?.Invoke();
        }

        public async UniTask<bool> DownloadDependenciesAsync(object label)
        {
            var getDownloadHandle = Addressables.GetDownloadSizeAsync(label);
            await getDownloadHandle.Task;
            if (getDownloadHandle.Status == AsyncOperationStatus.Succeeded && getDownloadHandle.Result > 0)
            {
                var downloadHandle = Addressables.DownloadDependenciesAsync(label, true);
                await downloadHandle.Task;
                if (downloadHandle.Status != AsyncOperationStatus.Succeeded)
                    return false;
            }

            return true;
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