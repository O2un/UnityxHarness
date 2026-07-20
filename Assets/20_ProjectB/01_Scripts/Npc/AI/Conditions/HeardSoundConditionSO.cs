using O2un.AI;
using UnityEngine;

namespace O2un.ProjectB.Platformer
{
    [CreateAssetMenu(menuName = "ProjectB/Platformer/AI/Conditions/HeardSound", fileName = "HeardSoundConditionSO")]
    public sealed class HeardSoundConditionSO : Enemy2DTransitionConditionSO
    {
        public override ITransitionCondition Build(Enemy2DAIContext context, IState fromState)
        {
            return new HeardSoundCondition(context.Blackboard);
        }
    }
}
