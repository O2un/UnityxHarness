using System;
using R3;

namespace O2un.Combat
{
    public sealed class EnemyDamageChannel : IEnemyDamageSource, IEnemyDamagePublisher, IDisposable
    {
        private readonly Subject<int> _onDamaged = new();
        public Observable<int> OnEnemyDamaged => _onDamaged;

        public void Publish(int amount)
        {
            _onDamaged.OnNext(amount);
        }

        public void Dispose()
        {
            _onDamaged.Dispose();
        }
    }
}
