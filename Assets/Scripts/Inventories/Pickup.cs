using UnityEngine;

namespace Inventories
{
    public class Pickup : MonoBehaviour
    {
        // STATE

        InventoryItem item;
        int number;

        // PUBLIC

        public void Setup(InventoryItem item, int number)
        {
            this.item = item;
            this.number = number;
        }

        public InventoryItem GetItem()
        {
            return item;
        }

        public int GetNumber()
        {
            return number;
        }

        public void PickupItem(Inventory inventory)
        {
            if (inventory.AddToFirstEmptySlot(item, number))
            {
                Destroy(gameObject);
            }
        }
    }
}