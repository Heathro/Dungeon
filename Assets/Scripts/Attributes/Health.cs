using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using Core;
using Stats;
using Saving;
using Utils;
using UI.Ingame;
using Inventories;
using Control;
using Animations;
using Abilities;
using UI;
using UI.HUD;
using Audio;

namespace Attributes
{
    public class Health : MonoBehaviour, ISaveable
    {
        // CONFIG

        [SerializeField] float regenerateFraction = 0.025f;
        [SerializeField] float regenerateInterval = 2f;
        [SerializeField] float delayAfterDamage = 10f;
        [SerializeField] float resurrectDelay = 5f;
        [SerializeField] bool isUndead = false;
        [SerializeField] float resurrectHelthFraction = 0.4f;

        // CACHE

        AnimationController animationController;
        ActionScheduler actionScheduler;
        BaseStats baseStats;
        OverheadUI overheadUI;
        Randomizer randomizer;
        FightScheduler fightScheduler;
        MagicArmor magicArmor;
        PhysicArmor physicArmor;
        LootRandomiser lootRandomiser;
        BuffStore buffStore;
        ControlSwitcher controlSwitcher;
        PlayerController playerController;
        ShowHideUI showHideUI;
        StatusBar statusBar;
        IController iController;
        AudioController audioController;

        // STATE

        public event Action onHealthChange;
        public event Action onDeath;
        Action aggrevate = null;

        LazyValue<float> healthPoints;
        bool isDead = false;

        float regenerateTimer = Mathf.Infinity;
        float lastDamageTimer = Mathf.Infinity;

        int deathSceneIndex = 0;

        float savedHealthPoints = -1f;

        // LIFECYCLE

        void Awake()
        {
            playerController = GetComponent<PlayerController>();
            animationController = GetComponent<AnimationController>();
            actionScheduler = GetComponent<ActionScheduler>();
            baseStats = GetComponent<BaseStats>();
            overheadUI = GetComponentInChildren<OverheadUI>();
            magicArmor = GetComponent<MagicArmor>();
            physicArmor = GetComponent<PhysicArmor>();
            lootRandomiser = GetComponent<LootRandomiser>();
            iController = GetComponent<IController>();
            audioController = GetComponent<AudioController>();
            buffStore = GetComponent<BuffStore>();
            buffStore.buffStoreUpdated += StatsUpdated;

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

            healthPoints = new LazyValue<float>(GetMaxHealth);
        }

        void Start()
        {
            randomizer = FindObjectOfType<Randomizer>();
            fightScheduler = FindObjectOfType<FightScheduler>();
            controlSwitcher = FindObjectOfType<ControlSwitcher>();
            showHideUI = FindObjectOfType<ShowHideUI>();
            statusBar = FindObjectOfType<StatusBar>();

            healthPoints.ForceInit();
            UpdateHealth();
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

        public void SpawnOverheadTip(DamageType damageType, string skillName)
        {
            overheadUI.AddPopUp(0f, damageType, false, skillName);
        }

        public void TakeDamage(float damage, DamageType damageType, BaseStats attacker, bool critical, bool skillSource = false)
        {
            bool isDamage = true;

            if (!skillSource && attacker != null && AttemptDodge(attacker, baseStats))
            {
                isDamage = false;
                overheadUI.AddPopUp(0f, DamageType.Miss, false, "Miss!");
            }
            else
            {
                PlayerController playerController = GetComponent<PlayerController>();
                if (playerController != null) playerController.StopAllActions();

                lastDamageTimer = 0f;

                if (attacker != null && critical)
                {
                    float criticalAdd = Mathf.Ceil(randomizer.ApplyForCritical(damage, attacker.GetStat(CharacterStat.CriticalChance)));

                    if (criticalAdd > 0)
                    {
                        damage += criticalAdd;
                        overheadUI.AddPopUp(0f, DamageType.Critical, false, "Critical!");
                    }
                }

                if (critical)
                {
                    damage = Mathf.Ceil(randomizer.Randomize(damage));
                }
                damage = Mathf.Ceil(ResistToElementals(damage, damageType));

                if (damage < 0)
                {
                    isDamage = false;
                    overheadUI.AddPopUp(-damage, DamageType.Heal, true);
                    healthPoints.Value = Mathf.Min(healthPoints.Value + (-damage), GetMaxHealth());
                }
                else
                {
                    if (damageType == DamageType.Physic)
                    {
                        damage = DealPhysicDamage(damage, damageType);
                    }
                    else if (damageType != DamageType.Health)
                    {
                        damage = DealMagicDamage(damage, damageType);
                    }

                    if (damage > 0)
                    {
                        if (damageType == DamageType.Physic || damageType == DamageType.Magic)
                        {
                            damageType = DamageType.Health;
                        }    
                        overheadUI.AddPopUp(-damage, damageType, false);
                        healthPoints.Value = Mathf.Max(0, healthPoints.Value - damage);
                    }
                }
            }

            if (healthPoints.Value <= 0f)
            {
                if (lootRandomiser != null)
                {
                    lootRandomiser.GenerateLoot();
                }

                deathSceneIndex = SceneManager.GetActiveScene().buildIndex;

                Die();
            }
            else if (isDamage)
            {                
                if (!(isUndead && damageType == DamageType.Poison))
                {
                    if (attacker == null)
                    {
                        audioController.PlayPain();
                    }
                    else
                    {
                        audioController.PlayHit();
                    }
                }

                if (object.ReferenceEquals(controlSwitcher.GetActivePlayer(), playerController))
                {
                    animationController.GetHit();
                }
                else if (GetComponent<Movement.Mover>().IsIdle())
                {
                    animationController.GetHit();
                }
            }

            UpdateHealth();

            if (aggrevate != null)
            {
                aggrevate();
            }
        }

        public void Heal(float heal, BaseStats attacker, bool critical)
        {
            if (critical)
            {
                float criticalAdd = Mathf.Ceil(randomizer.ApplyForCritical(heal, attacker.GetStat(CharacterStat.CriticalChance)));

                if (criticalAdd > 0)
                {
                    heal += criticalAdd;
                    overheadUI.AddPopUp(0f, DamageType.Critical, false, "Critical!");
                }
                heal = Mathf.Ceil(randomizer.Randomize(heal));
            }

            if (isUndead)
            {
                lastDamageTimer = 0f;

                healthPoints.Value = Mathf.Max(0, healthPoints.Value - heal);
                overheadUI.AddPopUp(-heal, DamageType.Health, false);

                if (healthPoints.Value <= 0)
                {
                    if (lootRandomiser != null)
                    {
                        lootRandomiser.GenerateLoot();
                    }

                    deathSceneIndex = SceneManager.GetActiveScene().buildIndex;

                    Die();
                }
                else
                {
                    audioController.PlayHit();
                    animationController.GetHit();
                }
            }
            else
            {
                healthPoints.Value = Mathf.Min(healthPoints.Value + heal, GetMaxHealth());
                overheadUI.AddPopUp(heal, DamageType.Heal, true);
            }

            if (aggrevate != null)
            {
                aggrevate();
            }

            UpdateHealth();
        }

        public bool IsDead()
        {
            return isDead;
        }

        public bool IsUndead()
        {
            return isUndead;
        }

        public void Resurrect()
        {
            if (!isDead) return;

            StartCoroutine(ResurrectRoutine());
        }

        public bool CanRegenerate()
        {
            return lastDamageTimer > delayAfterDamage;
        }

        public float GetHealth()
        {
            return healthPoints.Value;
        }

        public float GetMaxHealth()
        {
            return Mathf.Floor(baseStats.GetStat(CharacterStat.Health));
        }

        public float GetPercentage()
        {
            return 100 * (healthPoints.Value / GetMaxHealth());
        }

        public float GetFraction()
        {
            return healthPoints.Value / GetMaxHealth();
        }

        public void SetAggrevateTarget(Action aggrevate)
        {
            this.aggrevate = aggrevate;
        }

        public int GetDeathSceneIndex()
        {
            return deathSceneIndex;
        }

        public void RestoreActualHealthPoints()
        {
            if (savedHealthPoints < 0) return;
            healthPoints.Value = savedHealthPoints;
            UpdateHealth();
        }

        // PRIVATE

        bool AttemptDodge(BaseStats attacker, BaseStats target)
        {
            float targetDodge = target.GetStat(CharacterStat.Dodging);
            float attackerAccuracy = attacker.GetStat(CharacterStat.Accuracy);

            int attackerLevel = attacker.GetLevel();
            int targetLevel = target.GetLevel();

            if (attackerLevel > targetLevel)
            {
                int difference = attackerLevel - targetLevel;
                attackerAccuracy += (difference * 5f);
            }
            else if (targetLevel > attackerLevel)
            {
                int difference = targetLevel - attackerLevel;
                attackerAccuracy -= (difference * 5f);
            }

            return UnityEngine.Random.value > (1 - targetDodge / 100) * attackerAccuracy / 100;
        }

        float ResistToElementals(float damage, DamageType damageType)
        {
            switch (damageType)
            {
                case DamageType.Fire:
                    float fireResistance = baseStats.GetStat(CharacterStat.FireResistance);
                    if (fireResistance >= 100) return -damage * (fireResistance - 100) / 100;
                    else if (fireResistance == 0) return damage;
                    else return damage - (damage * fireResistance / 100);

                case DamageType.Water:
                    float waterResistance = baseStats.GetStat(CharacterStat.WaterResistance);
                    if (waterResistance >= 100) return -damage * (waterResistance - 100) / 100;
                    else if (waterResistance == 0) return damage;
                    else return damage - (damage * waterResistance / 100);

                case DamageType.Earth:
                    float earthResistance = baseStats.GetStat(CharacterStat.EarthResistance);
                    if (earthResistance >= 100) return -damage * (earthResistance - 100) / 100;
                    else if (earthResistance == 0) return damage;
                    else return damage - (damage * earthResistance / 100);

                case DamageType.Air:
                    float airResistance = baseStats.GetStat(CharacterStat.AirResistance);
                    if (airResistance >= 100) return -damage * (airResistance - 100) / 100;
                    else if (airResistance == 0) return damage;
                    else return damage - (damage * airResistance / 100);

                case DamageType.Poison:
                    float poisonResistance = baseStats.GetStat(CharacterStat.PoisonResistance);
                    if (poisonResistance >= 100) return -damage * (poisonResistance - 100) / 100;
                    else if (poisonResistance == 0) return damage;
                    else return damage - (damage * poisonResistance / 100);

                default: return damage;
            }
        }

        float DealMagicDamage(float damage, DamageType damageType)
        {
            float magic = magicArmor.GetMagicArmor();
            
            if (damage <= magic)
            {
                magicArmor.ReduseArmor(damage, damageType);
                damage = 0f;
            }
            else
            {
                if (magic > 0)
                {
                    magicArmor.ReduseArmor(magic, damageType);
                }
                damage -= magic;
            }
            return damage;
        }

        float DealPhysicDamage(float damage, DamageType damageType)
        {
            float physic = physicArmor.GetPhysicArmor();
            if (damage <= physic)
            {
                physicArmor.ReduseArmor(damage, damageType);
                damage = 0;
            }
            else
            {
                if (physic > 0)
                {
                    physicArmor.ReduseArmor(physic, damageType);
                }
                damage -= physic;
            }
            return damage;
        }

        void Regenerate()
        {
            if (fightScheduler.IsFightRunning()) return;
            if (IsDead()) return;
            if (!CanRegenerate()) return;

            if (regenerateTimer > regenerateInterval)
            {
                regenerateTimer = 0;
                healthPoints.Value = Mathf.Ceil(Mathf.Min(healthPoints.Value + GetMaxHealth() * regenerateFraction, GetMaxHealth()));
                UpdateHealth();
            }
        }

        void RegenerateFully()
        {
            healthPoints.Value = GetMaxHealth();
            UpdateHealth();
        }

        void UpdateTimers()
        {
            regenerateTimer += Time.deltaTime;
            lastDamageTimer += Time.deltaTime;
        }

        void Die(bool soundNeeded = true)
        {
            if (isDead) return;

            buffStore.ClearBuffStore();

            magicArmor.RemoveArmor();
            physicArmor.RemoveArmor();

            if (playerController != null && controlSwitcher != null && controlSwitcher.GetActivePlayer() == playerController)
            {
                showHideUI.CloseAll();
                if (statusBar != null) statusBar.EnableStatusBar(false);
            }

            isDead = true;
            animationController.Die();
            actionScheduler.CancelCurrentAction();
            iController.EnableAgent(false);

            if (soundNeeded)
            {
                audioController.PlayDeath();
            }

            if (onDeath != null)
            {
                onDeath();
            }
        }

        IEnumerator ResurrectRoutine()
        {
            isDead = false;
            animationController.Ressurect();
            healthPoints.Value = Mathf.Ceil(GetMaxHealth() * resurrectHelthFraction);
            UpdateHealth();

            if (fightScheduler.IsFightRunning())
            {
                yield return new WaitForSeconds(resurrectDelay);
                fightScheduler.AddResurrected(iController);
            }
            else
            {
                yield return new WaitForSeconds(resurrectDelay / 2);
                GetComponent<NavMeshObstacle>().enabled = false;
                yield return new WaitForSeconds(resurrectDelay / 2);
                GetComponent<NavMeshAgent>().enabled = true;

                if (playerController != null)
                {
                    GetComponent<Movement.Mover>().EnableCompanionMode(true);
                }
            }
        }

        void UpdateHealth()
        {
            if (onHealthChange != null)
            {
                onHealthChange();
            }
        }

        void StatsUpdated()
        {
            if (healthPoints.Value > GetMaxHealth())
            {
                healthPoints.Value = GetMaxHealth();
            }
            UpdateHealth();
        }

        [System.Serializable]
        class HealthRecord
        {
            public float healthPoints = 0f;
            public int deathSceneIndex = 0;
        }

        object ISaveable.CaptureState()
        {
            HealthRecord record = new HealthRecord();
            record.healthPoints = healthPoints.Value;
            record.deathSceneIndex = deathSceneIndex;
            return record;
        }

        void ISaveable.RestoreState(object state)
        {
            HealthRecord record = state as HealthRecord;
            healthPoints.Value = record.healthPoints;
            deathSceneIndex = record.deathSceneIndex;

            savedHealthPoints = record.healthPoints;

            if (healthPoints.Value <= 0)
            {
                Die(false);
            }
        }
    }
}