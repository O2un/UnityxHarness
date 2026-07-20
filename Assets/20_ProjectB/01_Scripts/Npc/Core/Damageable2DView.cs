using O2un.Actors;
using O2un.Combat;
using O2un.Feedback;
using UnityEngine;

namespace O2un.ProjectB.Platformer
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class Damageable2DView : MonoBehaviour, IDamageable
    {
        private ActorType _team;
        private IHealth _health;
        private IHitFeedbackPublisher _hitPublisher;

        public ActorType Team => _team;

        public void Bind(ActorType team, IHealth health, IHitFeedbackPublisher hitPublisher)
        {
            _team = team;
            _health = health;
            _hitPublisher = hitPublisher;
        }

        public void ApplyDamage(int amount)
        {
            if (null == _health)
            {
                return;
            }

            _health.VaryHP(-amount);
            _hitPublisher?.Publish(new HitFeedbackEvent(_team, amount, transform.position));
        }
    }
}
