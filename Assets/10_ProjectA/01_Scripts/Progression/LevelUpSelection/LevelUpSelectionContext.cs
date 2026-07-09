using O2un.Actors;
using UnityEngine;
using VContainer;

namespace O2un.Progression
{
    public sealed class LevelUpSelectionContext : MonoBehaviour
    {
        [SerializeField] private LevelUpSelectionView _view;

        private LevelUpSelectionViewModel _vm;

        [Inject]
        public void Init(IExperienceReader experienceReader, IActorQuery actorQuery, LevelUpSkillPoolSO pool)
        {
            LevelUpSelectionModule module = new(actorQuery, pool);
            _vm = new LevelUpSelectionViewModel(module, experienceReader);
            _view.Bind(_vm);
        }

        private void OnDestroy()
        {
            _vm?.Dispose();
        }
    }
}
