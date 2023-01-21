using UnityEngine;
using Utils.UI.Tooltips;
using Core;

namespace UI.Inventories
{
    public class ItemTooltipSpawner : TooltipSpawner
    {
        // CACHE

        FollowCamera followCamera = null;
        PauseHub pauseHub = null;
        ShowHideUI showHideUI = null;

        // LIFECYCLE

        void Start()
        {
            followCamera = FindObjectOfType<FollowCamera>();
            pauseHub = FindObjectOfType<PauseHub>();
            showHideUI = FindObjectOfType<ShowHideUI>();
        }

        void Update()
        {
            if (pauseHub.IsPaused() || showHideUI.IsSplitMenuActive() || !IsMouseOver())
            {
                ClearTooltip();
            }
            else if (tooltip != null && Input.GetMouseButtonUp(0) && GetComponent<IItemHolder>().GetItem() == null)
            {
                ClearTooltip();
            }
            else if (tooltip != null && followCamera.IsDragging())
            {
                ClearTooltip();
            }
            else if (tooltip == null && IsMouseOver())
            {
                CreateTooltip();
            }
        }

        bool IsMouseOver()
        {
            return Input.mousePosition.x > transform.position.x - 32 && Input.mousePosition.x < transform.position.x + 32 &&
                   Input.mousePosition.y > transform.position.y - 32 && Input.mousePosition.y < transform.position.y + 32;
        }

        // PUBLIC

        public void ForceClearTooltip()
        {
            ClearTooltip();
        }

        public override bool CanCreateTooltip()
        {
            if (followCamera.IsDragging()) return false;

            var item = GetComponent<IItemHolder>().GetItem();
            if (!item) return false;

            return true;
        }

        public override void UpdateTooltip(GameObject tooltip)
        {
            var itemTooltip = tooltip.GetComponent<ItemTooltip>();

            if (!itemTooltip) return;

            var item = GetComponent<IItemHolder>().GetItem();
            var type = GetComponent<IItemHolder>().GetSlotType();
            var number = GetComponent<IItemHolder>().GetNumber();
            var index = GetComponent<IItemHolder>().GetIndex();

            itemTooltip.Setup(item, type, number, index);
        }
    }
}