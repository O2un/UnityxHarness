using UnityEngine;

namespace O2un.ProjectB.Platformer
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class PlayerView : MonoBehaviour
    {
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
            Bounds bounds = Collider.bounds;
            Vector2 origin = new(bounds.center.x, bounds.min.y);
            RaycastHit2D hit = Physics2D.BoxCast(origin, size, 0f, Vector2.down, distance, mask);
            return null != hit.collider;
        }
    }
}
