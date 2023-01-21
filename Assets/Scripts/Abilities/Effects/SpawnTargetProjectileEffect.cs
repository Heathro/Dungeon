using System;
using UnityEngine;
using Combat;

namespace Abilities
{
    [CreateAssetMenu(fileName = "Spawn Target Projectile", menuName = "Abilities/Effects/Spawn Target Projectile", order = 0)]
    public class SpawnTargetProjectileEffect : EffectStrategy
    {
        // CONFIG

        [SerializeField] TargetProjectile projectilePrefab;
        [SerializeField] EffectStrategy[] impactEffects;

        // PUBLIC

        public override void StartEffect(AbilityData data, Action finished)
        {
            Fighter fighter = data.GetUser().GetComponent<Fighter>();
            Vector3 startPosition = fighter.GetCastingHandTransform().position;

            foreach (GameObject target in data.GetTargets())
            {
                Instantiate(projectilePrefab, startPosition, Quaternion.identity).SetTarget(data, impactEffects, finished, target, false);
            }

            foreach (GameObject target in data.GetMissedTargets())
            {
                Instantiate(projectilePrefab, startPosition, Quaternion.identity).SetTarget(data, impactEffects, finished, target, true);
            }
        }

        public override void StartEffect(AbilityData data, GameObject target)
        {

        }
    }
}