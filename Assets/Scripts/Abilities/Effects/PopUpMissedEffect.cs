using System;
using UnityEngine;
using UI.Ingame;
using Utils;
using Control;

namespace Abilities
{
    [CreateAssetMenu(fileName = "Pop Up Missings", menuName = "Abilities/Effects/Pop Up Missings", order = 0)]
    public class PopUpMissedEffect : EffectStrategy
    {
        // PUBLIC

        public override void StartEffect(AbilityData data, Action finished)
        {
            foreach (GameObject missedTarget in data.GetMissedTargets())
            {
                missedTarget.GetComponentInChildren<OverheadUI>().AddPopUp(0f, DamageType.Miss, false, "Miss!");

                AIController aiController = missedTarget.GetComponent<AIController>();
                if (aiController != null) aiController.Aggrevate();
            }

            finished();
        }

        public override void StartEffect(AbilityData data, GameObject target)
        {
            target.GetComponentInChildren<OverheadUI>().AddPopUp(0f, DamageType.Miss, false, "Miss!");

            AIController aiController = target.GetComponent<AIController>();
            if (aiController != null) aiController.Aggrevate();
        }
    }
}