using O2un.AI;

namespace O2un.ProjectB.Platformer
{
    public sealed class LostTargetCondition : ITransitionCondition
    {
        private readonly IEnemy2DBlackboard _blackboard;
        private readonly float _timeout;

        public LostTargetCondition(IEnemy2DBlackboard blackboard, float timeout)
        {
            _blackboard = blackboard;
            _timeout = timeout;
        }

        public bool IsMet()
        {
            return _blackboard.TimeSincePerceived > _timeout;
        }
    }
}
