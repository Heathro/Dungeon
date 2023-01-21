using System;
using System.Collections;
using UnityEngine;
using Control;

namespace Abilities
{
    [CreateAssetMenu(fileName = "Set Simple Cursor", menuName = "Abilities/Effects/Set Simple Cursor", order = 0)]
    public class SetSimpleCursor : EffectStrategy
    {
        // CONFIG

        [SerializeField] float delay = 0.05f;

        // PUBLIC

        public override void StartEffect(AbilityData data, Action finished)
        {
            PlayerController playerController = data.GetUser().GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.StartCoroutine(Setting(data, finished));
            }
            finished();
        }

        IEnumerator Setting(AbilityData data, Action finished)
        {
            yield return new WaitForSeconds(delay);
            data.GetUser().GetComponent<PlayerController>().SetSimpleCursor();
            finished();
        }

        public override void StartEffect(AbilityData data, GameObject target)
        {

        }
    }
}