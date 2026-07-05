using UnityEngine;

namespace O2un.Manager
{
    public interface IPoolHandle<T> where T : Component
    {
        T Get();
        void Release(T obj);
    }
}
