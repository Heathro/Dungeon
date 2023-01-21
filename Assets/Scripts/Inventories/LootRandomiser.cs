using Saving;
using System.Collections.Generic;
using UnityEngine;
using Shops;

namespace Inventories
{
    public class LootRandomiser : MonoBehaviour, ISaveable
    {
        // CONFIG

        [SerializeField] GuaranteedLoot[] guaranteedDrops;
        [SerializeField] ChoosenLoot choosenLoot;
        [SerializeField] RandomLoot[] randomDrops;

        [System.Serializable]
        struct GuaranteedLoot
        {
            public InventoryItem item;
            public int number;
        }

        [System.Serializable]
        struct ChoosenLoot
        {
            public List<InventoryItem> items;
            public int dropAmount;
        }

        [System.Serializable]
        struct RandomLoot
        {
            public InventoryItem item;
            [Range(0, 100)] public int maxNumber;
            [Range(0, 100)] public int dropChance;
        }

        // CACHE

        Inventory inventory;

        // STATE

        bool generated = false;

        // LIFECYCLE

        void Awake()
        {
            inventory = GetComponent<Inventory>();
        }

        void Start()
        {
            if (choosenLoot.dropAmount == 0) choosenLoot.dropAmount = 1;
        }

        // PUBLIC

        public void GenerateLoot()
        {
            if (generated) return;
            generated = true;

            foreach (GuaranteedLoot drop in guaranteedDrops)
            {
                if (drop.item == null || drop.number < 1) continue;

                inventory.AddToFirstEmptySlot(drop.item, drop.number, false);
            }

            Shop shop = GetComponent<Shop>();
            if (shop != null)
            {
                foreach (Shop.ShopItem shopItem in shop.GetAllItems())
                {
                    inventory.AddToFirstEmptySlot(shopItem.item, shopItem.availability, false);
                }
                return;
            }

            if (choosenLoot.dropAmount > choosenLoot.items.Count)
            {
                choosenLoot.dropAmount = choosenLoot.items.Count;
            }
            for (int i = 0; i < choosenLoot.dropAmount; i++)
            {
                int index = Random.Range(0, choosenLoot.items.Count);

                if (choosenLoot.items[index] != null)
                {
                    inventory.AddToFirstEmptySlot(choosenLoot.items[index], 1, false);
                }
                choosenLoot.items.RemoveAt(index);
            }

            foreach (RandomLoot drop in randomDrops)
            {
                if (drop.item == null || drop.maxNumber < 1 || drop.dropChance < 0 || drop.dropChance > 100) continue;

                if (Random.Range(0, 100) <= drop.dropChance)
                {
                    int number = Random.Range(1, drop.maxNumber + 1);
                    inventory.AddToFirstEmptySlot(drop.item, number, false);
                }
            }
        }

        // PRIVATE

        object ISaveable.CaptureState()
        {
            return generated;
        }

        void ISaveable.RestoreState(object state)
        {
            generated = (bool)state;
        }
    }
}