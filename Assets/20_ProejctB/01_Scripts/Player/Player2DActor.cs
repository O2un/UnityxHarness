using O2un.Actors;
using O2un.Input;
using R3;
using UnityEngine;

namespace O2un.ProjectB.Platformer
{
    public sealed class Player2DActor : Actor<PlayerView>, IActorTickable, IActorFixedTickable
    {
        private readonly PlayerMover _mover;
        private readonly LayerMask _groundMask;
        private readonly Vector2 _groundCastSize;
        private readonly float _groundCastDistance;
        private readonly CompositeDisposable _disposables = new();

        private float _moveX;

        public override ActorType Type => ActorType.Player;

        public Player2DActor(MovementData data, IInputReader input, PlayerView view, IActorRegistry registry)
            : base(view, registry)
        {
            _mover = new PlayerMover(data);
            _groundMask = data.GroundMask;
            _groundCastSize = data.GroundCastSize;
            _groundCastDistance = data.GroundCastDistance;

            input.Move.Subscribe(v => _moveX = v.x).AddTo(_disposables);
            input.IsJumpPressed.Subscribe(_ => _mover.QueueJump()).AddTo(_disposables);
            input.IsJumpReleased.Subscribe(_ => _mover.RequestJumpCut()).AddTo(_disposables);
        }

        public override void Tick(float deltaTime)
        {
            _mover.SetMoveInput(_moveX);
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
            base.Dispose();
        }
    }
}
