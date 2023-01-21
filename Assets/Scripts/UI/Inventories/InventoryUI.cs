using System.Collections.Generic;
using UnityEngine;
using Inventories;

namespace UI.Inventories
{
    public class InventoryUI : MonoBehaviour
    {
        // CONFIG

        [SerializeField] InventorySlotUI InventoryItemPrefab = null;
        [SerializeField] Inventory inventory = null;
        [SerializeField] int startingPoolSize = 18;
        [SerializeField] int extensionSize = 9;

        // STATE

        List<InventorySlotUI> slotPool = new List<InventorySlotUI>();

        // LIFECYCLE

        void Awake() 
        {
            PopulatePool();
            inventory.inventoryUpdated += Redraw;
        }

        void Start()
        {
            Redraw();
        }

        // PRIVATE

        void Redraw()
        {
            int inventorySize = inventory.GetSize();

            while (inventorySize > slotPool.Count)
            {
                ExtendPool();
            }

            for (int i = 0; i < slotPool.Count; i++)
            {
                if (i < inventorySize)
                {
                    slotPool[i].gameObject.SetActive(true);
                    slotPool[i].Setup(inventory, i);
                }
                else
                {
                    slotPool[i].gameObject.SetActive(false);
                }    
            }
        }

        void PopulatePool()
        {
            for (int i = 0; i < startingPoolSize; i++)
            {
                slotPool.Add(Instantiate(InventoryItemPrefab, transform));
            }
        }

        void ExtendPool()
        {
            for (int i = 0; i < extensionSize; i++)
            {
                slotPool.Add(Instantiate(InventoryItemPrefab, transform));
            }
        }
    }
}