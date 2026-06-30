namespace O2un.Manager
{
    public interface IItemData
    {
        string Id { get; }
        ItemCategory Category { get; }
        int MaxStack { get; }
        string IconKey { get; }
    }
}
