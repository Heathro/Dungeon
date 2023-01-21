using UnityEngine;
using TMPro;
using Control;
using Attributes;

namespace UI.HUD
{
    public class AttributeBar : MonoBehaviour
    {
        // CONFIG

        [SerializeField] PlayerController playerController;
        [SerializeField] RectTransform healthBar;
        [SerializeField] RectTransform physicBar;
        [SerializeField] RectTransform magicBar;
        [SerializeField] TMP_Text physicText;
        [SerializeField] TMP_Text magicText;
        [SerializeField] TMP_Text healthText;
        [SerializeField] APBar apBar;

        // CACHE

        Health health;
        MagicArmor magicArmor;
        PhysicArmor physicArmor;
        bool subscribed = false;

        // LIFECYCLE

        void Start()
        {
            playerController.onActionPointsUsageUpdate += UpdateAPBar;

            health = playerController.GetComponent<Health>();
            health.onHealthChange += SetHealthBar;

            magicArmor = playerController.GetComponent<MagicArmor>();
            magicArmor.onMagicArmorChange += SetMagicBar;

            physicArmor = playerController.GetComponent<PhysicArmor>();
            physicArmor.onPhysicArmorChange += SetPhysicBar;

            UpdateAttributes();
        }

        // PUBLIC

        public void SubscribeAPBars(FightScheduler fightScheduler)
        {
            if (subscribed) return;
            subscribed = true;

            fightScheduler.onFightFinish += DisableAPBar;

            fightScheduler.playerTurn += EnableAPBar;
            fightScheduler.enemyTurn += DisableAPBar;

            DisableAPBar();
        }

        // PRIVATE

        void UpdateAttributes()
        {
            SetHealthBar();
            SetPhysicBar();
            SetMagicBar();
        }

        void SetHealthBar()
        {
            healthText.text = health.GetHealth() + "/" + health.GetMaxHealth();
            healthBar.localScale = new Vector3(health.GetFraction(), 1, 1);
        }

        void SetPhysicBar()
        {
            physicText.text = physicArmor.GetPhysicArmor() + "/" + physicArmor.GetMaxPhysicArmor();
            physicBar.localScale = new Vector3(physicArmor.GetFraction(), 1, 1);
        }

        void SetMagicBar()
        {
            magicText.text = magicArmor.GetMagicArmor() + "/" + magicArmor.GetMaxMagicArmor();
            magicBar.localScale = new Vector3(magicArmor.GetFraction(), 1, 1);
        }

        void UpdateAPBar()
        {
            apBar.SetPoints(playerController.GetActionPoints(), playerController.GetUsingActionPoints());
        }

        void EnableAPBar()
        {
            apBar.gameObject.SetActive(true);
        }

        void DisableAPBar()
        {
            apBar.gameObject.SetActive(false);
        }
    }
}