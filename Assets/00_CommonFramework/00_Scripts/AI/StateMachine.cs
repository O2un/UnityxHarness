using System.Collections.Generic;

namespace O2un.AI
{
    public sealed class StateMachine
    {
        private readonly Dictionary<IState, List<Transition>> _transitions = new();
        private readonly List<Transition> _empty = new();
        private IState _current;

        public void AddTransition(IState from, ITransitionCondition condition, IState to)
        {
            if (false == _transitions.TryGetValue(from, out List<Transition> list))
            {
                list = new List<Transition>();
                _transitions.Add(from, list);
            }

            list.Add(new Transition(condition, to));
        }

        public void SetInitial(IState state)
        {
            _current = state;
            _current?.Enter();
        }

        public void Tick(float dt)
        {
            if (null == _current)
            {
                return;
            }

            List<Transition> candidates = _transitions.TryGetValue(_current, out List<Transition> list) ? list : _empty;
            for (int i = 0; i < candidates.Count; i++)
            {
                Transition transition = candidates[i];
                if (transition.Condition.IsMet())
                {
                    _current.Exit();
                    _current = transition.To;
                    _current.Enter();
                    break;
                }
            }

            _current.Tick(dt);
        }
    }
}
