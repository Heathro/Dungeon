using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using Inventories;
using Control;
using Audio;

namespace UI.Inventories
{
    public class ContainerUI : MonoBehaviour
    {
        // CONFIG

        [SerializeField] GameObject content = null;
        [SerializeField] InventorySlotUI InventoryItemPrefab = null;
        [SerializeField] SplitMenu splitMenu = null;
        [SerializeField] int startingPoolSize = 16;
        [SerializeField] int extensionSize = 16;

        // CACHE

        ControlSwitcher controlSwitcher;
        Inventory inventory;
        ShowHideUI showHideUI;
        AudioHub audioHub;

        // STATE

        List<InventorySlotUI> slotPool = new List<InventorySlotUI>();

        // LIFECYCLE

        void Awake()
        {
            PopulatePool();
        }

        void Start()
        {
            controlSwitcher = FindObjectOfType<ControlSwitcher>();
            showHideUI = FindObjectOfType<ShowHideUI>();
            audioHub = FindObjectOfType<AudioHub>();
        }

        void Update()
        {
            if (!EventSystem.current.IsPointerOverGameObject() && Input.GetMouseButtonDown(0))
            {
                showHideUI.CloseContainer();
            }
        }

        // PUBLIC

        public void SetupContainer(Inventory inventory)
        {
            content.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 0f);
            splitMenu.SetCurrentLoot(inventory);
            this.inventory = inventory;
            this.inventory.inventoryUpdated += Redraw;
            Redraw();
        }

        public void TakeAll()
        {
            audioHub.PlayMove();
            Inventory playerInventory = controlSwitcher.GetActivePlayer().GetComponent<Inventory>();

            for (int i = 0; i < inventory.GetSize(); i++)
            {
                InventoryItem item = inventory.GetItemInSlot(i);
                if (item == null) continue;

                int number = inventory.GetNumberInSlot(i);

                playerInventory.AddToFirstEmptySlot(item, number, false);
                inventory.RemoveFromSlot(i, number, false);
            }
            playerInventory.UpdateInventory();
            inventory.UpdateInventory();

            showHideUI.CloseContainer();
        }

        // PRIVATE

        void Redraw()
        {
            int inventorySize = inventory.GetSize();

            while (inventorySize > slotPool.Count)
            {
                ExtendPool();
            }

            for (int i = 0; i < slotPool.Count; i++)
            {
                if (i < inventorySize)
                {
                    slotPool[i].gameObject.SetActive(true);
                    slotPool[i].Setup(inventory, i);
                }
                else
                {
                    slotPool[i].gameObject.SetActive(false);
                }
            }
        }

        void PopulatePool()
        {
            for (int i = 0; i < startingPoolSize; i++)
            {
                slotPool.Add(Instantiate(InventoryItemPrefab, content.transform));
            }
        }

        void ExtendPool()
        {
            for (int i = 0; i < extensionSize; i++)
            {
                slotPool.Add(Instantiate(InventoryItemPrefab, content.transform));
            }
        }
    }
}