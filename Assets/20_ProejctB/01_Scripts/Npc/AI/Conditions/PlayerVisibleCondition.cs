using O2un.AI;

namespace O2un.ProjectB.Platformer
{
    public sealed class PlayerVisibleCondition : ITransitionCondition
    {
        private readonly IEnemy2DBlackboard _blackboard;

        public PlayerVisibleCondition(IEnemy2DBlackboard blackboard)
        {
            _blackboard = blackboard;
        }

        public bool IsMet()
        {
            return _blackboard.IsPlayerVisible;
        }
    }
}
