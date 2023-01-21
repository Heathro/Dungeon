using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Utils;
using Core;

namespace UI.Ingame
{
    public class OverheadUI : MonoBehaviour
    {
        // CONFIG

        [SerializeField] GameObject popUpPrefab;
        [SerializeField] float popUpInterval = 0.4f;
        [SerializeField] float popUpDestroyDelay = 1.5f;
        [SerializeField] float minScale = 0.25f;
        [SerializeField] float maxScale = 1f;

        [SerializeField] PopUpColor health;
        [SerializeField] PopUpColor physic;
        [SerializeField] PopUpColor magic;
        [SerializeField] PopUpColor fire;
        [SerializeField] PopUpColor water;
        [SerializeField] PopUpColor earth;
        [SerializeField] PopUpColor air;
        [SerializeField] PopUpColor poison;
        [SerializeField] PopUpColor experience;
        [SerializeField] PopUpColor miss;
        [SerializeField] PopUpColor heal;
        [SerializeField] PopUpColor critical;

        [System.Serializable]
        public class PopUpColor
        {
            public byte r = 0;
            public byte g = 0;
            public byte b = 0;
            public byte a = 255;
        }

        // CACHE

        FollowCamera followCamera;

        // STATE

        float popUpTimer = Mathf.Infinity;
        Queue<PopUp> popUps = new Queue<PopUp>();

        public class PopUp
        {
            public float amount = 0f;
            public Color32 color = Color.red;
            public bool plus = false;
            public string message = "";
        }

        // LIFECYCLE

        void Start()
        {
            followCamera = FindObjectOfType<FollowCamera>();
        }

        void Update()
        {
            if (popUps.Count == 0)
            {
                popUpTimer = Mathf.Infinity;
                return;
            }

            popUpTimer += Time.deltaTime;

            if (popUpTimer > popUpInterval)
            {
                SpawnPopUp(popUps.Dequeue());
                popUpTimer = 0f;
            }
        }

        void LateUpdate()
        {
            transform.forward = Camera.main.transform.forward;
        }

        // PUBLIC

        public void AddPopUp(float amount, DamageType damageType, bool plus, string message = "")
        {
            PopUp popUp = new PopUp();
            popUp.amount = amount;
            popUp.color = GetColor(damageType);
            popUp.plus = plus;
            popUp.message = message;
            popUps.Enqueue(popUp);
        }

        public void SpawnPopUp(PopUp popUp)
        {
            GameObject popUpUI = Instantiate(popUpPrefab, transform);

            string popUpNumber = popUp.plus ? "+" : "";
            string popUpMessage = "";

            if (popUp.amount != 0f)
            {
                popUpNumber += popUp.amount.ToString();
            }

            if (!string.IsNullOrEmpty(popUp.message))
            {
                popUpMessage = " " + popUp.message;
            }

            TMP_Text textComponent = popUpUI.GetComponentInChildren<TMP_Text>();
            textComponent.text = popUpNumber + popUpMessage;
            textComponent.color = popUp.color;
            SetCorrectScale(popUpUI);

            Destroy(popUpUI, popUpDestroyDelay);
        }

        // PRIVATE

        void SetCorrectScale(GameObject popUp)
        {
            float zoomPercent = followCamera.GetZoomPercentage();
            float scale = minScale + ((maxScale - minScale) * zoomPercent / 100);
            popUp.transform.localScale = new Vector3(scale, scale, scale);
        }

        Color32 GetColor(DamageType damageType)
        {
            switch (damageType)
            {
                case DamageType.Health: return new Color32(health.r, health.g, health.b, health.a);
                case DamageType.Physic: return new Color32(physic.r, physic.g, physic.b, physic.a);
                case DamageType.Magic: return new Color32(magic.r, magic.g, magic.b, magic.a);
                case DamageType.Fire: return new Color32(fire.r, fire.g, fire.b, fire.a);
                case DamageType.Water: return new Color32(water.r, water.g, water.b, water.a);
                case DamageType.Earth: return new Color32(earth.r, earth.g, earth.b, earth.a);
                case DamageType.Air: return new Color32(air.r, air.g, air.b, air.a);
                case DamageType.Poison: return new Color32(poison.r, poison.g, poison.b, poison.a);
                case DamageType.Experience: return new Color32(experience.r, experience.g, experience.b, experience.a);
                case DamageType.Miss: return new Color32(miss.r, miss.g, miss.b, miss.a);
                case DamageType.Heal: return new Color32(heal.r, heal.g, heal.b, heal.a);
                case DamageType.Critical: return new Color32(critical.r, critical.g, critical.b, critical.a);
                default: return Color.black;
            }
        }
    }
}