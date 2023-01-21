using UnityEngine;
using UI.Inventories;
using Utils.UI.Tooltips;

namespace UI.Menu
{
    public class SimpleTooltipSpawner : TooltipSpawner
    {
        // CONFIG

        [SerializeField][TextArea] string hint = "";

        // PUBLIC

        public override bool CanCreateTooltip()
        {
            bool canCreate = true;

            IItemHolder itemHolder = GetComponent<IItemHolder>();
            if (itemHolder != null && itemHolder.GetItem() != null)
            {
                canCreate = false;
            }

            return canCreate && !string.IsNullOrEmpty(hint);
        }

        public override void UpdateTooltip(GameObject tooltip)
        {
            var simpleTooltip = tooltip.GetComponent<SimpleTooltip>();
            simpleTooltip.Setup(hint);
        }
    }
}