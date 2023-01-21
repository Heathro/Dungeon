using System;
using UnityEngine;
using Audio;

namespace Abilities
{
    [CreateAssetMenu(fileName = "Targets Sound", menuName = "Abilities/Effects/Targets Sound", order = 0)]
    public class TargetsSoundEffect : EffectStrategy
    {
        // CONFIG

        [SerializeField] AudioClip sound;

        // PUBLIC

        public override void StartEffect(AbilityData data, Action finished)
        {
            foreach (GameObject target in data.GetTargets())
            {
                target.GetComponent<AudioController>().PlaySound(sound);
            }
            finished();
        }

        public override void StartEffect(AbilityData data, GameObject target)
        {
            AudioController audioController = target.GetComponent<AudioController>();
            if (audioController == null) return;

            audioController.PlaySound(sound);
        }
    }
}