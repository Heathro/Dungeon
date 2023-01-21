using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inventories;
using Control;
using Core;
using Skills;
using Stats;
using Animations;
using Utils;
using Combat;
using Attributes;

namespace Abilities
{
    [CreateAssetMenu(fileName = "Ability", menuName = "Abilities/Ability", order = 0)]
    public class Ability : ActionItem
    {
        // CONFIG

        [SerializeField] Ability skill = null;
        [SerializeField] CharacterClass skillClass = CharacterClass.None;

        [SerializeField] bool isScaleable = false;
        [SerializeField] CharacterStat damageBase = CharacterStat.IntelligenceDamage;

        [SerializeField] int cost = 2;
        [SerializeField] float cooldown = 0f;
        [SerializeField] float distance = 5f;
        [SerializeField] bool mustUnsheath = false;
        [SerializeField] bool requiresMeleeWeapon = false;
        [SerializeField] bool requiresRangedWeapon = false;

        [SerializeField] DamageType popUpTip = DamageType.None;

        [SerializeField] TargetingStrategy targeting;
        [SerializeField] FilterStrategy[] filters;
        [SerializeField] FilterStrategy[] alternativeFilters;

        [SerializeField] EffectStrategy[] targetingEffects;
        [SerializeField] EffectStrategy[] afterUseEffects;

        [SerializeField] DelayedEffectStrategy[] delayedEffects;

        [System.Serializable]
        class DelayedEffectStrategy
        {
            public float delay = 0f;
            public EffectStrategy effect;
        }

        // PUBLIC

        public CharacterClass GetClass()
        {
            return skillClass;
        }

        public override bool Use(GameObject user, GameObject target)
        {
            CooldownStore cooldownStore = user.GetComponent<CooldownStore>();
            if (cooldownStore.GetTimeRemaining(this) > 0)
            {
                return false;
            }

            AIController AIController = user.GetComponent<AIController>();
            if (AIController.GetActionPoints() < cost)
            {
                return false;
            }

            AbilityData data = new AbilityData(user);
            data.SetDistance(distance);
            data.SetCost(cost);
            user.GetComponent<ActionScheduler>().StartAction(data);

            StartTargetingEffects(data);

            AIController.UseActionPoints(cost);

            if (popUpTip != DamageType.None) data.GetUser().GetComponent<Health>().SpawnOverheadTip(popUpTip, GetDisplayName());

            StartCooldown(data);

            if (targeting is SelfTargeting)
            {
                data.SetTargets(new GameObject[] { data.GetUser() });
                data.SetTargetPoint(data.GetUser().transform.position);
            } 
            else if (targeting is AroundTargeting)
            {
                targeting.StartTargeting(data, AITargetingFinished);
                data.SetTargetPoint(target.transform.position);
            }
            else
            {
                data.SetTargets(new GameObject[] { target });
                data.SetTargetPoint(target.transform.position);
            }

            FilterTargets(data);
            FilterMissed(data);

            StartAfterUseEffects(data);
            StartDelayedEffects(data);

            return true;
        }
        
        public override bool Use(GameObject user, Action consumeItem)
        {
            if (requiresMeleeWeapon || requiresRangedWeapon)
            {
                Equipment equipment = user.GetComponent<Equipment>();
                WeaponConfig weapon = equipment.GetItemInSlot(EquipLocation.Weapon) as WeaponConfig;
                if (weapon == null) return false;

                if (requiresMeleeWeapon && !weapon.IsMelee()) return false;
                if (requiresRangedWeapon && !weapon.IsRanged()) return false;
            }

            if (skill != null && user.GetComponent<SkillStore>().HasSkill(skill))
            {
                return false;
            }

            if (skill != null && user.GetComponent<BaseStats>().GetClass() != skillClass)
            {
                return false;
            }

            CooldownStore cooldownStore = user.GetComponent<CooldownStore>();
            if (cooldownStore.GetTimeRemaining(this) > 0)
            {
                return false;
            }

            PlayerController playerController = user.GetComponent<PlayerController>();
            if (playerController.IsTakingTurn() && playerController.GetActionPoints() < cost)
            {
                return false;
            }

            ControlSwitcher controlSwitcher = playerController.GetComponentInParent<ControlSwitcher>();
            controlSwitcher.DisableLeaderPositionUpdate();

            AbilityData data = new AbilityData(user);
            data.SetDistance(distance);
            data.SetCost(cost);
            user.GetComponent<ActionScheduler>().StartAction(data);

            playerController.EnableControl(false);
            playerController.EnableTargetSystem(false);

            StartTargetingEffects(data);

            AnimationController animationController = playerController.GetComponent<AnimationController>();
            animationController.ResetUnsheathTime();
            animationController.SetUnsheathTimerActive(false);

            targeting.StartTargeting(data, () =>
            {
                if (mustUnsheath && !data.IsCancelled())
                {
                    animationController.UnsheathForAbility(() => TargetAquired(data, consumeItem));
                }
                else
                {
                    TargetAquired(data, consumeItem);
                }
            });
            return true;
        }

        public int GetCost()
        {
            return cost;
        }

        public float GetCooldown()
        {
            return cooldown;
        }

        public float GetDistance()
        {
            return distance;
        }

        public bool IsScaleable()
        {
            return isScaleable;
        }

        public Ability GetTeachSkill()
        {
            return skill;
        }

        public bool IsMeleeRequired()
        {
            return requiresMeleeWeapon;
        }

        public bool IsRangedRequired()
        {
            return requiresRangedWeapon;
        }

        public string GetScaleWith()
        {
            switch (damageBase)
            {
                case CharacterStat.StrengthDamage: return "Strength";
                case CharacterStat.FinesseDamage: return "Finesse";
                case CharacterStat.IntelligenceDamage: return "Intelligence";
                default: return "Strength";
            }
        }

        // PRIVATE

        void TargetAquired(AbilityData data, Action consumeItem)
        {
            PlayerController playerController = data.GetUser().GetComponent<PlayerController>();
            playerController.GetComponent<AnimationController>().SetUnsheathTimerActive(true);

            if (data.IsCancelled()) return;

            if (consumeItem != null) consumeItem();

            if (playerController.IsTakingTurn()) playerController.UseActionPoints(cost);

            StartCooldown(data);

            FilterTargets(data);
            FilterMissed(data);

            StartAfterUseEffects(data);
            StartDelayedEffects(data);

            playerController.DisableAPDisplay();
            playerController.SetSimpleCursor();
        }

        void StartTargetingEffects(AbilityData data)
        {
            foreach (EffectStrategy effect in targetingEffects)
            {
                effect.StartEffect(data, TargetingEffectFinished);
            }
        }

        void StartCooldown(AbilityData data)
        {
            CooldownStore cooldownStore = data.GetUser().GetComponent<CooldownStore>();
            cooldownStore.StartCooldown(this, cooldown);
        }

        void FilterTargets(AbilityData data)
        {
            if (data.GetUser().GetComponent<IController>().IsEnemy())
            {
                if (data.GetUser().GetComponent<BuffStore>().IsCharmed())
                {
                    foreach (FilterStrategy filter in filters)
                    {
                        data.SetTargets(filter.Filter(data.GetTargets()));
                    }
                }
                else
                {
                    foreach (FilterStrategy filter in alternativeFilters)
                    {
                        data.SetTargets(filter.Filter(data.GetTargets()));
                    }
                }
            }   
            else
            {
                foreach (FilterStrategy filter in filters)
                {
                    data.SetTargets(filter.Filter(data.GetTargets()));
                }
            }
        }

        void FilterMissed(AbilityData data)
        {
            var inputTargets = data.GetTargets();

            List<GameObject> hit = new List<GameObject>();
            List<GameObject> miss = new List<GameObject>();

            foreach (GameObject target in inputTargets)
            {
                if (object.ReferenceEquals(data.GetUser(), target))
                {
                    hit.Add(target);
                }
                else if (AttemptDodge(data.GetUser().GetComponent<BaseStats>(), target.GetComponent<BaseStats>()))
                {
                    miss.Add(target);
                }
                else
                {
                    hit.Add(target);
                }
            }

            data.SetTargets(hit);
            data.SetMissedTargets(miss);
        }

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

        void StartAfterUseEffects(AbilityData data)
        {
            foreach (EffectStrategy effect in afterUseEffects)
            {
                effect.StartEffect(data, AfterUseEffectFinished);
            }
        }

        void StartDelayedEffects(AbilityData data)
        {
            foreach (DelayedEffectStrategy effect in delayedEffects)
            {
                data.StartCoroutine(DelayedLaunch(data, effect));
            }
        }

        IEnumerator DelayedLaunch(AbilityData data, DelayedEffectStrategy delayedEffect)
        {
            yield return new WaitForSeconds(delayedEffect.delay);
            delayedEffect.effect.StartEffect(data, DelayedEffectFinished);
        }

        void TargetingEffectFinished()
        {

        }

        void AfterUseEffectFinished()
        {

        }

        void DelayedEffectFinished()
        {
            
        }

        void AITargetingFinished()
        {
            
        }
    }
}