using O2un.Actors;
using O2un.Combat;
using O2un.Input;
using O2un.Manager;
using R3;
using UnityEngine;

namespace O2un.ProjectB.Platformer
{
    public sealed class Player2DActor : Actor<PlayerView>, IActorTickable, IActorFixedTickable
    {
        private const float MELEE_HITBOX_LIFETIME = float.MaxValue;

        private static readonly int _skillHash = Animator.StringToHash("Skill");

        private readonly PlayerMover _mover;
        private readonly LayerMask _groundMask;
        private readonly Vector2 _groundCastSize;
        private readonly float _groundCastDistance;

        private readonly MeleeComboModule _combo;
        private readonly MeleeComboData _comboData;
        private readonly MeleeAttackView _attackView;
        private readonly MeleeAnimationEventBridge _bridge;
        private readonly HitboxModule[] _stageHitboxes;

        private readonly IPlayerStatReader _stat;

        private readonly RangedSkillModule _rangedSkill;
        private readonly RangedSkillData _rangedData;
        private readonly IPoolService _pool;

        private readonly PassiveSkillModule _passive;
        private readonly PassiveSkillData _passiveData;
        private readonly IActorQuery _actorQuery;
        private readonly ITargetStrategy _missileTargeting = new NearestEnemyStrategy();

        private readonly CompositeDisposable _disposables = new();

        private float _moveX;

        public override ActorType Type => ActorType.Player;

        // 원거리 스킬 미할당으로 시작할 수 있으므로 null을 반환할 수 있다
        public ReadOnlyReactiveProperty<float> RangedCooldownNormalized => _rangedSkill?.CooldownNormalized;

        public Player2DActor(MovementData data, IInputReader input, PlayerView view, IActorRegistry registry, MeleeComboRefs meleeRefs, RangedSkillRefs rangedRefs, IPoolService pool, IPlayerStatReader stat, PassiveSkillData passiveData, IPassiveSkillQuery passiveQuery, IActorQuery actorQuery)
            : base(view, registry)
        {
            _actorQuery = actorQuery;

            if (null != passiveData && null != passiveQuery)
            {
                _passiveData = passiveData;
                _passive = new PassiveSkillModule(passiveData, passiveQuery);
            }

            _mover = new PlayerMover(data);
            _groundMask = data.GroundMask;
            _groundCastSize = data.GroundCastSize;
            _groundCastDistance = data.GroundCastDistance;

            _stat = stat;
            _stat.MoveSpeed.Subscribe(_mover.SetMaxMoveSpeed).AddTo(_disposables);

            if (null != meleeRefs)
            {
                _comboData = meleeRefs.Data;
                _attackView = meleeRefs.AttackView;
                _bridge = meleeRefs.Bridge;

                _combo = new MeleeComboModule(_comboData.StageCount, _comboData.InputBufferTime);
                _stageHitboxes = CreateStageHitboxes(_comboData);

                _combo.OnAttackTriggered.Subscribe(OnAttackTriggered).AddTo(_disposables);
                _combo.OnComboReset.Subscribe(_ => OnComboReset()).AddTo(_disposables);

                _bridge.OnHitboxOn.Subscribe(_ => OnHitboxOn()).AddTo(_disposables);
                _bridge.OnHitboxOff.Subscribe(_ => _attackView.DisableHitbox()).AddTo(_disposables);
                _bridge.OnComboWindowOpen.Subscribe(_ => _combo.OpenComboWindow()).AddTo(_disposables);
                _bridge.OnComboWindowClose.Subscribe(_ => _combo.CloseComboWindow()).AddTo(_disposables);
                _bridge.OnAttackEnd.Subscribe(_ => _combo.NotifyStageEnd()).AddTo(_disposables);

                input.IsAttackPressed.Subscribe(_ => PressMeleeAttack()).AddTo(_disposables);
            }

            if (null != rangedRefs && null != pool)
            {
                _rangedData = rangedRefs.Data;
                _pool = pool;

                _rangedSkill = new RangedSkillModule(_rangedData.Cooldown, _rangedData.MaxCastTime);

                _rangedSkill.OnActivated.Subscribe(_ => View.SetAnimatorTrigger(_skillHash)).AddTo(_disposables);

                rangedRefs.Bridge.OnFireProjectile.Subscribe(_ => FireProjectile()).AddTo(_disposables);
                rangedRefs.Bridge.OnSkillEnd.Subscribe(_ => _rangedSkill.NotifyCastEnd()).AddTo(_disposables);

                input.IsSkillPressed.Subscribe(_ => _rangedSkill.TryActivate()).AddTo(_disposables);
            }

            input.Move.Subscribe(v => _moveX = v.x).AddTo(_disposables);
            input.IsJumpPressed.Subscribe(_ =>
            {
                _mover.QueueJump();
                CancelAttackIfActive();
            }).AddTo(_disposables);
            input.IsJumpReleased.Subscribe(_ => _mover.RequestJumpCut()).AddTo(_disposables);
        }

        public override void Tick(float deltaTime)
        {
            _mover.SetMoveInput(_moveX);
            _combo?.Tick(deltaTime);
            _rangedSkill?.Tick(deltaTime);
            _passive?.Tick(deltaTime);
        }

        public void FixedTick(float fixedDeltaTime)
        {
            bool grounded = View.CheckGrounded(_groundMask, _groundCastSize, _groundCastDistance);
            Vector2 velocity = _mover.ResolveVelocity(grounded, View.VerticalVelocity, fixedDeltaTime);
            View.ApplyPhysics(velocity, grounded);
        }

        public override void Dispose()
        {
            _disposables.Dispose();
            _combo?.Dispose();
            _rangedSkill?.Dispose();
            base.Dispose();
        }

        private HitboxModule[] CreateStageHitboxes(MeleeComboData comboData)
        {
            var hitboxes = new HitboxModule[comboData.StageCount];
            for (int i = 0; i < hitboxes.Length; i++)
            {
                int stageDamage = comboData.GetStage(i + 1).Damage;

                var config = new HitboxConfig(
                    stageDamage,
                    ActorType.Enemy,
                    HitPolicy.OncePerTarget,
                    0f,
                    MELEE_HITBOX_LIFETIME);

                var hitbox = new HitboxModule(config);
                // HitboxConfig가 readonly struct라 생성 시 피해량이 굳는다. 강화 반영을 위해 적중 시점에 다시 해석한다
                hitbox.OnHit.Subscribe(e => OnMeleeHit(e, stageDamage)).AddTo(_disposables);
                hitboxes[i] = hitbox;
            }

            return hitboxes;
        }

        private void OnMeleeHit(DamageEvent e, int stageDamage)
        {
            e.Target.ApplyDamage(ResolveMeleeDamage(stageDamage));
            TryFireHomingMissile();
        }

        // 스테이지 base + AttackBonus → 크리티컬 배수 → 최소 1 순서로 계산한다.
        private int ResolveMeleeDamage(int stageDamage)
        {
            int damage = stageDamage + _stat.AttackBonus.CurrentValue;

            if (null == _passive)
            {
                return Mathf.Max(1, damage);
            }

            return _passive.ResolveMeleeDamage(damage).Damage;
        }

        private void TryFireHomingMissile()
        {
            if (null == _passive || false == _passive.CanFireMissile)
            {
                return;
            }

            if (null == _pool || null == _passiveData.MissilePrefab)
            {
                Debug.LogWarning("[Player2DActor] 유도 미사일 프리팹 또는 IPoolService가 없어 발사를 건너뜁니다.");
                return;
            }

            Vector2 direction = View.FacingDirection;
            Transform target = ResolveMissileTarget();

            if (null != target)
            {
                Vector2 toTarget = (Vector2)(target.position - View.transform.position);
                if (toTarget.sqrMagnitude > Mathf.Epsilon)
                {
                    direction = toTarget.normalized;
                }
            }

            Projectile2DView missile = SpawnProjectile(
                _passiveData.MissilePrefab,
                _passiveData.MissilePoolKey,
                _passiveData.MissileDamage,
                _passiveData.MissileSpeed,
                _passiveData.MissileLifetime,
                _passiveData.MissileMuzzleOffset,
                direction);

            if (null == missile)
            {
                return;
            }

            missile.SetHoming(target, _passiveData.MissileTurnRate);
            _passive.NotifyMissileFired();
        }

        private Transform ResolveMissileTarget()
        {
            if (null == _actorQuery)
            {
                return null;
            }

            IActor nearest = _missileTargeting.Select(_actorQuery, View.transform.position);
            return null != nearest ? nearest.Transform : null;
        }

        private void OnAttackTriggered(int stage)
        {
            _attackView.Configure(_stageHitboxes[stage - 1]);
            _attackView.PlayAttack(stage);
        }

        private void OnComboReset()
        {
            _attackView.DisableHitbox();
        }

        private void OnHitboxOn()
        {
            int stage = _combo.CurrentStage;
            if (0 == stage)
            {
                return;
            }

            MeleeComboStage stageData = _comboData.GetStage(stage);
            _attackView.EnableHitbox(stageData.HitboxSize, stageData.HitboxOffset);
        }

        private void PressMeleeAttack()
        {
            if (null != _rangedSkill && true == _rangedSkill.IsCasting)
            {
                return;
            }

            _combo.PressAttack();
        }

        private void FireProjectile()
        {
            if (null == _rangedSkill || null == _rangedData.ProjectilePrefab)
            {
                return;
            }

            SpawnProjectile(
                _rangedData.ProjectilePrefab,
                _rangedData.PoolKey,
                _rangedData.Damage,
                _rangedData.ProjectileSpeed,
                _rangedData.Lifetime,
                _rangedData.MuzzleOffset,
                View.FacingDirection);
        }

        private Projectile2DView SpawnProjectile(Projectile2DView prefab, string poolKey, int damage, float speed, float lifetime, Vector2 muzzleOffset, Vector2 direction)
        {
            if (false == _pool.IsRegistered(poolKey))
            {
                _pool.Register(poolKey, prefab);
            }

            IPoolHandle<Projectile2DView> handle = _pool.GetHandle<Projectile2DView>(poolKey);
            if (null == handle)
            {
                return null;
            }

            Projectile2DView projectile = handle.Get();

            var config = new HitboxConfig(
                damage,
                ActorType.Enemy,
                HitPolicy.OncePerTarget,
                0f,
                lifetime);

            var hitbox = new HitboxModule(config);

            Vector2 facing = View.FacingDirection;
            Vector3 origin = View.transform.position + new Vector3(facing.x < 0f ? -muzzleOffset.x : muzzleOffset.x, muzzleOffset.y, 0f);

            projectile.Configure(hitbox, direction, speed, origin, true);
            return projectile;
        }

        private void CancelAttackIfActive()
        {
            if (null == _combo || false == _combo.IsAttacking)
            {
                return;
            }

            _combo.Cancel();
            // 자연 종료는 Animator가 스스로 복귀하므로 강제 전이 트리거는 캔슬 경로에서만 쏜다
            _attackView.PlayCancel();
        }
    }
}
