using UnityEngine;

namespace O2un.Actors
{
    public interface IActor
    {
        ActorType Type { get; }
        Transform Transform { get; }
    }
}
