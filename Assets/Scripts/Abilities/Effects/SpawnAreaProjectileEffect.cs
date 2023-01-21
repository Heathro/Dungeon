using System;
using UnityEngine;
using Combat;

namespace Abilities
{
    [CreateAssetMenu(fileName = "Spawn Area Projectile", menuName = "Abilities/Effects/Spawn Area Projectile", order = 0)]
    public class SpawnAreaProjectileEffect : EffectStrategy
    {
        // CONFIG

        [SerializeField] AreaProjectile projectilePrefab;
        [SerializeField] EffectStrategy[] impactEffects;

        // PUBLIC

        public override void StartEffect(AbilityData data, Action finished)
        {
            Fighter fighter = data.GetUser().GetComponent<Fighter>();
            Vector3 startPosition = fighter.GetCastingHandTransform().position;
            Instantiate(projectilePrefab, startPosition, Quaternion.identity).SetTarget(data, impactEffects, finished);
        }

        public override void StartEffect(AbilityData data, GameObject target)
        {

        }
    }
}