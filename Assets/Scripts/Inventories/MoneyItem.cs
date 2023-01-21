using UnityEngine;

namespace Inventories
{
    [CreateAssetMenu(fileName = "Money Item", menuName = "Items/Money Item", order = 0)]
    public class MoneyItem : InventoryItem
    {
        // CONFIG

        [SerializeField] Pickup[] pickups;

        // PUBLIC

        public override Pickup SpawnPickup(Vector3 position, int number)
        {
            var pickup = Instantiate(GetCorrectPickup(number));
            pickup.transform.position = position;
            pickup.Setup(this, number);
            return pickup;
        }

        // PRIVATE

        Pickup GetCorrectPickup(int amount)
        {
            if (amount == 1)
            {
                return pickups[0];
            }
            else if (amount <= 10)
            {
                return pickups[1];
            }
            else if (amount <= 50)
            {
                return pickups[2];
            }
            else
            {
                return pickups[3];
            }
        }
    }
}