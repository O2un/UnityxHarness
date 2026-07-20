using O2un.AI;
using UnityEngine;

namespace O2un.ProjectB.Platformer
{
    [CreateAssetMenu(menuName = "ProjectB/Platformer/AI/Conditions/PlayerVisible", fileName = "PlayerVisibleConditionSO")]
    public sealed class PlayerVisibleConditionSO : Enemy2DTransitionConditionSO
    {
        public override ITransitionCondition Build(Enemy2DAIContext context, IState fromState)
        {
            return new PlayerVisibleCondition(context.Blackboard);
        }
    }
}
