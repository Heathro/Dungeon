using System;
using UnityEngine;
using Combat;
using Inventories;
using Utils;

namespace Abilities
{
    [CreateAssetMenu(fileName = "Spawn Ranged Projectile", menuName = "Abilities/Effects/Spawn Ranged Projectile", order = 0)]
    public class SpawnRangedProjectileEffect : EffectStrategy
    {
        // CONFIG

        [SerializeField] TargetProjectile arrowPrefab;
        [SerializeField] TargetProjectile boltPrefab;
        [SerializeField] EffectStrategy[] impactEffects;

        // PUBLIC

        public override void StartEffect(AbilityData data, Action finished)
        {
            Fighter fighter = data.GetUser().GetComponent<Fighter>();

            RangedProjectileType projectileType = fighter.GetProjectileType();

            TargetProjectile projectilePrefab = boltPrefab;
            if (projectileType == RangedProjectileType.Arrow)
            {
                projectilePrefab = arrowPrefab;
            }
            
            Vector3 startPosition = fighter.GetShootHandTransform().position;
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