using System.Collections.Generic;
using R3;

namespace O2un.Manager
{
    public interface IInventoryReader
    {
        ReadOnlyReactiveProperty<IReadOnlyList<InventorySlot>> Slots { get; }
        ReadOnlyReactiveProperty<IReadOnlyList<InventorySlot>> Equipment { get; }
        Observable<IItemData> OnItemUsed { get; }
    }
}
