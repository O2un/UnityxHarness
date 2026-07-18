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

        public void Register<T>(string key, T prefab, Transform parent = null) where T : Component
        {
            if (true == _handles.ContainsKey(key))
            {
                Debug.LogWarning($"[PoolManager] key already registered, ignored: {key}");
                return;
            }

            _handles.Add(key, new PoolModule<T>(_resolver, prefab, parent));
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

        // 룸 언로드처럼 풀 대상이 통째로 파괴되는 시점에 부른다. 남겨두면 파괴된 인스턴스를 다시 꺼내 쓴다.
        public void Unregister(string key)
        {
            if (false == _handles.TryGetValue(key, out var handle))
            {
                return;
            }

            if (handle is IDisposable disposable)
            {
                disposable.Dispose();
            }

            _handles.Remove(key);
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
