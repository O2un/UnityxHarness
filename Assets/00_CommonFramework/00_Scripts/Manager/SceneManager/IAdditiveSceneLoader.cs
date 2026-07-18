using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;
using VContainer.Unity;

namespace O2un.Manager
{
    public interface IAdditiveSceneLoader
    {
        UniTask<Scene> LoadAdditiveSceneAsync(string key, LifetimeScope parentScope);
        UniTask UnloadSceneAsync(Scene scene);
    }
}
