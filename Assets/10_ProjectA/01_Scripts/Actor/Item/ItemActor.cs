using System;
using R3;

namespace O2un.Actors
{
    public sealed class ItemActor : IDisposable
    {
        private readonly Subject<int> _onPicked = new();
        public Observable<int> OnPicked => _onPicked;

        private int _amount;
        private bool _picked;

        public int Amount => _amount;

        public void Configure(int amount)
        {
            _amount = amount;
            _picked = false;
        }

        public void Pick()
        {
            if (_picked)
            {
                return;
            }

            _picked = true;
            _onPicked.OnNext(_amount);
        }

        public void Dispose()
        {
            _onPicked.Dispose();
        }
    }
}
