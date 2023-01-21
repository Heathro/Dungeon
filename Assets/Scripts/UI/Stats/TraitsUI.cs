using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Stats;
using Inventories;
using Combat;
using Attributes;
using Core;
using Dialogues;
using Abilities;

namespace UI.Stats
{
    public class TraitsUI : MonoBehaviour
    {
        // CONFIG

        [SerializeField] TraitStore traitStore;

        [SerializeField] TMP_Text availableText;
        [SerializeField] TMP_Text strengthValue;
        [SerializeField] TMP_Text finesseValue;
        [SerializeField] TMP_Text intelligenceValue;
        [SerializeField] TMP_Text constitutionValue;
        [SerializeField] TMP_Text witsValue;
        [SerializeField] Button strengthButton;
        [SerializeField] Button finesseButton;
        [SerializeField] Button intelligenceButton;
        [SerializeField] Button constitutionButton;
        [SerializeField] Button witsButton;

        [SerializeField] TMP_Text damageValue;
        [SerializeField] TMP_Text criticalChanceValue;
        [SerializeField] TMP_Text accuracyValue;
        [SerializeField] TMP_Text dodgingValue;
        [SerializeField] TMP_Text physicArmorValue;
        [SerializeField] TMP_Text magicArmorValue;
        [SerializeField] TMP_Text movementValue;
        [SerializeField] TMP_Text initiativeValue;
        [SerializeField] TMP_Text experienceValue;
        [SerializeField] TMP_Text nextLevelValue;
        [SerializeField] TMP_Text fireValue;
        [SerializeField] TMP_Text waterValue;
        [SerializeField] TMP_Text earthValue;
        [SerializeField] TMP_Text airValue;
        [SerializeField] TMP_Text poisonValue;

        [SerializeField] TMP_Text nameLevelText;

        // CACHE

        BaseStats baseStats;
        Experience experience;
        Fighter fighter;
        Equipment equipment;
        PhysicArmor physicArmor;
        MagicArmor magicArmor;
        Randomizer randomizer;
        AIConversant aIConversant;
        BuffStore buffStore;

        // STATE

        bool updating = false;

        // LIFECYCLE

        void Start()
        {
            randomizer = FindObjectOfType<Randomizer>();

            traitStore.onTraitChange += UpdateUI;

            fighter = traitStore.GetComponent<Fighter>();

            equipment = traitStore.GetComponent<Equipment>();
            equipment.equipmentUpdated += UpdateUI;

            magicArmor = traitStore.GetComponent<MagicArmor>();
            magicArmor.onMagicArmorChange += UpdateUI;

            physicArmor = traitStore.GetComponent<PhysicArmor>();
            physicArmor.onPhysicArmorChange += UpdateUI;

            experience = traitStore.GetComponent<Experience>();
            experience.onExperienceGained += UpdateUI;

            baseStats = traitStore.GetComponent<BaseStats>();
            baseStats.onLevelUp += UpdateUI;

            buffStore = traitStore.GetComponent<BuffStore>();
            buffStore.buffStoreUpdated += UpdateUI;

            aIConversant = traitStore.GetComponent<AIConversant>();

            strengthButton.onClick.AddListener(() => Allocate(CharacterTrait.Strength));
            finesseButton.onClick.AddListener(() => Allocate(CharacterTrait.Finesse));
            intelligenceButton.onClick.AddListener(() => Allocate(CharacterTrait.Intelligence));
            constitutionButton.onClick.AddListener(() => Allocate(CharacterTrait.Constitution));
            witsButton.onClick.AddListener(() => Allocate(CharacterTrait.Wits));

            UpdateAtStart();
        }

        // PRIVATE

        void Allocate(CharacterTrait trait)
        {
            traitStore.Allocate(trait);
            UpdateUI();
        }

        void UpdateUI()
        {
            if (updating) return;
            updating = true;
            randomizer.StartCoroutine(Updating());
        }

        void UpdateAtStart()
        {
            UpdateTraits();
            UpdateDamage();
            UpdateStats();
            UpdatePhysicArmor();
            UpdateMagicArmor();
            UpdateExperience();
            UpdateNextLevel();
        }

        IEnumerator Updating()
        {
            yield return new WaitForSeconds(0.1f);

            UpdateTraits();
            UpdateDamage();
            UpdateStats();
            UpdatePhysicArmor();
            UpdateMagicArmor();
            UpdateExperience();
            UpdateNextLevel();

            updating = false;
        }

        void UpdateTraits()
        {
            int availablePoints = traitStore.GetAvailablePoints();

            if (availablePoints == 0)
            {
                availableText.text = "";
                strengthButton.gameObject.SetActive(false);
                finesseButton.gameObject.SetActive(false);
                intelligenceButton.gameObject.SetActive(false);
                constitutionButton.gameObject.SetActive(false);
                witsButton.gameObject.SetActive(false);
            }
            else
            {
                availableText.text = "Available " + availablePoints;
                strengthButton.gameObject.SetActive(true);
                finesseButton.gameObject.SetActive(true);
                intelligenceButton.gameObject.SetActive(true);
                constitutionButton.gameObject.SetActive(true);
                witsButton.gameObject.SetActive(true);
            }
            
            strengthValue.text = traitStore.GetTraitValue(CharacterTrait.Strength).ToString();
            finesseValue.text = traitStore.GetTraitValue(CharacterTrait.Finesse).ToString();
            intelligenceValue.text = traitStore.GetTraitValue(CharacterTrait.Intelligence).ToString();
            constitutionValue.text = traitStore.GetTraitValue(CharacterTrait.Constitution).ToString();
            witsValue.text = traitStore.GetTraitValue(CharacterTrait.Wits).ToString();
        }

        void UpdateDamage()
        {
            float baseDamage = fighter.GetDamage();
            float bottomDamage = randomizer.GetBottom(baseDamage);
            float topDamage = randomizer.GetTop(baseDamage);
            damageValue.text = bottomDamage.ToString("N0") + " - " + topDamage.ToString("N0");
        }

        void UpdateStats()
        {
            criticalChanceValue.text = baseStats.GetStat(CharacterStat.CriticalChance).ToString("N0") + "%";
            accuracyValue.text = baseStats.GetStat(CharacterStat.Accuracy).ToString("N0") + "%";
            dodgingValue.text = baseStats.GetStat(CharacterStat.Dodging).ToString("N0") + "%";
            movementValue.text = baseStats.GetStat(CharacterStat.Speed).ToString("N1");
            initiativeValue.text = baseStats.GetStat(CharacterStat.Initiative).ToString("N0");            
            fireValue.text = baseStats.GetStat(CharacterStat.FireResistance).ToString("N0") + "%";
            waterValue.text = baseStats.GetStat(CharacterStat.WaterResistance).ToString("N0") + "%";
            earthValue.text = baseStats.GetStat(CharacterStat.EarthResistance).ToString("N0") + "%";
            airValue.text = baseStats.GetStat(CharacterStat.AirResistance).ToString("N0") + "%";
            poisonValue.text = baseStats.GetStat(CharacterStat.PoisonResistance).ToString("N0") + "%";
        }

        void UpdatePhysicArmor()
        {
            physicArmorValue.text = physicArmor.GetPhysicArmor().ToString("N0") + "/" + physicArmor.GetMaxPhysicArmor().ToString("N0");
        }

        void UpdateMagicArmor()
        {
            magicArmorValue.text = magicArmor.GetMagicArmor().ToString("N0") + "/" + magicArmor.GetMaxMagicArmor().ToString("N0");
        }

        void UpdateExperience()
        {
            experienceValue.text = experience.GetPoints().ToString("N0");
        }

        void UpdateNextLevel()
        {
            nextLevelValue.text = baseStats.GetNextLevel().ToString("N0");
            nameLevelText.text = aIConversant.GetAIName() + "  -  Level " + baseStats.GetLevel();
        }
    }
}