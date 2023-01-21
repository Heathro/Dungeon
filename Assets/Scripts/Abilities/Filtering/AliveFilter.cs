using System.Collections.Generic;
using UnityEngine;
using Attributes;

namespace Abilities
{
    [CreateAssetMenu(fileName = "Alive", menuName = "Abilities/Filters/Alive", order = 0)]
    public class AliveFilter : FilterStrategy
    {
        // PUBLIC

        public override IEnumerable<GameObject> Filter(IEnumerable<GameObject> targets)
        {
            foreach (GameObject target in targets)
            {
                Health health = target.GetComponent<Health>();
                if (health == null) continue;
                if (health.IsDead()) continue;

                yield return target;
            }
        }
    }
}