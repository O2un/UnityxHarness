using System;
using O2un.DataStore;
using R3;

namespace O2un.Combat
{
    public sealed class PlayerHealthAdapter : IHealth, IDisposable
    {
        private readonly IPlayerDataReader _reader;
        private readonly IPlayerDataWriter _writer;
        private readonly Subject<Unit> _onDeath = new();
        private readonly IDisposable _hpSubscription;
        private bool _dead;

        public PlayerHealthAdapter(IPlayerDataReader reader, IPlayerDataWriter writer)
        {
            _reader = reader;
            _writer = writer;
            _hpSubscription = _reader.CurrentHP.Subscribe(OnHpChanged);
        }

        public ReadOnlyReactiveProperty<int> CurrentHP => _reader.CurrentHP;
        public int MaxHP => _reader.MaxHP.CurrentValue;
        public Observable<Unit> OnDeath => _onDeath;

        public void VaryHP(int delta)
        {
            _writer.VaryHP(delta);
        }

        private void OnHpChanged(int hp)
        {
            if (0 < hp)
            {
                _dead = false;
                return;
            }

            if (true == _dead)
            {
                return;
            }

            _dead = true;
            _onDeath.OnNext(Unit.Default);
        }

        public void Dispose()
        {
            _hpSubscription.Dispose();
            _onDeath.Dispose();
        }
    }
}
