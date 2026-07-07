namespace O2un.AI
{
    public abstract class BaseEnemyAI
    {
        private readonly StateMachine _stateMachine = new();

        protected void AddTransition(IState from, ITransitionCondition condition, IState to)
        {
            _stateMachine.AddTransition(from, condition, to);
        }

        protected void SetInitial(IState state)
        {
            _stateMachine.SetInitial(state);
        }

        public void Tick(float dt)
        {
            _stateMachine.Tick(dt);
        }
    }
}
