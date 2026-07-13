using O2un.Actors;
using O2un.DI;
using UnityEngine;
using VContainer;

namespace O2un.Progression
{
    public sealed class LevelUpSelectionContext : MonoBehaviour, ISceneInitializable
    {
        [SerializeField] private LevelUpSelectionView _view;

        [Inject] private IExperienceReader _experienceReader;
        [Inject] private IActorQuery _actorQuery;
        [Inject] private LevelUpSkillPoolSO _pool;

        private LevelUpSelectionViewModel _vm;

        public void Init()
        {
            if (null == _experienceReader || null == _actorQuery || null == _pool)
            {
                Debug.LogError($"[LevelUpSelectionContext] '{name}' 의존성 주입 실패 — experienceReader={_experienceReader != null}, actorQuery={_actorQuery != null}, pool={_pool != null}");
                return;
            }

            LevelUpSelectionModule module = new(_actorQuery, _pool);
            _vm = new LevelUpSelectionViewModel(module, _experienceReader);
            _view.Bind(_vm);
        }

        private void OnDestroy()
        {
            _vm?.Dispose();
        }
    }
}
