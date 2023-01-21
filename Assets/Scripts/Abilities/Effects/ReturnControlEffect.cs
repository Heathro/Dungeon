using System;
using UnityEngine;
using Control;

namespace Abilities
{
    [CreateAssetMenu(fileName = "Return Control", menuName = "Abilities/Effects/Return Control", order = 0)]
    public class ReturnControlEffect : EffectStrategy
    {
        // PUBLIC

        public override void StartEffect(AbilityData data, Action finished)
        {
            if (!data.IsCancelled())
            {
                PlayerController playerController = data.GetUser().GetComponent<PlayerController>();
                if (playerController != null)
                {                
                    playerController.EnableControl(true);
                    playerController.EnableTargetSystem(true);
                    playerController.SetTargeting(false);
                    playerController.ActionFinished();
                }

            }
            
            finished();
        }

        public override void StartEffect(AbilityData data, GameObject target)
        {

        }
    }
}