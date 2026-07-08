using O2un.Actors;
using UnityEngine;

namespace O2un.Combat
{
    public interface ITargetStrategy
    {
        IActor Select(IActorQuery query, Vector3 origin);
    }
}
