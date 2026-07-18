using O2un.AI;

namespace O2un.ProjectB.Platformer
{
    public sealed class DetectConfirmedCondition : ITransitionCondition
    {
        private readonly IEnemy2DBlackboard _blackboard;
        private readonly ITimedState _fromState;
        private readonly float _confirmTime;
        private readonly float _perceivedWithin;

        public DetectConfirmedCondition(
            IEnemy2DBlackboard blackboard,
            ITimedState fromState,
            float confirmTime,
            float perceivedWithin)
        {
            _blackboard = blackboard;
            _fromState = fromState;
            _confirmTime = confirmTime;
            _perceivedWithin = perceivedWithin;
        }

        public bool IsMet()
        {
            if (null == _fromState)
            {
                return false;
            }

            if (_fromState.TimeInState < _confirmTime)
            {
                return false;
            }

            return _blackboard.TimeSincePerceived <= _perceivedWithin;
        }
    }
}
