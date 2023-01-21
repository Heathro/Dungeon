using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Utils;
using Shops;
using UI.Shops;
using Audio;

namespace Inventories
{
    public class SplitMenu : MonoBehaviour, IPointerExitHandler
    {
        // CONFIG

        [SerializeField] TMP_Text amountText;
        [SerializeField] Button minusButton;
        [SerializeField] Button plusButton;
        [SerializeField] Button confirmButton;
        [SerializeField] Button quitButton;
        [SerializeField] Inventory priestInventory;
        [SerializeField] Inventory paladinInventory;
        [SerializeField] Inventory hunterInventory;
        [SerializeField] Inventory mageInventory;
        [SerializeField] Shopper shopper;
        [SerializeField] ShopUI shopUI;

        // CACHE

        Inventory lootingInventory = null;
        InventoryItem item = null;
        SlotType type = SlotType.None;
        int number = 0;
        int maxNumber = 0;
        int index = 0;
        AudioHub audioHub = null;

        // LIFECYCLE

        void Start()
        {
            minusButton.onClick.AddListener(Substract);
            plusButton.onClick.AddListener(Add);
            confirmButton.onClick.AddListener(Confirm);
            quitButton.onClick.AddListener(CloseSplitMenu);
        }

        // PUBLIC

        public void SetCurrentLoot(Inventory lootingInventory)
        {
            this.lootingInventory = lootingInventory;
        }

        public void Setup(InventoryItem item, SlotType type, int number, int index, AudioHub audioHub)
        {
            transform.position = Input.mousePosition;

            this.item = item;
            this.type = type;
            this.number = 1;
            this.maxNumber = number - 1;
            this.index = index;
            this.audioHub = audioHub;

            UpdateUI();
        }

        void Confirm()
        {
            audioHub.PlayClick();
            switch (type)
            {
                case SlotType.PlayerBag: shopper.MakeSplit(SlotType.PlayerBag, item, number, index);
                                         shopUI.UpdatePlayerBag(false); break;

                case SlotType.PlayerTable: shopper.MakeSplit(SlotType.PlayerTable, item, number, index);
                                           shopUI.UpdatePlayerTable(false); break;

                case SlotType.VendorBag: shopper.MakeSplit(SlotType.VendorBag, item, number, index);
                                         shopUI.UpdateVendorBag(false); break;

                case SlotType.VendorTable: shopper.MakeSplit(SlotType.VendorTable, item, number, index);
                                           shopUI.UpdateVendorTable(false); break;

                case SlotType.PriestInventory: priestInventory.MakeSplit(item, number, index); break;
                case SlotType.PaladinInventory: paladinInventory.MakeSplit(item, number, index); break;
                case SlotType.HunterInventory: hunterInventory.MakeSplit(item, number, index); break;
                case SlotType.MageInventory: mageInventory.MakeSplit(item, number, index); break;
                case SlotType.None: lootingInventory.MakeSplit(item, number, index); break;

                default: break;
            }
            CloseSplitMenu();
        }

        void Add()
        {
            audioHub.PlayClick();
            number++;
            if (number > maxNumber)
            {
                number = maxNumber;
            }
            UpdateUI();
        }

        void Substract()
        {
            audioHub.PlayClick();
            number--;
            if (number < 1)
            {
                number = 1;
            }
            UpdateUI();
        }

        void UpdateUI()
        {
            amountText.text = number.ToString();
        }

        void CloseSplitMenu()
        {
            gameObject.SetActive(false);
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            CloseSplitMenu();
        }
    }
}