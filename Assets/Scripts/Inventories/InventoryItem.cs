using Quests;
using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace Inventories
{
    public abstract class InventoryItem : ScriptableObject, ISerializationCallbackReceiver
    {
        // CONFIG 

        [SerializeField] string itemID = null;
        [SerializeField] string displayName = null;
        [SerializeField][TextArea] string description = null;
        [SerializeField][TextArea] string additionalInfo = "";
        [SerializeField] ItemCategory itemCategory = ItemCategory.None;
        [SerializeField] Pickup pickup = null;
        [SerializeField] Sprite icon = null;
        [SerializeField] float weight = 1f;
        [SerializeField] bool stackable = false;
        [SerializeField] int price = 1;

        // STATE

        static Dictionary<string, InventoryItem> itemLookupCache;

        // PUBLIC

        public static InventoryItem GetFromID(string itemID)
        {
            if (itemLookupCache == null)
            {
                itemLookupCache = new Dictionary<string, InventoryItem>();
                var itemList = Resources.LoadAll<InventoryItem>("");
                foreach (var item in itemList)
                {
                    if (itemLookupCache.ContainsKey(item.itemID))
                    {
                        Debug.LogError(string.Format("Duplicate in InventoryItem lookUp for objects: {0} and {1}", itemLookupCache[item.itemID], item));
                        continue;
                    }
                    itemLookupCache[item.itemID] = item;
                }
            }
            if (string.IsNullOrEmpty(itemID) || !itemLookupCache.ContainsKey(itemID)) return null;
            return itemLookupCache[itemID];
        }

        public virtual Pickup SpawnPickup(Vector3 position, int number)
        {
            var pickup = Instantiate(this.pickup);
            pickup.transform.position = position;
            pickup.Setup(this, number);
            return pickup;
        }

        public Sprite GetIcon()
        {
            return icon;
        }

        public string GetItemID()
        {
            return itemID;
        }

        public bool IsStackable()
        {
            return stackable;
        }
        
        public string GetDisplayName()
        {
            return displayName;
        }

        public string GetDescription()
        {
            return description;
        }

        public string GetAdditionalInfo()
        {
            return additionalInfo;
        }

        public float GetWeight()
        {
            return weight;
        }

        public int GetPrice()
        {
            return price;
        }

        public ItemCategory GetItemCategory()
        {
            return itemCategory;
        }

        // PRIVATE

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            if (string.IsNullOrWhiteSpace(itemID))
            {
                itemID = System.Guid.NewGuid().ToString();
            }
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            
        }
    }
}
