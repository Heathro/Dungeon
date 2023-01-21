using UnityEngine;
using Saving;

namespace Inventories
{
    public class PickupSpawner : MonoBehaviour, ISaveable
    {
        // CONFIG

        [SerializeField] InventoryItem item = null;
        [SerializeField] int number = 1;

        // LIFECYCLE

        void Awake()
        {
            SpawnPickup();
        }

        // PUBLIC

        public Pickup GetPickup()
        {
            return GetComponentInChildren<Pickup>();
        }

        public bool isCollected()
        {
            return GetPickup() == null;
        }

        //PRIVATE

        void SpawnPickup()
        {
            var spawnedPickup = item.SpawnPickup(transform.position, number);
            spawnedPickup.transform.SetParent(transform);
        }

        void DestroyPickup()
        {
            if (GetPickup())
            {
                Destroy(GetPickup().gameObject);
            }
        }

        object ISaveable.CaptureState()
        {
            return isCollected();
        }

        void ISaveable.RestoreState(object state)
        {
            bool shouldBeCollected = (bool)state;

            if (shouldBeCollected && !isCollected())
            {
                DestroyPickup();
            }

            if (!shouldBeCollected && isCollected())
            {
                SpawnPickup();
            }
        }
    }
}