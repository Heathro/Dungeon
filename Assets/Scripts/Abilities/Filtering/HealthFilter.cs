using System.Collections.Generic;
using UnityEngine;
using Attributes;

namespace Abilities
{
    [CreateAssetMenu(fileName = "Health", menuName = "Abilities/Filters/Health", order = 0)]
    public class HealthFilter : FilterStrategy
    {
        // PUBLIC

        public override IEnumerable<GameObject> Filter(IEnumerable<GameObject> targets)
        {
            if (targets == null) yield break;

            foreach (GameObject target in targets)
            {
                Health health = target.GetComponent<Health>();
                if (health == null) continue;

                yield return target;
            }    
        }
    }
}