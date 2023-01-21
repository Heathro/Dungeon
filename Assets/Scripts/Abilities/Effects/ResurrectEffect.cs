using System;
using UnityEngine;
using Attributes;

namespace Abilities
{
    [CreateAssetMenu(fileName = "Resurrect Effect", menuName = "Abilities/Effects/Resurrect", order = 0)]
    public class ResurrectEffect : EffectStrategy
    {
        // PUBLIC

        public override void StartEffect(AbilityData data, Action finished)
        {
            foreach (GameObject target in data.GetTargets())
            {
                target.GetComponent<Health>().Resurrect();
            }
            finished();
        }

        public override void StartEffect(AbilityData data, GameObject target)
        {
            Health health = target.GetComponent<Health>();
            if (health == null) return;

            health.Resurrect();
        }
    }
}