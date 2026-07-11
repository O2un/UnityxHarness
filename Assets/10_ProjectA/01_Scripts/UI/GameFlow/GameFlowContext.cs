using O2un.Manager;
using UnityEngine;
using VContainer;

namespace O2un.UI
{
    public sealed class GameFlowContext : MonoBehaviour
    {
        [SerializeField] private GameFlowView _view;

        private GameFlowVM _vm;

        [Inject]
        public void Construct(IGameManager gameManager)
        {
            _vm = new GameFlowVM(gameManager);
            _view.Bind(_vm);
            _view.GameSelectRequested += OnGameSelectRequested;
        }

        private void OnGameSelectRequested()
        {
            Debug.Log("[GameFlowContext] Game select requested (scene transition wired in stage 5).");
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
