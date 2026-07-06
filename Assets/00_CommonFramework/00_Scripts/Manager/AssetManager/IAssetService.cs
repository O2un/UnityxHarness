using Cysharp.Threading.Tasks;
using UnityEngine;

namespace O2un.Manager
{
    public interface IAssetService
    {
        UniTask<T> LoadAsync<T>(string key) where T : Object;
        void Release(string key);
    }
}
