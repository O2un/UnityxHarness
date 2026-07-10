using O2un.AI;
using UnityEngine;

namespace O2un.Actors
{
    [CreateAssetMenu(menuName = "O2un/AI/Armored Melee Enemy Profile", fileName = "ArmoredMeleeEnemyProfile")]
    public sealed class ArmoredMeleeEnemyProfile : EnemyAIProfileSO
    {
        [SerializeField] private EnemyStateSO _seekState;
        [SerializeField] private EnemyStateSO _meleeAttackState;
        [SerializeField] private EnemyTransitionConditionSO _enterMeleeCondition;
        [SerializeField] private EnemyTransitionConditionSO _leaveMeleeCondition;

        public override BaseEnemyAI Build(EnemyBlackboard blackboard, CharacterMover mover)
        {
            IState seek = _seekState.Build(blackboard, mover);
            IState meleeAttack = _meleeAttackState.Build(blackboard, mover);
            ITransitionCondition enterMeleeCondition = _enterMeleeCondition.Build(blackboard, mover);
            ITransitionCondition leaveMeleeCondition = _leaveMeleeCondition.Build(blackboard, mover);
            return new ArmoredMeleeEnemyAI(seek, meleeAttack, enterMeleeCondition, leaveMeleeCondition);
        }
    }
}
