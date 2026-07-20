using O2un.AI;

namespace O2un.ProjectB.Platformer
{
    public sealed class TookDamageCondition : ITransitionCondition
    {
        private readonly IEnemy2DBlackboard _blackboard;

        public TookDamageCondition(IEnemy2DBlackboard blackboard)
        {
            _blackboard = blackboard;
        }

        public bool IsMet()
        {
            return _blackboard.WasHitThisTick;
        }
    }
}
