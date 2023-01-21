using UnityEngine;
using Stats;
using Utils;

namespace Control
{
    public class LootHolder : MonoBehaviour, IRaycastable
    {
        // CONFIG

        [SerializeField] Loot loot = null;

        // PUBLIC

        public bool HandleRaycast(PlayerController callingController, bool actionAvailable)
        {
            return loot.HandleRaycast(callingController, actionAvailable);
        }

        public void Interact(PlayerController callingController)
        {
            loot.Interact(callingController);
        }

        public void EnableHintDisplay()
        {
            loot.EnableHintDisplay();
        }

        public CursorType GetCursorType(PlayerController callingController)
        {
            return CursorType.Loot;
        }

        public int GetInteractCost(PlayerController callingController)
        {
            return loot.GetInteractCost(callingController);
        }

        public float GetInteractRange(PlayerController callingController)
        {
            return loot.GetInteractRange();
        }

        public Vector3 GetPosition()
        {
            return transform.position;
        }

        public BaseStats GetBaseStats()
        {
            return null;
        }

        public Transform GetTransform()
        {
            return transform;
        }

        public void EnableBattleMarker(bool isEnable, bool isSelfTarget = false)
        {

        }

        public void EnableObstacle(bool isEnable)
        {

        }

        public void EnableStatusBar()
        {

        }

        public void SetupStatusBar()
        {

        }
    }
}