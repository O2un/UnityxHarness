using UnityEngine;

namespace O2un.ProjectB.Platformer
{
    public sealed class DetectState : TimedState
    {
        private readonly IEnemy2DBlackboard _blackboard;
        private readonly Npc2DMover _mover;

        public DetectState(IEnemy2DBlackboard blackboard, Npc2DMover mover)
        {
            _blackboard = blackboard;
            _mover = mover;
        }

        protected override void OnTick(float dt)
        {
            _mover.Stop();

            if (false == TryGetFocusPosition(out Vector2 focus))
            {
                return;
            }

            _mover.SetFacing(focus.x - _blackboard.SelfPosition.x);
        }

        private bool TryGetFocusPosition(out Vector2 focus)
        {
            if (true == _blackboard.HasTarget)
            {
                focus = _blackboard.TargetPosition;
                return true;
            }

            if (true == _blackboard.HasLastKnownTargetPosition)
            {
                focus = _blackboard.LastKnownTargetPosition;
                return true;
            }

            focus = Vector2.zero;
            return false;
        }
    }
}
