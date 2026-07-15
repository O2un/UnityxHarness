using O2un.Actors;
using UnityEngine;

namespace O2un.ProjectB.Platformer
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D), typeof(SpriteRenderer))]
    [RequireComponent(typeof(Animator))]
    public sealed class PlayerView : MonoBehaviour, IActorView
    {
        private const float FACING_VELOCITY_THRESHOLD = 0.01f;

        private static readonly RaycastHit2D[] _hits = new RaycastHit2D[4];
        private static readonly int _speedHash = Animator.StringToHash("Speed");
        private static readonly int _groundDistanceHash = Animator.StringToHash("GroundDistance");
        private static readonly int _fallSpeedHash = Animator.StringToHash("FallSpeed");

        private ContactFilter2D _filter;

        private Rigidbody2D _body;
        private Collider2D _collider;
        private SpriteRenderer _renderer;
        private Animator _animator;
        private Actor _actor;

        private Rigidbody2D Body => _body ??= GetComponent<Rigidbody2D>();
        private Collider2D Collider => _collider ??= GetComponent<Collider2D>();
        private SpriteRenderer Renderer => _renderer ??= GetComponent<SpriteRenderer>();
        private Animator Animator => _animator ??= GetComponent<Animator>();

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

        private void LateUpdate()
        {
            UpdateFacing();
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

        public void ApplyPhysics(Vector2 velocity, bool grounded)
        {
            Body.linearVelocity = velocity;
            UpdateAnimation(grounded);
        }

        private void UpdateFacing()
        {
            float horizontalVelocity = Body.linearVelocity.x;
            if (FACING_VELOCITY_THRESHOLD < horizontalVelocity)
            {
                Renderer.flipX = false;
            }
            else if (-FACING_VELOCITY_THRESHOLD > horizontalVelocity)
            {
                Renderer.flipX = true;
            }
        }

        private void UpdateAnimation(bool grounded)
        {
            Vector2 velocity = Body.linearVelocity;
            Animator.SetFloat(_speedHash, Mathf.Abs(velocity.x));
            Animator.SetFloat(_groundDistanceHash, true == grounded ? 0f : 1f);
            Animator.SetFloat(_fallSpeedHash, velocity.y);
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
