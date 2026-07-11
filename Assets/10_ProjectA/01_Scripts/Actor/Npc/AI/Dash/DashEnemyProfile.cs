using O2un.AI;
using UnityEngine;

namespace O2un.Actors
{
    [CreateAssetMenu(menuName = "O2un/AI/Dash Enemy Profile", fileName = "DashEnemyProfile")]
    public sealed class DashEnemyProfile : EnemyAIProfileSO
    {
        [SerializeField] private EnemyStateSO _seekState;
        [SerializeField] private EnemyStateSO _windupState;
        [SerializeField] private EnemyStateSO _dashState;
        [SerializeField] private EnemyStateSO _recoverState;
        [SerializeField] private EnemyTransitionConditionSO _dashTrigger;

        public override BaseEnemyAI Build(EnemyBlackboard blackboard, CharacterMover mover)
        {
            IState seek = _seekState.Build(blackboard, mover);
            IState windup = _windupState.Build(blackboard, mover);
            IState dash = _dashState.Build(blackboard, mover);
            IState recover = _recoverState.Build(blackboard, mover);
            ITransitionCondition trigger = _dashTrigger.Build(blackboard, mover);
            return new DashEnemyAI(seek, windup, dash, recover, trigger);
        }
    }
}
