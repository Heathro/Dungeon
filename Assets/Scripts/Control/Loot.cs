using UnityEngine;
using Inventories;
using UI;
using UI.HUD;
using UI.Inventories;

namespace Control
{
    public class Loot : MonoBehaviour
    {
        // CONFIG

        [SerializeField] int interactCost = 1;
        [SerializeField] float interactRange = 1f;
        [SerializeField] string hintName = "Loot";
        [SerializeField] HintDisplay hintDisplay;

        // CACHE

        ShowHideUI showHideUI;
        ContainerUI containerUI;
        Inventory inventory;
        LootRandomiser lootRandomiser;

        // LIFECYCLE

        void Awake()
        {
            inventory = GetComponent<Inventory>();
            lootRandomiser = GetComponent<LootRandomiser>();
        }

        void Start()
        {
            showHideUI = FindObjectOfType<ShowHideUI>();
            containerUI = showHideUI.GetContainer().GetComponent<ContainerUI>();
            lootRandomiser.GenerateLoot();
            hintDisplay.SetText(hintName);
        }

        // PUBLIC

        public bool HandleRaycast(PlayerController callingController, bool actionAvailable)
        {
            EnableHintDisplay();
            if (actionAvailable && Input.GetMouseButtonDown(0))
            {
                callingController.SetupInteraction(transform.parent.GetComponent<IRaycastable>());
            }
            return true;
        }

        public void Interact(PlayerController callingController)
        {
            callingController.transform.LookAt(transform);
            showHideUI.OpenContainer();
            containerUI.SetupContainer(inventory);
        }

        public void EnableHintDisplay()
        {
            hintDisplay.EnableHint();
        }

        public int GetInteractCost(PlayerController callingController)
        {
            return callingController.IsTakingTurn() ? interactCost : 0;
        }

        public float GetInteractRange()
        {
            return interactRange;
        }
    }
}