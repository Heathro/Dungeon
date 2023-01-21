using System;
using UnityEngine;
using Audio;
using Combat;

namespace Abilities
{
    [CreateAssetMenu(fileName = "Weapon Sound", menuName = "Abilities/Effects/Weapon Sound", order = 0)]
    public class WeaponSoundEffect : EffectStrategy
    {
        // PUBLIC

        public override void StartEffect(AbilityData data, Action finished)
        {
            data.GetUser().GetComponent<Fighter>().PlayWeaponSound();
            finished();
        }

        public override void StartEffect(AbilityData data, GameObject target)
        {
            data.GetUser().GetComponent<Fighter>().PlayWeaponSound();
        }
    }
}