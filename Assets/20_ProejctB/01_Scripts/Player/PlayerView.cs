using UnityEngine;

namespace O2un.ProjectB.Platformer
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class PlayerView : MonoBehaviour
    {
        private static readonly RaycastHit2D[] _hits = new RaycastHit2D[4];

        private Rigidbody2D _body;
        private Collider2D _collider;

        private Rigidbody2D Body => _body ??= GetComponent<Rigidbody2D>();
        private Collider2D Collider => _collider ??= GetComponent<Collider2D>();

        public Transform FollowTarget => transform;
        public float VerticalVelocity => Body.linearVelocity.y;

        public void ApplyVelocity(Vector2 velocity)
        {
            Body.linearVelocity = velocity;
        }

        public bool CheckGrounded(LayerMask mask, Vector2 size, float distance)
        {
            Collider2D self = Collider;
            Bounds bounds = self.bounds;
            Vector2 origin = new(bounds.center.x, bounds.min.y);
            int count = Physics2D.BoxCastNonAlloc(origin, size, 0f, Vector2.down, _hits, distance, mask);

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
