using UnityEngine;

namespace Core
{
    public class PersistentObjectsSpawner : MonoBehaviour
    {
        // CONFIG

        [SerializeField] GameObject persistentObjects;

        // STATE

        static bool hasSpawned = false;

        // LIFECYCLE

        void Awake()
        {
            if (hasSpawned) return;
            DontDestroyOnLoad(Instantiate(persistentObjects));
            hasSpawned = true;
        }
    }
}