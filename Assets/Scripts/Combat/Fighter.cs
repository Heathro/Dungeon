using System;
using System.Collections;
using UnityEngine;
using Core;
using Movement;
using Animations;
using Saving;
using Attributes;
using Stats;
using Utils;
using Inventories;
using Control;

namespace Combat
{
    public class Fighter : MonoBehaviour, IAction, ISaveable
    {
        // CONFIG
        
        [SerializeField] Transform rightHand = null;
        [SerializeField] Transform leftHand = null;
        [SerializeField] WeaponConfig defaultWeapon = null;
        [SerializeField] WeaponConfig unarmedConfig = null;

        // CACHE

        ActionScheduler actionScheduler;
        AnimationController animationController;
        Animator animator;
        Mover mover;
        Health target;
        BaseStats baseStats;
        Equipment equipment;
        Randomizer randomizer;

        // STATE

        public event Action onWeaponChange;
        public event Action onWeaponSwitchStart;
        public event Action onWeaponSwitchEnd;

        bool nonSkillAttackAnimation = false;
        bool shouldAttack = false;
        bool fightMode = false;
        int actionPoints = 0;
        Action<int> useActionPoints = null;
        Action actionFinished = null;
        WeaponConfig currentWeaponConfig;
        LazyValue<Weapon> currentWeapon;

        Action<float> setMileage = null;
        float mileage = 0f;

        GameObject cachedTarget = null;
        bool movementStarted = false;
        bool attackStarted = false;
        bool animationFinished = false;
        bool updatingWeapon = false;

        Action leaderControlEnable = null;

        // LIFECYCLE

        void Awake()
        {            
            actionScheduler = GetComponent<ActionScheduler>();

            animator = GetComponent<Animator>();
            animationController = GetComponent<AnimationController>();

            mover = GetComponent<Mover>();
            baseStats = GetComponent<BaseStats>();

            currentWeaponConfig = defaultWeapon;
            currentWeapon = new LazyValue<Weapon>(SetupDefaultWeapon);

            equipment = GetComponent<Equipment>();
            if (equipment != null)
            {
                equipment.weaponUpdated += UpdateWeapon;
            }
        }

        void Start()
        {
            currentWeapon.ForceInit();
            randomizer = FindObjectOfType<Randomizer>();

            foreach (AttackAnimation attackAnimation in animator.GetBehaviours<AttackAnimation>())
            {
                attackAnimation.SetAnimationCallback(OnAnimationFinish);
            }
        }

        void Update()
        {
            if (attackStarted && !shouldAttack && fightMode && animationFinished)
            {
                FinishFightMode();
            }

            if (!shouldAttack) return;
            if (target == null) return;
            if (target.IsDead()) return;

            if (fightMode)
            {
                FightBehaviour();
            }
            else
            {
                CivilBehaviour();
            }
        }

        // PUBLIC

        public RangedProjectileType GetProjectileType()
        {
            return currentWeaponConfig.GetProjectileType();
        }

        public void Unsheath()
        {
            if (animationController.IsUnsheated()) return;
            animationController.Unsheath();
        }

        public void Sheath()
        {
            if (!animationController.IsUnsheated()) return;
            animationController.Sheath();
        }

        public void SetLeaderControlEnable(Action leaderControlEnable)
        {
            this.leaderControlEnable = leaderControlEnable;
        }

        public bool CanAttack(GameObject combatTarget)
        {
            if (combatTarget == null) return false;
            Health candidate = combatTarget.GetComponent<Health>();
            return candidate != null && !candidate.IsDead();
        }

        public void StartAttackAction(GameObject combatTarget, bool friendlyAttack)
        {
            if (leaderControlEnable != null && !friendlyAttack)
            {
                leaderControlEnable();
            }

            fightMode = false;

            shouldAttack = true;
            actionScheduler.StartAction(this);
            target = combatTarget.GetComponent<Health>();
        }

        public void StartAttackAction(GameObject combatTarget, int actionPoints, Action<int> useActionPoints, Action actionFinished, float mileage, Action<float> setMileage)
        {
            fightMode = true;
            movementStarted = false;
            attackStarted = false;

            shouldAttack = true;
            actionScheduler.StartAction(this);
            target = combatTarget.GetComponent<Health>();

            this.actionPoints = actionPoints;
            this.useActionPoints = useActionPoints;
            this.actionFinished = actionFinished;
            this.mileage = mileage;
            this.setMileage = setMileage;

            cachedTarget = combatTarget;
        }

        public void CancelAction()
        {
            mover.MoveTo(transform.position);
            shouldAttack = false;
            animationController.StopAttack();
            target = null;
        }

        public int GetAttackPrice()
        {
            return currentWeaponConfig.GetCost();
        }

        public float GetAttackRange()
        {
            return currentWeaponConfig.GetRange();
        }

        public bool IsMelee()
        {
            return currentWeaponConfig.IsMelee();
        }

        public void EquipWeapon(WeaponConfig weapon)
        {
            currentWeaponConfig = weapon;
            currentWeapon.Value = AttachWeapon(weapon);
        }

        public Transform GetCastingHandTransform()
        {
            return currentWeaponConfig.IsRightHanded() ? leftHand : rightHand;
        }

        public Transform GetShootHandTransform()
        {
            return leftHand;
        }

        public Randomizer GetRandomizer()
        {
            return randomizer;
        }

        public float GetDamage()
        {
            return Mathf.Ceil(baseStats.GetStat(currentWeaponConfig.GetDamageBase(), currentWeaponConfig.GetDamage()));
        }

        public DamageType GetDamageType()
        {
            return currentWeaponConfig.GetDamageType();
        }

        public CharacterStat GetDamageBase()
        {
            return currentWeaponConfig.GetDamageBase();
        }

        public bool IsUpdating()
        {
            return updatingWeapon;
        }

        public void PlayWeaponSound()
        {
            currentWeapon.Value.OnHit();
        }

        // PRIVATE

        void OnAnimationFinish()
        {
            animationFinished = true;
        }

        void UpdateWeapon()
        {
            if (updatingWeapon) return;
            updatingWeapon = true;
            StartCoroutine(WeaponUpdating());
        }

        IEnumerator WeaponUpdating()
        {
            yield return new WaitForEndOfFrame();

            WeaponConfig weaponConfig = equipment.GetItemInSlot(EquipLocation.Weapon) as WeaponConfig;

            if (onWeaponChange != null)
            {
                onWeaponChange();
            }

            if (animationController.IsUnsheated())
            {
                SwapWeaponRoutine(weaponConfig);
            }
            else
            {
                SwapImmidiatly(weaponConfig);
            }
        }

        void SwapImmidiatly(WeaponConfig weaponConfig)
        {   
            actionScheduler.StartAction(this);
            mover.CancelAction();

            if (weaponConfig == null)
            {
                EquipWeapon(unarmedConfig);
            }
            else
            {
                EquipWeapon(weaponConfig);
            }

            updatingWeapon = false;
        }

        void SwapWeaponRoutine(WeaponConfig weaponConfig)
        {
            animationController.ResetUnsheathTime();

            WeaponConfig cachedConfig = weaponConfig;

            if (onWeaponSwitchStart != null)
            {
                onWeaponSwitchStart();
            }

            animationController.Sheath(() =>
            {
                if (weaponConfig == null)
                {
                    EquipWeapon(unarmedConfig);
                }
                else
                {
                    EquipWeapon(weaponConfig);
                }

                animationController.Unsheath(() =>
                {
                    if (onWeaponSwitchEnd != null)
                    {
                        onWeaponSwitchEnd();
                    }

                    WeaponConfig currentConfig = equipment.GetItemInSlot(EquipLocation.Weapon) as WeaponConfig;

                    updatingWeapon = false;
                    if (currentConfig != cachedConfig)
                    {
                        UpdateWeapon();
                    }
                });
            });
        }

        Weapon SetupDefaultWeapon()
        {
            return AttachWeapon(defaultWeapon);
        }

        Weapon AttachWeapon(WeaponConfig weapon)
        {
            animationController.SetWeaponType(currentWeaponConfig.GetWeaponType());
            return weapon.SpawnWeapon(rightHand, leftHand, animator);
        }

        void FightBehaviour()
        {
            if (IsTargetInRange())
            {
                PerformAttack(actionPoints, mileage);
            }
            else if (!movementStarted)
            {
                movementStarted = true;
                mover.StartMoveAction(target.transform.position, actionPoints, useActionPoints, null, PerformAttack, mileage, setMileage, currentWeaponConfig.GetRange());
            }
        }

        void CivilBehaviour()
        {
            if (IsTargetInRange())
            {
                mover.CancelAction();
                Attack();
            }
            else
            {
                mover.MoveTo(target.transform.position);
            }
        }

        void FinishFightMode()
        {
            fightMode = false;
            actionPoints = 0;
            setMileage(mileage);
            movementStarted = false;
            actionFinished();
        }

        void PerformAttack(int actionPoints, float mileage)
        {
            this.actionPoints = actionPoints;
            this.mileage = mileage;
            shouldAttack = true;

            if (this.actionPoints <= 0 || this.actionPoints < currentWeaponConfig.GetCost())
            {
                FinishFightMode();
                return;
            }

            this.actionPoints -= currentWeaponConfig.GetCost();
            useActionPoints(currentWeaponConfig.GetCost());

            target = cachedTarget.GetComponent<Health>();
            Attack();
        }

        void Attack()
        {
            shouldAttack = false;
            animationFinished = false; 

            if (!IsTargetInRange())
            {
                FinishFightMode();
                return;
            }
            attackStarted = true;
            transform.LookAt(target.transform);

            nonSkillAttackAnimation = true;
            animationController.Attack();
        }

        void MakeWeaponAttack()
        {
            if (target == null) return;

            PlayWeaponSound();

            DamageType damageType = currentWeaponConfig.GetDamageType();
            float damage = GetDamage();

            if (currentWeaponConfig.HasProjectile())
            {
                currentWeaponConfig.LaunchProjectile(target, damage, baseStats, rightHand, leftHand);
            }
            else
            {
                target.TakeDamage(damage, damageType, baseStats, true);
            }
        }

        void Hit()
        {
            if (nonSkillAttackAnimation)
            {
                MakeWeaponAttack();
                nonSkillAttackAnimation = false;
            }
        }

        void Shoot()
        {
            if (nonSkillAttackAnimation)
            {
                MakeWeaponAttack();
                nonSkillAttackAnimation = false;
            }
        }

        void WeaponSwitch()
        {
            foreach (Transform equip in rightHand)
            {
                equip.gameObject.SetActive(!animationController.IsSheating());
            }
            foreach (Transform equip in leftHand)
            {
                equip.gameObject.SetActive(!animationController.IsSheating());
            }
        }

        bool IsTargetInRange()
        {
            return Vector3.Distance(transform.position, target.transform.position) <= currentWeaponConfig.GetRange();
        }

        object ISaveable.CaptureState()
        {
            return currentWeaponConfig.GetItemID();
        }

        void ISaveable.RestoreState(object state)
        {
            WeaponConfig[] weaponList = Resources.LoadAll<WeaponConfig>("");

            foreach (WeaponConfig weapon in weaponList)
            {
                if (weapon.GetItemID() == (string)state)
                {
                    EquipWeapon(weapon);
                }
            }
        }
    }
}