using UnityEngine;
using VContainer;

namespace O2un.Manager
{
    public sealed class ScoreContext : MonoBehaviour
    {
        [SerializeField] private ScoreView _view;

        [Inject]
        public void Init(IScoreManager scoreManager)
        {
            var vm = new ScoreVM(scoreManager);
            _view.Bind(vm);
        }
    }
}
