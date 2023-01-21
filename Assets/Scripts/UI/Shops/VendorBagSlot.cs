using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Utils.UI.Dragging;
using UI.Inventories;
using UI.Shops;
using Inventories;
using Utils;

namespace Shops
{
    public class VendorBagSlot : MonoBehaviour, IItemHolder, IDragContainer<InventoryItem>, IPointerClickHandler
    {
        // CONFIG

        [SerializeField] InventoryItemIcon icon = null;
        [SerializeField] SlotType slotType = SlotType.VendorBag;
        [SerializeField] List<SlotType> allowedSources = new List<SlotType>();

        // STATE

        int index;
        Shopper shopper;
        ShopUI shopUI;

        // PUBLIC

        public void Setup(int index, Shopper shopper, ShopUI shopUI)
        {
            this.index = index;
            this.shopper = shopper;
            this.shopUI = shopUI;
            icon.SetItem(shopper.GetItemInSlot(slotType, index), shopper.GetNumberInSlot(slotType, index));
        }

        public void AddItems(InventoryItem item, int number)
        {
            shopper.AddItemToSlot(slotType, index, item, number);
            shopUI.UpdateVendorBag();
        }

        public InventoryItem GetItem()
        {
            return shopper.GetItemInSlot(slotType, index);
        }

        public SlotType GetSlotType()
        {
            return slotType;
        }

        public int GetNumber()
        {
            return shopper.GetNumberInSlot(slotType, index);
        }

        public int GetIndex()
        {
            return index;
        }

        public int MaxAcceptable(InventoryItem item, SlotType slotType)
        {
            if (!allowedSources.Contains(slotType)) return 0;
            return int.MaxValue;
        }

        public void RemoveItems(int number)
        {
            shopper.RemoveItemFromSlot(slotType, index, number);
            shopUI.UpdateVendorBag();
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
            int number = GetNumber();

            RemoveItems(number);
            shopper.AddToFirstEmptySlot(SlotType.VendorTable, item, number);
            shopUI.UpdateVendorTable();
        }
    }
}