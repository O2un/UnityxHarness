using O2un.Combat;
using UnityEngine;

namespace O2un.Actors
{
    [RequireComponent(typeof(SphereCollider))]
    public sealed class DashContactHitbox : MonoBehaviour
    {
        private int _damage;
        private SphereCollider _collider;

        private SphereCollider Collider => _collider ??= GetComponent<SphereCollider>();

        public void Configure(int damage, float radius)
        {
            _damage = damage;
            Collider.isTrigger = true;
            Collider.radius = radius;
        }

        private void OnTriggerEnter(Collider other)
        {
            DamageableView target = other.GetComponentInParent<DamageableView>();
            if (null == target || ActorType.Player != target.Team)
            {
                return;
            }

            target.ApplyDamage(_damage);
        }
    }
}
