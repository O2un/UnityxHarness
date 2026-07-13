using R3;

namespace O2un.Combat
{
    public interface IEnemyDamageSource
    {
        Observable<int> OnEnemyDamaged { get; }
    }
}
