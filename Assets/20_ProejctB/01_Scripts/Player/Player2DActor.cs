using O2un.Actors;
using O2un.Combat;
using O2un.Input;
using R3;
using UnityEngine;

namespace O2un.ProjectB.Platformer
{
    public sealed class Player2DActor : Actor<PlayerView>, IActorTickable, IActorFixedTickable
    {
        private const float MELEE_HITBOX_LIFETIME = float.MaxValue;

        private readonly PlayerMover _mover;
        private readonly LayerMask _groundMask;
        private readonly Vector2 _groundCastSize;
        private readonly float _groundCastDistance;

        private readonly MeleeComboModule _combo;
        private readonly MeleeComboData _comboData;
        private readonly MeleeAttackView _attackView;
        private readonly MeleeAnimationEventBridge _bridge;
        private readonly HitboxModule[] _stageHitboxes;

        private readonly CompositeDisposable _disposables = new();

        private float _moveX;

        public override ActorType Type => ActorType.Player;

        public Player2DActor(MovementData data, IInputReader input, PlayerView view, IActorRegistry registry, MeleeComboRefs meleeRefs)
            : base(view, registry)
        {
            _mover = new PlayerMover(data);
            _groundMask = data.GroundMask;
            _groundCastSize = data.GroundCastSize;
            _groundCastDistance = data.GroundCastDistance;

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

                input.IsAttackPressed.Subscribe(_ => _combo.PressAttack()).AddTo(_disposables);
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
            base.Dispose();
        }

        private HitboxModule[] CreateStageHitboxes(MeleeComboData comboData)
        {
            var hitboxes = new HitboxModule[comboData.StageCount];
            for (int i = 0; i < hitboxes.Length; i++)
            {
                var config = new HitboxConfig(
                    comboData.GetStage(i + 1).Damage,
                    ActorType.Enemy,
                    HitPolicy.OncePerTarget,
                    0f,
                    MELEE_HITBOX_LIFETIME);

                var hitbox = new HitboxModule(config);
                hitbox.OnHit.Subscribe(e => e.Target.ApplyDamage(e.Damage)).AddTo(_disposables);
                hitboxes[i] = hitbox;
            }

            return hitboxes;
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
