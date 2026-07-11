using O2un.AI;

namespace O2un.Actors
{
    public sealed class StateCompleteCondition : ITransitionCondition
    {
        private readonly IStateProgress _progress;

        public StateCompleteCondition(IStateProgress progress)
        {
            _progress = progress;
        }

        public bool IsMet() => _progress.IsComplete;
    }
}
