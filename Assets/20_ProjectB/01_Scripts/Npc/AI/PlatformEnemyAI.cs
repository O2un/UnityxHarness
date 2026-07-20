using O2un.AI;

namespace O2un.ProjectB.Platformer
{
    public sealed class PlatformEnemyAI : BaseEnemyAI
    {
        public PlatformEnemyAI(PlatformEnemyStates states, PlatformEnemyConditions conditions)
        {
            AddTransition(states.Patrol, conditions.PlayerVisible, states.Detect);
            AddTransition(states.Patrol, conditions.HeardSound, states.Detect);
            AddTransition(states.Patrol, conditions.TookDamage, states.Chase);

            AddTransition(states.Detect, conditions.TookDamage, states.Chase);
            AddTransition(states.Detect, conditions.DetectConfirmed, states.Chase);
            AddTransition(states.Detect, conditions.DetectFailed, states.Patrol);

            AddTransition(states.Chase, conditions.WithinAttackRange, states.Attack);
            AddTransition(states.Chase, conditions.ChaseGiveUp, states.Patrol);

            AddTransition(states.Attack, conditions.TookDamage, states.Chase);
            AddTransition(states.Attack, conditions.AttackFinished, states.Chase);

            SetInitial(states.Patrol);
        }
    }
}
