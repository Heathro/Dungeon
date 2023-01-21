using System;
using System.Collections.Generic;
using UnityEngine;
using Inventories;
using Saving;

namespace Shops
{
    public class Shop : MonoBehaviour, ISaveable
    {
        // CONFIG

        [SerializeField] List<ShopItem> shopItems = new List<ShopItem>();

        // STATE

        [System.Serializable]
        public class ShopItem
        {
            public InventoryItem item;
            public int availability;
        }

        // PUBLIC

        public IEnumerable<ShopItem> GetAllItems()
        {
            return shopItems;
        }

        public void AddItem(InventoryItem item, int number)
        {
            if (item.IsStackable())
            {
                foreach (ShopItem shopItem in shopItems)
                {
                    if (object.ReferenceEquals(shopItem.item, item))
                    {
                        shopItem.availability += number;
                        return;
                    }
                }
            }
            
            ShopItem newItem = new ShopItem();
            newItem.item = item;
            newItem.availability = number;
            shopItems.Add(newItem);
        }

        public void RemoveItem(InventoryItem item, int number)
        {
            foreach (ShopItem shopItem in shopItems)
            {
                if (object.ReferenceEquals(shopItem.item, item))
                {
                    shopItem.availability -= number;
                    if (shopItem.availability <= 0)
                    {
                        shopItems.Remove(shopItem);
                    }
                    return;
                }
            }
        }

        // PRIVATE
        
        [System.Serializable]
        struct ShopSlotRecord
        {
            public string id;
            public int availability;
        }

        object ISaveable.CaptureState()
        {
            ShopSlotRecord[] record = new ShopSlotRecord[shopItems.Count];

            for (int i = 0; i < shopItems.Count; i++)
            {
                record[i].id = shopItems[i].item.GetItemID();
                record[i].availability = shopItems[i].availability;
            }

            return record;
        }

        void ISaveable.RestoreState(object state)
        {
            ShopSlotRecord[] record = (ShopSlotRecord[])state;
            shopItems.Clear();

            for (int i = 0; i < record.Length; i++)
            {
                ShopItem newItem = new ShopItem();
                newItem.item = InventoryItem.GetFromID(record[i].id);
                newItem.availability = record[i].availability;
                shopItems.Add(newItem);
            }
        }
    }
}