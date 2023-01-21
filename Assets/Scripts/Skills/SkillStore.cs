using System;
using System.Collections.Generic;
using UnityEngine;
using Inventories;
using Abilities;
using Saving;

namespace Skills
{
    public class SkillStore : MonoBehaviour, ISaveable
    {
        // CONFIG

        [SerializeField] List<Ability> skills = new List<Ability>();

        // STATE

        public event Action storeUpdated;

        ActionStore actionStore;

        // LIFECYCLE

        void Awake()
        {
            actionStore = GetComponent<ActionStore>();
        }

        // PUBLIC

        public int GetSize()
        {
            return skills.Count;
        }

        public void AddSkill(InventoryItem item, bool saving = false)
        {
            if (HasSkill(item as Ability)) return;
            
            skills.Add(item as Ability);
            
            if (!saving) actionStore.AddToFirstEmptySlot(item, false);

            if (storeUpdated != null)
            {
                storeUpdated();
            }
        }

        public bool HasSkill(Ability ability)
        {
            return skills.Contains(ability);
        }

        public Ability GetSkill(int index)
        {
            return skills[index];
        }

        public IEnumerable<Ability> GetAllSkills()
        {
            return skills;
        }

        object ISaveable.CaptureState()
        {
            string[] record = new string[skills.Count];

            for (int i = 0; i < skills.Count; i++)
            {
                record[i] = skills[i].GetItemID();
            }

            return record;
        }

        void ISaveable.RestoreState(object state)
        {
            string[] record = (string[])state;

            foreach (string id in record)
            {
                AddSkill(InventoryItem.GetFromID(id), true);
            }

            if (storeUpdated != null)
            {
                storeUpdated();
            }
        }
    }
}