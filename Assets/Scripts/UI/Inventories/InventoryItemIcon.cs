using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Inventories;

namespace UI.Inventories
{
    public class InventoryItemIcon : MonoBehaviour
    {
        // CONFIG

        [SerializeField] TMP_Text numberText;

        // PUBLIC

        public void SetItem(InventoryItem item, int number)
        {
            Image iconImage = GetComponent<Image>();

            if (item == null)
            {
                iconImage.enabled = false;
            }
            else
            {
                iconImage.enabled = true;
                iconImage.sprite = item.GetIcon();
            }

            if (number < 2)
            {
                numberText.enabled = false;
            }
            else
            {
                numberText.enabled = true;
                numberText.text = number.ToString();
            }
        }
    }
}