using System;
using UnityEngine;
using UnityEngine.Pool;
using VContainer;
using VContainer.Unity;

namespace O2un.Manager
{
    public sealed class PoolModule<T> : IPoolHandle<T>, IDisposable where T : Component
    {
        private readonly IObjectResolver _resolver;
        private readonly T _prefab;
        private readonly ObjectPool<T> _pool;

        public PoolModule(IObjectResolver resolver, T prefab)
        {
            _resolver = resolver;
            _prefab = prefab;
            _pool = new ObjectPool<T>(OnCreate, OnGet, OnRelease, OnDestroy);
        }

        public T Get()
        {
            return _pool.Get();
        }

        public void Release(T obj)
        {
            _pool.Release(obj);
        }

        public void Dispose()
        {
            _pool.Dispose();
        }

        private T OnCreate()
        {
            T instance = _resolver.Instantiate(_prefab);

            if (instance is IPoolable poolable)
            {
                poolable.SetReleaseCallback(() => Release(instance));
            }

            return instance;
        }

        private void OnGet(T obj)
        {
            Transform t = obj.transform;
            t.SetParent(null);
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
            t.localScale = Vector3.one;
            obj.gameObject.SetActive(true);
        }

        private void OnRelease(T obj)
        {
            obj.gameObject.SetActive(false);
        }

        private void OnDestroy(T obj)
        {
            UnityEngine.Object.Destroy(obj.gameObject);
        }
    }
}
