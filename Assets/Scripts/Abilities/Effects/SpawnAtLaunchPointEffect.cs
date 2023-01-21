using System;
using UnityEngine;

namespace Abilities
{
    [CreateAssetMenu(fileName = "Spawn At Launch Point", menuName = "Abilities/Effects/Spawn At Launch Point", order = 0)]
    public class SpawnAtLaunchPointEffect : EffectStrategy
    {
        // CONFIG

        [SerializeField] GameObject effectPrefab;
        [SerializeField] float destroyDelay = -1f;
        [SerializeField] float highOffset = 0f;

        // PUBLIC

        public override void StartEffect(AbilityData data, Action finished)
        {
            GameObject effect = Instantiate(effectPrefab, data.GetUser().transform);
            effect.transform.position += Vector3.up * highOffset;

            if (destroyDelay > 0) Destroy(effect, destroyDelay);

            finished();
        }

        public override void StartEffect(AbilityData data, GameObject target)
        {

        }
    }
}