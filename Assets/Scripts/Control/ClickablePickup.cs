using UnityEngine;
using Inventories;
using Utils;
using UI.HUD;
using Stats;
using Animations;

namespace Control
{
    public class ClickablePickup : MonoBehaviour, IRaycastable
    {
        // CONFIG

        [SerializeField] int interactCost = 1;
        [SerializeField] float interactRange = 1f;
        [SerializeField] HintDisplay hintDisplay;

        // STATE

        Pickup pickup;

        // LIFECYCLE

        void Awake()
        {
            pickup = GetComponent<Pickup>();
        }

        void Start()
        {
            hintDisplay.SetText(pickup.GetItem().GetDisplayName());
        }

        // PUBLIC

        public bool HandleRaycast(PlayerController callingController, bool actionAvailable)
        {
            EnableHintDisplay();
            if (actionAvailable && Input.GetMouseButtonDown(0))
            {
                callingController.SetupInteraction(this);
            }
            return true;
        }

        public void Interact(PlayerController callingController)
        {
            callingController.transform.LookAt(transform);
            callingController.EnableControl(false);
            callingController.GetComponent<AnimationController>().Pickup(() =>
            {
                pickup.PickupItem(callingController.GetComponent<Inventory>());
            });
        }

        public Vector3 GetPosition()
        {
            return transform.position;
        }

        public int GetInteractCost(PlayerController callingController)
        {
            return callingController.IsTakingTurn() ? interactCost : 0;
        }

        public float GetInteractRange(PlayerController callingController)
        {
            return interactRange;
        }

        public CursorType GetCursorType(PlayerController callingController)
        {
            return CursorType.Loot;
        }

        public void EnableStatusBar()
        {

        }

        public void SetupStatusBar()
        {

        }

        public void EnableHintDisplay()
        {
            hintDisplay.EnableHint();
        }

        public void EnableBattleMarker(bool isEnable, bool isSelfTarget = false)
        {

        }

        public void EnableObstacle(bool isEnable)
        {

        }


        public BaseStats GetBaseStats()
        {
            return null;
        }

        public Transform GetTransform()
        {
            return transform;
        }
    }
}