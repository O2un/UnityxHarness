using O2un.AI;
using UnityEngine;

namespace O2un.Actors
{
    public sealed class DashState : IState, IStateProgress
    {
        private const float DIRECTION_EPSILON = 0.0001f;

        private readonly EnemyBlackboard _blackboard;
        private readonly CharacterMover _mover;
        private readonly float _dashSpeed;
        private readonly float _dashDistance;
        private readonly int _contactDamage;
        private readonly float _contactRadius;

        private Vector3 _startPosition;
        private Vector3 _direction;
        private DashContactHitbox _contactHitbox;

        public DashState(EnemyBlackboard blackboard, CharacterMover mover, float dashSpeed, float dashDistance, int contactDamage, float contactRadius)
        {
            _blackboard = blackboard;
            _mover = mover;
            _dashSpeed = dashSpeed;
            _dashDistance = dashDistance;
            _contactDamage = contactDamage;
            _contactRadius = contactRadius;
        }

        public bool IsComplete
        {
            get
            {
                Vector3 delta = _blackboard.SelfPosition - _startPosition;
                delta.y = 0f;
                return delta.sqrMagnitude >= _dashDistance * _dashDistance;
            }
        }

        public void Enter()
        {
            _startPosition = _blackboard.SelfPosition;

            Vector3 dir = _blackboard.TargetPosition - _blackboard.SelfPosition;
            dir.y = 0f;

            if (dir.sqrMagnitude < DIRECTION_EPSILON)
            {
                dir = _mover.TargetRotation * Vector3.forward;
                dir.y = 0f;
            }

            _direction = dir.normalized;
            _contactHitbox = _blackboard.Transform.gameObject.AddComponent<DashContactHitbox>();
            _contactHitbox.Configure(_contactDamage, _contactRadius);
        }

        public void Tick(float dt)
        {
            _mover.SetVelocity(_direction * _dashSpeed);
        }

        public void Exit()
        {
            if (null == _contactHitbox)
            {
                return;
            }

            Object.Destroy(_contactHitbox);
            _contactHitbox = null;
        }
    }
}
