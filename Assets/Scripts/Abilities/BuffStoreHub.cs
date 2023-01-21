using UnityEngine;

namespace Abilities
{
    public class BuffStoreHub : MonoBehaviour
    {
        // PUBLIC

        public bool IsOverweight()
        {
            foreach (BuffStore buffStore in GetComponentsInChildren<BuffStore>())
            {
                if (buffStore.IsOverweight())
                {
                    return true;
                }
            }
            return false;
        }
    }
}