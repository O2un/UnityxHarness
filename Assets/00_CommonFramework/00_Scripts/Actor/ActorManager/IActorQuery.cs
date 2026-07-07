using System.Collections.Generic;

namespace O2un.Actors
{
    public interface IActorQuery
    {
        IActor Player { get; }
        IReadOnlyList<IActor> GetActive(ActorType type);
    }
}
