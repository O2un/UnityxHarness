using O2un.AI;

namespace O2un.ProjectB.Platformer
{
    public sealed class PlatformEnemyStates
    {
        public IState Patrol { get; set; }
        public IState Detect { get; set; }
        public IState Chase { get; set; }
        public IState Attack { get; set; }
    }

    public sealed class PlatformEnemyConditions
    {
        public ITransitionCondition PlayerVisible { get; set; }
        public ITransitionCondition HeardSound { get; set; }
        public ITransitionCondition DetectConfirmed { get; set; }
        public ITransitionCondition DetectFailed { get; set; }
        public ITransitionCondition WithinAttackRange { get; set; }
        public ITransitionCondition AttackFinished { get; set; }
        public ITransitionCondition ChaseGiveUp { get; set; }
        public ITransitionCondition TookDamage { get; set; }
    }
}
