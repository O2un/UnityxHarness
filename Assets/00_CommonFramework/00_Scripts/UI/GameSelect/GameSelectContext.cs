using Cysharp.Threading.Tasks;
using O2un.Manager;
using R3;
using UnityEngine;
using VContainer;

namespace O2un.UI
{
    public sealed class GameSelectContext : MonoBehaviour
    {
        [SerializeField] private GameSelectView _view;

        private GameSelectVM _vm;
        private ISceneService _sceneService;
        private readonly CompositeDisposable _disposables = new();

        [Inject]
        public void Construct(ISceneService sceneService)
        {
            _sceneService = sceneService;
            _vm = new GameSelectVM();
            _view.Bind(_vm);

            _vm.ProjectASelected.Subscribe(_ => OnProjectASelected()).AddTo(_disposables);
            _vm.ProjectBSelected.Subscribe(_ => OnProjectBSelected()).AddTo(_disposables);
        }

        private void OnProjectASelected()
        {
            _sceneService.LoadSceneAsync(SCENE_NAME.GAME_SCENE).Forget();
        }

        private void OnProjectBSelected()
        {
            _sceneService.LoadSceneAsync(SCENE_NAME.GAME_2D_SCENE).Forget();
        }

        private void OnDestroy()
        {
            _disposables.Dispose();
            _vm?.Dispose();
        }
    }
}
