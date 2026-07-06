using System;
using O2un.DataStore;
using R3;
using UnityEngine;

namespace O2un.Actors
{
    public sealed class PlayerActor : IDisposable
    {
        private readonly CharacterMover _mover;
        private readonly ActorView _view;
        private readonly IMoveDirectionProvider _provider;
        private readonly IPlayerDataWriter _playerData;

        public PlayerActor(IMoveDirectionProvider provider, ActorView view, IPlayerDataWriter playerData, MoveStats stats)
        {
            _provider = provider;
            _view = view;
            _mover = new(stats);
            _playerData = playerData;
            _playerData.SetCurrentHP(100);
        }

        private readonly CompositeDisposable _disposables = new();

        public void Init()
        {
            Observable.Interval(TimeSpan.FromSeconds(1)).Subscribe(_ =>
            {
                _playerData.VaryHP(-1);
            }).AddTo(_disposables);
        }

        public void Tick()
        {
            Vector3 dir = _provider.GetDirection();
            _mover.SetDirection(dir);

            Vector3 velocity = _mover.Velocity.CurrentValue;
            _view.Move(velocity);

            if (velocity.sqrMagnitude > 0f)
            {
                _view.RotateTo(_mover.TargetRotation, _mover.RotationSpeed);
            }
        }

        public void Dispose()
        {
            _disposables.Dispose();
            _mover.Dispose();
        }
    }
}
