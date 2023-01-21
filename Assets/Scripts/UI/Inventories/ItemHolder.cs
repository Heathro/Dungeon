using Inventories;
using Utils;

namespace UI.Inventories
{
    public interface IItemHolder
    {
        InventoryItem GetItem();
        SlotType GetSlotType();
        int GetNumber();
        int GetIndex();
    }
}