using Cysharp.Threading.Tasks;
using O2un.DI;
using O2un.Manager;
using UnityEngine;
using VContainer;

namespace O2un.UI
{
    public sealed class GameFlowContext : MonoBehaviour, ISceneInitializable
    {
        [SerializeField] private GameFlowView _view;

        [Inject] private IGameManager _gameManager;
        [Inject] private ISceneService _sceneService;

        private GameFlowVM _vm;

        public void Init()
        {
            if (null == _gameManager || null == _sceneService)
            {
                Debug.LogError($"[GameFlowContext] '{name}' 의존성 주입 실패 — gameManager={_gameManager != null}, sceneService={_sceneService != null}");
                return;
            }

            _vm = new GameFlowVM(_gameManager);
            _view.Bind(_vm);
            _view.GameSelectRequested += OnGameSelectRequested;
        }

        private void OnGameSelectRequested()
        {
            _sceneService.LoadSceneAsync(SCENE_NAME.GAME_SELECT_SCENE).Forget();
        }

        private void OnDestroy()
        {
            if (null != _view)
            {
                _view.GameSelectRequested -= OnGameSelectRequested;
            }
            _vm?.Dispose();
        }
    }
}
