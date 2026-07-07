using System;
using System.Collections.Generic;

namespace O2un.Actors
{
    public sealed class ActorManager : IActorRegistry, IActorQuery
    {
        private static readonly IReadOnlyList<IActor> EMPTY = Array.Empty<IActor>();

        private readonly Dictionary<ActorType, List<IActor>> _actives = new();
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

            if (true == ReferenceEquals(_player, actor))
            {
                _player = null;
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
