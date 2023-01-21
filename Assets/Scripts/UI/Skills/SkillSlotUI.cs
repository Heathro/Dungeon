using UnityEngine;
using UnityEngine.EventSystems;
using Utils.UI.Dragging;
using UI.Inventories;
using Inventories;
using Skills;
using Utils;

namespace UI.Skills
{
    public class SkillSlotUI : MonoBehaviour, IItemHolder, IDragSource<InventoryItem>, IPointerClickHandler
    {
        // CONFIG

        [SerializeField] InventoryItemIcon icon = null;
        [SerializeField] SlotType slotType = SlotType.None;

        // CACHE

        SkillStore skillStore;
        ActionStore actionStore;
        int index;

        // PUBLIC

        public void Setup(int index, SkillStore skillStore)
        {
            this.index = index;
            this.skillStore = skillStore;
            this.actionStore = skillStore.GetComponent<ActionStore>();
            icon.SetItem(GetItem(), 1);
        }

        public InventoryItem GetItem()
        {
            return skillStore.GetSkill(index);
        }

        public int GetNumber()
        {
            return 1;
        }

        public SlotType GetSlotType()
        {
            return slotType;
        }

        public void RemoveItems(int number)
        {
            
        }

        public int GetIndex()
        {
            return index;
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;

            actionStore.AddToFirstEmptySlot(GetItem());
        }
    }
}
