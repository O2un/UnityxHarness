using System;
using R3;

namespace O2un.Combat
{
    public sealed class EnemyHealth : IHealth, IDisposable
    {
        private readonly int _maxHp;
        private readonly ReactiveProperty<int> _hp;
        private readonly Subject<Unit> _onDeath = new();
        private readonly Action<int> _onDamaged;

        public EnemyHealth(int maxHp, Action<int> onDamaged = null)
        {
            _maxHp = maxHp;
            _hp = new ReactiveProperty<int>(maxHp);
            _onDamaged = onDamaged;
        }

        public ReadOnlyReactiveProperty<int> CurrentHP => _hp;
        public int MaxHP => _maxHp;
        public Observable<Unit> OnDeath => _onDeath;
        public bool IsDead => _hp.Value <= 0;

        public void VaryHP(int delta)
        {
            if (true == IsDead)
            {
                return;
            }

            int before = _hp.Value;
            _hp.Value = Math.Clamp(before + delta, 0, _maxHp);

            int dealt = before - _hp.Value;
            if (0 < dealt)
            {
                _onDamaged?.Invoke(dealt);
            }

            if (0 == _hp.Value)
            {
                _onDeath.OnNext(Unit.Default);
            }
        }

        public void ResetFull()
        {
            _hp.Value = _maxHp;
        }

        public void Dispose()
        {
            _hp.Dispose();
            _onDeath.Dispose();
        }
    }
}
