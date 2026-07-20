using UnityEngine;

namespace O2un.ProjectB.Platformer
{
    public sealed class ChaseState : TimedState
    {
        private readonly IEnemy2DBlackboard _blackboard;
        private readonly Npc2DMover _mover;
        private readonly float _speed;

        public ChaseState(IEnemy2DBlackboard blackboard, Npc2DMover mover, float speed)
        {
            _blackboard = blackboard;
            _mover = mover;
            _speed = speed;
        }

        protected override void OnTick(float dt)
        {
            if (false == TryGetChasePosition(out Vector2 destination))
            {
                _mover.Stop();
                return;
            }

            _mover.SetFacing(destination.x - _blackboard.SelfPosition.x);

            if (false == _blackboard.GroundAhead)
            {
                _mover.Stop();
                return;
            }

            _mover.MoveTowards(_blackboard.SelfPosition.x, destination.x, _speed);
        }

        private bool TryGetChasePosition(out Vector2 destination)
        {
            if (true == _blackboard.HasTarget)
            {
                destination = _blackboard.TargetPosition;
                return true;
            }

            if (true == _blackboard.HasLastKnownTargetPosition)
            {
                destination = _blackboard.LastKnownTargetPosition;
                return true;
            }

            destination = Vector2.zero;
            return false;
        }
    }
}
