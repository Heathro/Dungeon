using System;
using UnityEngine;
using Attributes;
using Utils;
using Stats;
using Combat;

namespace Abilities
{
    [CreateAssetMenu(fileName = "Damage", menuName = "Abilities/Effects/Damage", order = 0)]
    public class DamageEffect : EffectStrategy
    {
        // CONFIG

        [SerializeField] bool equippedWeaponDamage = false;
        [SerializeField] float weaponDamageFraction = 1f;
        [SerializeField] DamageType damageType = DamageType.Health;
        [SerializeField] CharacterStat damageBase = CharacterStat.StrengthDamage;
        [SerializeField] float baseDamage = 100f;
        [SerializeField] bool modifierAffective = false;
        [SerializeField] bool directDamage = false;

        // PUBLIC

        public override void StartEffect(AbilityData data, Action finished)
        {   
            BaseStats baseStats = data.GetUser().GetComponent<BaseStats>();

            DamageType correctDamageType = damageType;
            float correctDamage = baseDamage;

            if (equippedWeaponDamage)
            {
                Fighter fighter = data.GetUser().GetComponent<Fighter>();
                correctDamageType = directDamage ? DamageType.Health : fighter.GetDamageType();
                correctDamage = Mathf.Ceil(fighter.GetDamage() * weaponDamageFraction);
            }
            else if (modifierAffective)
            {
                correctDamage = baseStats.GetStat(damageBase, correctDamage);
            }

            foreach (GameObject target in data.GetTargets())
            {
                target.GetComponent<Health>().TakeDamage(correctDamage, correctDamageType, baseStats, modifierAffective, true);
            }
            
            finished();
        }

        public override void StartEffect(AbilityData data, GameObject target)
        {
            BaseStats baseStats = data.GetUser().GetComponent<BaseStats>();

            DamageType correctDamageType = damageType;
            float correctDamage = baseDamage;

            if (equippedWeaponDamage)
            {
                Fighter fighter = data.GetUser().GetComponent<Fighter>();
                correctDamageType = directDamage ? DamageType.Health : fighter.GetDamageType();
                correctDamage = Mathf.Ceil(fighter.GetDamage() * weaponDamageFraction);
            }
            else if (modifierAffective)
            {
                correctDamage = baseStats.GetStat(damageBase, correctDamage);
            }

            target.GetComponent<Health>().TakeDamage(correctDamage, correctDamageType, baseStats, modifierAffective, true);
        }
    }
}