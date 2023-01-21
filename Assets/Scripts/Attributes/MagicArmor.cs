using System;
using UnityEngine;
using Inventories;
using Stats;
using Saving;
using Utils;
using Control;
using UI.Ingame;
using Core;
using Abilities;

namespace Attributes
{
    public class MagicArmor : MonoBehaviour, ISaveable
    {
        // CONFIG

        [SerializeField] float regenerateFraction = 0.025f;
        [SerializeField] float regenerateInterval = 2f;

        // CACHE

        BaseStats baseStats;
        FightScheduler fightScheduler;
        OverheadUI overheadUI;
        Health health;
        Randomizer randomizer;

        // STATE

        public event Action onMagicArmorChange;

        LazyValue<float> magicPoints;
        float regenerateTimer = Mathf.Infinity;

        float savedMagicPoints = -1f;

        // LIFECYCLE

        void Awake()
        {
            baseStats = GetComponent<BaseStats>();
            health = GetComponent<Health>();
            overheadUI = GetComponentInChildren<OverheadUI>();

            Equipment equipment = GetComponent<Equipment>();
            if (equipment != null)
            {
                equipment.equipmentUpdated += StatsUpdated;
            }

            TraitStore traitStore = GetComponent<TraitStore>();
            if (traitStore != null)
            {
                traitStore.onTraitChange += StatsUpdated;
            }

            GetComponent<BuffStore>().buffStoreUpdated += StatsUpdated;

            magicPoints = new LazyValue<float>(GetMaxMagicArmor);
        }

        void Start()
        {
            fightScheduler = FindObjectOfType<FightScheduler>();
            randomizer = FindObjectOfType<Randomizer>();

            magicPoints.ForceInit();
            UpdateMagicArmor();
        }

        void Update()
        {
            UpdateTimers();
            Regenerate();
        }

        void OnEnable()
        {
            baseStats.onLevelUp += RegenerateFully;
        }

        void OnDisable()
        {
            baseStats.onLevelUp -= RegenerateFully;
        }

        // PUBLIC

        public void RemoveArmor()
        {
            magicPoints.Value = 0f;
            UpdateMagicArmor();
        }

        public void IncreaseArmor(float amount, DamageType damageType, bool popUpNeeded = true)
        {
            if (popUpNeeded)
            {            
                overheadUI.AddPopUp(amount, damageType, true, "Magic Armor");
            }

            magicPoints.Value = Mathf.Min(magicPoints.Value + amount, GetMaxMagicArmor());
            UpdateMagicArmor();
        }

        public void IncreaseArmor(float amount, BaseStats attacker, bool critical)
        {
            if (critical)
            {
                float criticalAdd = Mathf.Ceil(randomizer.ApplyForCritical(amount, attacker.GetStat(CharacterStat.CriticalChance)));

                if (criticalAdd > 0)
                {
                    amount += criticalAdd;
                    overheadUI.AddPopUp(0f, DamageType.Critical, false, "Critical!");
                }
                amount = Mathf.Ceil(randomizer.Randomize(amount));
            }

            IncreaseArmor(amount, DamageType.Magic, true);
        }

        public void ReduseArmor(float amount, DamageType damageType)
        {
            overheadUI.AddPopUp(-amount, damageType, false, "Magic Armor");
            magicPoints.Value = Mathf.Max(0, magicPoints.Value - amount);
            UpdateMagicArmor();
        }

        public float GetMagicArmor()
        {
            return magicPoints.Value;
        }

        public float GetMaxMagicArmor()
        {
            return Mathf.Floor(baseStats.GetStat(CharacterStat.MagicArmor));
        }

        public float GetPercentage()
        {
            return 100 * (magicPoints.Value / GetMaxMagicArmor());
        }

        public float GetFraction()
        {
            float max = GetMaxMagicArmor();
            if (max == 0) return 0;

            return magicPoints.Value / GetMaxMagicArmor();
        }

        public void RestoreActualMagicPoints()
        {
            if (savedMagicPoints < 0) return;
            magicPoints.Value = savedMagicPoints;
            UpdateMagicArmor();
        }

        // PRIVATE

        void Regenerate()
        {
            if (magicPoints.Value == GetMaxMagicArmor()) return;
            if (fightScheduler.IsFightRunning()) return;
            if (health.IsDead()) return;
            if (!health.CanRegenerate()) return;

            if (regenerateTimer > regenerateInterval)
            {
                regenerateTimer = 0;
                IncreaseArmor(Mathf.Ceil(GetMaxMagicArmor() * regenerateFraction), DamageType.Magic, false);
            }
        }

        void RegenerateFully()
        {
            magicPoints.Value = GetMaxMagicArmor();
            UpdateMagicArmor();
        }

        void UpdateMagicArmor()
        {
            if (onMagicArmorChange != null)
            {
                onMagicArmorChange();
            }
        }

        void UpdateTimers()
        {
            regenerateTimer += Time.deltaTime;
        }

        void StatsUpdated()
        {
            if (magicPoints.Value > GetMaxMagicArmor())
            {
                magicPoints.Value = GetMaxMagicArmor();
            }
            UpdateMagicArmor();
        }

        object ISaveable.CaptureState()
        {
            return magicPoints.Value;
        }

        void ISaveable.RestoreState(object state)
        {
            this.magicPoints.Value = (float)state;
            savedMagicPoints = (float)state;
            UpdateMagicArmor();
        }
    }
}