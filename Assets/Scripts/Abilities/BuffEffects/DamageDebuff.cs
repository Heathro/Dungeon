using UnityEngine;
using Attributes;
using Utils;

namespace Abilities
{
    [CreateAssetMenu(fileName = "Damage Debuff", menuName = "Abilities/Buffs/Damage", order = 0)]
    public class DamageDebuff : BuffEffect
    {
        // CONFIG

        [SerializeField] float damage = 10f;

        // PUBLIC

        public override void ApplyEffect(GameObject target)
        {
            target.GetComponent<Health>().TakeDamage(damage, GetDamageType(), null, false, true);
        }
    }
}