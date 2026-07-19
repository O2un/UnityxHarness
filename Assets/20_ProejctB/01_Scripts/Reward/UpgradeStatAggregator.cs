using System;
using System.Collections.Generic;
using O2un.Manager;
using R3;
using VContainer.Unity;

namespace O2un.ProjectB.Platformer
{
    public interface IPassiveSkillQuery
    {
        bool IsUnlocked(PassiveSkillType skill);
    }

    public sealed class UpgradeStatAggregator : IPassiveSkillQuery, IInitializable, IDisposable
    {
        private readonly IInventoryReader _inventory;
        private readonly IPlayerStatWriter _stat;

        private readonly Dictionary<UpgradeStatType, float> _totals = new();
        private readonly HashSet<PassiveSkillType> _unlocked = new();

        private readonly CompositeDisposable _disposables = new();

        public UpgradeStatAggregator(IInventoryReader inventory, IPlayerStatWriter stat)
        {
            _inventory = inventory;
            _stat = stat;
        }

        public void Initialize()
        {
            _inventory.Slots.Subscribe(Recalculate).AddTo(_disposables);
        }

        public bool IsUnlocked(PassiveSkillType skill)
        {
            return _unlocked.Contains(skill);
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }

        private void Recalculate(IReadOnlyList<InventorySlot> slots)
        {
            _totals.Clear();
            _unlocked.Clear();

            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i].Item is not IUpgradeCardData card)
                {
                    continue;
                }

                if (UpgradeCardKind.PassiveSkill == card.Kind)
                {
                    _unlocked.Add(card.PassiveSkill);
                    continue;
                }

                _totals.TryGetValue(card.TargetStat, out float sum);
                _totals[card.TargetStat] = sum + card.ModifierValue;
            }

            // 슬롯 스냅샷 전체로 매번 다시 합산하므로 재계산이 멱등하도록 초기화 후 한 번씩만 넣는다
            _stat.ClearModifiers();

            foreach (KeyValuePair<UpgradeStatType, float> total in _totals)
            {
                _stat.AddModifier(total.Key, total.Value);
            }
        }
    }
}
