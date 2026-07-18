using System;
using R3;

namespace O2un.Actors
{
    public sealed class EnemyKillEvent : IEnemyKillEvent, IDisposable
    {
        private readonly Subject<EnemyKilledInfo> _onKilled = new();
        public Observable<EnemyKilledInfo> OnKilled => _onKilled;

        public void Publish(in EnemyKilledInfo info)
        {
            _onKilled.OnNext(info);
        }

        public void Dispose()
        {
            _onKilled.Dispose();
        }
    }
}
