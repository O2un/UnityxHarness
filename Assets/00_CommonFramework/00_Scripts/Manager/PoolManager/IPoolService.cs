using UnityEngine;

namespace O2un.Manager
{
    public interface IPoolService
    {
        void Register<T>(string key, T prefab) where T : Component;
        bool IsRegistered(string key);
        IPoolHandle<T> GetHandle<T>(string key) where T : Component;
    }
}
