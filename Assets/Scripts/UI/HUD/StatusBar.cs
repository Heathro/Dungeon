using UnityEngine;
using TMPro;
using Attributes;
using Abilities;
using Stats;
using Dialogues;

namespace UI.HUD
{
    public class StatusBar : MonoBehaviour
    {
        // CONFIG

        [SerializeField] TMP_Text nameText;
        [SerializeField] RectTransform healthBar;
        [SerializeField] RectTransform physicBar;
        [SerializeField] RectTransform magicBar;
        [SerializeField] TMP_Text healthText;
        [SerializeField] TMP_Text physicText;
        [SerializeField] TMP_Text magicText;
        [SerializeField] TMP_Text levelText;
        [SerializeField] BuffDisplay buffDisplay;
        [SerializeField] ResistanceDisplay resistanceDisplay;
 
        // CACHE

        Health health = null;
        MagicArmor magicArmor = null;
        PhysicArmor physicArmor = null;
        BaseStats baseStats = null;

        // PUBLIC

        public void EnableStatusBar(bool isEnable)
        {
            gameObject.SetActive(isEnable);
            buffDisplay.gameObject.SetActive(isEnable);
            resistanceDisplay.gameObject.SetActive(isEnable);
        }

        public void SetUpStatusBar(Health health)
        {
            nameText.text = health.GetComponent<AIConversant>().GetAIName();

            this.health = health;
            this.health.onHealthChange += UpdateHealth;

            magicArmor = health.GetComponent<MagicArmor>();
            magicArmor.onMagicArmorChange += UpdateMagicArmor;

            physicArmor = health.GetComponent<PhysicArmor>();
            physicArmor.onPhysicArmorChange += UpdatePhysicArmor;

            baseStats = health.GetComponent<BaseStats>();
            baseStats.onLevelUp += UpdateLevel;

            buffDisplay.SetBuffStore(health.GetComponent<BuffStore>());
            resistanceDisplay.SetResistanceBar(baseStats);

            Redraw();
        }

        // PRIVATE

        void Redraw()
        {
            UpdateHealth();
            UpdateMagicArmor();
            UpdatePhysicArmor();
            UpdateLevel();
        }

        void UpdateHealth()
        {
            healthBar.localScale = new Vector3(health.GetFraction(), 1, 1);
            healthText.text = health.GetHealth() + "/" + health.GetMaxHealth();
        }

        void UpdateMagicArmor()
        {
            magicBar.localScale = new Vector3(magicArmor.GetFraction(), 1, 1);
            magicText.text = magicArmor.GetMagicArmor() + "/" + magicArmor.GetMaxMagicArmor();
        }

        void UpdatePhysicArmor()
        {
            physicBar.localScale = new Vector3(physicArmor.GetFraction(), 1, 1);
            physicText.text = physicArmor.GetPhysicArmor() + "/" + physicArmor.GetMaxPhysicArmor();
        }

        void UpdateLevel()
        {
            levelText.text = "Level " + health.GetComponent<BaseStats>().GetLevel().ToString();
        }
    }
}