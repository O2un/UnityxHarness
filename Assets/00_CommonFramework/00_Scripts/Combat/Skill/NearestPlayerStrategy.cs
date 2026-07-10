using O2un.Actors;
using UnityEngine;

namespace O2un.Combat
{
    public sealed class NearestPlayerStrategy : ITargetStrategy
    {
        public IActor Select(IActorQuery query, Vector3 origin)
        {
            return query.Player;
        }
    }
}
