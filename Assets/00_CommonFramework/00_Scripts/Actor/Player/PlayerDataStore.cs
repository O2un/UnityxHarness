using System;
using R3;
using UnityEngine;

namespace O2un.DataStore 
{
    public interface IPlayerDataReader
    {
        ReadOnlyReactiveProperty<int> CurrentHP {get;}
        ReadOnlyReactiveProperty<int> MaxHP {get;}
    }

    public interface IPlayerDataWriter
    {
        void VaryHP(int hp);
        void VaryMaxHP(int delta);
        void SetCurrentHP(int hp);
    }

    public sealed class PlayerDataStore : IDisposable, IPlayerDataReader, IPlayerDataWriter
    {
        private readonly ReactiveProperty<int> _hp = new();
        private readonly ReactiveProperty<int> _maxHP = new(100);

        public ReadOnlyReactiveProperty<int> CurrentHP => _hp;
        public ReadOnlyReactiveProperty<int> MaxHP => _maxHP;

        public void SetCurrentHP(int hp)
        {
            _hp.Value = hp;
        }

        public void VaryHP(int hp)
        {
            _hp.Value = Math.Clamp(_hp.Value + hp, 0, MaxHP.CurrentValue);
        }

        public void VaryMaxHP(int delta)
        {
            int newMaxHP = Math.Max(1, _maxHP.Value + delta);
            int appliedDelta = newMaxHP - _maxHP.Value;

            _maxHP.Value = newMaxHP;
            _hp.Value = Math.Clamp(_hp.Value + appliedDelta, 0, newMaxHP);
        }

        public void Dispose()
        {
            _hp.Dispose();
            _maxHP.Dispose();
        }
    }
}
