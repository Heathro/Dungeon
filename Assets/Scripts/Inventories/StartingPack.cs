using UnityEngine;
using Abilities;
using Saving;
using Skills;
using Utils;
using System;

namespace Inventories
{
    public class StartingPack : MonoBehaviour, ISaveable
    {
        // CONFIG

        [SerializeField] Ability[] startingSkills;
        [SerializeField] InventoryPack[] startingInventory;
        [SerializeField] EquipmentPack[] startingEquipment;

        [System.Serializable]
        class InventoryPack
        {
            public InventoryItem item;
            public int number;
        }

        [System.Serializable]
        class EquipmentPack
        {
            public EquipLocation location;
            public EquipableItem item;
        }

        // CACHE

        SkillStore skillStore = null;
        Inventory inventory = null;
        Equipment equipment = null;

        // STATE

        bool startingPackGiven = false;

        // LIFECYCLE

        void Awake()
        {
            skillStore = GetComponent<SkillStore>();
            inventory = GetComponent<Inventory>();
            equipment = GetComponent<Equipment>();
        }

        // PUBLIC

        void Start()
        {
            if (startingPackGiven) return;

            GiveStartingSkills();
            GiveStartingInventory();
            GiveStartingEquipment();

            startingPackGiven = true;
        }

        // PRIVATE

        void GiveStartingSkills()
        {
            foreach (Ability ability in startingSkills)
            {
                if (ability == null) continue;

                skillStore.AddSkill(ability);
            }
        }

        void GiveStartingInventory()
        {
            foreach (InventoryPack pack in startingInventory)
            {
                if (pack.item == null || pack.number < 0) continue;

                inventory.AddToFirstEmptySlot(pack.item, pack.number, false);
            }
            inventory.UpdateInventory();
        }

        void GiveStartingEquipment()
        {
            foreach (EquipmentPack pack in startingEquipment)
            {
                if (pack.item == null) continue;

                equipment.AddItem(pack.location, pack.item, false);
            }
        }

        object ISaveable.CaptureState()
        {
            return startingPackGiven;
        }

        void ISaveable.RestoreState(object state)
        {
            startingPackGiven = (bool)state;
        }
    }
}