namespace O2un.Manager
{
    public interface IInventoryWriter
    {
        AddResult Add(IItemData item);
        IItemData Replace(int slotIndex, IItemData item);
        bool Use(int slotIndex);
        IItemData Drop(int slotIndex);
        IItemData Equip(IItemData item, int equipSlotIndex);
        IItemData Unequip(int equipSlotIndex);
    }
}
