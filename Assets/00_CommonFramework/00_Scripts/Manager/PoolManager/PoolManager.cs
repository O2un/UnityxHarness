using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

namespace O2un.Manager
{
    public sealed class PoolManager : IPoolService, IDisposable
    {
        private readonly IObjectResolver _resolver;
        private readonly Dictionary<string, object> _handles = new();

        public PoolManager(IObjectResolver resolver)
        {
            _resolver = resolver;
        }

        public void Register<T>(string key, T prefab) where T : Component
        {
            if (true == _handles.ContainsKey(key))
            {
                Debug.LogWarning($"[PoolManager] key already registered, ignored: {key}");
                return;
            }

            _handles.Add(key, new PoolModule<T>(_resolver, prefab));
        }

        public bool IsRegistered(string key)
        {
            return _handles.ContainsKey(key);
        }

        public IPoolHandle<T> GetHandle<T>(string key) where T : Component
        {
            if (false == _handles.TryGetValue(key, out var handle))
            {
                Debug.LogError($"[PoolManager] key not registered: {key}");
                return null;
            }

            if (handle is IPoolHandle<T> typed)
            {
                return typed;
            }

            Debug.LogError($"[PoolManager] type mismatch for key '{key}', requested {typeof(T)}");
            return null;
        }

        public void Dispose()
        {
            foreach (var handle in _handles.Values)
            {
                if (handle is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }

            _handles.Clear();
        }
    }
}
