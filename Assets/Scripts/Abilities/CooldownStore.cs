using System.Collections.Generic;
using UnityEngine;
using Inventories;
using Control;
using Saving;

namespace Abilities
{
    public class CooldownStore : MonoBehaviour, ISaveable
    {
        // CACHE

        FightScheduler fightScheduler;

        // STATE

        InventoryItem lastUsedAbility = null;
        float lastUsedTime = 0f;
        bool fightRunning = false;

        // STATE

        Dictionary<InventoryItem, float> timers = new Dictionary<InventoryItem, float>();

        // LIFECYCLE

        void Start()
        {
            fightScheduler = FindObjectOfType<FightScheduler>();
            fightScheduler.onEnemyAggro += StartFightMode;
            fightScheduler.onQueueEnd += UpdateFightTimers;
            fightScheduler.onFightFinish += ReturnToCivilMode;
        }

        void Update()
        {
            if (!fightRunning)
            {
                CivilMode();
            }
        }

        // PUBLIC

        public void StartCooldown(InventoryItem item, float time)
        {
            timers[item] = time;

            lastUsedAbility = item;
            lastUsedTime = time;
        }

        public float GetTimeRemaining(InventoryItem item)
        {
            if (item == null || !timers.ContainsKey(item))
            {
                return 0f;
            }
            return timers[item];
        }

        // PRIVATE

        void StartFightMode()
        {
            fightRunning = true;

            if (lastUsedAbility == null) return;
            if (!timers.ContainsKey(lastUsedAbility)) return;

            StartCooldown(lastUsedAbility, lastUsedTime);
        }

        void UpdateFightTimers()
        {
            List<InventoryItem> keys = new List<InventoryItem>(timers.Keys);

            foreach (InventoryItem item in keys)
            {
                timers[item] -= 1f;
                if (timers[item] <= 0)
                {
                    timers.Remove(item);
                }
            }
        }

        void CivilMode()
        {
            List<InventoryItem> keys = new List<InventoryItem>(timers.Keys);

            foreach (InventoryItem item in keys)
            {
                timers[item] -= Time.deltaTime;
                if (timers[item] <= 0)
                {
                    timers.Remove(item);
                }
            }
        }

        void ReturnToCivilMode()
        {
            fightRunning = false;
        }

        [System.Serializable]
        struct CooldownRecord
        {
            public string id;
            public float time;
        }

        object ISaveable.CaptureState()
        {
            CooldownRecord[] record = new CooldownRecord[timers.Count];

            int i = 0;

            foreach (KeyValuePair<InventoryItem, float> cooldown in timers)
            {
                record[i].id = cooldown.Key.GetItemID();
                record[i].time = cooldown.Value;
                i++;
            }

            return record;
        }

        void ISaveable.RestoreState(object state)
        {
            CooldownRecord[] record = (CooldownRecord[])state;

            foreach (CooldownRecord cooldown in record)
            {
                StartCooldown(InventoryItem.GetFromID(cooldown.id), cooldown.time);
            }
        }
    }
}