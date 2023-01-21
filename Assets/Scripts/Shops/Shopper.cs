using System;
using System.Collections.Generic;
using UnityEngine;
using Dialogues;
using Control;
using Inventories;
using Utils;
using Audio;

namespace Shops
{
    public class Shopper : MonoBehaviour
    {
        // CONFIG

        [SerializeField] int playerBagSize = 44;
        [SerializeField] int playerTableSize = 20;
        [SerializeField] int vendorBagSize = 44;
        [SerializeField] int vendorTableSize = 20;

        [SerializeField] int startingBagSize = 44;
        [SerializeField] int startingTableSize = 20;

        [SerializeField] int bagRowSize = 4;
        [SerializeField] int tableRowSize = 2;

        // CACHE

        ControlSwitcher controlSwitcher;
        InventoryHub inventoryHub;
        AudioHub audioHub;

        // STATE

        Sprite playerIcon = null;
        string playerName = "";
        Shop activeShop = null;
        Sprite vendorIcon = null;
        string vendorName = "";
        List<ShopSlot> playerBag = new List<ShopSlot>();
        List<ShopSlot> playerTable = new List<ShopSlot>();
        List<ShopSlot> vendorBag = new List<ShopSlot>();
        List<ShopSlot> vendorTable = new List<ShopSlot>();

        public class ShopSlot
        {
            public InventoryItem item;
            public int number;
        }

        // STATE

        public event Action onShopStart;
        public event Action onShopUpdate;
        public event Action onTransaction;

        // LIFECYCLE

        void Start()
        {
            controlSwitcher = GetComponent<ControlSwitcher>();
            inventoryHub = GetComponent<InventoryHub>();
            audioHub = FindObjectOfType<AudioHub>();
        }

        // PUBLIC

        public void StartShop(Shop shop)
        {
            PlayerController playerController = controlSwitcher.GetActivePlayer();
            playerController.EnableControl(false);

            AIConversant playerConversant = playerController.GetComponent<AIConversant>();
            playerIcon = playerConversant.GetAIIcon();
            playerName = playerConversant.GetAIName();

            activeShop = shop;
            AIConversant vendorConversant = shop.GetComponent<AIConversant>();
            vendorIcon = vendorConversant.GetAIIcon();
            vendorName = vendorConversant.GetAIName();

            FillStock();

            if (onShopStart != null)
            {
                onShopStart();
            }
        }

        public void LevelDeal()
        {
            audioHub.PlayLevel();

            TransferMoney(SlotType.PlayerTable, SlotType.PlayerBag, GetMoney(SlotType.PlayerTable));
            TransferMoney(SlotType.VendorTable, SlotType.VendorBag, GetMoney(SlotType.VendorTable));

            int playerTotal = GetPlayerTotal();
            int vendorTotal = GetVendorTotal();

            if (playerTotal < vendorTotal)
            {
                int difference = vendorTotal - playerTotal;
                int availableFunds = Mathf.Min(difference, GetPlayerPurse());
                TransferMoney(SlotType.PlayerBag, SlotType.PlayerTable, availableFunds);
            }
            else
            {
                int difference = playerTotal - vendorTotal;
                int availableFunds = Mathf.Min(difference, GetVendorPurse());
                TransferMoney(SlotType.VendorBag, SlotType.VendorTable, availableFunds);
            }

            UpdateBagSize(SlotType.PlayerBag);
            UpdateBagSize(SlotType.PlayerTable);
            UpdateBagSize(SlotType.VendorBag);
            UpdateBagSize(SlotType.VendorTable);

            if (onShopUpdate != null)
            {
                onShopUpdate();
            }
        }

        public void AcceptTransaction()
        {
            audioHub.PlayMoney();

            Inventory inventory = controlSwitcher.GetActivePlayer().GetComponent<Inventory>();
            foreach (ShopSlot shopSlot in vendorTable)
            {
                if (shopSlot.item == null) continue;

                inventory.AddToFirstEmptySlot(shopSlot.item, shopSlot.number, false);
                activeShop.RemoveItem(shopSlot.item, shopSlot.number);
            }
            inventory.UpdateInventory();

            foreach (ShopSlot shopSlot in playerTable)
            {
                if (shopSlot.item == null) continue;

                activeShop.AddItem(shopSlot.item, shopSlot.number);
                inventoryHub.RemoveItem(shopSlot.item, shopSlot.number);
            }

            if (onShopUpdate != null)
            {
                FillStock();
                onShopUpdate();
            }

            inventoryHub.SortInventory(false);

            if (onTransaction != null)
            {
                onTransaction();
            }
        }

        public int GetSize(SlotType area)
        {
            return GetCorrectArea(area).Count;
        }

        public void AddToFirstEmptySlot(SlotType area, InventoryItem item, int number, bool sound = true)
        {
            int i = FindSlot(area, item);

            List<ShopSlot> zone = GetCorrectArea(area);
            zone[i].item = item;
            zone[i].number += number;

            if (sound)
            {
                audioHub.PlayMove();
            }
            UpdateBagSize(area);
        }

        public void AddItemToSlot(SlotType area, int index, InventoryItem item, int number)
        {
            List<ShopSlot> zone = GetCorrectArea(area);

            if (zone[index].item != null && !object.ReferenceEquals(zone[index].item, item))
            {
                AddToFirstEmptySlot(area, item, number);
                return;
            }

            zone[index].item = item;
            zone[index].number += number;

            audioHub.PlayMove();
            UpdateBagSize(area);
        }

        public void RemoveItemFromSlot(SlotType area, int index, int number)
        {
            List<ShopSlot> zone = GetCorrectArea(area);

            zone[index].number -= number;
            if (zone[index].number <= 0)
            {
                zone[index].number = 0;
                zone[index].item = null;
            }

            UpdateBagSize(area);
        }

        public void MakeSplit(SlotType type, InventoryItem item, int number, int index)
        {
            RemoveItemFromSlot(type, index, number);

            int i = FindEmptySlot(type);
            AddItemToSlot(type, i, item, number);
        }

        public InventoryItem GetItemInSlot(SlotType area, int index)
        {
            return GetCorrectArea(area)[index].item;
        }

        public int GetNumberInSlot(SlotType area, int index)
        {
            List<ShopSlot> zone = GetCorrectArea(area);
            return zone[index].item == null ? 0 : zone[index].number;
        }

        public bool CanAccept()
        {
            int playerTotal = GetPlayerTotal();
            int vendorTotal = GetVendorTotal();

            return playerTotal >= vendorTotal ? playerTotal == 0 ? false : true : false;
        }

        public Sprite GetPlayerIcon()
        {
            return playerIcon;
        }

        public string GetPlayerName()
        {
            return playerName;
        }

        public int GetPlayerPurse()
        {
            return GetMoney(SlotType.PlayerBag);
        }

        public int GetPlayerTotal()
        {
            return GetTotal(SlotType.PlayerTable);
        }

        public Sprite GetVendorIcon()
        {
            return vendorIcon;
        }

        public string GetVendorName()
        {
            return vendorName;
        }

        public int GetVendorPurse()
        {
            return GetMoney(SlotType.VendorBag);
        }

        public int GetVendorTotal()
        {
            return GetTotal(SlotType.VendorTable);
        }

        // PRIVATE

        void FillStock()
        {
            playerBag.Clear();
            playerTable.Clear();
            vendorBag.Clear();
            vendorTable.Clear();

            playerBagSize = startingBagSize;
            playerTableSize = startingTableSize;
            vendorBagSize = startingBagSize;
            vendorTableSize = startingTableSize;

            SetupZones();
            GetPlayerItems();
            GetVendorItems();
        }

        void SetupZones()
        {
            while (playerBag.Count < playerBagSize) playerBag.Add(new ShopSlot());
            while (playerTable.Count < playerTableSize) playerTable.Add(new ShopSlot());
            while (vendorBag.Count < vendorBagSize) vendorBag.Add(new ShopSlot());
            while (vendorTable.Count < vendorTableSize) vendorTable.Add(new ShopSlot());
        }

        void GetPlayerItems()
        {
            foreach (Inventory inventory in GetComponentsInChildren<Inventory>())
            {
                for (int i = 0; i < inventory.GetSize(); i++)
                {
                    if (inventory.GetItemInSlot(i) is QuestItem) continue;

                    int number = inventory.GetNumberInSlot(i);
                    if (number > 0)
                    {
                        AddToFirstEmptySlot(SlotType.PlayerBag, inventory.GetItemInSlot(i), number, false);
                    }
                }
            }
        }

        void GetVendorItems()
        {
            foreach (Shop.ShopItem shopItem in activeShop.GetAllItems())
            {
                AddToFirstEmptySlot(SlotType.VendorBag, shopItem.item, shopItem.availability, false);
                UpdateBagSize(SlotType.VendorBag);
            }
        }

        int GetMoney(SlotType area)
        {
            int total = 0;

            foreach (ShopSlot shopSlot in GetCorrectArea(area))
            {
                MoneyItem moneyItem = shopSlot.item as MoneyItem;
                if (moneyItem == null) continue;
                total += (shopSlot.item.GetPrice() * shopSlot.number);
            }

            return total;
        }

        int GetTotal(SlotType area)
        {
            int total = 0;

            foreach (ShopSlot shopSlot in GetCorrectArea(area))
            {
                if (shopSlot.item == null) continue;

                int price = shopSlot.item.GetPrice();

                MoneyItem coin = shopSlot.item as MoneyItem;
                if (coin == null && (area == SlotType.VendorBag || area == SlotType.VendorTable))
                {
                    price *= 2;
                }

                total += (price * shopSlot.number);
            }

            return total;
        }

        void TransferMoney(SlotType source, SlotType destination, int amount)
        {
            if (amount <= 0) return;

            List<ShopSlot> zone = GetCorrectArea(source);
            InventoryItem moneyInventoryItem = null;
            int total = amount;

            for (int i = 0; i < zone.Count; i++)
            {
                MoneyItem moneyItem = zone[i].item as MoneyItem;
                if (moneyItem == null) continue;
                moneyInventoryItem = zone[i].item;

                int available = zone[i].number;
                if (available < total)
                {
                    RemoveItemFromSlot(source, i, available);
                    total -= available;
                }
                else
                {
                    RemoveItemFromSlot(source, i, total);
                    break;
                }
            }

            if (moneyInventoryItem != null)
            {
                AddToFirstEmptySlot(destination, moneyInventoryItem, amount, false);
            }
        }

        void RemoveMoney(InventoryItem item, int number)
        {
            int total = number;

            foreach (Inventory inventory in GetComponentsInChildren<Inventory>())
            {
                for (int i = 0; i < inventory.GetSize(); i++)
                {
                    if (object.ReferenceEquals(inventory.GetItemInSlot(i), item))
                    {
                        int available = inventory.GetNumberInSlot(i);

                        if (available < total)
                        {
                            inventory.RemoveFromSlot(i, available, false);
                            total -= available;
                            print(total.ToString());
                        }
                        else
                        {
                            inventory.RemoveFromSlot(i, available, false);
                            return;
                        }
                    }
                }
                inventory.UpdateInventory();
            }
        }

        int FindSlot(SlotType area, InventoryItem item)
        {
            int i = FindStack(area, item);
            if (i < 0)
            {
                i = FindEmptySlot(area);
            }
            return i;
        }

        int FindStack(SlotType area, InventoryItem item)
        {
            if (!item.IsStackable())
            {
                return -1;
            }

            List<ShopSlot> zone = GetCorrectArea(area);
            for (int i = 0; i < zone.Count; i++)
            {
                if (object.ReferenceEquals(zone[i].item, item))
                {
                    return i;
                }
            }
            return -1;
        }

        int FindEmptySlot(SlotType area)
        {
            List<ShopSlot> zone = GetCorrectArea(area);
            for (int i = 0; i < zone.Count; i++)
            {
                if (zone[i].item == null)
                {
                    return i;
                }
            }
            return -1;
        }

        void UpdateBagSize(SlotType area)
        {
            if (CalculateEmptySlots(area) <= 1)
            {
                AddNewRow(area);
            }
        }

        int CalculateEmptySlots(SlotType area)
        {
            int total = 0;
            foreach (ShopSlot slot in GetCorrectArea(area))
            {
                if (slot.item == null)
                {
                    total++;
                }
            }
            return total;
        }

        void AddNewRow(SlotType area)
        {
            List<ShopSlot> zone = GetCorrectArea(area);

            for (int i = 0; i < GetRowSize(area); i++)
            {
                zone.Add(new ShopSlot());
            }

            AdjustAreaSize(area);
        }

        int GetRowSize(SlotType area)
        {
            switch (area)
            {
                case SlotType.PlayerBag: return bagRowSize;
                case SlotType.PlayerTable: return tableRowSize;
                case SlotType.VendorBag: return bagRowSize;
                case SlotType.VendorTable: return tableRowSize;
                default: return 0;
            }
        }

        void AdjustAreaSize(SlotType area)
        {
            switch (area)
            {
                case SlotType.PlayerBag: playerBagSize += bagRowSize; break;
                case SlotType.PlayerTable: playerTableSize += tableRowSize; break;
                case SlotType.VendorBag: vendorBagSize += bagRowSize; break;
                case SlotType.VendorTable: vendorTableSize += tableRowSize; break;
            }
        }

        List<ShopSlot> GetCorrectArea(SlotType area)
        {
            switch (area)
            {
                case SlotType.PlayerBag: return playerBag;
                case SlotType.PlayerTable: return playerTable;
                case SlotType.VendorBag: return vendorBag;
                case SlotType.VendorTable: return vendorTable;
                default: return null;
            }
        }
    }
}