using System;
using System.Collections.Generic;
using R3;
using VContainer.Unity;

namespace O2un.Manager
{
    public sealed class InventoryManager : IInventoryReader, IInventoryWriter, IInitializable, IDisposable
    {
        private readonly InventoryModule _module = new();
        private readonly ReactiveProperty<IReadOnlyList<InventorySlot>> _slots;
        private readonly ReactiveProperty<IReadOnlyList<InventorySlot>> _equipment;
        private readonly Subject<IItemData> _onItemUsed = new();

        public InventoryManager()
        {
            _slots = new ReactiveProperty<IReadOnlyList<InventorySlot>>(Snapshot(_module.Slots));
            _equipment = new ReactiveProperty<IReadOnlyList<InventorySlot>>(Snapshot(_module.Equipment));
        }

        public ReadOnlyReactiveProperty<IReadOnlyList<InventorySlot>> Slots => _slots;
        public ReadOnlyReactiveProperty<IReadOnlyList<InventorySlot>> Equipment => _equipment;
        public Observable<IItemData> OnItemUsed => _onItemUsed;

        public void Initialize() { }

        public AddResult Add(IItemData item)
        {
            AddResult result = _module.AddItem(item);
            if (result == AddResult.SlotsFull) return result;

            if (item.Category == ItemCategory.Equipment) PublishEquipment();
            else PublishSlots();

            return result;
        }

        public IItemData Replace(int slotIndex, IItemData item)
        {
            IItemData displaced = _module.ReplaceAt(slotIndex, item);
            PublishSlots();
            return displaced;
        }

        public bool Use(int slotIndex)
        {
            IItemData used = _module.UseItem(slotIndex);
            if (used == null) return false;

            PublishSlots();
            _onItemUsed.OnNext(used);
            return true;
        }

        public IItemData Drop(int slotIndex)
        {
            IItemData dropped = _module.RemoveItem(slotIndex);
            if (dropped != null) PublishSlots();
            return dropped;
        }

        public IItemData Equip(IItemData item, int equipSlotIndex)
        {
            IItemData previous = _module.Equip(item, equipSlotIndex);
            PublishEquipment();
            return previous;
        }

        public IItemData Unequip(int equipSlotIndex)
        {
            IItemData item = _module.Unequip(equipSlotIndex);
            if (item != null) PublishEquipment();
            return item;
        }

        public void Dispose()
        {
            _slots.Dispose();
            _equipment.Dispose();
            _onItemUsed.Dispose();
        }

        private void PublishSlots() => _slots.Value = Snapshot(_module.Slots);
        private void PublishEquipment() => _equipment.Value = Snapshot(_module.Equipment);

        private static IReadOnlyList<InventorySlot> Snapshot(IReadOnlyList<InventorySlot> source)
        {
            var copy = new InventorySlot[source.Count];
            for (int i = 0; i < source.Count; i++) copy[i] = source[i];
            return copy;
        }
    }
}
