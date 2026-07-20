using O2un.AI;
using UnityEngine;

namespace O2un.ProjectB.Platformer
{
    public sealed class AttackFinishedCondition : ITransitionCondition
    {
        private readonly IEnemy2DBlackboard _blackboard;
        private readonly IAttackState _fromState;
        private readonly float _range;

        public AttackFinishedCondition(IEnemy2DBlackboard blackboard, IAttackState fromState, float range)
        {
            _blackboard = blackboard;
            _fromState = fromState;
            _range = range;
        }

        public bool IsMet()
        {
            if (null == _fromState)
            {
                return false;
            }

            if (false == _fromState.IsAttackFinished)
            {
                return false;
            }

            if (false == _blackboard.HasTarget)
            {
                return true;
            }

            Vector2 delta = _blackboard.TargetPosition - _blackboard.SelfPosition;
            return delta.sqrMagnitude > _range * _range;
        }
    }
}
