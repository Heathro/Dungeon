using System.Collections.Generic;
using UnityEngine;
using Control;

namespace Abilities
{
    [CreateAssetMenu(fileName = "Ally Filter", menuName = "Abilities/Filters/Ally", order = 0)]
    public class AllyFilter : FilterStrategy
    {
        // PUBLIC

        public override IEnumerable<GameObject> Filter(IEnumerable<GameObject> targets)
        {
            foreach (GameObject target in targets)
            {
                IController iController = target.GetComponent<IController>();
                if (iController == null) continue;
                if (iController.IsEnemy()) continue;

                yield return target;
            }
        }
    }
}