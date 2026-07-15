using System;
using O2un.Input;
using R3;
using UnityEngine;

namespace O2un.ProjectB.Platformer
{
    public sealed class Player2DActor : IDisposable
    {
        private readonly PlayerMover _mover;
        private readonly CompositeDisposable _disposables = new();

        private float _moveX;

        public Player2DActor(MovementData data, IInputReader input)
        {
            _mover = new PlayerMover(data);

            input.Move.Subscribe(v => _moveX = v.x).AddTo(_disposables);
            input.IsJumpPressed.Subscribe(_ => _mover.QueueJump()).AddTo(_disposables);
        }

        public void Tick()
        {
            _mover.SetMoveInput(_moveX);
        }

        public Vector2 ResolvePhysics(bool grounded, float currentVerticalVelocity)
        {
            return _mover.ResolveVelocity(grounded, currentVerticalVelocity);
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
}
