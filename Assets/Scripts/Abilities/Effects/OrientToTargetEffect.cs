using System;
using UnityEngine;

namespace Abilities
{
    [CreateAssetMenu(fileName = "Orient To Target", menuName = "Abilities/Effects/Orient To Target", order = 0)]
    public class OrientToTargetEffect : EffectStrategy
    {
        // PUBLIC

        public override void StartEffect(AbilityData data, Action finished)
        {
            data.GetUser().transform.LookAt(data.GetTargetPoint());
            finished();
        }

        public override void StartEffect(AbilityData data, GameObject target)
        {

        }
    }
}