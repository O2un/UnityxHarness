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
        [SerializeField] private ChaseAIProfile _profile;
        [SerializeField] private MonsterDataSO _monsterData;
        [SerializeField] private DamageableView _damageable;

        private readonly CompositeDisposable _disposables = new();

        private NpcActor _actor;
        private EnemyHealth _health;
        private EnemyContext _enemyContext;
        private SkillModule _attackSkills;
        private IActorRegistry _registry;
        private IActorQuery _query;
        private IAttackSpawner _spawner;

        [Inject]
        public void Construct(IActorRegistry registry, IActorQuery query, IAttackSpawner spawner)
        {
            _registry = registry;
            _query = query;
            _spawner = spawner;
            Build();
        }

        private void Build()
        {
            EnemyBlackboard blackboard = new();
            MoveStats moveStats = null != _monsterData ? _monsterData.Move : _stats;
            CharacterMover mover = new(moveStats);

            int maxHp = null != _monsterData ? _monsterData.MaxHp : 1;
            _health = new EnemyHealth(maxHp);

            _actor = new NpcActor(_profile.Build(blackboard, mover), blackboard, mover, _view, _registry, _query, _health);

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

            _attackSkills = BuildAttackSkills();
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
            _enemyContext?.Release();
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            _actor?.Tick(dt);
            _attackSkills?.Tick(dt);
        }

        private void OnDestroy()
        {
            _disposables.Dispose();
            _actor?.Dispose();
            _attackSkills?.Dispose();
        }
    }
}
