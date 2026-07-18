using O2un.Actors;
using O2un.Combat;
using O2un.DI;
using O2un.Sound;
using R3;
using UnityEngine;
using VContainer;

namespace O2un.ProjectB.Platformer
{
    public sealed class Npc2DContext : MonoBehaviour, ISceneInitializable
    {
        [SerializeField] private NpcView _view;
        [SerializeField] private Damageable2DView _damageable;
        [SerializeField] private Enemy2DSensorView _sensor;
        [SerializeField] private Enemy2DAIProfileSO _aiProfile;
        [SerializeField] private EnemyMeleeAttackView _attackView;
        [SerializeField] private int _maxHp = 10;

        private readonly CompositeDisposable _disposables = new();

        private Npc2DActor _actor;
        private EnemyHealth _health;
        private EnemyContext _enemyContext;
        private IActorRegistry _registry;
        private ISoundSignalSource _soundSource;
        private IEnemyKillEvent _killEvent;

        public IEnemy2DBlackboard Blackboard => _actor?.Blackboard;

        [Inject]
        public void Construct(IActorRegistry registry, ISoundSignalSource soundSource, IEnemyKillEvent killEvent)
        {
            _registry = registry;
            _soundSource = soundSource;
            _killEvent = killEvent;
            Build();
        }

        // 씬 배치 적은 InjectGameObject 직후 Init()이 한 번 더 불리므로 Build()는 멱등이어야 한다.
        public void Init()
        {
            Build();
        }

        private void Build()
        {
            if (null != _actor)
            {
                return;
            }

            if (null == _view || null == _damageable || null == _registry)
            {
                Debug.LogError($"[Npc2DContext] '{name}' 의존성 누락 — view={_view != null}, damageable={_damageable != null}, registry={_registry != null}");
                return;
            }

            _health = new EnemyHealth(_maxHp);
            _actor = new Npc2DActor(_view, _registry, _health, _sensor, _aiProfile, _attackView);
            _damageable.Bind(ActorType.Enemy, _health);

            if (null != _sensor)
            {
                _sensor.Init(_soundSource, _health.OnDamaged);
            }

            _enemyContext = GetComponentInParent<EnemyContext>(true);
            if (null != _enemyContext)
            {
                _enemyContext.SetLifecycleCallbacks(_health.ResetFull, null);
            }
            else
            {
                Debug.LogWarning($"[Npc2DContext] '{name}'에서 EnemyContext를 찾지 못해 풀 재사용 시 체력이 리셋되지 않습니다.");
            }

            _health.OnDeath
                .Subscribe(_ => OnDeath())
                .AddTo(_disposables);
        }

        private void OnDeath()
        {
            Vector3 position = transform.position;

            if (null != _enemyContext)
            {
                _enemyContext.Release();
            }
            else
            {
                gameObject.SetActive(false);
            }

            _killEvent?.Publish(new EnemyKilledInfo(position, 0));
        }

        private void OnDestroy()
        {
            _disposables.Dispose();
            _actor?.Dispose();
        }
    }
}
