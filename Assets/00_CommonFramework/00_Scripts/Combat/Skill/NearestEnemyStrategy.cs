using System.Collections.Generic;
using O2un.Actors;
using UnityEngine;

namespace O2un.Combat
{
    public sealed class NearestEnemyStrategy : ITargetStrategy
    {
        public IActor Select(IActorQuery query, Vector3 origin)
        {
            IReadOnlyList<IActor> enemies = query.GetActive(ActorType.Enemy);

            IActor nearest = null;
            float bestDistance = float.MaxValue;

            for (int i = 0; i < enemies.Count; i++)
            {
                IActor candidate = enemies[i];
                float distance = (candidate.Transform.position - origin).sqrMagnitude;

                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    nearest = candidate;
                }
            }

            return nearest;
        }
    }
}
