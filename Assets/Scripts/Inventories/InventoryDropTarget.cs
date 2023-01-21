using System.Collections.Generic;
using UnityEngine;
using Utils.UI.Dragging;
using Control;
using Utils;
using Abilities;
using Audio;

namespace Inventories
{
    public class InventoryDropTarget : MonoBehaviour, IDragDestination<InventoryItem>
    {
        // CONFIG

        [SerializeField] List<SlotType> allowedSources = new List<SlotType>();

        // CACHE

        ItemDropper itemDropper;
        ControlSwitcher controlSwitcher;
        AudioHub audioHub;

        // LIFECYCLE

        void Start()
        {
            controlSwitcher = FindObjectOfType<ControlSwitcher>();
            itemDropper = controlSwitcher.GetComponent<ItemDropper>();
            audioHub = FindObjectOfType<AudioHub>();
        }

        // PUBLIC

        public void AddItems(InventoryItem item, int number)
        {
            Ability ability = item as Ability;
            if (ability != null && !ability.IsConsumable()) return;

            audioHub.PlayDrop();

            itemDropper.DropItem(item, number, controlSwitcher.GetActivePlayer().transform);
        }

        public int MaxAcceptable(InventoryItem item, SlotType slotType)
        {
            if (!allowedSources.Contains(slotType)) return 0;
            if (item is QuestItem) return 0;

            return int.MaxValue;
        }

        public bool IsItemStackable()
        {
            return true;
        }

        public void FlushItem()
        {
            
        }
    }
}