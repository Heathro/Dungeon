using UnityEngine;
using TMPro;
using Abilities;

namespace UI.HUD
{
    public class BuffTooltip : MonoBehaviour
    {
        // CONFIG

        [SerializeField] TMP_Text title;
        [SerializeField] TMP_Text description;

        // PUBLIC

        public void Setup(BuffEffect buffEffect, float duration)
        {
            title.text = buffEffect.GetDisplayName();
            description.text = buffEffect.GetDescription();
        }
    }
}