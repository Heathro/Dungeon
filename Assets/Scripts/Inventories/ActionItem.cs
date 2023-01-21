using System;
using UnityEngine;

namespace Inventories
{
    [CreateAssetMenu(fileName = "Action Item", menuName = "Items/Action Item", order = 0)]
    public class ActionItem : InventoryItem
    {
        // CONFIG

        [SerializeField] bool consumable = false;

        // PUBLIC

        public virtual bool Use(GameObject user, Action consume)
        {
            Debug.Log("Using action: " + this);
            return true;
        }

        public virtual bool Use(GameObject user, GameObject target)
        {
            Debug.Log("Using action: " + this);
            return true;
        }

        public bool IsConsumable()
        {
            return consumable;
        }
    }
}