namespace O2un.ProjectB.Platformer
{
    public sealed class Enemy2DAIContext
    {
        public IEnemy2DBlackboard Blackboard { get; }
        public Npc2DMover Mover { get; }
        public IEnemyAttackExecutor AttackExecutor { get; }

        public Enemy2DAIContext(IEnemy2DBlackboard blackboard, Npc2DMover mover, IEnemyAttackExecutor attackExecutor)
        {
            Blackboard = blackboard;
            Mover = mover;
            AttackExecutor = attackExecutor;
        }
    }
}
