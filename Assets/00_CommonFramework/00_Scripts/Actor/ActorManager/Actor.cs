using System;
using UnityEngine;

namespace O2un.Actors
{
    public abstract class Actor : IActor, IDisposable
    {
        private readonly IActorRegistry _registry;

        private bool _registered;

        public abstract ActorType Type { get; }
        public abstract Transform Transform { get; }

        protected Actor(IActorRegistry registry)
        {
            _registry = registry;
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
            Unregister();
        }
    }

    public abstract class Actor<TView> : Actor where TView : class, IActorView
    {
        private readonly TView _view;

        public override Transform Transform => _view.transform;

        protected TView View => _view;

        protected Actor(TView view, IActorRegistry registry)
            : base(registry)
        {
            _view = view;
            _view.Bind(this);
        }

        public override void Dispose()
        {
            _view.Unbind(this);
            base.Dispose();
        }
    }
}
