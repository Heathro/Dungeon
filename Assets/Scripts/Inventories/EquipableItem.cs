using System.Collections.Generic;
using UnityEngine;
using Utils;
using Stats;

namespace Inventories
{
    [CreateAssetMenu(fileName = "Equipable Item", menuName = "Items/Equipable Item", order = 0)]
    public class EquipableItem : InventoryItem, IModifierProvider
    {
        // CONFIG

        [SerializeField] EquipLocation allowedEquipLocation = EquipLocation.Weapon;
        [SerializeField] Modifier[] additiveModifiers = null;
        [SerializeField] Modifier[] percentageModifiers = null;

        [System.Serializable]
        class Modifier
        {
            public CharacterStat stat;
            public float amount;
        }

        // PUBLIC

        public EquipLocation GetAllowedEquipLocation()
        {
            return allowedEquipLocation;
        }

        public IEnumerable<float> GetAdditiveModifiers(CharacterStat stat)
        {
            foreach (Modifier modifier in additiveModifiers)
            {
                if (modifier.stat == stat)
                {
                    yield return modifier.amount;
                }
            }
        }

        public IEnumerable<float> GetPercentageModifiers(CharacterStat stat)
        {
            foreach (Modifier modifier in percentageModifiers)
            {
                if (modifier.stat == stat)
                {
                    yield return modifier.amount;
                }
            }
        }
    }
}