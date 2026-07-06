using System;
using O2un.DataStore;
using O2un.Manager;
using UnityEngine;
using VContainer;

namespace O2un.Actors
{
    public sealed class PlayerContext : MonoBehaviour, IPoolable
    {
        [SerializeField] private MoveStats _stats;
        [SerializeField] private ActorView _view;
        private PlayerActor _actor;

        [Inject]
        public void Init(IMoveDirectionProvider provider, IPlayerDataWriter playerData)
        {
            _actor = new(provider, _view, playerData, _stats);
            _actor.Init();
        }

        private void Update()
        {
            _actor?.Tick();
        }

        private void OnDestroy()
        {
            _actor?.Dispose();
        }

        private Action _release;
        public void SetReleaseCallback(Action release)
        {
            _release = release;
        }

        public void Release()
        {
            _release?.Invoke();
        }
    }
}
