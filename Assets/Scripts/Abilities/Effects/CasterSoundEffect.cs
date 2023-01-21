using System;
using UnityEngine;
using Audio;

namespace Abilities
{
    [CreateAssetMenu(fileName = "Caster Sound", menuName = "Abilities/Effects/Caster Sound", order = 0)]
    public class CasterSoundEffect : EffectStrategy
    {
        // CONFIG

        [SerializeField] AudioClip sound;

        // PUBLIC

        public override void StartEffect(AbilityData data, Action finished)
        {
            data.GetUser().GetComponent<AudioController>().PlaySound(sound);
            finished();
        }

        public override void StartEffect(AbilityData data, GameObject target)
        {
            data.GetUser().GetComponent<AudioController>().PlaySound(sound);
        }
    }
}