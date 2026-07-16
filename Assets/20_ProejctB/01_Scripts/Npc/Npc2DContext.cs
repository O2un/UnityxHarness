using O2un.Actors;
using O2un.Combat;
using O2un.DI;
using R3;
using UnityEngine;
using VContainer;

namespace O2un.ProjectB.Platformer
{
    public sealed class Npc2DContext : MonoBehaviour, ISceneInitializable
    {
        [SerializeField] private NpcView _view;
        [SerializeField] private Damageable2DView _damageable;
        [SerializeField] private int _maxHp = 10;

        [Inject] private IActorRegistry _registry;

        private readonly CompositeDisposable _disposables = new();

        private Npc2DActor _actor;

        public void Init()
        {
            if (null == _view || null == _damageable || null == _registry)
            {
                Debug.LogError($"[Npc2DContext] '{name}' 의존성 누락 — view={_view != null}, damageable={_damageable != null}, registry={_registry != null}");
                return;
            }

            EnemyHealth health = new(_maxHp);
            _actor = new Npc2DActor(_view, _registry, health);
            _damageable.Bind(ActorType.Enemy, health);

            health.CurrentHP
                .Subscribe(hp => Debug.Log($"[Npc2DContext] '{name}' HP {hp}/{health.MaxHP}"))
                .AddTo(_disposables);
            health.OnDeath
                .Subscribe(_ => OnDeath())
                .AddTo(_disposables);
        }

        private void OnDeath()
        {
            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            _disposables.Dispose();
            _actor?.Dispose();
        }
    }
}
