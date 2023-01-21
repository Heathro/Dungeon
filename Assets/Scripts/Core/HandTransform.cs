using UnityEngine;

namespace Core
{
    public class HandTransform : MonoBehaviour
    {
        // CONFIG

        [SerializeField] Transform hand;

        // LIFECYCLE

        void Update()
        {
            if (hand == null) return;
            transform.position = hand.position;
            transform.rotation = hand.rotation;
        }
    }
}