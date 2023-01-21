using System;
using UnityEngine;
using Skills;

namespace Abilities
{
    [CreateAssetMenu(fileName = "Teach Skill", menuName = "Abilities/Effects/Teach Skill", order = 0)]
    public class LearnSkillEffect : EffectStrategy
    {
        // CONFIG

        [SerializeField] Ability skill = null;

        // PUBLIC

        public override void StartEffect(AbilityData data, Action finished)
        {
            data.GetUser().GetComponent<SkillStore>().AddSkill(skill);
            finished();
        }

        public override void StartEffect(AbilityData data, GameObject target)
        {
            SkillStore skillStore = target.GetComponent<SkillStore>();
            if (skillStore == null) return;
            
            skillStore.AddSkill(skill);
        }
    }
}