using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Shops;
using Utils;

namespace UI.Shops
{
    public class ShopUI : MonoBehaviour
    {
        // CONFIG

        [SerializeField] ShowHideUI showHideUI;
        [SerializeField] Shopper shopper;
        [SerializeField] PlayerBagSlot playerBagSlotPrefab = null;
        [SerializeField] PlayerTableSlot playerTableSlotPrefab = null;
        [SerializeField] VendorBagSlot vendorBagSlotPrefab = null;
        [SerializeField] VendorTableSlot vendorTableSlotPrefab = null;
        [SerializeField] Image playerIcon;
        [SerializeField] TMP_Text playerName;
        [SerializeField] TMP_Text playerPurse;
        [SerializeField] TMP_Text playerTotal;
        [SerializeField] Transform playerBag;
        [SerializeField] Transform playerTable;
        [SerializeField] Image vendorIcon;
        [SerializeField] TMP_Text vendorName;
        [SerializeField] TMP_Text vendorPurse;
        [SerializeField] TMP_Text vendorTotal;
        [SerializeField] Transform vendorBag;
        [SerializeField] Transform vendorTable;
        [SerializeField] Button levelButton;
        [SerializeField] Button acceptButton;
        [SerializeField] int startingBagPoolSize = 44;
        [SerializeField] int startingTablePoolSize = 20;
        [SerializeField] int bagExtensionSize = 16;
        [SerializeField] int tableExtensionSize = 8;

        // STATE

        List<PlayerBagSlot> playerBagPool = new List<PlayerBagSlot>();
        List<PlayerTableSlot> playerTablePool = new List<PlayerTableSlot>();
        List<VendorBagSlot> vendorBagPool = new List<VendorBagSlot>();
        List<VendorTableSlot> vendorTablePool = new List<VendorTableSlot>();

        // LIFECYCLE

        void Awake()
        {
            PopulatePools();

            shopper.onShopStart += StartShop;
            shopper.onShopUpdate += RedrawUI;
            levelButton.onClick.AddListener(shopper.LevelDeal);
            acceptButton.onClick.AddListener(shopper.AcceptTransaction);
        }

        // PUBLIC

        public void UpdatePlayerBag(bool update = true)
        {
            int size = shopper.GetSize(SlotType.PlayerBag);
            while (size > playerBagPool.Count) ExtendPlayerBagPool();

            for (int i = 0; i < playerBagPool.Count; i++)
            {
                if (i < size)
                {
                    playerBagPool[i].gameObject.SetActive(true);
                    playerBagPool[i].Setup(i, shopper, this);
                }
                else
                {
                    playerBagPool[i].gameObject.SetActive(false);
                }
            }

            if (update)
            {
                RedrawPurse();
                UpdateAcceptButton();
            }
        }

        public void UpdatePlayerTable(bool update = true)
        {
            int size = shopper.GetSize(SlotType.PlayerTable);
            while (size > playerTablePool.Count) ExtendPlayerTablePool();

            for (int i = 0; i < playerTablePool.Count; i++)
            {
                if (i < size)
                {
                    playerTablePool[i].gameObject.SetActive(true);
                    playerTablePool[i].Setup(i, shopper, this);
                }
                else
                {
                    playerTablePool[i].gameObject.SetActive(false);
                }
            }

            if (update)
            {
                RedrawPurse();
                UpdateAcceptButton();
            }
        }

        public void UpdateVendorBag(bool update = true)
        {
            int size = shopper.GetSize(SlotType.VendorBag);
            while (size > vendorBagPool.Count) ExtendVendorBagPool();

            for (int i = 0; i < vendorBagPool.Count; i++)
            {
                if (i < size)
                {
                    vendorBagPool[i].gameObject.SetActive(true);
                    vendorBagPool[i].Setup(i, shopper, this);
                }
                else
                {
                    vendorBagPool[i].gameObject.SetActive(false);
                }
            }

            if (update)
            {
                RedrawPurse();
                UpdateAcceptButton();
            }
        }

        public void UpdateVendorTable(bool update = true)
        {
            int size = shopper.GetSize(SlotType.VendorTable);
            while (size > vendorTablePool.Count) ExtendVendorTablePool();

            for (int i = 0; i < vendorTablePool.Count; i++)
            {
                if (i < size)
                {
                    vendorTablePool[i].gameObject.SetActive(true);
                    vendorTablePool[i].Setup(i, shopper, this);
                }
                else
                {
                    vendorTablePool[i].gameObject.SetActive(false);
                }
            }

            if (update)
            {
                RedrawPurse();
                UpdateAcceptButton();
            }
        }

        // PRIVATE

        void StartShop()
        {
            showHideUI.OpenShop();
            SetupUI();
        }

        void SetupUI()
        {
            playerIcon.sprite = shopper.GetPlayerIcon();
            playerName.text = shopper.GetPlayerName();
            vendorIcon.sprite = shopper.GetVendorIcon();
            vendorName.text = shopper.GetVendorName();
            RedrawUI();
        }

        void RedrawPurse()
        {
            playerPurse.text = shopper.GetPlayerPurse().ToString();
            vendorPurse.text = shopper.GetVendorPurse().ToString();
            playerTotal.text = shopper.GetPlayerTotal().ToString();
            vendorTotal.text = shopper.GetVendorTotal().ToString();
        }

        void UpdateAcceptButton()
        {
            acceptButton.interactable = shopper.CanAccept();
        }

        void RedrawUI()
        {
            RedrawPurse();
            UpdateAcceptButton();

            UpdatePlayerBag(false);
            UpdatePlayerTable(false);
            UpdateVendorBag(false);
            UpdateVendorTable(false);
        }

        void PopulatePools()
        {
            for (int i = 0; i < startingBagPoolSize; i++)
            {
                playerBagPool.Add(Instantiate(playerBagSlotPrefab, playerBag));
                vendorBagPool.Add(Instantiate(vendorBagSlotPrefab, vendorBag));
            }

            for (int i = 0; i < startingTablePoolSize; i++)
            {
                playerTablePool.Add(Instantiate(playerTableSlotPrefab, playerTable));
                vendorTablePool.Add(Instantiate(vendorTableSlotPrefab, vendorTable));
            }
        }

        void ExtendPlayerBagPool()
        {
            for (int i = 0; i < bagExtensionSize; i++)
            {
                playerBagPool.Add(Instantiate(playerBagSlotPrefab, playerBag));
            }
        }

        void ExtendPlayerTablePool()
        {
            for (int i = 0; i < tableExtensionSize; i++)
            {
                playerTablePool.Add(Instantiate(playerTableSlotPrefab, playerTable));
            }
        }

        void ExtendVendorBagPool()
        {
            for (int i = 0; i < bagExtensionSize; i++)
            {
                vendorBagPool.Add(Instantiate(vendorBagSlotPrefab, vendorBag));
            }
        }

        void ExtendVendorTablePool()
        {
            for (int i = 0; i < tableExtensionSize; i++)
            {
                vendorTablePool.Add(Instantiate(vendorTableSlotPrefab, vendorTable));
            }
        }
    }
}