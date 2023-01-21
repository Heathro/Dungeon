using UnityEngine;
using UnityEngine.UI;

namespace UI.HUD
{
    public class CircleIndicator : MonoBehaviour
    {
        // CONFIG

        [SerializeField] Image grey;
        [SerializeField] Image red;
        [SerializeField] Image green;

        // PUBLIC

        public void SetIndicator(int number)
        {
            switch (number)
            {
                case 1:
                    grey.enabled = true;
                    red.enabled = false;
                    green.enabled = false;
                    break;

                case 2:
                    grey.enabled = false;
                    red.enabled = true;
                    green.enabled = false;
                    break;

                case 3:
                    grey.enabled = false;
                    red.enabled = false;
                    green.enabled = true;
                    break;

                default: break;
            }
        }
    }
}