using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UI.HUD
{
    public class FightAlert : MonoBehaviour
    {
        // CONFIG

        [SerializeField] Image alertBackground;
        [SerializeField] TMP_Text alertText;
        [SerializeField] float alertTime = 1f;

        // STATE

        float airTime = 0f;
        bool trigger = false;

        // LIFECYCLE

        void Start()
        {
            alertBackground.enabled = false;
            alertText.enabled = false;
        }

        void Update()
        {
            if (trigger == false) return;

            airTime += Time.deltaTime;

            if (airTime > alertTime)
            {
                trigger = false;
                airTime = 0f;
                alertText.enabled = false;
                alertBackground.enabled = false;
            }
        }

        // PUBLIC

        public void TriggerAlert()
        {
            trigger = true;
            alertText.enabled = true;
            alertBackground.enabled = true;
        }
    }
}