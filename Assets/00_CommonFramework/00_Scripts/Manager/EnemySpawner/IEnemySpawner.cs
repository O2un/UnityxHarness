using R3;

namespace O2un.Manager
{
    public interface IEnemySpawner
    {
        Observable<Unit> OnCleared { get; }
        int ReachedWave { get; }
        int TotalWaves { get; }
        void Begin();
        void Reset();
    }
}
