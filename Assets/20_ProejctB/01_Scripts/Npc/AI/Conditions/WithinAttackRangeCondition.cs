using O2un.AI;
using UnityEngine;

namespace O2un.ProjectB.Platformer
{
    public sealed class WithinAttackRangeCondition : ITransitionCondition
    {
        private readonly IEnemy2DBlackboard _blackboard;
        private readonly float _range;

        public WithinAttackRangeCondition(IEnemy2DBlackboard blackboard, float range)
        {
            _blackboard = blackboard;
            _range = range;
        }

        public bool IsMet()
        {
            if (false == _blackboard.HasTarget)
            {
                return false;
            }

            Vector2 delta = _blackboard.TargetPosition - _blackboard.SelfPosition;
            return delta.sqrMagnitude <= _range * _range;
        }
    }
}
