namespace O2un.Manager
{
    public readonly struct InventorySlot
    {
        public IItemData Item { get; }
        public int Count { get; }

        public InventorySlot(IItemData item, int count)
        {
            Item = item;
            Count = count;
        }

        public bool IsEmpty => Item == null;
    }
}
