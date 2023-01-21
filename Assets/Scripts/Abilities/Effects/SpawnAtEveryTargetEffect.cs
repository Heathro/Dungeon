using System;
using UnityEngine;

namespace Abilities
{
    [CreateAssetMenu(fileName = "Spawn At Every Target", menuName = "Abilities/Effects/Spawn At Every Target", order = 0)]
    public class SpawnAtEveryTargetEffect : EffectStrategy
    {
        // CONFIG

        [SerializeField] GameObject effectPrefab;
        [SerializeField] float destroyDelay = -1f;
        [SerializeField] float highOffset = 0f;

        // PUBLIC

        public override void StartEffect(AbilityData data, Action finished)
        {
            foreach (GameObject target in data.GetTargets())
            {
                GameObject effect = Instantiate(effectPrefab, target.transform.position, Quaternion.identity);
                effect.transform.position += Vector3.up * highOffset;

                if (destroyDelay > 0) Destroy(effect, destroyDelay);
            }

            finished();
        }

        public override void StartEffect(AbilityData data, GameObject target)
        {
            GameObject effect = Instantiate(effectPrefab, target.transform.position, Quaternion.identity);
            effect.transform.position += Vector3.up * highOffset;

            if (destroyDelay > 0) Destroy(effect, destroyDelay);
        }
    }
}