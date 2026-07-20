namespace O2un.ProjectB.Platformer
{
    public sealed class PatrolState : TimedState
    {
        private readonly IEnemy2DBlackboard _blackboard;
        private readonly Npc2DMover _mover;
        private readonly float _speed;

        private bool _wasBlocked;

        public PatrolState(IEnemy2DBlackboard blackboard, Npc2DMover mover, float speed)
        {
            _blackboard = blackboard;
            _mover = mover;
            _speed = speed;
        }

        protected override void OnEnter()
        {
            _wasBlocked = false;
        }

        protected override void OnTick(float dt)
        {
            bool blocked = false == _blackboard.GroundAhead || true == _blackboard.WallAhead;

            if (true == blocked && false == _wasBlocked)
            {
                _mover.Flip();
            }

            _wasBlocked = blocked;
            _mover.MoveForward(_speed);
        }
    }
}
