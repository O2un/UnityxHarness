using System;
using O2un.AI;

namespace O2un.Actors
{
    public sealed class DashEnemyAI : BaseEnemyAI
    {
        public DashEnemyAI(IState seek, IState windup, IState dash, IState recover, ITransitionCondition dashTrigger)
        {
            SetInitial(seek);
            AddTransition(seek, dashTrigger, windup);
            AddTransition(windup, new StateCompleteCondition(RequireProgress(windup)), dash);
            AddTransition(dash, new StateCompleteCondition(RequireProgress(dash)), recover);
            AddTransition(recover, new StateCompleteCondition(RequireProgress(recover)), seek);
        }

        private static IStateProgress RequireProgress(IState state)
        {
            return state as IStateProgress
                ?? throw new InvalidOperationException($"{state.GetType().Name} must implement IStateProgress.");
        }
    }
}
