using System;
using UnityEngine;
using Attributes;
using Stats;

namespace Abilities
{
    [CreateAssetMenu(fileName = "Heal", menuName = "Abilities/Effects/Heal", order = 0)]
    public class HealEffect : EffectStrategy
    {
        // CONFIG

        [SerializeField] float baseHeal = 10f;
        [SerializeField] bool modifierAffective = false;

        // PUBLIC

        public override void StartEffect(AbilityData data, Action finished)
        {
            BaseStats baseStats = data.GetUser().GetComponent<BaseStats>();
            float heal = baseHeal;

            if (modifierAffective)
            {
                heal = baseStats.GetStat(CharacterStat.IntelligenceDamage, heal);
            }

            foreach (GameObject target in data.GetTargets())
            {
                target.GetComponent<Health>().Heal(heal, baseStats, modifierAffective);
            }
            finished();
        }

        public override void StartEffect(AbilityData data, GameObject target)
        {
            BaseStats baseStats = data.GetUser().GetComponent<BaseStats>();
            float heal = baseHeal;

            if (modifierAffective)
            {
                heal = baseStats.GetStat(CharacterStat.IntelligenceDamage, heal);
            }

            target.GetComponent<Health>().Heal(heal, baseStats, modifierAffective);
        }
    }
}