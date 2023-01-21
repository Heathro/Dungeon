using System.Collections.Generic;
using UnityEngine;
using Attributes;
using Control;

namespace Abilities
{
    [CreateAssetMenu(fileName = "Resurrect Filter", menuName = "Abilities/Filters/Resurrect", order = 0)]
    public class ResurrectFilter : FilterStrategy
    {
        // PUBLIC

        public override IEnumerable<GameObject> Filter(IEnumerable<GameObject> targets)
        {
            foreach (GameObject target in targets)
            {
                Health health = target.GetComponent<Health>();
                CombatTarget combatTarget = target.GetComponent<CombatTarget>();

                if (health != null && health.IsDead() && combatTarget != null && combatTarget.IsFriendly())
                {
                    yield return target;
                }
            }
        }
    }
}