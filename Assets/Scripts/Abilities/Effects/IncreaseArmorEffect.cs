using System;
using UnityEngine;
using Attributes;
using Stats;
using Control;

namespace Abilities
{
    [CreateAssetMenu(fileName = "Increase Armor Effect", menuName = "Abilities/Effects/Increase Armor", order = 0)]
    public class IncreaseArmorEffect : EffectStrategy
    {
        // CONFIG

        [SerializeField] float amount = 20f;
        [SerializeField] CharacterStat armorType = CharacterStat.PhysicArmor;
        [SerializeField] bool modifierAffective = false;

        // PUBLIC

        public override void StartEffect(AbilityData data, Action finished)
        {
            BaseStats baseStats = data.GetUser().GetComponent<BaseStats>();
            float baseAmount = amount;

            if (modifierAffective)
            {
                baseAmount = baseStats.GetStat(CharacterStat.IntelligenceDamage, amount);
            }

            if (armorType == CharacterStat.PhysicArmor)
            {
                foreach (GameObject target in data.GetTargets())
                {
                    target.GetComponent<PhysicArmor>().IncreaseArmor(baseAmount, baseStats, modifierAffective);
                }
            }
            else if (armorType == CharacterStat.MagicArmor)
            {
                foreach (GameObject target in data.GetTargets())
                {
                    target.GetComponent<MagicArmor>().IncreaseArmor(baseAmount, baseStats, modifierAffective);
                }
            }

            foreach (GameObject target in data.GetTargets())
            {
                AIController aiController = target.GetComponent<AIController>();
                if (aiController != null) aiController.Aggrevate();
            }

            finished();
        }

        public override void StartEffect(AbilityData data, GameObject target)
        {
            BaseStats baseStats = data.GetUser().GetComponent<BaseStats>();
            float baseAmount = amount;

            if (modifierAffective)
            {
                baseAmount = baseStats.GetStat(CharacterStat.IntelligenceDamage, amount);
            }

            if (armorType == CharacterStat.PhysicArmor)
            {
                target.GetComponent<PhysicArmor>().IncreaseArmor(baseAmount, baseStats, modifierAffective);
            }
            else if (armorType == CharacterStat.MagicArmor)
            {
                target.GetComponent<MagicArmor>().IncreaseArmor(baseAmount, baseStats, modifierAffective);
            }

            AIController aiController = target.GetComponent<AIController>();
            if (aiController != null) aiController.Aggrevate();
        }
    }
}