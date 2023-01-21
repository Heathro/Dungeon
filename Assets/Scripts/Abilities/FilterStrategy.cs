using System.Collections.Generic;
using UnityEngine;

namespace Abilities
{
    public abstract class FilterStrategy : ScriptableObject
    {
        public abstract IEnumerable<GameObject> Filter(IEnumerable<GameObject> targets);
    }
}