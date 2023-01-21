using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Abilities;
using Utils.UI.Tooltips;

namespace UI.HUD
{
    public class BuffElement : TooltipSpawner
    {
        // CONFIG

        [SerializeField] Image icon;
        [SerializeField] TMP_Text number;

        // STATE

        BuffEffect buffEffect = null;
        float duration = 0f;

        // PUBLIC

        public void Setup(BuffEffect buffEffect, float duration)
        {
            this.buffEffect = buffEffect;
            this.duration = Mathf.Ceil(duration);

            icon.sprite = buffEffect.GetIcon();
            number.text = buffEffect.IsPermanent() ? "" : this.duration.ToString();
        }        
        
        public override bool CanCreateTooltip()
        {
            return true;
        }

        public override void UpdateTooltip(GameObject tooltip)
        {
            var itemTooltip = tooltip.GetComponent<BuffTooltip>();
            itemTooltip.Setup(buffEffect, duration);
        }
    }
}