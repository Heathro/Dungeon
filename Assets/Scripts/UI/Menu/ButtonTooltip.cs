using UnityEngine;
using TMPro;

namespace UI.Inventories
{
    public class ButtonTooltip : MonoBehaviour
    {
        // CONFIG

        [SerializeField] TMP_Text hintText = null;

        // PUBLIC

        public void Setup(string hintText)
        {
            this.hintText.text = hintText;
        }
    }
}