using System;
using System.Collections.Generic;
using UnityEngine;

namespace Abilities
{
    public abstract class EffectStrategy : ScriptableObject
    {
        public abstract void StartEffect(AbilityData data, Action finished);

        public abstract void StartEffect(AbilityData data, GameObject target);
    }
}