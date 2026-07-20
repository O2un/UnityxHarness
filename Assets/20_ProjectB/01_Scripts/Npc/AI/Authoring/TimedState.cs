using O2un.AI;

namespace O2un.ProjectB.Platformer
{
    public interface ITimedState
    {
        float TimeInState { get; }
    }

    public abstract class TimedState : IState, ITimedState
    {
        private float _timeInState;

        public float TimeInState => _timeInState;

        public void Enter()
        {
            _timeInState = 0f;
            OnEnter();
        }

        public void Tick(float dt)
        {
            _timeInState += dt;
            OnTick(dt);
        }

        public void Exit()
        {
            OnExit();
        }

        protected virtual void OnEnter() { }

        protected abstract void OnTick(float dt);

        protected virtual void OnExit() { }
    }
}
