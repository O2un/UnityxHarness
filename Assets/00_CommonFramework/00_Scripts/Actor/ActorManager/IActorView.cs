using UnityEngine;

namespace O2un.Actors
{
    public interface IActorView
    {
        Transform transform { get; }

        void Bind(Actor actor);
        void Unbind(Actor actor);
    }
}
