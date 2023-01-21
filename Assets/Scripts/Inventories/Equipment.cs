using System;
using System.Collections.Generic;
using UnityEngine;
using Saving;
using Utils;
using Stats;
using Combat;
using Audio;

namespace Inventories
{
    public class Equipment : MonoBehaviour, ISaveable, IModifierProvider
    {
        // CACHE

        AudioHub audioHub = null;

        // STATE

        Dictionary<EquipLocation, EquipableItem> equippedItems = new Dictionary<EquipLocation, EquipableItem>();

        public event Action equipmentUpdated;
        public event Action weaponUpdated;

        // LIFECYCLE

        void Start()
        {
            audioHub = FindObjectOfType<AudioHub>();
        }

        // PUBLIC

        public EquipableItem GetItemInSlot(EquipLocation equipLocation)
        {
            if (!equippedItems.ContainsKey(equipLocation))
            {
                return null;
            }
            return equippedItems[equipLocation];
        }

        public void AddItem(EquipLocation slot, EquipableItem item, bool sound = true)
        {
            if (sound) audioHub.PlayEquip();

            equippedItems[slot] = item;

            if (equipmentUpdated != null)
            {
                equipmentUpdated();
            }

            if (slot == EquipLocation.Weapon && weaponUpdated != null)
            {
                weaponUpdated();
            }
        }

        public void RemoveItem(EquipLocation slot)
        {
            audioHub.PlayEquip();
            equippedItems.Remove(slot);

            if (equipmentUpdated != null)
            {
                equipmentUpdated();
            }

            if (slot == EquipLocation.Weapon && weaponUpdated != null)
            {
                weaponUpdated();
            }
        }

        // PRIVATE

        IEnumerable<float> IModifierProvider.GetAdditiveModifiers(CharacterStat stat)
        {
            foreach (EquipableItem item in equippedItems.Values)
            {
                foreach (float modifier in item.GetAdditiveModifiers(stat))
                {
                    yield return modifier;
                }
            }
        }

        IEnumerable<float> IModifierProvider.GetPercentageModifiers(CharacterStat stat)
        {
            foreach (EquipableItem item in equippedItems.Values)
            {
                foreach (float modifier in item.GetPercentageModifiers(stat))
                {
                    yield return modifier;
                }
            }
        }

        object ISaveable.CaptureState()
        {
            var equippedItemsForSerialization = new Dictionary<EquipLocation, string>();
            foreach (var pair in equippedItems)
            {
                equippedItemsForSerialization[pair.Key] = pair.Value.GetItemID();
            }
            return equippedItemsForSerialization;
        }

        void ISaveable.RestoreState(object state)
        {
            equippedItems = new Dictionary<EquipLocation, EquipableItem>();

            var equippedItemsForSerialization = (Dictionary<EquipLocation, string>)state;

            foreach (var pair in equippedItemsForSerialization)
            {
                var item = (EquipableItem)InventoryItem.GetFromID(pair.Value);
                if (item != null)
                {
                    equippedItems[pair.Key] = item;
                }
            }
        }
    }
}