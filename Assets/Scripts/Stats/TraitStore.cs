using System;
using System.Collections.Generic;
using UnityEngine;
using Saving;
using Inventories;
using Abilities;
using Audio;

namespace Stats
{
    public class TraitStore : MonoBehaviour, ISaveable, IModifierProvider
    {
        // CONFIG

        [SerializeField] int strength = 0;
        [SerializeField] int finesse = 0;
        [SerializeField] int intelligence = 0;
        [SerializeField] int constitution = 0;
        [SerializeField] int wits = 0;
        [SerializeField] int levelUpPoints = 2;

        [SerializeField] TraitBonus[] strengthBonus;
        [SerializeField] TraitBonus[] finesseBonus;
        [SerializeField] TraitBonus[] intelligenceBonus;
        [SerializeField] TraitBonus[] constitutionBonus;
        [SerializeField] TraitBonus[] witsBonus;

        [System.Serializable]
        class TraitBonus
        {
            public CharacterStat stat;
            public float additiveBonus = 0f;
            public float percentBonus = 0f;
        }

        // CACHE

        AudioHub audioHub = null;

        // STATE

        public event Action onTraitChange;

        int availablePoints = 0;

        // LIFECYCLE

        void Start()
        {
            GetComponent<BaseStats>().onLevelUp += AddNewPoints;
            GetComponent<Equipment>().equipmentUpdated += UpdateTraits;
            GetComponent<Equipment>().weaponUpdated += UpdateTraits;
            GetComponent<BuffStore>().buffStoreUpdated += UpdateTraits;
            audioHub = FindObjectOfType<AudioHub>();
        }

        // PUBLIC

        public int GetAvailablePoints()
        {
            return availablePoints;
        }

        public void AddNewPoints()
        {
            availablePoints += levelUpPoints;

            UpdateTraits();
        }    

        public int GetTraitValue(CharacterTrait trait)
        {
            switch (trait)
            {
                case CharacterTrait.Strength: return strength + GetBonusTraits(CharacterStat.Strength);
                case CharacterTrait.Finesse: return finesse + GetBonusTraits(CharacterStat.Finesse);
                case CharacterTrait.Intelligence: return intelligence + GetBonusTraits(CharacterStat.Intelligence);
                case CharacterTrait.Constitution: return constitution + GetBonusTraits(CharacterStat.Constitution);
                case CharacterTrait.Wits: return wits + GetBonusTraits(CharacterStat.Wits);
                default: return 0;
            }
        }

        public void Allocate(CharacterTrait trait)
        {
            audioHub.PlayClick();

            switch (trait)
            {
                case CharacterTrait.Strength: strength++; break;
                case CharacterTrait.Finesse: finesse++; break;
                case CharacterTrait.Intelligence: intelligence++; break;
                case CharacterTrait.Constitution: constitution++; break;
                case CharacterTrait.Wits: wits++; break;
            }
            availablePoints--;

            UpdateTraits();
        }

        // PRIVATE

        void UpdateTraits()
        {
            if (onTraitChange != null)
            {
                onTraitChange();
            }
        }

        int GetBonusTraits(CharacterStat stat)
        {
            float total = 0;
            foreach (IModifierProvider provider in GetComponents<IModifierProvider>())
            {
                foreach (float modifier in provider.GetAdditiveModifiers(stat))
                {
                    total += modifier;
                }
            }
            return (int)total;
        }

        IEnumerable<float> IModifierProvider.GetAdditiveModifiers(CharacterStat stat)
        {
            foreach (TraitBonus bonus in strengthBonus)
            {
                if (stat != bonus.stat) continue;
                yield return bonus.additiveBonus * GetTraitValue(CharacterTrait.Strength);
            }

            foreach (TraitBonus bonus in finesseBonus)
            {
                if (stat != bonus.stat) continue;
                yield return bonus.additiveBonus * GetTraitValue(CharacterTrait.Finesse);
            }

            foreach (TraitBonus bonus in intelligenceBonus)
            {
                if (stat != bonus.stat) continue;
                yield return bonus.additiveBonus * GetTraitValue(CharacterTrait.Intelligence);
            }

            foreach (TraitBonus bonus in constitutionBonus)
            {
                if (stat != bonus.stat) continue;
                yield return bonus.additiveBonus * GetTraitValue(CharacterTrait.Constitution);
            }

            foreach (TraitBonus bonus in witsBonus)
            {
                if (stat != bonus.stat) continue;
                yield return bonus.additiveBonus * GetTraitValue(CharacterTrait.Wits);
            }
        }

        IEnumerable<float> IModifierProvider.GetPercentageModifiers(CharacterStat stat)
        {
            foreach (TraitBonus bonus in strengthBonus)
            {
                if (stat != bonus.stat) continue;
                yield return bonus.percentBonus * GetTraitValue(CharacterTrait.Strength);
            }

            foreach (TraitBonus bonus in finesseBonus)
            {
                if (stat != bonus.stat) continue;
                yield return bonus.percentBonus * GetTraitValue(CharacterTrait.Finesse);
            }

            foreach (TraitBonus bonus in intelligenceBonus)
            {
                if (stat != bonus.stat) continue;
                yield return bonus.percentBonus * GetTraitValue(CharacterTrait.Intelligence);
            }

            foreach (TraitBonus bonus in constitutionBonus)
            {
                if (stat != bonus.stat) continue;
                yield return bonus.percentBonus * GetTraitValue(CharacterTrait.Constitution);
            }

            foreach (TraitBonus bonus in witsBonus)
            {
                if (stat != bonus.stat) continue;
                yield return bonus.percentBonus * GetTraitValue(CharacterTrait.Wits);
            }
        }

        [System.Serializable]
        class TraitRecord
        {
            public int availablePoints;
            public int strength;
            public int finesse;
            public int intelligence;
            public int constitution;
            public int wits;
        }

        object ISaveable.CaptureState()
        {
            TraitRecord state = new TraitRecord();
            state.availablePoints = availablePoints;
            state.strength = strength;
            state.finesse = finesse;
            state.intelligence = intelligence;
            state.constitution = constitution;
            state.wits = wits;
            return state;
        }

        void ISaveable.RestoreState(object state)
        {
            TraitRecord record = (TraitRecord)state;
            availablePoints = record.availablePoints;
            strength = record.strength;
            finesse = record.finesse;
            intelligence = record.intelligence;
            constitution = record.constitution;
            wits = record.wits;

            UpdateTraits();
        }
    }
}