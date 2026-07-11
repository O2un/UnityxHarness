using Cysharp.Threading.Tasks;
using O2un.Manager;
using UnityEngine;
using VContainer;

namespace O2un.UI
{
    public sealed class GameSelectContext : MonoBehaviour
    {
        [SerializeField] private GameSelectView _view;

        private GameSelectVM _vm;
        private ISceneService _sceneService;

        [Inject]
        public void Construct(ISceneService sceneService)
        {
            _sceneService = sceneService;
            _vm = new GameSelectVM();
            _view.Bind(_vm);
            _vm.ProjectASelected += OnProjectASelected;
        }

        private void OnProjectASelected()
        {
            _sceneService.LoadSceneAsync(SCENE_NAME.GAME_SCENE).Forget();
        }

        private void OnDestroy()
        {
            if (null != _vm)
            {
                _vm.ProjectASelected -= OnProjectASelected;
            }
        }
    }
}
