using System;

namespace O2un.AI
{
    public sealed class Transition
    {
        public ITransitionCondition Condition { get; }
        public IState To { get; }

        public Transition(ITransitionCondition condition, IState to)
        {
            Condition = condition ?? throw new ArgumentNullException(nameof(condition));
            To = to ?? throw new ArgumentNullException(nameof(to));
        }
    }
}
