using System;
using System.Collections.Generic;
using UnityEngine;
using Control;
using UI.HUD;

namespace Abilities
{
    [CreateAssetMenu(fileName = "Around Targeting", menuName = "Abilities/Targeting/Around", order = 0)]
    public class AroundTargeting : TargetingStrategy
    {
        // CONFIG

        [SerializeField] float radius = 3f;

        // PUBLIC

        public override void StartTargeting(AbilityData data, Action finished)
        {
            data.SetTargets(GetObjectsInRadius(data));
            data.SetTargetPoint(data.GetUser().transform.position);
            finished();
        }

        // PRIVATE

        IEnumerable<GameObject> GetObjectsInRadius(AbilityData data)
        {
            foreach (var hit in Physics.SphereCastAll(data.GetUser().transform.position, radius, Vector3.up, 0))
            {
                if (!Scanner.IsTargetVisible(data.GetUser(), hit.collider.gameObject)) continue;

                yield return hit.collider.gameObject;
            }
        }
    }
}