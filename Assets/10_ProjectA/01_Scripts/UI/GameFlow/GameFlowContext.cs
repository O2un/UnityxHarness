using Cysharp.Threading.Tasks;
using O2un.Manager;
using UnityEngine;
using VContainer;

namespace O2un.UI
{
    public sealed class GameFlowContext : MonoBehaviour
    {
        [SerializeField] private GameFlowView _view;

        private GameFlowVM _vm;
        private ISceneService _sceneService;

        [Inject]
        public void Construct(IGameManager gameManager, ISceneService sceneService)
        {
            _sceneService = sceneService;
            _vm = new GameFlowVM(gameManager);
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
