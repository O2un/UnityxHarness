using System;
using UnityEngine;

namespace O2un.Actors
{
    public abstract class Actor : IActor, IDisposable
    {
        private readonly IActorRegistry _registry;
        private readonly ActorView _view;

        private bool _registered;

        public abstract ActorType Type { get; }
        public Transform Transform => _view.transform;

        protected ActorView View => _view;

        protected Actor(ActorView view, IActorRegistry registry)
        {
            _view = view;
            _registry = registry;
            _view.Bind(this);
        }

        public abstract void Tick(float dt);

        public void Register()
        {
            if (true == _registered)
            {
                return;
            }

            _registry.Register(this);
            _registered = true;
        }

        public void Unregister()
        {
            if (false == _registered)
            {
                return;
            }

            _registry.Unregister(this);
            _registered = false;
        }

        public virtual void Dispose()
        {
            _view.Unbind(this);
            Unregister();
        }
    }
}
