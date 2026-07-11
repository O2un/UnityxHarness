using O2un.AI;
using UnityEngine;

namespace O2un.Actors
{
    public sealed class DistanceOutsideRangeCondition : ITransitionCondition
    {
        private readonly EnemyBlackboard _blackboard;
        private readonly float _range;

        public DistanceOutsideRangeCondition(EnemyBlackboard blackboard, float range)
        {
            _blackboard = blackboard;
            _range = range;
        }

        public bool IsMet()
        {
            if (false == _blackboard.HasTarget)
            {
                return true;
            }

            Vector3 delta = _blackboard.TargetPosition - _blackboard.SelfPosition;
            delta.y = 0f;
            return delta.sqrMagnitude > _range * _range;
        }
    }
}
