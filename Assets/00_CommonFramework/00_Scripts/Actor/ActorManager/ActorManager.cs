using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer.Unity;

namespace O2un.Actors
{
    public sealed class ActorManager : IActorRegistry, IActorQuery, ITickable, IFixedTickable
    {
        private static readonly IReadOnlyList<IActor> EMPTY = Array.Empty<IActor>();

        private readonly Dictionary<ActorType, List<IActor>> _actives = new();
        private readonly HashSet<IActor> _activeActors = new();
        private readonly List<IActor> _tickActors = new();
        private readonly List<IActor> _tickSnapshot = new();
        private readonly List<IActor> _fixedTickActors = new();
        private readonly List<IActor> _fixedTickSnapshot = new();
        private IActor _player;

        public IActor Player => _player;

        public void Register(IActor actor)
        {
            if (null == actor)
            {
                return;
            }

            List<IActor> list = GetOrCreate(actor.Type);
            if (true == list.Contains(actor))
            {
                return;
            }

            list.Add(actor);
            _activeActors.Add(actor);

            if (actor is IActorTickable)
            {
                _tickActors.Add(actor);
            }

            if (actor is IActorFixedTickable)
            {
                _fixedTickActors.Add(actor);
            }

            if (ActorType.Player == actor.Type)
            {
                _player = actor;
            }
        }

        public void Unregister(IActor actor)
        {
            if (null == actor)
            {
                return;
            }

            if (true == _actives.TryGetValue(actor.Type, out List<IActor> list))
            {
                list.Remove(actor);
            }

            _activeActors.Remove(actor);

            if (actor is IActorTickable)
            {
                _tickActors.Remove(actor);
            }

            if (actor is IActorFixedTickable)
            {
                _fixedTickActors.Remove(actor);
            }

            if (true == ReferenceEquals(_player, actor))
            {
                _player = null;
            }
        }

        public void Tick()
        {
            float deltaTime = Time.deltaTime;
            _tickSnapshot.Clear();
            _tickSnapshot.AddRange(_tickActors);

            for (int i = 0; i < _tickSnapshot.Count; i++)
            {
                IActor actor = _tickSnapshot[i];
                if (true == _activeActors.Contains(actor) && actor is IActorTickable tickable)
                {
                    tickable.Tick(deltaTime);
                }
            }
        }

        public void FixedTick()
        {
            float fixedDeltaTime = Time.fixedDeltaTime;
            _fixedTickSnapshot.Clear();
            _fixedTickSnapshot.AddRange(_fixedTickActors);

            for (int i = 0; i < _fixedTickSnapshot.Count; i++)
            {
                IActor actor = _fixedTickSnapshot[i];
                if (true == _activeActors.Contains(actor) && actor is IActorFixedTickable fixedTickable)
                {
                    fixedTickable.FixedTick(fixedDeltaTime);
                }
            }
        }

        public IReadOnlyList<IActor> GetActive(ActorType type)
        {
            return _actives.TryGetValue(type, out List<IActor> list) ? list : EMPTY;
        }

        private List<IActor> GetOrCreate(ActorType type)
        {
            if (false == _actives.TryGetValue(type, out List<IActor> list))
            {
                list = new();
                _actives[type] = list;
            }

            return list;
        }
    }
}
