using System;
using System.Collections;
using UnityEngine;
using Control;
using Utils;
using Core;
using Movement;
using Saving;
using Attributes;
using Audio;
using UI;

namespace Animations
{
    public class AnimationController : MonoBehaviour, IAction, ISaveable
    {
        // CONFIG

        [SerializeField] float unsheathTimeLimit = 20f;
        [SerializeField] float pickupDelay = 0.4f;

        // CACHE

        FightScheduler fightScheduler = null;
        Animator animator = null;
        ActionScheduler actionScheduler = null;
        Mover mover = null;
        PlayerController playerController = null;
        Health health = null;
        ControlSwitcher controlSwitcher = null;
        AudioController audioController = null;
        ShowHideUI showHideUI = null;

        // STATE

        WeaponType weaponType = WeaponType.None;
        bool unsheathed = false;
        bool sheating = false;
        bool animationFinished = true;
        float unsheathTime = 0f;
        bool unsheathTimerWorking = true;

        // LIFECYCLE

        void Awake()
        {
            animator = GetComponent<Animator>();
            actionScheduler = GetComponent<ActionScheduler>();
            mover = GetComponent<Mover>();
            playerController = GetComponent<PlayerController>();
            health = GetComponent<Health>();
            controlSwitcher = GetComponentInParent<ControlSwitcher>();
            audioController = GetComponent<AudioController>();
        }

        void Start()
        {
            foreach (UnsheathAnimation unsheathAnimation in animator.GetBehaviours<UnsheathAnimation>())
            {
                unsheathAnimation.SetOnFinishCallBack(OnAnimationFinish);
            }
            foreach (SheathAnimation sheathAnimation in animator.GetBehaviours<SheathAnimation>())
            {
                sheathAnimation.SetOnFinishCallBack(OnAnimationFinish);
            }
            foreach (PickupAnimation pickupAnimation in animator.GetBehaviours<PickupAnimation>())
            {
                pickupAnimation.SetOnFinishCallBack(OnAnimationFinish);
            }
            foreach (HitAnimation hitAnimation in animator.GetBehaviours<HitAnimation>())
            {
                hitAnimation.SetOnFinishCallBack(OnAnimationFinish);
            }

            fightScheduler = FindObjectOfType<FightScheduler>();
            showHideUI = FindObjectOfType<ShowHideUI>();
        }

        void Update()
        {
            if (health.IsDead()) return;
            if (fightScheduler.IsFightRunning()) return;
            if (!unsheathed) return;

            if (unsheathTimerWorking)
            {
                unsheathTime += Time.deltaTime;
            }

            if (unsheathTime > unsheathTimeLimit)
            {
                Sheath();
            }
        }

        // PUBLIC

        public void SetUnsheathTimerActive(bool isWorking)
        {
            unsheathTimerWorking = isWorking;
        }

        public void ResetUnsheathTime()
        {
            unsheathTime = 0f;
        }

        public void SetWeaponType(WeaponType weaponType)
        {
            this.weaponType = weaponType;
        }

        public bool IsUnsheated()
        {
            return unsheathed;
        }

        public bool IsSheating()
        {
            return sheating;
        }

        public void ToggleUnsheath()
        {
            showHideUI.GetAudioHub().PlayClick();

            if (showHideUI.IsSinglePlayerActionRunning()) return;
            if (health.IsDead()) return;

            playerController.DisableTargetMarker();
            playerController.EnableControl(false);

            if (unsheathed)
            {
                Sheath(() => playerController.EnableControl(true));
            }
            else
            {
                Unsheath(() => playerController.EnableControl(true));
            }
        }

        public void Unsheath(Action finished)
        {
            if (unsheathed)
            {
                finished();
            }
            else
            {
                fightScheduler.StartCoroutine(UnsheathRoutine(finished));
            }
        }

        public void UnsheathForAbility(Action finished)
        {
            if (unsheathed)
            {
                finished();
            }
            else
            {
                fightScheduler.StartCoroutine(UnsheathForAbilityRoutine(finished));
            }
        }

        public IEnumerator UnsheathForAbilityRoutine(Action finished)
        {
            if (!animationFinished) yield break;
            animationFinished = false;

            unsheathTime = 0f;
            unsheathed = true;
            sheating = false;

            if (weaponType == WeaponType.OneHandAxe || weaponType == WeaponType.OneHandDagger ||
                weaponType == WeaponType.OneHandHammer || weaponType == WeaponType.OneHandMace ||
                weaponType == WeaponType.OneHandSword || weaponType == WeaponType.OneHandWand)
            {
                animator.ResetTrigger("armed unsheath");
                animator.SetTrigger("armed unsheath");
                audioController.PlaySheath(true);
            }
            else
            {
                animator.ResetTrigger(GetTriggerPrefix(weaponType) + "unsheath");
                animator.SetTrigger(GetTriggerPrefix(weaponType) + "unsheath");
                if (weaponType != WeaponType.Unarmed) audioController.PlaySheath(true);
            }

            while (animationFinished == false)
            {
                yield return null;
            }         
            finished();
        }

        public void Sheath(Action finished)
        {
            if (!unsheathed)
            {
                finished();
            }
            else
            {
                fightScheduler.StartCoroutine(SheathRoutine(finished));
            }
        }

        public void Unsheath()
        {
            if (!animationFinished) return;
            animationFinished = false;

            actionScheduler.StartAction(this);
            mover.CancelAction();

            unsheathTime = 0f;
            unsheathed = true;
            sheating = false;

            if (weaponType == WeaponType.OneHandAxe || weaponType == WeaponType.OneHandDagger ||
                weaponType == WeaponType.OneHandHammer || weaponType == WeaponType.OneHandMace ||
                weaponType == WeaponType.OneHandSword || weaponType == WeaponType.OneHandWand)
            {
                animator.ResetTrigger("armed unsheath");
                animator.SetTrigger("armed unsheath");
                audioController.PlaySheath(true);
            }
            else
            {
                animator.ResetTrigger(GetTriggerPrefix(weaponType) + "unsheath");
                animator.SetTrigger(GetTriggerPrefix(weaponType) + "unsheath");
                if (weaponType != WeaponType.Unarmed) audioController.PlaySheath(true);
            }
        }

        public void Sheath()
        {
            if (!animationFinished) return;
            animationFinished = false;
            
            actionScheduler.StartAction(this);
            mover.CancelAction();

            unsheathed = false;
            sheating = true;

            animator.ResetTrigger("sheath");
            animator.SetTrigger("sheath");
            if (weaponType != WeaponType.Unarmed) audioController.PlaySheath(false);
        }

        public void Attack()
        {
            if (weaponType == WeaponType.OneHandAxe || weaponType == WeaponType.OneHandDagger ||
                weaponType == WeaponType.OneHandHammer || weaponType == WeaponType.OneHandMace ||
                weaponType == WeaponType.OneHandSword || weaponType == WeaponType.OneHandWand)
            {
                animator.ResetTrigger("stopAttack");
                animator.SetTrigger(GetTriggerPrefix(weaponType) + "attack");
            }
            else
            {
                animator.ResetTrigger("stopAttack");
                animator.SetTrigger("attack");
            }
        }

        public void StopAttack()
        {
            animator.ResetTrigger("attack");
            animator.SetTrigger("stopAttack");
        }

        public void Die()
        {
            if (!unsheathed)
            {
                animator.ResetTrigger("relax death");
                animator.SetTrigger("relax death");
            }
            else if (weaponType == WeaponType.OneHandAxe || weaponType == WeaponType.OneHandDagger ||
                     weaponType == WeaponType.OneHandHammer || weaponType == WeaponType.OneHandMace ||
                     weaponType == WeaponType.OneHandSword || weaponType == WeaponType.OneHandWand)
            {
                animator.ResetTrigger("armed death");
                animator.SetTrigger("armed death");
            }
            else
            {
                animator.ResetTrigger(GetTriggerPrefix(weaponType) + "death");
                animator.SetTrigger(GetTriggerPrefix(weaponType) + "death");
            }
        }

        public void Cast(string trigger)
        {
            if (!unsheathed)
            {
                animator.ResetTrigger("relax " + trigger);
                animator.SetTrigger("relax " + trigger);
            }
            else if (weaponType == WeaponType.OneHandAxe || weaponType == WeaponType.OneHandDagger ||
                     weaponType == WeaponType.OneHandHammer || weaponType == WeaponType.OneHandMace ||
                     weaponType == WeaponType.OneHandSword || weaponType == WeaponType.OneHandWand)
            {
                animator.ResetTrigger("armed " + trigger);
                animator.SetTrigger("armed " + trigger);
            }
            else
            {
                animator.ResetTrigger(GetTriggerPrefix(weaponType) + trigger);
                animator.SetTrigger(GetTriggerPrefix(weaponType) + trigger);
            }
        }

        public void Pickup(Action finished)
        {
            StartCoroutine(PickupRoutine(finished));
        }

        public void GetHit()
        {
            if (!animationFinished) return;

            StartCoroutine(GetHitRoutine());
        }

        public void Ressurect()
        {
            animator.ResetTrigger("ressurect");
            animator.SetTrigger("ressurect");
            ResetUnsheathTime();
        }

        public bool IsAnimationFinished()
        {
            return animationFinished;
        }

        public void CancelAction()
        {
            
        }

        public void TriggerAnimation(string trigger)
        {
            if (trigger == "attack")
            {
                Attack();
            }
            else if (trigger == "cast" || trigger == "buff")
            {
                Cast(trigger);
            }
        }

        // PRIVATE

        IEnumerator UnsheathRoutine(Action finished)
        {
            playerController.EnableControl(false);
            Unsheath();
            while (animationFinished == false)
            {
                yield return null;
            }
            playerController.EnableControl(true);
            finished();
        }

        IEnumerator SheathRoutine(Action finished)
        {
            playerController.EnableControl(false);
            Sheath();
            while (animationFinished == false)
            {
                yield return null;
            }
            playerController.EnableControl(true);
            finished();
        }

        IEnumerator PickupRoutine(Action finished)
        {
            animationFinished = false;

            if (!unsheathed)
            {
                animator.ResetTrigger("relax pickup");
                animator.SetTrigger("relax pickup");
            }
            else if (weaponType == WeaponType.OneHandAxe || weaponType == WeaponType.OneHandDagger ||
                     weaponType == WeaponType.OneHandHammer || weaponType == WeaponType.OneHandMace ||
                     weaponType == WeaponType.OneHandSword || weaponType == WeaponType.OneHandWand)
            {
                animator.ResetTrigger("armed pickup");
                animator.SetTrigger("armed pickup");
            }
            else
            {
                animator.ResetTrigger(GetTriggerPrefix(weaponType) + "pickup");
                animator.SetTrigger(GetTriggerPrefix(weaponType) + "pickup");
            }

            yield return new WaitForSeconds(pickupDelay);
            finished();

            while (animationFinished == false)
            {
                yield return null;
            }

            playerController.EnableControl(true);
        }

        IEnumerator GetHitRoutine()
        {
            animationFinished = false;

            if (playerController != null) playerController.EnableControl(false);

            if (!unsheathed)
            {
                animator.ResetTrigger("relax hit");
                animator.SetTrigger("relax hit");
            }
            else if (weaponType == WeaponType.OneHandAxe || weaponType == WeaponType.OneHandDagger ||
                     weaponType == WeaponType.OneHandHammer || weaponType == WeaponType.OneHandMace ||
                     weaponType == WeaponType.OneHandSword || weaponType == WeaponType.OneHandWand)
            {
                animator.ResetTrigger("armed hit");
                animator.SetTrigger("armed hit");
            }
            else
            {
                animator.ResetTrigger(GetTriggerPrefix(weaponType) + "hit");
                animator.SetTrigger(GetTriggerPrefix(weaponType) + "hit");
            }

            while (animationFinished == false)
            {
                yield return null;
            }

            if (playerController != null && controlSwitcher.GetActivePlayer() == playerController && !fightScheduler.IsFightRunning())
            {
                playerController.EnableControl(true);
            }

            if (fightScheduler.IsFightRunning() && !unsheathed) Unsheath();
        }

        void OnAnimationFinish()
        {
            animationFinished = true;
        }

        string GetTriggerPrefix(WeaponType weaponType)
        {
            switch (weaponType)
            {
                case WeaponType.Unarmed: return "unarmed ";
                case WeaponType.TwoHandAxe: return "2HandAxe ";
                case WeaponType.TwoHandBow: return "2HandBow ";
                case WeaponType.TwoHandCrossbow: return "2HandCrossbow ";
                case WeaponType.TwoHandSpear: return "2HandSpear ";
                case WeaponType.TwoHandStaff: return "2HandStaff ";
                case WeaponType.TwoHandSword: return "2HandSword ";
                case WeaponType.TwoHandHammer: return "2HandHammer ";
                case WeaponType.TwoHandMace: return "2HandMace ";
                case WeaponType.OneHandAxe: return "1HandAxe ";
                case WeaponType.OneHandDagger: return "1HandDagger ";
                case WeaponType.OneHandHammer: return "1HandHammer ";
                case WeaponType.OneHandMace: return "1HandMace ";
                case WeaponType.OneHandSword: return "1HandSword ";
                case WeaponType.OneHandWand: return "1HandWand ";
                default: return "unarmed ";
            }
        }

        object ISaveable.CaptureState()
        {
            return unsheathed;
        }

        void ISaveable.RestoreState(object state)
        {
            unsheathed = (bool)state;
            if (unsheathed)
            {
                Unsheath();
                ResetUnsheathTime();
            }
        }
    }
}