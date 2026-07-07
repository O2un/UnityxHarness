using System;
using O2un.DataStore;
using R3;
using UnityEngine;

namespace O2un.Actors
{
    public sealed class PlayerActor : Actor
    {
        private readonly CharacterMover _mover;
        private readonly IMoveDirectionProvider _provider;
        private readonly IPlayerDataWriter _playerData;
        private readonly CompositeDisposable _disposables = new();

        public override ActorType Type => ActorType.Player;

        public PlayerActor(IMoveDirectionProvider provider, ActorView view, IPlayerDataWriter playerData, MoveStats stats, IActorRegistry registry)
            : base(view, registry)
        {
            _provider = provider;
            _mover = new(stats);
            _playerData = playerData;
            _playerData.SetCurrentHP(100);
        }

        public void Init()
        {
            Observable.Interval(TimeSpan.FromSeconds(1)).Subscribe(_ =>
            {
                _playerData.VaryHP(-1);
            }).AddTo(_disposables);
        }

        public override void Tick(float dt)
        {
            Vector3 dir = _provider.GetDirection();
            _mover.SetDirection(dir);

            Vector3 velocity = _mover.Velocity.CurrentValue;
            View.Move(velocity);

            if (velocity.sqrMagnitude > 0f)
            {
                View.RotateTo(_mover.TargetRotation, _mover.RotationSpeed);
            }
        }

        public override void Dispose()
        {
            _disposables.Dispose();
            _mover.Dispose();
            base.Dispose();
        }
    }
}
