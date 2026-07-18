using R3;

namespace O2un.ProjectB.Platformer
{
    public interface IEnemyAttackExecutor
    {
        Observable<Unit> OnAttackAnimationEnd { get; }
        bool IsAttacking { get; }

        void PlayAttack();
    }
}
