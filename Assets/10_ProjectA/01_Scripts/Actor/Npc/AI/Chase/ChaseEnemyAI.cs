using O2un.AI;

namespace O2un.Actors
{
    public sealed class ChaseEnemyAI : BaseEnemyAI
    {
        public ChaseEnemyAI(IState initial)
        {
            SetInitial(initial);
        }
    }
}
