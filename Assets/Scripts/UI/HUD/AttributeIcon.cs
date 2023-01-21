using UnityEngine;
using UnityEngine.UI;
using Attributes;
using Control;
using Dialogues;
using Stats;

namespace UI.HUD
{
    public class AttributeIcon : MonoBehaviour
    {
        // CONFIG

        [SerializeField] bool isAvatarIcon = false;
        [SerializeField] Health iconOwner;
        [SerializeField] Image avatar;
        [SerializeField] Image indicator;
        [SerializeField] RectTransform healthBar;
        [SerializeField] RectTransform physicBar;
        [SerializeField] RectTransform magicBar;
        [SerializeField] Image traitFlag;

        // STATE

        MagicArmor magicArmor;
        PhysicArmor physicArmor;
        ControlSwitcher controlSwitcher;
        TraitStore traitStore;

        // LIFECYCLE

        void Start()
        {
            indicator.enabled = false;
            traitFlag.enabled = false;

            avatar.sprite = iconOwner.GetComponent<AIConversant>().GetAIIcon();

            iconOwner.onHealthChange += SetHealthBar;

            magicArmor = iconOwner.GetComponent<MagicArmor>();
            magicArmor.onMagicArmorChange += SetMagicBar;

            physicArmor = iconOwner.GetComponent<PhysicArmor>();
            physicArmor.onPhysicArmorChange += SetPhysicBar;

            if (isAvatarIcon)
            {
                controlSwitcher = FindObjectOfType<ControlSwitcher>();
                controlSwitcher.onControlChange += UpdateIndicator;
                UpdateIndicator();

                traitStore = iconOwner.GetComponent<TraitStore>();
                traitStore.onTraitChange += UpdateTraitFlag;
                UpdateTraitFlag();
            }

            UpdateDisplay();
        }
        
        void OnDisable()
        {
            iconOwner.onHealthChange -= SetHealthBar;
            magicArmor.onMagicArmorChange -= SetMagicBar;
            physicArmor.onPhysicArmorChange -= SetPhysicBar;
            
            if (isAvatarIcon)
            {
                controlSwitcher.onControlChange -= UpdateIndicator;
                traitStore.onTraitChange -= UpdateTraitFlag;
            }
        }

        // PUBLIC

        public void SetOwner(Health owner)
        {
            iconOwner = owner;
        }

        // PRIVATE

        void UpdateDisplay()
        {
            SetHealthBar();
            SetPhysicBar();
            SetMagicBar();
        }

        void UpdateIndicator()
        {
            indicator.enabled = iconOwner.GetComponent<PlayerController>() == controlSwitcher.GetActivePlayer();
        }

        void UpdateTraitFlag()
        {
            traitFlag.enabled = iconOwner.GetComponent<TraitStore>().GetAvailablePoints() > 0;
        }

        void SetHealthBar()
        {
            if (!isAvatarIcon && iconOwner.IsDead()) Destroy(gameObject);

            healthBar.localScale = new Vector3(iconOwner.GetFraction(), 1, 1);
        }

        void SetPhysicBar()
        {
            physicBar.localScale = new Vector3(physicArmor.GetFraction(), 1, 1);
        }

        void SetMagicBar()
        {
            magicBar.localScale = new Vector3(magicArmor.GetFraction(), 1, 1);
        }
    }
}