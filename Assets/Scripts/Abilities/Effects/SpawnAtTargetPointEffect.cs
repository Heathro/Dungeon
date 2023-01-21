using System;
using UnityEngine;

namespace Abilities
{
    [CreateAssetMenu(fileName = "Spawn At Target Point", menuName = "Abilities/Effects/Spawn At Target Point", order = 0)]
    public class SpawnAtTargetPointEffect : EffectStrategy
    {
        // CONFIG

        [SerializeField] GameObject effectPrefab;
        [SerializeField] float destroyDelay = -1f;
        [SerializeField] float highOffset = 0f;

        // PUBLIC

        public override void StartEffect(AbilityData data, Action finished)
        {
            GameObject effect = Instantiate(effectPrefab, data.GetTargetPoint(), Quaternion.identity);
            effect.transform.position += Vector3.up * highOffset;

            if (destroyDelay > 0) Destroy(effect, destroyDelay);

            finished();
        }

        public override void StartEffect(AbilityData data, GameObject target)
        {

        }
    }
}