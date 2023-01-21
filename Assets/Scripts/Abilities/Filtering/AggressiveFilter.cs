using System.Collections.Generic;
using UnityEngine;
using Control;

namespace Abilities
{
    [CreateAssetMenu(fileName = "Aggressive", menuName = "Abilities/Filters/Aggressive", order = 0)]
    public class AggressiveFilter : FilterStrategy
    {
        // PUBLIC

        public override IEnumerable<GameObject> Filter(IEnumerable<GameObject> targets)
        {
            foreach (GameObject target in targets)
            {
                IController iController = target.GetComponent<IController>();
                if (iController == null) continue;
                if (!iController.IsEnemy()) continue;

                AIController aIController = target.GetComponent<AIController>();
                if (!aIController.IsAggressive()) continue;

                yield return target;
            }
        }
    }
}