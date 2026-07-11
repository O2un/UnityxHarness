using UnityEngine;

namespace O2un.Actors
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class ActorView : MonoBehaviour
    {
        private static readonly int SPEED_HASH = Animator.StringToHash("Speed");

        [SerializeField] private Animator _animator;

        private CharacterController _controller;
        private Actor _actor;

        private CharacterController Controller => _controller ??= GetComponent<CharacterController>();
        private Animator Animator => _animator ??= GetComponentInChildren<Animator>();

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

        public void Move(Vector3 velocity)
        {
            Controller.Move(velocity * Time.deltaTime);

            if (null != Animator)
            {
                Animator.SetFloat(SPEED_HASH, velocity.magnitude);
            }
        }

        public void RotateTo(Quaternion targetRotation, float rotationSpeed)
        {
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        public void SetCollisionDetection(bool enabled)
        {
            Controller.detectCollisions = enabled;
        }
    }
}
