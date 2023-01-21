using System;
using UnityEngine;
using Audio;

namespace Abilities
{
    [CreateAssetMenu(fileName = "Point Sound", menuName = "Abilities/Effects/Point Sound", order = 0)]
    public class PointSoundEffect : EffectStrategy
    {
        // CONFIG

        [SerializeField] AudioClip sound;
        [SerializeField] GameObject audioSourcePrefab;

        // CACHE

        GameObject audioSource = null;
        float heightOffset = 0.2f;

        // PUBLIC

        public override void StartEffect(AbilityData data, Action finished)
        {
            if (audioSource == null)
            {
                audioSource = Instantiate(audioSourcePrefab);
            }

            audioSource.transform.position = data.GetTargetPoint() + Vector3.up * heightOffset;
            audioSource.GetComponent<AudioSource>().PlayOneShot(sound);

            finished();
        }

        public override void StartEffect(AbilityData data, GameObject target)
        {
            data.GetUser().GetComponent<AudioController>().PlaySound(sound);
        }
    }
}