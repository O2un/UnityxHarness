using O2un.AI;

namespace O2un.Actors
{
    public sealed class MeleeAttackCompleteCondition : ITransitionCondition
    {
        private readonly IStateProgress _progress;
        private readonly ITransitionCondition _leaveRangeCondition;

        public MeleeAttackCompleteCondition(
            IStateProgress progress,
            ITransitionCondition leaveRangeCondition)
        {
            _progress = progress;
            _leaveRangeCondition = leaveRangeCondition;
        }

        public bool IsMet()
        {
            return _progress.IsComplete && _leaveRangeCondition.IsMet();
        }
    }
}
