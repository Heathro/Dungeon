using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Utils.UI.Dragging;
using Inventories;
using Utils;
using Control;

namespace UI.Inventories
{
    public class EquipmentSlotUI : MonoBehaviour, IItemHolder, IDragContainer<InventoryItem>, IPointerClickHandler
    {
        // CONFIG

        [SerializeField] Equipment equipment = null;
        [SerializeField] InventoryItemIcon icon = null;
        [SerializeField] EquipLocation equipLocation = EquipLocation.Weapon;
        [SerializeField] Image defaultImage = null;

        // CACHE

        FightScheduler fightScheduler;

        // LIFECYCLE
       
        void Start() 
        {
            fightScheduler = FindObjectOfType<FightScheduler>();
            equipment.equipmentUpdated += RedrawUI;
            RedrawUI();
        }

        // PUBLIC

        public int MaxAcceptable(InventoryItem item, SlotType area)
        {
            if (fightScheduler.IsFightRunning()) return 0;

            EquipableItem equipableItem = item as EquipableItem;
            if (equipableItem == null) return 0;
            if (equipableItem.GetAllowedEquipLocation() != equipLocation) return 0;
            if (GetItem() != null) return 0;

            return 1;
        }

        public void AddItems(InventoryItem item, int number)
        {
            equipment.AddItem(equipLocation, (EquipableItem)item);
        }

        public InventoryItem GetItem()
        {
            return equipment.GetItemInSlot(equipLocation);
        }

        public SlotType GetSlotType()
        {
            return SlotType.Equipment;
        }

        public int GetNumber()
        {
            return GetItem() != null ? 1 : 0;
        }

        public int GetIndex()
        {
            return -1;
        }

        public void RemoveItems(int number)
        {
            equipment.RemoveItem(equipLocation);
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

        void RedrawUI()
        {
            EquipableItem item = equipment.GetItemInSlot(equipLocation);

            icon.SetItem(item, 1);

            if (defaultImage == null) return;
            defaultImage.enabled = item == null;
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;
            if (fightScheduler.IsFightRunning()) return;

            InventoryItem item = GetItem();
            if (item == null) return;

            Inventory currentInventory = equipment.GetComponent<Inventory>();
            currentInventory.AddToFirstEmptySlot(item, 1);
            RemoveItems(1);
        }
    }
}