using O2un.Actors;
using O2un.Input;
using R3;
using UnityEngine;

namespace O2un.ProjectB.Platformer
{
    public sealed class Player2DActor : Actor<PlayerView>
    {
        private readonly PlayerMover _mover;
        private readonly CompositeDisposable _disposables = new();

        private float _moveX;

        public override ActorType Type => ActorType.Player;

        public Player2DActor(MovementData data, IInputReader input, PlayerView view, IActorRegistry registry)
            : base(view, registry)
        {
            _mover = new PlayerMover(data);

            input.Move.Subscribe(v => _moveX = v.x).AddTo(_disposables);
            input.IsJumpPressed.Subscribe(_ => _mover.QueueJump()).AddTo(_disposables);
            input.IsJumpReleased.Subscribe(_ => _mover.RequestJumpCut()).AddTo(_disposables);
        }

        public override void Tick(float dt)
        {
            _mover.SetMoveInput(_moveX);
        }

        public Vector2 ResolvePhysics(bool grounded, float currentVerticalVelocity, float dt)
        {
            return _mover.ResolveVelocity(grounded, currentVerticalVelocity, dt);
        }

        public override void Dispose()
        {
            _disposables.Dispose();
            base.Dispose();
        }
    }
}
