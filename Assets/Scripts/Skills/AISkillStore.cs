using UnityEngine;
using Abilities;

namespace Skills
{
    public class AISkillStore : MonoBehaviour
    {
        // CONFIG

        [SerializeField] Ability healingSpell;
        [SerializeField] Ability[] damagingSpells;
        [SerializeField] Ability[] buffs;

        // STATE

        CooldownStore cooldownStore = null;

        // LIFECYCLE

        void Awake()
        {
            cooldownStore = GetComponent<CooldownStore>();
        }

        // PUBLIC

        public Ability GetHealingSpell()
        {
            return healingSpell;
        }

        public Ability GetAvailableDamagingSpell()
        {
            foreach (Ability damagingSpell in damagingSpells)
            {
                if (cooldownStore.GetTimeRemaining(damagingSpell) > 0) continue;

                return damagingSpell;
            }

            return null;
        }

        public Ability GetAvailableBuff()
        {
            foreach (Ability buff in buffs)
            {
                if (cooldownStore.GetTimeRemaining(buff) > 0) continue;

                return buff;
            }

            return null;
        }
    }
}