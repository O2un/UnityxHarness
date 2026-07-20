using O2un.AI;
using UnityEngine;

namespace O2un.ProjectB.Platformer
{
    [CreateAssetMenu(menuName = "ProjectB/Platformer/AI/Conditions/LostTarget", fileName = "LostTargetConditionSO")]
    public sealed class LostTargetConditionSO : Enemy2DTransitionConditionSO
    {
        [SerializeField, Min(0f)] private float _timeout = 3f;

        public override ITransitionCondition Build(Enemy2DAIContext context, IState fromState)
        {
            return new LostTargetCondition(context.Blackboard, _timeout);
        }
    }
}
