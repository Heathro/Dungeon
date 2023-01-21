using UnityEngine;
using TMPro;

namespace UI.HUD
{
    public class ResistanceElement : MonoBehaviour
    {
        // CONFIG

        [SerializeField] TMP_Text value;
        [SerializeField] byte r = 0;
        [SerializeField] byte g = 0;
        [SerializeField] byte b = 0;

        // STATE

        Color32 white = Color.white;
        Color32 red = Color.white;

        void Start()
        {
            red = new Color32(r, g, b, 255);
        }

        // PUBLIC

        public void Setup(float amount)
        {
            if (amount == 0f)
            {
                gameObject.SetActive(false);
                return;
            }
            else
            {
                gameObject.SetActive(true);
                value.text = amount.ToString("N0") + "%";
                value.color = amount > 0f ? white : red;
            }
        }
    }
}