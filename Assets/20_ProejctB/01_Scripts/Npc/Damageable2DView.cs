using O2un.Actors;
using O2un.Combat;
using UnityEngine;

namespace O2un.ProjectB.Platformer
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class Damageable2DView : MonoBehaviour, IDamageable
    {
        private ActorType _team;
        private IHealth _health;

        public ActorType Team => _team;

        public void Bind(ActorType team, IHealth health)
        {
            _team = team;
            _health = health;
        }

        public void ApplyDamage(int amount)
        {
            _health?.VaryHP(-amount);
        }
    }
}
