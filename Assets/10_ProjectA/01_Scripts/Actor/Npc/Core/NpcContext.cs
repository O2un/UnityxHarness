using System.Collections.Generic;
using O2un.Combat;
using R3;
using UnityEngine;
using VContainer;

namespace O2un.Actors
{
    public sealed class NpcContext : MonoBehaviour
    {
        [SerializeField] private MoveStats _stats;
        [SerializeField] private ActorView _view;
        [SerializeField] private EnemyAIProfileSO _profile;
        [SerializeField] private MonsterDataSO _monsterData;
        [SerializeField] private DamageableView _damageable;

        private readonly CompositeDisposable _disposables = new();

        private NpcActor _actor;
        private EnemyHealth _health;
        private EnemyContext _enemyContext;
        private IActorRegistry _registry;
        private IActorQuery _query;
        private IAttackSpawner _spawner;
        private IEnemyKillEvent _killEvent;
        private IEnemyDamagePublisher _damagePublisher;

        [Inject]
        public void Construct(IActorRegistry registry, IActorQuery query, IAttackSpawner spawner, IEnemyKillEvent killEvent, IEnemyDamagePublisher damagePublisher)
        {
            _registry = registry;
            _query = query;
            _spawner = spawner;
            _killEvent = killEvent;
            _damagePublisher = damagePublisher;
            Build();
        }

        private void Build()
        {
            EnemyBlackboard blackboard = new();
            MoveStats moveStats = null != _monsterData ? _monsterData.Move : _stats;
            CharacterMover mover = new(moveStats);

            int maxHp = null != _monsterData ? _monsterData.MaxHp : 1;
            _health = new EnemyHealth(maxHp, _damagePublisher.Publish);

            SkillModule attackSkills = BuildAttackSkills();
            _actor = new NpcActor(_profile.Build(blackboard, mover), blackboard, mover, _view, _registry, _query, _health, attackSkills);

            if (null != _damageable)
            {
                _damageable.Bind(ActorType.Enemy, _health);
            }

            _health.OnDeath.Subscribe(_ => OnDeath()).AddTo(_disposables);

            _enemyContext = GetComponentInParent<EnemyContext>(true);
            if (null != _enemyContext)
            {
                _enemyContext.SetLifecycleCallbacks(_health.ResetFull, null);
            }
            else
            {
                Debug.LogWarning($"[NpcContext] '{name}'에서 EnemyContext를 찾지 못해 풀 재사용 시 체력이 리셋되지 않습니다.");
            }

        }

        private SkillModule BuildAttackSkills()
        {
            if (null == _monsterData || null == _monsterData.AttackSkill)
            {
                return null;
            }

            List<ISkillDefinition> defs = new(1) { _monsterData.AttackSkill.Build() };
            SkillContext context = new(_query, _spawner, _view.transform);
            return new SkillModule(defs, context);
        }

        private void OnDeath()
        {
            Vector3 position = transform.position;
            int exp = null != _monsterData ? _monsterData.Exp : 0;

            _enemyContext?.Release();
            _killEvent?.Publish(new EnemyKilledInfo(position, exp));
        }

        private void OnDestroy()
        {
            _disposables.Dispose();
            _actor?.Dispose();
        }
    }
}
