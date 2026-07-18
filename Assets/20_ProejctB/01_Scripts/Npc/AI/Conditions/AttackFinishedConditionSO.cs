using O2un.AI;
using UnityEngine;

namespace O2un.ProjectB.Platformer
{
    [CreateAssetMenu(menuName = "ProjectB/Platformer/AI/Conditions/AttackFinished", fileName = "AttackFinishedConditionSO")]
    public sealed class AttackFinishedConditionSO : Enemy2DTransitionConditionSO
    {
        [SerializeField] private EnemyAttackData _attackData;

        public override ITransitionCondition Build(Enemy2DAIContext context, IState fromState)
        {
            return new AttackFinishedCondition(context.Blackboard, fromState as IAttackState, _attackData.AttackRange);
        }
    }
}
