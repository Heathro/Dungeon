using UnityEngine;
using UnityEngine.UI;
using Control;

namespace UI.Inventories
{
    public class IconSwitcher : MonoBehaviour
    {
        // CONFIG

        [SerializeField] Image[] icons;

        // STATE

        ControlSwitcher controlSwitcher;
        int currentPlayer;

        // LIFECYCLE

        void Start()
        {
            controlSwitcher = FindObjectOfType<ControlSwitcher>();
            controlSwitcher.onControlChange += SwitchIcon;
            SwitchIcon();
        }

        // PRIVATE

        void SwitchIcon()
        {
            currentPlayer = controlSwitcher.GetActivePlayerIndex();
            for (int i = 0; i < icons.Length; i++)
            {
                icons[i].enabled = i == currentPlayer;
            }
        }
    }
}