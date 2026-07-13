using O2un.DI;
using UnityEngine;
using VContainer;

namespace O2un.Manager
{
    public sealed class ScoreContext : MonoBehaviour, ISceneInitializable
    {
        [SerializeField] private ScoreView _view;

        [Inject] private IScoreManager _scoreManager;

        public void Init()
        {
            if (null == _scoreManager)
            {
                Debug.LogError($"[ScoreContext] '{name}' 의존성 주입 실패 — scoreManager=null");
                return;
            }

            ScoreVM vm = new(_scoreManager);
            _view.Bind(vm);
        }
    }
}
