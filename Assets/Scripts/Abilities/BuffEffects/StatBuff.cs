using UnityEngine;

namespace Abilities
{
    [CreateAssetMenu(fileName = "Stat Buff", menuName = "Abilities/Buffs/Stat", order = 0)]
    public class StatBuff : BuffEffect
    {
        // PUBLIC

        public override void ApplyEffect(GameObject target) { }
    }
}