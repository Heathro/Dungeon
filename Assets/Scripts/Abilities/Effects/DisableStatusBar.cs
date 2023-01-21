using System;
using System.Collections;
using UnityEngine;
using Control;

namespace Abilities
{
    [CreateAssetMenu(fileName = "Disable StatusBar", menuName = "Abilities/Effects/Disable StatusBar", order = 0)]
    public class DisableStatusBar : EffectStrategy
    {
        // CONFIG

        [SerializeField] float delay = 0.05f;

        // PUBLIC

        public override void StartEffect(AbilityData data, Action finished)
        {
            PlayerController playerController = data.GetUser().GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.StartCoroutine(Disabling(data, finished));
            }
            finished();
        }

        IEnumerator Disabling(AbilityData data, Action finished)
        {
            yield return new WaitForSeconds(delay);

            PlayerController playerController = data.GetUser().GetComponent<PlayerController>();
            playerController.DisableStatusBar();
            playerController.DisableAPDisplay();
            finished();
        }

        public override void StartEffect(AbilityData data, GameObject target)
        {
            
        }
    }
}