using O2un.AI;
using UnityEngine;

namespace O2un.ProjectB.Platformer
{
    [CreateAssetMenu(menuName = "ProjectB/Platformer/AI/Conditions/DetectConfirmed", fileName = "DetectConfirmedConditionSO")]
    public sealed class DetectConfirmedConditionSO : Enemy2DTransitionConditionSO
    {
        [SerializeField, Min(0f)] private float _confirmTime = 0.5f;
        [SerializeField, Min(0f)] private float _perceivedWithin = 0.5f;

        public override ITransitionCondition Build(Enemy2DAIContext context, IState fromState)
        {
            return new DetectConfirmedCondition(context.Blackboard, fromState as ITimedState, _confirmTime, _perceivedWithin);
        }
    }
}
