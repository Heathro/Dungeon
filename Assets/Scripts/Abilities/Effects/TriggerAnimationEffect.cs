using System;
using UnityEngine;
using Animations;

namespace Abilities
{
    [CreateAssetMenu(fileName = "Trigger Animation", menuName = "Abilities/Effects/Trigger Animation", order = 0)]
    public class TriggerAnimationEffect : EffectStrategy
    {
        // CONFIG

        [SerializeField] string trigger = "";
        
        // PUBLIC

        public override void StartEffect(AbilityData data, Action finished)
        {
            data.GetUser().GetComponent<AnimationController>().TriggerAnimation(trigger);
            finished();
        }

        public override void StartEffect(AbilityData data, GameObject target)
        {
            AnimationController animationController = target.GetComponent<AnimationController>();
            if (animationController == null) return;
            
            animationController.TriggerAnimation(trigger);
        }
    }
}