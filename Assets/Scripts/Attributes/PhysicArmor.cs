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
    public class PhysicArmor : MonoBehaviour, ISaveable
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

        public event Action onPhysicArmorChange;

        LazyValue<float> physicPoints;
        float regenerateTimer = Mathf.Infinity;

        float savedPhysicPoints = -1f;

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

            physicPoints = new LazyValue<float>(GetMaxPhysicArmor);
        }

        void Start()
        {
            fightScheduler = FindObjectOfType<FightScheduler>();
            randomizer = FindObjectOfType<Randomizer>();

            physicPoints.ForceInit();
            UpdatePhysicArmor();
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
            physicPoints.Value = 0f;
            UpdatePhysicArmor();
        }

        public void IncreaseArmor(float amount, DamageType damageType, bool popUpNeeded = true)
        {
            if (popUpNeeded)
            {
                overheadUI.AddPopUp(amount, damageType, true, "Physical Armor");
            }

            physicPoints.Value = Mathf.Min(physicPoints.Value + amount, GetMaxPhysicArmor());
            UpdatePhysicArmor();
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

            IncreaseArmor(amount, DamageType.Physic, true);
        }

        public void ReduseArmor(float amount, DamageType damageType)
        {
            overheadUI.AddPopUp(-amount, damageType, false, "Physical Armor");
            physicPoints.Value = Mathf.Max(0, physicPoints.Value - amount);
            UpdatePhysicArmor();
        }

        public float GetPhysicArmor()
        {
            return physicPoints.Value;
        }

        public float GetMaxPhysicArmor()
        {
            return Mathf.Floor(baseStats.GetStat(CharacterStat.PhysicArmor));
        }

        public float GetPercentage()
        {
            return 100 * (physicPoints.Value / GetMaxPhysicArmor());
        }

        public float GetFraction()
        {
            float max = GetMaxPhysicArmor();
            if (max == 0) return 0;

            return physicPoints.Value / GetMaxPhysicArmor();
        }

        public void RestoreActualPhysicPoints()
        {
            if (savedPhysicPoints < 0) return;
            physicPoints.Value = savedPhysicPoints;
            UpdatePhysicArmor();
        }

        // PRIVATE

        void Regenerate()
        {
            if (physicPoints.Value == GetMaxPhysicArmor()) return;
            if (fightScheduler.IsFightRunning()) return;
            if (health.IsDead()) return;
            if (!health.CanRegenerate()) return;

            if (regenerateTimer > regenerateInterval)
            {
                regenerateTimer = 0;
                IncreaseArmor(Mathf.Ceil(GetMaxPhysicArmor() * regenerateFraction), DamageType.Physic, false);
            }
        }

        void RegenerateFully()
        {
            physicPoints.Value = GetMaxPhysicArmor();
            UpdatePhysicArmor();
        }

        void UpdatePhysicArmor()
        {
            if (onPhysicArmorChange != null)
            {
                onPhysicArmorChange();
            }
        }

        void UpdateTimers()
        {
            regenerateTimer += Time.deltaTime;
        }

        void StatsUpdated()
        {
            if (physicPoints.Value > GetMaxPhysicArmor())
            {
                physicPoints.Value = GetMaxPhysicArmor();
            }
            UpdatePhysicArmor();
        }

        object ISaveable.CaptureState()
        {
            return physicPoints.Value;
        }

        void ISaveable.RestoreState(object state)
        {
            this.physicPoints.Value = (float)state;
            savedPhysicPoints = (float)state;
            UpdatePhysicArmor();
        }
    }
}