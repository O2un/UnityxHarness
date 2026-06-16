using System;
using Cysharp.Threading.Tasks;
using O2un.DataStore;
using O2un.Input;
using R3;
using UnityEngine;

namespace O2un.Actors 
{
    public sealed class PlayerActor : IDisposable
    {
        private readonly PlayerMover _mover;
        private readonly PlayerView _view;
        private readonly IPlayerDataWriter _playerData;
        private readonly CompositeDisposable _disposables = new();

        public PlayerActor(IInputReader input, PlayerView view, IPlayerDataWriter playerData)
        {
            _view = view;
            _mover = new(input);
            _playerData = playerData;
            _playerData.SetCurrentHP(100);
        }

        public void Init()
        {
            _mover.Velocity.Subscribe(v=> _view.SetVelocity(v)).AddTo(_disposables);
            //_mover.JumpImpulse.Subscribe(v=> _view.SetVelocity(v)).AddTo(_disposables);

            Observable.Interval(TimeSpan.FromSeconds(1)).Subscribe(_ =>
            {
                _playerData.VaryHP(-1);
            }).AddTo(_disposables);
        }
        

        public void Dispose()
        {
            _disposables.Dispose();
            _mover.Dispose();
        }
    }
}
