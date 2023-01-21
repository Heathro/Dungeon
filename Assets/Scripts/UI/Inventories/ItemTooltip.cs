using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Inventories;
using Utils;
using Abilities;
using Combat;
using Skills;
using Control;
using Stats;

namespace UI.Inventories
{
    public class ItemTooltip : MonoBehaviour
    {
        // CONFIG

        [SerializeField] Image icon = null;
        [SerializeField] TMP_Text displayName = null;
        [SerializeField] TMP_Text itemCategory = null;
        [SerializeField] Transform costParent = null;
        [SerializeField] GameObject costPrefab = null;

        [SerializeField] GameObject skillbookInfo = null;
        [SerializeField] Image skillIcon = null;
        [SerializeField] TMP_Text skillName = null;
        [SerializeField] TMP_Text skillDescription = null;
        [SerializeField] GameObject skillAdditionalInfoGap = null;
        [SerializeField] TMP_Text skillAdditionalInfo = null;
        [SerializeField] GameObject skillScalesGap = null;
        [SerializeField] TMP_Text skillScalesInstruction = null;
        [SerializeField] GameObject skillDistanceGap = null;
        [SerializeField] TMP_Text skillDistance = null;
        [SerializeField] GameObject skillWeaponRequirementGap = null;
        [SerializeField] TMP_Text skillWeaponRequirement = null;
        [SerializeField] TMP_Text skillCooldownText = null;
        [SerializeField] GameObject skillKnowGap = null;
        [SerializeField] TMP_Text skillKnowInstruction = null;
        [SerializeField] GameObject skillClassGap = null;
        [SerializeField] TMP_Text skillClassInstructionTrue = null;
        [SerializeField] TMP_Text skillClassInstructionFalse = null;

        [SerializeField] TMP_Text description = null;
        
        [SerializeField] GameObject additionalInfoGap = null;
        [SerializeField] TMP_Text additionalInfo = null;

        [SerializeField] GameObject scalesGap = null;
        [SerializeField] TMP_Text scalesInstruction = null;

        [SerializeField] GameObject distanceGap = null;
        [SerializeField] TMP_Text distance = null;

        [SerializeField] GameObject weaponRequirementGap = null;
        [SerializeField] TMP_Text weaponRequirementTrue = null;
        [SerializeField] TMP_Text weaponRequirementFalse = null;

        [SerializeField] Image cooldownIcon = null;
        [SerializeField] TMP_Text cooldownText = null;

        [SerializeField] Image weightIcon = null;
        [SerializeField] TMP_Text weightText = null;

        [SerializeField] Image priceIcon = null;
        [SerializeField] TMP_Text priceText = null;

        [SerializeField] GameObject instructionsGap = null;
        [SerializeField] GameObject learnInstruction = null;
        [SerializeField] GameObject useInstruction = null;
        [SerializeField] GameObject splitInstruction = null;
        [SerializeField] GameObject equipInstruction = null;
        [SerializeField] GameObject unequipInstruction = null;
        [SerializeField] GameObject collectInstruction = null;
        [SerializeField] GameObject addInstruction = null;
        [SerializeField] GameObject removeInstruction = null;

        // CACHE

        InventoryItem item = null;
        SlotType type = SlotType.None;
        int number = 0;
        int index = 0;
        bool isSplitable = false;

        // LIFECYCLE

        void Update()
        {
            if (Input.GetMouseButtonDown(1) && isSplitable)
            {
                SetupSpawnMenu();
                Destroy(gameObject);
            }
        }

        // PUBLIC

        public void Setup(InventoryItem item, SlotType type, int number, int index)
        {
            if (item == null)
            {
                Destroy(gameObject);
                return;
            }

            this.item = item;
            this.type = type;
            this.number = number;
            this.index = index;

            additionalInfo.enabled = false;
            additionalInfoGap.SetActive(false);

            weaponRequirementGap.SetActive(false);
            weaponRequirementFalse.enabled = false;
            weaponRequirementTrue.enabled = false;

            distanceGap.SetActive(false);
            distance.enabled = false;

            cooldownIcon.enabled = false;
            cooldownText.enabled = false;

            scalesGap.SetActive(false);
            scalesInstruction.enabled = false;

            skillbookInfo.SetActive(false);

            instructionsGap.SetActive(type == SlotType.None);
            collectInstruction.SetActive(type == SlotType.None);

            learnInstruction.SetActive(false);
            useInstruction.SetActive(false);
            equipInstruction.SetActive(false);
            unequipInstruction.SetActive(false);
            splitInstruction.SetActive(false);

            instructionsGap.SetActive(type == SlotType.PlayerBag || type == SlotType.VendorBag || type == SlotType.PlayerTable || type == SlotType.VendorTable);
            addInstruction.SetActive(type == SlotType.PlayerBag || type == SlotType.VendorBag);
            removeInstruction.SetActive(type == SlotType.PlayerTable || type == SlotType.VendorTable);

            SetMainAttributes();
            SetCategory();
            SetupPrice();
            SetupWeight();

            Ability ability = item as Ability;
            if (ability != null) SetupAbility(ability);

            WeaponConfig weaponConfig = item as WeaponConfig;
            if (weaponConfig != null) SetupWeaponConfig(weaponConfig);

            EquipableItem equipableItem = item as EquipableItem;
            if (equipableItem != null) SetupEquipableItem(equipableItem);

            SetupSplitInstruction();
        }

        // PRIVATE

        void SetMainAttributes()
        {
            icon.sprite = item.GetIcon();
            displayName.text = item.GetDisplayName();
            description.text = item.GetDescription();

            string additional = item.GetAdditionalInfo();
            if (!string.IsNullOrEmpty(additional))
            {
                additionalInfoGap.SetActive(true);
                additionalInfo.enabled = true;
                additionalInfo.text = additional;
            }
        }

        void SetCategory()
        {
            switch (item.GetItemCategory())
            {
                case ItemCategory.None: itemCategory.text = "Trash (Other)"; break;
                case ItemCategory.Weapon: itemCategory.text = "Weapon (Equipment)"; break;
                case ItemCategory.Trinket: itemCategory.text = "Trinket (Equipment)"; break;
                case ItemCategory.Necklace: itemCategory.text = "Necklace (Equipment)"; break;
                case ItemCategory.Ring: itemCategory.text = "Ring (Equipment)"; break;
                case ItemCategory.Skill: itemCategory.text = "Spell (Skill)"; break;
                case ItemCategory.Potion: itemCategory.text = "Potion (Consumable)"; break;
                case ItemCategory.Book: itemCategory.text = "Skillbook (Consumable)"; break;
                case ItemCategory.Currency: itemCategory.text = "Currency (Consumable)"; break;
                case ItemCategory.Scroll: itemCategory.text = "Spell (Scroll)"; break;
                case ItemCategory.Arrow: itemCategory.text = "Spell (Arrow)"; break;
                case ItemCategory.Quest: itemCategory.text = "Quest (Other)"; break;
                default: itemCategory.text = "Trash (Other)"; break;
            }
        }

        void SetupPrice()
        {
            if (item is QuestItem)
            {
                priceText.text = "not for sale";
                return;
            }

            int price = item.GetPrice();

            if (!(item is MoneyItem) && (type == SlotType.VendorBag || type == SlotType.VendorTable))
            {
                price *= 2;
            }

            priceText.text = (price * number).ToString();

            if (number > 1 && !(item is MoneyItem))
            {
                priceText.text += " (" + price + ")";
            }
        }

        void SetupWeight()
        {
            weightText.text = item.GetWeight().ToString("N1");
        }

        void SetupAbility(Ability ability)
        {
            Ability teachingSkill = ability.GetTeachSkill();

            cooldownIcon.enabled = true;
            cooldownText.enabled = true;
            cooldownText.text = ability.GetCooldown().ToString("N0");

            if (teachingSkill != null)
            {
                SetupSkillbook(teachingSkill, ability);
            }
            else
            {
                SetupSkill(ability);
            }
        }

        private void SetupSkill(Ability ability)
        {
            SetupUseCost(ability.GetCost());

            float range = ability.GetDistance();
            if (range > 0f)
            {
                distanceGap.SetActive(true);
                distance.enabled = range > 0f;
                distance.text = "Range " + range.ToString("N1") + " m";
            }

            if (!ability.IsConsumable())
            {
                weightIcon.enabled = false;
                weightText.enabled = false;
                priceIcon.enabled = false;
                priceText.enabled = false;
            }

            if (ability.IsScaleable())
            {
                scalesGap.SetActive(true);
                scalesInstruction.enabled = true;
                scalesInstruction.text = "Skill power scales with " + ability.GetScaleWith();
            }

            if (type == SlotType.PriestInventory || type == SlotType.PaladinInventory ||
                type == SlotType.HunterInventory || type == SlotType.MageInventory ||
                type == SlotType.PriestAction || type == SlotType.PaladinAction ||
                type == SlotType.HunterAction || type == SlotType.MageAction)
            {
                instructionsGap.SetActive(true);
                useInstruction.SetActive(true);
            }
            else if (type == SlotType.SkillDeck)
            {
                instructionsGap.SetActive(true);
                addInstruction.SetActive(true);
            }

            if (ability.IsMeleeRequired() || ability.IsRangedRequired())
            {
                weaponRequirementGap.SetActive(true);

                Equipment equipment = FindObjectOfType<ControlSwitcher>().GetActivePlayer().GetComponent<Equipment>();
                WeaponConfig weapon = equipment.GetItemInSlot(EquipLocation.Weapon) as WeaponConfig;

                if (ability.IsMeleeRequired())
                {
                    if (weapon != null && weapon.IsMelee())
                    {
                        weaponRequirementTrue.enabled = true;
                        weaponRequirementTrue.text = "Requires melee weapon";
                    }
                    else
                    {
                        weaponRequirementFalse.enabled = true;
                        weaponRequirementFalse.text = "Requires melee weapon";
                    }
                }
                if (ability.IsRangedRequired())
                {
                    if (weapon != null && weapon.IsRanged())
                    {
                        weaponRequirementTrue.enabled = true;
                        weaponRequirementTrue.text = "Requires ranged weapon";
                    }
                    else
                    {
                        weaponRequirementFalse.enabled = true;
                        weaponRequirementFalse.text = "Requires ranged weapon";
                    }
                }
            }
        }

        private void SetupSkillbook(Ability teachingSkill, Ability book)
        {
            SetupUseCost(teachingSkill.GetCost());

            skillbookInfo.SetActive(true);

            description.enabled = false;
            scalesGap.SetActive(false);
            scalesInstruction.enabled = false;
            distanceGap.SetActive(false);
            distance.enabled = false;

            PlayerController currentPlayer = FindObjectOfType<ControlSwitcher>().GetActivePlayer();

            skillClassGap.SetActive(true);
            CharacterClass currentClass = currentPlayer.GetComponent<BaseStats>().GetClass();
            if (currentClass == book.GetClass())
            {
                skillClassInstructionTrue.enabled = true;
                skillClassInstructionTrue.text = "Class: " + book.GetClass();
                skillClassInstructionFalse.enabled = false;
            }
            else
            {
                skillClassInstructionTrue.enabled = false;
                skillClassInstructionFalse.enabled = true;
                skillClassInstructionFalse.text = "Class: " + book.GetClass();
            }
            
            if (currentPlayer.GetComponent<SkillStore>().HasSkill(teachingSkill))
            {
                skillKnowGap.SetActive(true);
                skillKnowInstruction.enabled = true;
            }
            else
            {
                skillKnowGap.SetActive(false);
                skillKnowInstruction.enabled = false;
            }

            skillIcon.sprite = teachingSkill.GetIcon();
            skillName.text = teachingSkill.GetDisplayName();
            skillDescription.text = teachingSkill.GetDescription();

            string skillAdditional = teachingSkill.GetAdditionalInfo();
            if (!string.IsNullOrEmpty(skillAdditional))
            {
                skillAdditionalInfoGap.SetActive(true);
                skillAdditionalInfo.enabled = true;
                skillAdditionalInfo.text = skillAdditional;
            }

            float skillRange = teachingSkill.GetDistance();
            if (skillRange > 0f)
            {
                skillDistanceGap.SetActive(true);
                skillDistance.enabled = skillRange > 0f;
                skillDistance.text = "Range " + skillRange.ToString("N1") + " m";
            }

            skillDistance.enabled = skillRange > 0f;
            skillDistance.text = "Range " + skillRange.ToString("N1") + " m";

            skillCooldownText.text = "Cooldown " + teachingSkill.GetCooldown().ToString("N0") + " turns";

            if (teachingSkill.IsScaleable())
            {
                skillScalesGap.SetActive(true);
                skillScalesInstruction.enabled = true;
                skillScalesInstruction.text = "Skill power scales with " + teachingSkill.GetScaleWith();
            }

            if (type == SlotType.PriestInventory || type == SlotType.PaladinInventory ||
                type == SlotType.HunterInventory || type == SlotType.MageInventory ||
                type == SlotType.PriestAction || type == SlotType.PaladinAction ||
                type == SlotType.HunterAction || type == SlotType.MageAction)
            {
                instructionsGap.SetActive(true);
                learnInstruction.SetActive(true);
            }

            if (teachingSkill.IsMeleeRequired())
            {
                skillWeaponRequirementGap.SetActive(true);
                skillWeaponRequirement.enabled = true;
                skillWeaponRequirement.text = "Requires melee weapon";
            }
            else if (teachingSkill.IsRangedRequired())
            {
                skillWeaponRequirementGap.SetActive(true);
                skillWeaponRequirement.enabled = true;
                skillWeaponRequirement.text = "Requires ranged weapon";
            }
            else
            {
                skillWeaponRequirementGap.SetActive(false);
                skillWeaponRequirement.enabled = false;
            }
        }

        void SetupWeaponConfig(WeaponConfig weaponConfig)
        {
            SetupUseCost(weaponConfig.GetCost());

            distanceGap.SetActive(true);
            distance.enabled = true;
            distance.text = "Range: " + weaponConfig.GetRange().ToString("N1") + " m";
            
            scalesGap.SetActive(true);
            scalesInstruction.enabled = true;
            scalesInstruction.text = "Damage scales with " + weaponConfig.GetScaleWith();
        }

        void SetupEquipableItem(EquipableItem equipableItem)
        {
            if (type == SlotType.PriestInventory || type == SlotType.PaladinInventory ||
                type == SlotType.HunterInventory || type == SlotType.MageInventory)
            {
                instructionsGap.SetActive(true);
                equipInstruction.SetActive(true);
            }
            else if (type == SlotType.Equipment)
            {
                instructionsGap.SetActive(true);
                unequipInstruction.SetActive(true);
            }
        }

        void SetupUseCost(int amount)
        {
            foreach (Transform child in costParent)
            {
                Destroy(child.gameObject);
            }

            for (int i = 0; i < amount; i++)
            {
                Instantiate(costPrefab, costParent);
            }
        }

        void SetupSplitInstruction()
        {
            isSplitable = number > 1 && (type == SlotType.PriestInventory || type == SlotType.PaladinInventory ||
                                         type == SlotType.HunterInventory || type == SlotType.MageInventory ||
                                         type == SlotType.PlayerBag || type == SlotType.PlayerTable ||
                                         type == SlotType.VendorBag || type == SlotType.VendorTable ||
                                         type == SlotType.None);

            if (isSplitable)
            {
                instructionsGap.SetActive(true);
                splitInstruction.SetActive(true);
            }
        }

        void SetupSpawnMenu()
        {
            ShowHideUI showHideUI = FindObjectOfType<ShowHideUI>();            
            SplitMenu splitMenu = showHideUI.GetSplitMenu().GetComponent<SplitMenu>();
            showHideUI.OpenSplitMenu();
            splitMenu.Setup(item, type, number, index, showHideUI.GetAudioHub());
        }
    }
}