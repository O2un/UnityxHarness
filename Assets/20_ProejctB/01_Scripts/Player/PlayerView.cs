using O2un.Actors;
using UnityEngine;

namespace O2un.ProjectB.Platformer
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class PlayerView : MonoBehaviour, IActorView
    {
        private static readonly RaycastHit2D[] _hits = new RaycastHit2D[4];

        private ContactFilter2D _filter;

        private Rigidbody2D _body;
        private Collider2D _collider;
        private Actor _actor;

        private Rigidbody2D Body => _body ??= GetComponent<Rigidbody2D>();
        private Collider2D Collider => _collider ??= GetComponent<Collider2D>();

        public Transform FollowTarget => transform;
        public float VerticalVelocity => Body.linearVelocity.y;

        private void OnEnable()
        {
            _actor?.Register();
        }

        private void OnDisable()
        {
            _actor?.Unregister();
        }

        public void Bind(Actor actor)
        {
            _actor = actor;

            if (true == isActiveAndEnabled)
            {
                _actor.Register();
            }
        }

        public void Unbind(Actor actor)
        {
            if (false == ReferenceEquals(_actor, actor))
            {
                return;
            }

            _actor = null;
        }

        public void ApplyVelocity(Vector2 velocity)
        {
            Body.linearVelocity = velocity;
        }

        public bool CheckGrounded(LayerMask mask, Vector2 size, float distance)
        {
            Collider2D self = Collider;
            Bounds bounds = self.bounds;
            Vector2 origin = new(bounds.center.x, bounds.min.y);

            _filter.useTriggers = false;
            _filter.useLayerMask = true;
            _filter.layerMask = mask;

            int count = Physics2D.BoxCast(origin, size, 0f, Vector2.down, _filter, _hits, distance);

            for (int i = 0; i < count; i++)
            {
                Collider2D hit = _hits[i].collider;
                if (null != hit && hit != self)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
