using O2un.AI;
using UnityEngine;

namespace O2un.Actors
{
    public sealed class SeekPlayerState : IState
    {
        private readonly EnemyBlackboard _blackboard;
        private readonly CharacterMover _mover;

        public SeekPlayerState(EnemyBlackboard blackboard, CharacterMover mover)
        {
            _blackboard = blackboard;
            _mover = mover;
        }

        public void Enter() { }

        public void Tick(float dt)
        {
            if (false == _blackboard.HasTarget)
            {
                _mover.SetDirection(Vector3.zero);
                return;
            }

            Vector3 dir = _blackboard.TargetPosition - _blackboard.SelfPosition;
            dir.y = 0f;
            _mover.SetDirection(dir.normalized);
        }

        public void Exit() { }
    }
}
