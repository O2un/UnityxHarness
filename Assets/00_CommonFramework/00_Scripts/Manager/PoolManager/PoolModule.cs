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
        private readonly Transform _parent;
        private readonly Vector3 _prefabScale;
        private readonly ObjectPool<T> _pool;

        public PoolModule(IObjectResolver resolver, T prefab, Transform parent = null)
        {
            _resolver = resolver;
            _prefab = prefab;
            _parent = parent;
            _prefabScale = prefab.transform.localScale;
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
            T instance = null != _parent
                    ? _resolver.Instantiate(_prefab, _parent)
                    : _resolver.Instantiate(_prefab);

            if (instance is IPoolable poolable)
            {
                poolable.SetReleaseCallback(() => Release(instance));
            }

            return instance;
        }

        private void OnGet(T obj)
        {
            Transform t = obj.transform;
            t.SetParent(_parent);
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
            t.localScale = _prefabScale;
            obj.gameObject.SetActive(true);

            if (obj is IPoolable poolable)
            {
                poolable.OnSpawned();
            }
        }

        private void OnRelease(T obj)
        {
            if (obj is IPoolable poolable)
            {
                poolable.OnDespawned();
            }

            obj.gameObject.SetActive(false);
        }

        private void OnDestroy(T obj)
        {
            if (null != obj)
            {
                UnityEngine.Object.Destroy(obj.gameObject);
            }
        }
    }
}
