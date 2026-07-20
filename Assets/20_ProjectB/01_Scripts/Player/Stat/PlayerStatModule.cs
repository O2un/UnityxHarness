using System;
using R3;
using UnityEngine;

namespace O2un.ProjectB.Platformer
{
    public sealed class PlayerStatModule : IPlayerStatReader, IPlayerStatWriter, IDisposable
    {
        private const int STAT_COUNT = 3;

        private readonly float[] _bases = new float[STAT_COUNT];
        private readonly float[] _modifiers = new float[STAT_COUNT];

        private readonly ReactiveProperty<float> _moveSpeed = new();
        private readonly ReactiveProperty<int> _maxHealth = new();
        private readonly ReactiveProperty<int> _attackBonus = new();

        public ReadOnlyReactiveProperty<float> MoveSpeed => _moveSpeed;
        public ReadOnlyReactiveProperty<int> MaxHealth => _maxHealth;
        public ReadOnlyReactiveProperty<int> AttackBonus => _attackBonus;

        public void SetBase(UpgradeStatType stat, float baseValue)
        {
            _bases[(int)stat] = baseValue;
            Publish(stat);
        }

        public void AddModifier(UpgradeStatType stat, float value)
        {
            _modifiers[(int)stat] += value;
            Publish(stat);
        }

        public void ClearModifiers()
        {
            for (int i = 0; i < STAT_COUNT; i++)
            {
                _modifiers[i] = 0f;
                Publish((UpgradeStatType)i);
            }
        }

        public void Dispose()
        {
            _moveSpeed.Dispose();
            _maxHealth.Dispose();
            _attackBonus.Dispose();
        }

        private void Publish(UpgradeStatType stat)
        {
            float final = _bases[(int)stat] + _modifiers[(int)stat];

            switch (stat)
            {
                case UpgradeStatType.MoveSpeed:
                    _moveSpeed.Value = Mathf.Max(0f, final);
                    break;

                case UpgradeStatType.MaxHealth:
                    _maxHealth.Value = Mathf.Max(1, Mathf.RoundToInt(final));
                    break;

                case UpgradeStatType.AttackDamage:
                    _attackBonus.Value = Mathf.RoundToInt(final);
                    break;
            }
        }
    }
}
