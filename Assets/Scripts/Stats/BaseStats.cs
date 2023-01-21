using System;
using UnityEngine;
using Utils;
using UI.Ingame;
using Saving;
using Attributes;

namespace Stats
{
    public class BaseStats : MonoBehaviour
    {
        // CONFIG

        [SerializeField] Progression progression = null;
        [SerializeField] CharacterClass characterClass;
        [SerializeField] int startingLevel = 1;
        [SerializeField] GameObject levelUpEffectPrefab = null;
        [SerializeField] float effectDestroyDelay = 10f;

        // CACHE

        OverheadUI overheadUI;
        Experience experience;

        // STATE

        public event Action onLevelUp;

        LazyValue<int> currentLevel;

        // LIFECYCLE

        void Awake()
        {
            experience = GetComponent<Experience>();
            overheadUI = GetComponentInChildren<OverheadUI>();
            currentLevel = new LazyValue<int>(CalculateLevel);
        }

        void Start()
        {
            currentLevel.ForceInit();
            if (experience != null)
            {
                experience.onExperienceGained += UpdateLevel;
            }
        }

        // PUBLIC

        public void InitLevel()
        {
            currentLevel.Value = CalculateLevel();
        }

        public CharacterClass GetClass()
        {
            return characterClass;
        }

        public float GetStat(CharacterStat stat)
        {
            return (GetBaseStat(stat) + GetAdditiveModifier(stat)) * (1 + GetPercentageModifier(stat) / 100);
        }

        public float GetStat(CharacterStat stat, float amount)
        {
            return (amount + GetAdditiveModifier(stat)) * (1 + GetPercentageModifier(stat) / 100) * GetBaseStat(stat);
        }

        public int GetLevel()
        {
            if (currentLevel == null)
            {
                return 1;
            }
            return currentLevel.Value;
        }

        public float GetNextLevel()
        {
            return progression.GetStat(CharacterStat.ExperienceToLevelUp, characterClass, currentLevel.Value);
        }

        // PRIVATE

        void UpdateLevel()
        {
            int newLevel = CalculateLevel();
            if (newLevel > currentLevel.Value)
            {
                currentLevel.Value = newLevel;
                LevelUp();
            }
        }

        void LevelUp()
        {
            if (onLevelUp != null)
            {
                onLevelUp();
            }

            if (GetComponent<Health>().IsDead()) return;

            GameObject effect = Instantiate(levelUpEffectPrefab, transform);
            Destroy(effect, effectDestroyDelay);

            overheadUI.AddPopUp(0f, DamageType.Experience, false, "New Level!");
        }

        float GetBaseStat(CharacterStat stat)
        {
            return progression.GetStat(stat, characterClass, GetLevel());
        }

        float GetAdditiveModifier(CharacterStat stat)
        {
            float total = 0;
            foreach (IModifierProvider provider in GetComponents<IModifierProvider>())
            {
                foreach (float modifier in provider.GetAdditiveModifiers(stat))
                {
                    total += modifier;
                }
            }
            return total;
        }

        float GetPercentageModifier(CharacterStat stat)
        {
            float total = 0;
            foreach (IModifierProvider provider in GetComponents<IModifierProvider>())
            {
                foreach (float modifier in provider.GetPercentageModifiers(stat))
                {
                    total += modifier;
                }
            }
            return total;
        }

        int CalculateLevel()
        {
            Experience experience = GetComponent<Experience>();
            if (experience == null) return startingLevel;

            float currentXP = experience.GetPoints();
            int penultimateLevel = progression.GetLevels(CharacterStat.ExperienceToLevelUp, characterClass);
            for (int level = 1; level <= penultimateLevel; level++)
            {
                float XPToLevelUp = progression.GetStat(CharacterStat.ExperienceToLevelUp, characterClass, level);
                if (XPToLevelUp > currentXP)
                {
                    return level;
                }
            }

            return penultimateLevel + 1;
        }
    }
}