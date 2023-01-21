using System.Collections.Generic;
using UnityEngine;
using Attributes;

namespace Abilities
{
    [CreateAssetMenu(fileName = "Dead", menuName = "Abilities/Filters/Dead", order = 0)]
    public class DeadFilter : FilterStrategy
    {
        // PUBLIC

        public override IEnumerable<GameObject> Filter(IEnumerable<GameObject> targets)
        {
            foreach (GameObject target in targets)
            {
                Health health = target.GetComponent<Health>();
                if (health == null) continue;
                if (!health.IsDead()) continue;

                yield return target;
            }
        }
    }
}