using System.Collections.Generic;
using UnityEngine;
using Control;

namespace Abilities
{
    [CreateAssetMenu(fileName = "Enemy Filter", menuName = "Abilities/Filters/Enemy", order = 0)]
    public class EnemyFilter : FilterStrategy
    {
        // PUBLIC

        public override IEnumerable<GameObject> Filter(IEnumerable<GameObject> targets)
        {
            foreach (GameObject target in targets)
            {
                IController iController = target.GetComponent<IController>();
                if (iController == null) continue;
                if (!iController.IsEnemy()) continue;

                yield return target;
            }
        }
    }
}