using System;
using System.Collections.Generic;

namespace O2un.Manager
{
    public sealed class InventoryModule
    {
        public const int DEFAULT_SLOT_COUNT = 12;
        public const int MAX_STACK_SIZE = 99;
        public const int DEFAULT_EQUIPMENT_SLOT_COUNT = 4;

        private readonly InventorySlot[] _slots;
        private readonly InventorySlot[] _equipment;

        public InventoryModule(int slotCount = DEFAULT_SLOT_COUNT, int equipmentSlotCount = DEFAULT_EQUIPMENT_SLOT_COUNT)
        {
            _slots = new InventorySlot[slotCount];
            _equipment = new InventorySlot[equipmentSlotCount];
        }

        public IReadOnlyList<InventorySlot> Slots => _slots;
        public IReadOnlyList<InventorySlot> Equipment => _equipment;

        public AddResult AddItem(IItemData item)
        {
            if (item == null) return AddResult.SlotsFull;

            return item.Category switch
            {
                ItemCategory.Equipment => AddToFirstEmpty(_equipment, item),
                ItemCategory.Consumable => AddConsumable(item),
                _ => AddToFirstEmpty(_slots, item),
            };
        }

        public IItemData ReplaceAt(int slotIndex, IItemData item)
        {
            if (!IsValid(_slots, slotIndex)) return item;

            IItemData displaced = _slots[slotIndex].Item;
            _slots[slotIndex] = item == null ? default : new InventorySlot(item, 1);
            return displaced;
        }

        public IItemData UseItem(int slotIndex)
        {
            if (!IsValid(_slots, slotIndex)) return null;

            InventorySlot slot = _slots[slotIndex];
            if (slot.IsEmpty || slot.Item.Category != ItemCategory.Consumable) return null;

            int newCount = slot.Count - 1;
            _slots[slotIndex] = newCount <= 0 ? default : new InventorySlot(slot.Item, newCount);
            return slot.Item;
        }

        public IItemData RemoveItem(int slotIndex)
        {
            if (!IsValid(_slots, slotIndex)) return null;

            IItemData item = _slots[slotIndex].Item;
            _slots[slotIndex] = default;
            return item;
        }

        public IItemData Equip(IItemData item, int equipSlotIndex)
        {
            if (!IsValid(_equipment, equipSlotIndex)) return item;

            IItemData previous = _equipment[equipSlotIndex].Item;
            _equipment[equipSlotIndex] = item == null ? default : new InventorySlot(item, 1);
            return previous;
        }

        public IItemData Unequip(int equipSlotIndex)
        {
            if (!IsValid(_equipment, equipSlotIndex)) return null;

            IItemData item = _equipment[equipSlotIndex].Item;
            _equipment[equipSlotIndex] = default;
            return item;
        }

        private AddResult AddConsumable(IItemData item)
        {
            int maxStack = Math.Min(Math.Max(1, item.MaxStack), MAX_STACK_SIZE);
            int remaining = 1;
            bool stackedExisting = false;

            for (int i = 0; i < _slots.Length && remaining > 0; i++)
            {
                InventorySlot slot = _slots[i];
                if (slot.IsEmpty || slot.Item.Id != item.Id) continue;

                int space = maxStack - slot.Count;
                if (space <= 0) continue;

                int moved = Math.Min(space, remaining);
                _slots[i] = new InventorySlot(item, slot.Count + moved);
                remaining -= moved;
                stackedExisting = true;
            }

            if (remaining <= 0) return stackedExisting ? AddResult.Stacked : AddResult.Added;

            for (int i = 0; i < _slots.Length && remaining > 0; i++)
            {
                if (!_slots[i].IsEmpty) continue;

                int moved = Math.Min(maxStack, remaining);
                _slots[i] = new InventorySlot(item, moved);
                remaining -= moved;
            }

            return remaining > 0 ? AddResult.SlotsFull : AddResult.Added;
        }

        private static AddResult AddToFirstEmpty(InventorySlot[] slots, IItemData item)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (!slots[i].IsEmpty) continue;

                slots[i] = new InventorySlot(item, 1);
                return AddResult.Added;
            }

            return AddResult.SlotsFull;
        }

        private static bool IsValid(InventorySlot[] slots, int index)
        {
            return index >= 0 && index < slots.Length;
        }
    }
}
