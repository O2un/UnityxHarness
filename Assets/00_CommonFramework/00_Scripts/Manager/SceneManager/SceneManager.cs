using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using O2un.UI;
using R3;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using VContainer.Unity;

namespace O2un.Manager 
{
    public interface ISceneService
    {
        UniTask LoadSceneAsync(string sceneName);
    }

    public static class SCENE_NAME
    {
        public const string LOADING_SCENE = "Loading";
        public const string GAME_SCENE = "GameScene";
        public const string GAME_SELECT_SCENE = "GameSelect";
        public const string GAME_2D_SCENE = "2D_GameScene";
    }

    public sealed class SceneManager : IDisposable, ISceneService, ILoadingSource, IAdditiveSceneLoader
    {
        public enum SceneState
        {
            Idle,
            TransitionToLoading,
            LoadingTarget,
            TransitionToTarget,
        }

        private readonly ReactiveProperty<SceneState> _currentState = new(SceneState.Idle);
        private readonly ReactiveProperty<float> _loadingProgress = new(0f);
        private readonly Dictionary<Scene, AsyncOperationHandle<SceneInstance>> _additiveHandles = new();
        public ReadOnlyReactiveProperty<SceneState> CurrentState => _currentState;
        public ReadOnlyReactiveProperty<float> LoadingProgress => _loadingProgress;

        public void Dispose()
        {
            _additiveHandles.Clear();
            _currentState.Dispose();
            _loadingProgress.Dispose();
        }

        public async UniTask LoadSceneAsync(string sceneName)
        {
            if(SceneState.Idle != _currentState.Value)
            {
                return;
            }

            try
            {
                _currentState.Value = SceneState.TransitionToLoading;
                _loadingProgress.Value = 0;

                await UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(SCENE_NAME.LOADING_SCENE, LoadSceneMode.Single);
                _currentState.Value = SceneState.LoadingTarget;

                var loadOp = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
                loadOp.allowSceneActivation = false;

                while(loadOp.progress < 0.9f)
                {
                    _loadingProgress.Value = loadOp.progress;
                    await UniTask.Yield(PlayerLoopTiming.Update);
                    await UniTask.Delay(1000);
                }

                _loadingProgress.Value = 1f;
                _currentState.Value = SceneState.TransitionToTarget;

                loadOp.allowSceneActivation = true;

                await loadOp;
            }
            finally
            {
                _currentState.Value = SceneState.Idle;
                _loadingProgress.Value = 0;
            }
        }

        public async UniTask<Scene> LoadAdditiveSceneAsync(string key, LifetimeScope parentScope)
        {
            var handle = default(AsyncOperationHandle<SceneInstance>);

            try
            {
                if(null != parentScope)
                {
                    using(LifetimeScope.EnqueueParent(parentScope))
                    {
                        handle = Addressables.LoadSceneAsync(key, LoadSceneMode.Additive);
                        await handle.ToUniTask();
                    }
                }
                else
                {
                    handle = Addressables.LoadSceneAsync(key, LoadSceneMode.Additive);
                    await handle.ToUniTask();
                }
            }
            catch(Exception e)
            {
                Debug.LogError($"[SceneManager] Additive load failed. key={key}\n{e}");

                if(handle.IsValid())
                {
                    Addressables.Release(handle);
                }

                throw;
            }

            var scene = handle.Result.Scene;
            _additiveHandles[scene] = handle;
            return scene;
        }

        public async UniTask UnloadSceneAsync(Scene scene)
        {
            if(false == _additiveHandles.TryGetValue(scene, out var handle))
            {
                Debug.LogWarning($"[SceneManager] Unload skipped. Not loaded by this loader. scene={scene.name}");
                return;
            }

            _additiveHandles.Remove(scene);
            await Addressables.UnloadSceneAsync(handle).ToUniTask();
        }
    }
}
