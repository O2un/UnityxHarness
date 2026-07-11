using O2un.AI;

namespace O2un.Actors
{
    public sealed class ArmoredMeleeEnemyAI : BaseEnemyAI
    {
        public ArmoredMeleeEnemyAI(
            IState seek,
            IState meleeAttack,
            ITransitionCondition enterMeleeCondition,
            ITransitionCondition leaveMeleeCondition)
        {
            SetInitial(seek);
            AddTransition(seek, enterMeleeCondition, meleeAttack);
            AddTransition(
                meleeAttack,
                new MeleeAttackCompleteCondition(RequireProgress(meleeAttack), leaveMeleeCondition),
                seek);
        }

        private static IStateProgress RequireProgress(IState state)
        {
            return state as IStateProgress
                ?? throw new System.InvalidOperationException($"{state.GetType().Name} must implement IStateProgress.");
        }
    }
}
