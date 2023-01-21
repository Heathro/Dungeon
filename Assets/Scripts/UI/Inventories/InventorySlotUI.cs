using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Inventories;
using Utils.UI.Dragging;
using Utils;
using Abilities;
using Attributes;
using Control;
using Skills;
using Stats;

namespace UI.Inventories
{
    public class InventorySlotUI : MonoBehaviour, IItemHolder, IDragContainer<InventoryItem>, IPointerClickHandler
    {
        // CONFIG

        [SerializeField] InventoryItemIcon icon = null;
        [SerializeField] List<SlotType> allowedSources = new List<SlotType>();

        // CACHE

        ControlSwitcher controlSwitcher;
        FightScheduler fightScheduler;

        // STATE

        int index;
        Inventory inventory;

        // LIFECYCLE

        void Start()
        {
            controlSwitcher = FindObjectOfType<ControlSwitcher>();
            fightScheduler = FindObjectOfType<FightScheduler>();
        }

        // PUBLIC

        public void Setup(Inventory inventory, int index)
        {
            this.inventory = inventory;
            this.index = index;
            icon.SetItem(inventory.GetItemInSlot(index), inventory.GetNumberInSlot(index));
        }

        public int MaxAcceptable(InventoryItem item, SlotType slotType)
        {
            if (!allowedSources.Contains(slotType)) return 0;

            if (item is QuestItem && inventory.GetSlotType() == SlotType.None)
            {
                if (slotType == SlotType.PriestInventory || slotType == SlotType.PaladinInventory ||
                    slotType == SlotType.HunterInventory || slotType == SlotType.MageInventory)
                {
                    return 0;
                }

            }

            return int.MaxValue;
        }

        public void AddItems(InventoryItem item, int number)
        {
            Ability ability = item as Ability;
            if (ability != null && !ability.IsConsumable()) return;

            inventory.AddItemToSlot(index, item, number);
        }

        public InventoryItem GetItem()
        {
            return inventory.GetItemInSlot(index);
        }

        public SlotType GetSlotType()
        {
            return inventory.GetSlotType();
        }

        public int GetNumber()
        {
            return inventory.GetNumberInSlot(index);
        }

        public int GetIndex()
        {
            return index;
        }

        public void RemoveItems(int number)
        {
            inventory.RemoveFromSlot(index, number);
        }

        public bool IsItemStackable()
        {
            InventoryItem item = GetItem();
            if (item == null) return false;

            return item.IsStackable();
        }

        public void FlushItem()
        {

        }

        // PRIVATE

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;
            InventoryItem item = GetItem();
            if (item == null) return;

            PlayerController playerController = controlSwitcher.GetActivePlayer();
            if (playerController.GetComponent<Health>().IsDead()) return;

            if (inventory.GetSlotType() == SlotType.None)
            {
                if (!playerController.enabled && !playerController.IsLooting()) return;

                Inventory playerInventory = playerController.GetComponent<Inventory>();

                InventoryItem collectableItem = item;
                int collectableAmount = GetNumber();

                playerInventory.AddToFirstEmptySlot(collectableItem, collectableAmount);
                RemoveItems(collectableAmount);

                return;
            }    

            if (!playerController.enabled) return;

            Ability ability = item as Ability;
            if (ability != null)
            {
                Ability teachSkill = ability.GetTeachSkill();
                SkillStore skillStore = playerController.GetComponent<SkillStore>();
                BaseStats baseStats = playerController.GetComponent<BaseStats>();
                if (teachSkill != null && skillStore.HasSkill(teachSkill)) return;
                if (teachSkill != null && baseStats.GetClass() != ability.GetClass()) return;

                if (playerController.GetComponent<CooldownStore>().GetTimeRemaining(ability) > 0) return;

                controlSwitcher.GetAudioHub().PlayClick();

                ability.Use(playerController.gameObject, () =>
                {
                    inventory.RemoveFromSlot(index, 1);
                });                

                return;
            }

            EquipableItem newEquipment = item as EquipableItem;
            if (newEquipment != null)
            {
                if (fightScheduler.IsFightRunning()) return;

                Equipment equipment = playerController.GetComponent<Equipment>();
                EquipLocation equipLocation = newEquipment.GetAllowedEquipLocation();
                EquipableItem oldEquipment = equipment.GetItemInSlot(equipLocation);

                RemoveItems(1);

                GetComponent<ItemTooltipSpawner>().ForceClearTooltip();

                if (oldEquipment != null)
                {
                    equipment.RemoveItem(equipLocation);
                    AddItems(oldEquipment, 1);
                }

                equipment.AddItem(equipLocation, newEquipment);

                return;
            }
        }
    }
}