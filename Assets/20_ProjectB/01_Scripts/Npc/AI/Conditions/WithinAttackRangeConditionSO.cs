using O2un.AI;
using UnityEngine;

namespace O2un.ProjectB.Platformer
{
    [CreateAssetMenu(menuName = "ProjectB/Platformer/AI/Conditions/WithinAttackRange", fileName = "WithinAttackRangeConditionSO")]
    public sealed class WithinAttackRangeConditionSO : Enemy2DTransitionConditionSO
    {
        [SerializeField] private EnemyAttackData _attackData;

        public override ITransitionCondition Build(Enemy2DAIContext context, IState fromState)
        {
            return new WithinAttackRangeCondition(context.Blackboard, _attackData.AttackRange);
        }
    }
}
