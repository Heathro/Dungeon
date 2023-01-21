using UnityEngine;
using System.Collections.Generic;
using Core;
using Quests;
using Saving;
using Audio;

namespace Inventories
{
    public class InventoryHub : MonoBehaviour, ISaveable, IPredicateEvaluator
    {
        // CACHE

        AudioHub audioHub = null;

        // STATE

        List<string> takenQuestItems = new List<string>();

        // LIFECYCLE

        void Start()
        {
            audioHub = FindObjectOfType<AudioHub>();
        }

        // PUBLIC

        public void SortInventory(bool sound)
        {
            if (sound)
            {
                audioHub.PlayClick();
                audioHub.PlayMove();
            }
            foreach (Inventory inventory in GetComponentsInChildren<Inventory>())
            {
                inventory.SortInventory();
            }
        }    

        public void RemoveItem(InventoryItem item, int number)
        {
            int total = number;

            foreach (Inventory inventory in GetComponentsInChildren<Inventory>())
            {
                for (int i = 0; i < inventory.GetSize(); i++)
                {
                    if (object.ReferenceEquals(inventory.GetItemInSlot(i), item))
                    {
                        int available = inventory.GetNumberInSlot(i);
                        
                        if (available < total)
                        {
                            inventory.RemoveFromSlot(i, available, false);
                            total -= available;
                        }
                        else
                        {
                            inventory.RemoveFromSlot(i, total, false);
                            return;
                        }    
                    }
                }

                inventory.UpdateInventory();
            }
        }

        public void TakeQuestItem(QuestItem item)
        {
            if (takenQuestItems.Contains(item.GetItemID())) return;

            QuestCompletion[] questCompletions = GetComponents<QuestCompletion>();

            foreach (QuestCompletion questCompletion in questCompletions)
            {
                if (item.GetQuest() == questCompletion.GetQuest())
                {
                    takenQuestItems.Add(item.GetItemID());
                    questCompletion.CompleteObjective();
                }
            }
        }

        // PRIVATE

        bool HasInventoryItem(InventoryItem item)
        {
            foreach (Inventory inventory in GetComponentsInChildren<Inventory>())
            {
                if (inventory.HasItem(item))
                {
                    return true;
                }
            }
            return false;
        }

        bool? IPredicateEvaluator.Evaluate(string predicate, string[] parameters)
        {
            switch (predicate)
            {
                case "HasInventoryItem":
                    return HasInventoryItem(InventoryItem.GetFromID(parameters[0]));
            }

            return null;
        }

        public object CaptureState()
        {
            return takenQuestItems;
        }

        public void RestoreState(object state)
        {
            takenQuestItems = (List<string>)state;
        }
    }
}