using R3;

namespace O2un.Actors
{
    public interface IEnemyKillEvent
    {
        Observable<EnemyKilledInfo> OnKilled { get; }
        void Publish(in EnemyKilledInfo info);
    }
}
