using UnityEngine;
using TMPro;

namespace UI.HUD
{
    public class APDisplay : MonoBehaviour
    {
        // CONFIG

        [SerializeField] TMP_Text accuracyText;
        [SerializeField] TMP_Text actionPointsText;
        [SerializeField] TMP_Text distanceText;
        [SerializeField] TMP_Text messageText;
        [SerializeField] Vector3 offset;

        // LIFECYCLE

        void Start()
        {
            accuracyText.enabled = false;
            actionPointsText.enabled = false;
            distanceText.enabled = false;
            messageText.enabled = false;
        }

        void LateUpdate()
        {
            transform.position = Input.mousePosition + offset;
        }

        // PUBLIC

        public void SetAccuracy(bool isEnable, float accuracy = 0)
        {
            if (!isEnable)
            {
                accuracyText.enabled = false;
                return;
            }

            accuracyText.enabled = true;
            accuracyText.text = accuracy.ToString("N0") + "%";
        }

        public void SetPoints(bool isEnable, int actionPoints = 0, bool isAvailable = true)
        {
            if (!isEnable)
            {
                actionPointsText.enabled = false;
                return;
            }

            actionPointsText.enabled = true;
            actionPointsText.color = isAvailable ? Color.white : Color.red;
            actionPointsText.text = actionPoints.ToString() + " AP";
        }

        public void SetDistance(bool isEnable, float distance = 0f)
        {
            if (!isEnable)
            {
                distanceText.enabled = false;
                return;
            }

            distanceText.enabled = true;
            distanceText.text = distance == 0 ? "" : distance.ToString("N1") + "m";
        }

        public void SetMessage(bool isEnable, string message = "", Color color = default(Color))
        {
            if (!isEnable)
            {
                messageText.enabled = false;
                return;
            }

            messageText.enabled = true;
            messageText.color = color;
            messageText.text = message;
        }
    }
}