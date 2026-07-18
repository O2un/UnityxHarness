using O2un.Actors;
using UnityEngine;

namespace O2un.ProjectB.Platformer
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D), typeof(SpriteRenderer))]
    public sealed class NpcView : MonoBehaviour, IActorView
    {
        private const float MOVING_SPEED_THRESHOLD = 0.01f;

        [SerializeField] private string _idleStateName = "idle";
        [SerializeField] private string _runStateName = "run";

        private Actor _actor;
        private Rigidbody2D _body;
        private SpriteRenderer _renderer;
        private Animator _animator;
        private string _currentLocomotionState;

        private Rigidbody2D Body => _body ??= GetComponent<Rigidbody2D>();
        private SpriteRenderer Renderer => _renderer ??= GetComponent<SpriteRenderer>();
        private Animator Animator => _animator ??= GetComponent<Animator>();

        public void ApplyPhysics(float velocityX, float facing, bool attacking)
        {
            Body.linearVelocity = new Vector2(velocityX, Body.linearVelocity.y);
            Renderer.flipX = 0f > facing;

            UpdateLocomotion(velocityX, attacking);
        }

        private void UpdateLocomotion(float velocityX, bool attacking)
        {
            if (true == attacking)
            {
                _currentLocomotionState = null;
                return;
            }

            if (null == Animator)
            {
                return;
            }

            string target = Mathf.Abs(velocityX) > MOVING_SPEED_THRESHOLD ? _runStateName : _idleStateName;
            if (target == _currentLocomotionState)
            {
                return;
            }

            _currentLocomotionState = target;
            Animator.Play(target, 0, 0f);
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

        private void OnEnable()
        {
            _actor?.Register();
        }

        private void OnDisable()
        {
            _actor?.Unregister();
        }
    }
}
