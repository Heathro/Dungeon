using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Utils.UI.Dragging;
using Inventories;
using Abilities;
using Control;
using Utils;
using Attributes;
using Combat;

namespace UI.Inventories
{
    public class ActionSlotUI : MonoBehaviour, IItemHolder, IDragContainer<InventoryItem>, IPointerClickHandler
    {
        // CONFIG

        [SerializeField] ActionStore actionStore = null;
        [SerializeField] InventoryItemIcon icon = null;
        [SerializeField] KeyCode keycode = KeyCode.None;
        [SerializeField] SlotType slotType = SlotType.None;
        [SerializeField] List<SlotType> allowedSources = new List<SlotType>();
        [SerializeField] CooldownStore cooldownStore = null;
        [SerializeField] Image cooldownTint = null;
        [SerializeField] TMP_Text cooldownTime;

        // CACHE

        PlayerController playerController;
        Health health;
        Inventory inventory;
        Equipment equipment;

        // LIFECYCLE

        void Awake()
        {
            actionStore.storeUpdated += UpdateIcon;
        }

        void Start()
        {
            playerController = actionStore.GetComponent<PlayerController>();
            health = playerController.GetComponent<Health>();
            inventory = playerController.GetComponent<Inventory>();
            equipment = playerController.GetComponent<Equipment>();
        }

        void Update()
        {
            InventoryItem item = GetItem();
            if (item == null)
            {
                cooldownTint.enabled = false;
                cooldownTime.text = "";
                return;
            }
            
            if (health.IsDead())
            {
                cooldownTint.enabled = true;
                cooldownTime.text = "";
                return;
            }

            float time = cooldownStore.GetTimeRemaining(item);
            if (time > 0)
            {
                cooldownTint.enabled = true;
                cooldownTime.text = Mathf.CeilToInt(time).ToString();
            }
            else
            {
                cooldownTint.enabled = false;
                cooldownTime.text = "";
            }

            if (!playerController.enabled)
            {
                cooldownTint.enabled = true;
                return;
            }

            Ability ability = item as Ability;
            if (ability == null) return;
            if (ability.IsMeleeRequired() || ability.IsRangedRequired())
            {
                WeaponConfig weapon = equipment.GetItemInSlot(EquipLocation.Weapon) as WeaponConfig;
                if (weapon == null)
                {
                    cooldownTint.enabled = true;
                }
                else if (ability.IsMeleeRequired() && !weapon.IsMelee())
                {
                    cooldownTint.enabled = true;
                }
                else if (ability.IsRangedRequired() && !weapon.IsRanged())
                {
                    cooldownTint.enabled = true;
                }
            }
            if (playerController.IsTakingTurn() && playerController.GetActionPoints() < ability.GetCost())
            {
                cooldownTint.enabled = true;
            }
        }

        // PUBLIC

        public void AddItems(InventoryItem item, int number)
        {
            actionStore.AddAction(item, keycode, number);
        }

        public InventoryItem GetItem()
        {
            return actionStore.GetAction(keycode);
        }

        public int GetNumber()
        {
            return actionStore.GetNumber(keycode);
        }

        public int GetIndex()
        {
            return -1;
        }

        public SlotType GetSlotType()
        {
            return slotType;
        }

        public int MaxAcceptable(InventoryItem item, SlotType slotType)
        {
            if (!allowedSources.Contains(slotType)) return 0;
            return actionStore.MaxAcceptable(item, keycode);
        }

        public void RemoveItems(int number)
        {
            actionStore.RemoveItems(keycode, number);
        }

        public bool IsItemStackable()
        {
            InventoryItem item = GetItem();
            if (item == null) return false;

            return item.IsStackable();
        }

        public void FlushItem()
        {
            InventoryItem item = GetItem();
            if (item == null) return;

            int number = GetNumber();

            if (item.IsStackable())
            {
                inventory.AddToFirstEmptySlot(item, number);
            }
            
            RemoveItems(number);
        }

        // PRIVATE

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;

            if (!playerController.enabled) return;
            if (health.IsDead()) return;

            actionStore.Use(keycode, playerController.gameObject);
        }

        void UpdateIcon()
        {
            icon.SetItem(GetItem(), GetNumber());
        }
    }
}
