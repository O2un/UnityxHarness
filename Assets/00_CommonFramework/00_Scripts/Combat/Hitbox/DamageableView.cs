using O2un.Actors;
using UnityEngine;

namespace O2un.Combat
{
    [RequireComponent(typeof(Collider))]
    public sealed class DamageableView : MonoBehaviour, IDamageable
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
