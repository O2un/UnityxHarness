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
            _hp.Value += hp;
            _hp.Value = Math.Clamp(CurrentHP.CurrentValue, 0, MaxHP.CurrentValue);
        }

        public void Dispose()
        {
            _hp.Dispose();
            _maxHP.Dispose();
        }
    }
}
