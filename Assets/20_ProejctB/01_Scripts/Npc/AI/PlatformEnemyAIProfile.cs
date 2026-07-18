using O2un.AI;
using UnityEngine;

namespace O2un.ProjectB.Platformer
{
    [CreateAssetMenu(menuName = "ProjectB/Platformer/AI/PlatformEnemyAIProfile", fileName = "PlatformEnemyAIProfile")]
    public sealed class PlatformEnemyAIProfile : Enemy2DAIProfileSO
    {
        [SerializeField] private Enemy2DStateSO _patrolState;
        [SerializeField] private Enemy2DStateSO _detectState;
        [SerializeField] private Enemy2DStateSO _chaseState;
        [SerializeField] private Enemy2DStateSO _attackState;

        [SerializeField] private Enemy2DTransitionConditionSO _playerVisible;
        [SerializeField] private Enemy2DTransitionConditionSO _heardSound;
        [SerializeField] private Enemy2DTransitionConditionSO _detectConfirmed;
        [SerializeField] private Enemy2DTransitionConditionSO _detectFailed;
        [SerializeField] private Enemy2DTransitionConditionSO _withinAttackRange;
        [SerializeField] private Enemy2DTransitionConditionSO _attackFinished;
        [SerializeField] private Enemy2DTransitionConditionSO _chaseGiveUp;
        [SerializeField] private Enemy2DTransitionConditionSO _tookDamage;

        public override BaseEnemyAI Build(Enemy2DAIContext context)
        {
            PlatformEnemyStates states = new()
            {
                Patrol = _patrolState.Build(context),
                Detect = _detectState.Build(context),
                Chase = _chaseState.Build(context),
                Attack = _attackState.Build(context),
            };

            PlatformEnemyConditions conditions = new()
            {
                PlayerVisible = _playerVisible.Build(context, states.Patrol),
                HeardSound = _heardSound.Build(context, states.Patrol),
                DetectConfirmed = _detectConfirmed.Build(context, states.Detect),
                DetectFailed = _detectFailed.Build(context, states.Detect),
                WithinAttackRange = _withinAttackRange.Build(context, states.Chase),
                AttackFinished = _attackFinished.Build(context, states.Attack),
                ChaseGiveUp = _chaseGiveUp.Build(context, states.Chase),
                TookDamage = _tookDamage.Build(context, null),
            };

            return new PlatformEnemyAI(states, conditions);
        }
    }
}
