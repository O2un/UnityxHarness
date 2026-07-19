using System.Collections.Generic;
using O2un.Manager;

namespace O2un.ProjectB.Platformer
{
    public readonly struct AcquiredItemEntry
    {
        public readonly IUpgradeCardData Card;
        public readonly int Count;

        public AcquiredItemEntry(IUpgradeCardData card, int count)
        {
            Card = card;
            Count = count;
        }
    }

    public sealed class AcquiredItemGroupModule
    {
        private readonly List<AcquiredItemEntry> _entries = new();
        private readonly Dictionary<string, int> _indexById = new();

        public IReadOnlyList<AcquiredItemEntry> Entries => _entries;

        // UpgradeCardSO는 MaxStack이 1이라 같은 카드를 또 먹으면 별도 슬롯으로 쌓인다. Id로 묶어 개수를 레벨로 환산한다
        public IReadOnlyList<AcquiredItemEntry> Group(IReadOnlyList<InventorySlot> slots)
        {
            _entries.Clear();
            _indexById.Clear();

            if (null == slots)
            {
                return _entries;
            }

            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i].Item is not IUpgradeCardData card)
                {
                    continue;
                }

                int added = slots[i].Count > 0 ? slots[i].Count : 1;

                if (true == _indexById.TryGetValue(card.Id, out int index))
                {
                    _entries[index] = new AcquiredItemEntry(card, _entries[index].Count + added);
                    continue;
                }

                _indexById[card.Id] = _entries.Count;
                _entries.Add(new AcquiredItemEntry(card, added));
            }

            return _entries;
        }
    }
}
