using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Inventories;
using Stats;
using Dialogues;
using Abilities;
using Audio;

namespace UI.Inventories
{
    public class BagUI : MonoBehaviour
    {
        // CONFIG

        [SerializeField] Inventory inventory = null;
        [SerializeField] TMP_Text nameText;
        [SerializeField] TMP_Text weightText;
        [SerializeField] GameObject content = null;
        [SerializeField] Button toggleButton = null;
        [SerializeField] TMP_Text on;
        [SerializeField] TMP_Text off;

        // STATE

        BuffStore buffStore;
        AudioHub audioHub;

        // LIFECYCLE

        void Awake()
        {
            inventory.inventoryUpdated += Redraw;
            inventory.GetComponent<TraitStore>().onTraitChange += Redraw;
            inventory.GetComponent<BuffStore>().buffStoreUpdated += Redraw;

            buffStore = inventory.GetComponent<BuffStore>();

            toggleButton.onClick.AddListener(ToggleBag);

            nameText.text = inventory.GetComponent<AIConversant>().GetAIName() + "'s Bag";
        }

        void Start()
        {
            audioHub = FindObjectOfType<AudioHub>();
            Redraw();
            off.enabled = false;
        }

        // PUBLIC

        public void ToggleBag()
        {
            audioHub.PlayClick();
            if (content.activeSelf)
            {
                content.SetActive(false);
                on.enabled = false;
                off.enabled = true;
            }
            else
            {
                content.SetActive(true);
                on.enabled = true;
                off.enabled = false;
            }
        }

        // PRIVATE

        void Redraw()
        {
            if (inventory.IsOverweighted())
            {
                buffStore.SetOverweight(true);
                weightText.color = Color.red;
            }
            else
            {
                buffStore.SetOverweight(false);
                weightText.color = Color.white;
            }
            weightText.text = inventory.GetTotalWeight().ToString("N1") + "/" + inventory.GetMaxWeight().ToString("N1");
        }
    }
}