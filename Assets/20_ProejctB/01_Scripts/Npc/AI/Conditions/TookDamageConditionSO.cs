using O2un.AI;
using UnityEngine;

namespace O2un.ProjectB.Platformer
{
    [CreateAssetMenu(menuName = "ProjectB/Platformer/AI/Conditions/TookDamage", fileName = "TookDamageConditionSO")]
    public sealed class TookDamageConditionSO : Enemy2DTransitionConditionSO
    {
        public override ITransitionCondition Build(Enemy2DAIContext context, IState fromState)
        {
            return new TookDamageCondition(context.Blackboard);
        }
    }
}
