using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using Saving;

namespace Inventories
{
    public class ItemDropper : MonoBehaviour, ISaveable
    {
        // CONFIG
        
        [SerializeField] Transform droppedPickups;
        [SerializeField] float dropRadius = 1f;
        [SerializeField] int dropAttempts = 30;
        [SerializeField] float highOffset = 0.04336333f;

        // STATE

        List<Pickup> droppedItems = new List<Pickup>();
        List<DropRecord> otherSceneDropRecords = new List<DropRecord>();

        // PUBLIC

        public void DropItem(InventoryItem item, int number, Transform dropper)
        {
            SpawnPickup(item, number, GetDropLocation(dropper));
        }

        public void SpawnPickup(InventoryItem item, int number, Vector3 spawnLocation)
        {
            Pickup pickup = item.SpawnPickup(spawnLocation, number);
            pickup.transform.parent = droppedPickups;
            pickup.transform.rotation = GetRandomRotation(pickup.transform.rotation);
            droppedItems.Add(pickup);
        }

        // PRIVATE

        Vector3 GetDropLocation(Transform location)
        {
            for (int i = 0; i < dropAttempts; i++)
            {
                Vector3 randomPoint = location.position + Random.insideUnitSphere * dropRadius;
                NavMeshHit hit;
                if (NavMesh.SamplePosition(randomPoint, out hit, 0.1f, NavMesh.AllAreas))
                {
                    return hit.position + Vector3.down * highOffset;
                }
            }
            return location.position;
        }

        Quaternion GetRandomRotation(Quaternion rotation)
        {
            rotation.eulerAngles = new Vector3(rotation.x, Random.Range(0f, 360f), rotation.z);
            return rotation;
        }

        void RemoveDestroyedDrops()
        {
            List<Pickup> newList = new List<Pickup>();
            foreach (Pickup item in droppedItems)
            {
                if (item != null)
                {
                    newList.Add(item);
                }
            }
            droppedItems = newList;
        }

        void RemoveUnpickedDrops()
        {
            foreach (Transform pickup in droppedPickups)
            {
                Destroy(pickup.gameObject);
            }
        }

        [System.Serializable]
        struct DropRecord
        {
            public string itemID;
            public int number;
            public SerializableTransform position;
            public int scene;
        }

        object ISaveable.CaptureState()
        {
            RemoveDestroyedDrops();

            List<DropRecord> dropRecords = new List<DropRecord>();
            int sceneBuildIndex = SceneManager.GetActiveScene().buildIndex;

            foreach (Pickup pickup in droppedItems)
            {
                DropRecord drop = new DropRecord();

                drop.itemID = pickup.GetItem().GetItemID();
                drop.number = pickup.GetNumber();
                drop.position = new SerializableTransform(pickup.transform);
                drop.scene = sceneBuildIndex;

                dropRecords.Add(drop);
            }

            dropRecords.AddRange(otherSceneDropRecords);
            return dropRecords;
        }

        void ISaveable.RestoreState(object state)
        {
            RemoveUnpickedDrops();

            List<DropRecord> dropRecords = (List<DropRecord>)state;
            int sceneBuildIndex = SceneManager.GetActiveScene().buildIndex;
            otherSceneDropRecords.Clear();

            foreach (DropRecord drop in dropRecords)
            {
                if (drop.scene != sceneBuildIndex)
                {
                    otherSceneDropRecords.Add(drop);
                    continue;
                }

                InventoryItem pickupItem = InventoryItem.GetFromID(drop.itemID);
                int number = drop.number;
                Vector3 position = drop.position.GetPosition();

                SpawnPickup(pickupItem, number, position);
            }
        }
    }
}