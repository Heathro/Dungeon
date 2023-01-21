using System;
using System.Collections.Generic;
using UnityEngine;
using Saving;
using Utils;
using Stats;
using Audio;

namespace Inventories
{
    public class Inventory : MonoBehaviour, ISaveable
    {
        // CONFIG


        [SerializeField] int defaultInventorySize = 18;
        [SerializeField] int inventoryRowSize = 9;
        [SerializeField] SlotType slotType = SlotType.None;

        // CACHE

        BaseStats baseStats;
        InventoryHub inventoryHub;
        AudioHub audioHub;

        // STATE

        public event Action inventoryUpdated;

        int inventorySize = 0;
        List<InventorySlot> slots = new List<InventorySlot>();

        public class InventorySlot
        {
            public InventoryItem item;
            public int number;
        }

        // LIFECYCLE

        void Awake()
        {
            baseStats = GetComponent<BaseStats>();
            inventoryHub = GetComponentInParent<InventoryHub>();

            inventorySize = defaultInventorySize;
            SetupSlots(inventorySize);
        }

        void Start()
        {
            audioHub = FindObjectOfType<AudioHub>();
        }

        // PUBLIC

        public int GetSize()
        {
            return slots.Count;
        }

        public void UpdateInventory()
        {
            if (inventoryUpdated != null)
            {
                inventoryUpdated();
            }
        }

        public bool AddToFirstEmptySlot(InventoryItem item, int number, bool update = true)
        {
            int i = FindSlot(item);

            if (i < 0)
            {
                return false;
            }

            slots[i].item = item;
            slots[i].number += number;

            UpdateBagSize();

            if (update)
            {
                audioHub.PlayMove();
                UpdateInventory();
            }

            if (item is QuestItem && inventoryHub != null) inventoryHub.TakeQuestItem(item as QuestItem);

            return true;
        }

        public bool HasItem(InventoryItem item)
        {
            for (int i = 0; i < slots.Count; i++)
            {
                if (object.ReferenceEquals(slots[i].item, item))
                {
                    return true;
                }
            }
            return false;
        }

        public InventoryItem GetItemInSlot(int slot)
        {
            return slots[slot].item;
        }

        public int GetNumberInSlot(int slot)
        {
            if (slots[slot].item == null)
            {
                return 0;
            }
            return slots[slot].number;
        }

        public bool AddItemToSlot(int slot, InventoryItem item, int number)
        {
            if (slots[slot].item != null && !object.ReferenceEquals(slots[slot].item, item))
            { 
                return AddToFirstEmptySlot(item, number);
            }

            audioHub.PlayMove();

            slots[slot].item = item;
            slots[slot].number += number;

            UpdateBagSize();
            UpdateInventory();

            if (item is QuestItem && inventoryHub != null) inventoryHub.TakeQuestItem(item as QuestItem);

            return true;
        }

        public void RemoveFromSlot(int slot, int number, bool update = true)
        {
            slots[slot].number -= number;
            if (slots[slot].number <= 0)
            {
                slots[slot].number = 0;
                slots[slot].item = null;
            }

            UpdateBagSize();
            
            if (update) UpdateInventory();
        }

        public void MakeSplit(InventoryItem item, int number, int index)
        {
            RemoveFromSlot(index, number, false);

            int i = FindEmptySlot();
            AddItemToSlot(i, item, number);
        }

        public void SortInventory()
        {
            List<InventorySlot> buffer = new List<InventorySlot>();
            foreach (InventorySlot slot in slots)
            {
                if (slot.item != null)
                {
                    buffer.Add(slot);
                }
            }

            inventorySize = defaultInventorySize;
            SetupSlots(inventorySize);

            foreach (InventorySlot slot in buffer)
            {
                AddToFirstEmptySlot(slot.item, slot.number, false);
            }

            UpdateInventory();
        }    

        public float GetTotalWeight()
        {
            float total = 0f;
            foreach (InventorySlot slot in slots)
            {
                if (slot.item == null) continue;

                total += slot.item.GetWeight() * slot.number;
            }
            return total;
        }

        public float GetMaxWeight()
        {
            return baseStats.GetStat(CharacterStat.CarryWeight);
        }

        public bool IsOverweighted()
        {
            return GetTotalWeight() > GetMaxWeight();
        }

        public SlotType GetSlotType()
        {
            return slotType;
        }

        // PRIVATE

        int FindSlot(InventoryItem item)
        {
            int i = FindStack(item);
            if (i < 0)
            {
                i = FindEmptySlot();
            }    
            return i;
        }

        int FindStack(InventoryItem item)
        {
            if (!item.IsStackable())
            {
                return -1;
            }

            for (int i = 0; i < slots.Count; i++)
            {
                if (object.ReferenceEquals(slots[i].item, item))
                {
                    return i;
                }
            }
            return -1;
        }

        int FindEmptySlot()
        {
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i].item == null)
                {
                    return i;
                }
            }
            return -1;
        }

        void UpdateBagSize()
        {
            if (CalculateEmptySlots() <= 1)
            {
                AddNewRow();
            }
        }

        int CalculateEmptySlots()
        {
            int total = 0;
            foreach (InventorySlot slot in slots)
            {
                if (slot.item == null)
                {
                    total++;
                }
            }
            return total;
        }
        
        void AddNewRow()
        {
            for (int i = 0; i < inventoryRowSize; i++)
            {
                slots.Add(new InventorySlot());
            }
            inventorySize += inventoryRowSize;
        }

        void SetupSlots(int size)
        {
            slots.Clear();
            for (int i = 0; i < size; i++)
            {
                slots.Add(new InventorySlot());
            }
        }

        [System.Serializable]
        struct InventorySlotRecord
        {
            public string id;
            public int number;
        }

        object ISaveable.CaptureState()
        {
            InventorySlotRecord[] record = new InventorySlotRecord[inventorySize];

            for (int i = 0; i < inventorySize; i++)
            {
                if (slots[i].item != null)
                {
                    record[i].id = slots[i].item.GetItemID();
                    record[i].number = slots[i].number;
                }
            }
            return record;
        }

        void ISaveable.RestoreState(object state)
        {
            InventorySlotRecord[] record = (InventorySlotRecord[])state;
            inventorySize = record.Length;

            SetupSlots(inventorySize);

            for (int i = 0; i < inventorySize; i++)
            {
                if (record[i].id == null) continue;

                slots[i].item = InventoryItem.GetFromID(record[i].id);
                slots[i].number = record[i].number;
            }

            UpdateInventory();
        }
    }
}