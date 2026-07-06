using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace O2un.Manager
{
    public sealed class AssetManager : IAssetService
    {
        private readonly Dictionary<string, AsyncOperationHandle<Object>> _handles = new();

        public async UniTask<T> LoadAsync<T>(string key) where T : Object
        {
            if(_handles.TryGetValue(key, out var cached))
            {
                var cachedAsset = await cached.ToUniTask();
                if(cachedAsset is T typedCached)
                {
                    return typedCached;
                }

                var cachedType = cachedAsset != null ? cachedAsset.GetType() : null;
                Debug.LogError($"[AssetManager] Cached type mismatch. key={key}, cached={cachedType}, requested={typeof(T)}");
                throw new InvalidCastException($"[AssetManager] key={key} is cached as {cachedType}, not {typeof(T)}.");
            }

            await EnsureTypeAsync<T>(key);

            var handle = Addressables.LoadAssetAsync<Object>(key);
            _handles[key] = handle;

            try
            {
                return (T)await handle.ToUniTask();
            }
            catch(Exception e)
            {
                Debug.LogError($"[AssetManager] Load failed. key={key}\n{e}");
                _handles.Remove(key);
                Addressables.Release(handle);
                throw;
            }
        }

        private static async UniTask EnsureTypeAsync<T>(string key) where T : Object
        {
            var locationHandle = Addressables.LoadResourceLocationsAsync(key, typeof(T));
            try
            {
                var locations = await locationHandle.ToUniTask();
                if(0 == locations.Count)
                {
                    Debug.LogError($"[AssetManager] No asset of type {typeof(T)} found. key={key}");
                    throw new InvalidOperationException($"[AssetManager] key={key} has no asset of type {typeof(T)}.");
                }
            }
            finally
            {
                Addressables.Release(locationHandle);
            }
        }

        public void Release(string key)
        {
            if(false == _handles.TryGetValue(key, out var handle))
            {
                return;
            }

            Addressables.Release(handle);
            _handles.Remove(key);
        }
    }
}
