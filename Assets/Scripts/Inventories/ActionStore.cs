using System;
using System.Collections.Generic;
using UnityEngine;
using Saving;
using Audio;
using Abilities;

namespace Inventories
{
    public class ActionStore : MonoBehaviour, ISaveable
    {
        // CONFIG

        [SerializeField] KeyCode[] keyCodes;

        // CACHE

        AudioHub audioHub = null;
        CooldownStore cooldownStore = null;

        // STATE

        public event Action storeUpdated;
        Dictionary<KeyCode, DockedItemSlot> dockedItems = new Dictionary<KeyCode, DockedItemSlot>();

        class DockedItemSlot
        {
            public ActionItem item;
            public int number;
        }

        // LIFECYCLE

        void Awake()
        {
            cooldownStore = GetComponent<CooldownStore>();
        }

        void Start()
        {
            audioHub = FindObjectOfType<AudioHub>();
        }

        // PUBLIC

        public ActionItem GetAction(KeyCode keycode)
        {
            if (dockedItems.ContainsKey(keycode))
            {
                return dockedItems[keycode].item;
            }
            return null;
        }

        public int GetNumber(KeyCode keycode)
        {
            if (dockedItems.ContainsKey(keycode))
            {
                return dockedItems[keycode].number;
            }
            return 0;
        }

        public void AddToFirstEmptySlot(InventoryItem item, bool sound = true)
        {
            foreach (KeyCode keyCode in keyCodes)
            {
                if (!dockedItems.ContainsKey(keyCode))
                {
                    AddAction(item, keyCode, 1, sound);
                    break;
                }
            }
        }

        public void AddAction(InventoryItem item, KeyCode keycode, int number, bool sound = true)
        {
            if (dockedItems.ContainsKey(keycode))
            {
                if (object.ReferenceEquals(item, dockedItems[keycode].item))
                {
                    if (item.IsStackable()) dockedItems[keycode].number += number;
                }
            }
            else
            {
                var slot = new DockedItemSlot();
                slot.item = item as ActionItem;
                slot.number = number;
                dockedItems[keycode] = slot;
            }

            if (sound) audioHub.PlayMove();

            if (storeUpdated != null)
            {
                storeUpdated();
            }
        }

        public bool Use(KeyCode keycode, GameObject user)
        {
            if (dockedItems.ContainsKey(keycode))
            {
                if (cooldownStore.GetTimeRemaining(dockedItems[keycode].item) <= 0)
                {
                    audioHub.PlayClick();
                }

                dockedItems[keycode].item.Use(user, () =>
                {
                    if (dockedItems[keycode].item.IsConsumable()) RemoveItems(keycode, 1);
                });

                return true;
            }
            return false;
        }

        public void RemoveItems(KeyCode keycode, int number)
        {
            if (dockedItems.ContainsKey(keycode))
            {
                dockedItems[keycode].number -= number;
                if (dockedItems[keycode].number <= 0)
                {
                    dockedItems.Remove(keycode);
                }
                if (storeUpdated != null)
                {
                    storeUpdated();
                }
            }

        }

        public int MaxAcceptable(InventoryItem item, KeyCode keycode)
        {
            var actionItem = item as ActionItem;
            if (!actionItem) return 0;

            if (dockedItems.ContainsKey(keycode) && !object.ReferenceEquals(item, dockedItems[keycode].item))
            {
                return 0;
            }
            if (actionItem.IsConsumable())
            {
                return int.MaxValue;
            }
            if (dockedItems.ContainsKey(keycode))
            {
                return 0;
            }

            return 1;
        }

        /// PRIVATE

        [System.Serializable]
        private struct DockedItemRecord
        {
            public string itemID;
            public int number;
        }

        object ISaveable.CaptureState()
        {
            var state = new Dictionary<KeyCode, DockedItemRecord>();
            foreach (var pair in dockedItems)
            {
                var record = new DockedItemRecord();
                record.itemID = pair.Value.item.GetItemID();
                record.number = pair.Value.number;
                state[pair.Key] = record;
            }
            return state;
        }

        void ISaveable.RestoreState(object state)
        {
            var stateDict = (Dictionary<KeyCode, DockedItemRecord>)state;
            foreach (var pair in stateDict)
            {
                AddAction(InventoryItem.GetFromID(pair.Value.itemID), pair.Key, pair.Value.number, false);
            }
        }
    }
}