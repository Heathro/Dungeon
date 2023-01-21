using System;
using UnityEngine;
using Control;

namespace Abilities
{
    [CreateAssetMenu(fileName = "Self Targeting", menuName = "Abilities/Targeting/Self", order = 0)]
    public class SelfTargeting : TargetingStrategy
    {
        // PUBLIC

        public override void StartTargeting(AbilityData data, Action finished)
        {
            data.SetTargets(new GameObject[] { data.GetUser() });
            data.SetTargetPoint(data.GetUser().transform.position);
            data.GetUser().GetComponent<PlayerController>().DisableAPDisplay();
            finished();
        }
    }
}