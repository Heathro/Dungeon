using UnityEngine;
using UI.Inventories;
using Utils.UI.Tooltips;

namespace UI.Menu
{
    public class ButtonTooltipSpawner : TooltipSpawner
    {
        // CONFIG

        [SerializeField] string hint = "";

        // PUBLIC

        public override bool CanCreateTooltip()
        {
            return !string.IsNullOrEmpty(hint);
        }

        public override void UpdateTooltip(GameObject tooltip)
        {
            var buttonTooltip = tooltip.GetComponent<ButtonTooltip>();
            buttonTooltip.Setup(hint);
        }
    }
}