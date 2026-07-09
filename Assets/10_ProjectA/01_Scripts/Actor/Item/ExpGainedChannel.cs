using System;
using R3;

namespace O2un.Actors
{
    public sealed class ExpGainedChannel : IExpGainedPublisher, IExpGainedSource, IDisposable
    {
        private readonly Subject<int> _onGained = new();
        public Observable<int> OnGained => _onGained;

        public void Publish(int amount)
        {
            _onGained.OnNext(amount);
        }

        public void Dispose()
        {
            _onGained.Dispose();
        }
    }
}
