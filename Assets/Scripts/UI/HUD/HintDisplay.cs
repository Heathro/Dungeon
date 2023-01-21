using UnityEngine;
using TMPro;
using Core;

namespace UI.HUD
{
    public class HintDisplay : MonoBehaviour
    {
        // CONFIG

        [SerializeField] TMP_Text hint;
        [SerializeField] float minScale = 0.01f;
        [SerializeField] float maxScale = 0.02f;

        // CACHE

        FollowCamera followCamera;

        // STATE

        bool show = false;

        // LIFECYCLE

        void Start()
        {
            followCamera = FindObjectOfType<FollowCamera>();
        }

        void Update()
        {
            if (!show) hint.enabled = false;

            float zoomPercent = followCamera.GetZoomPercentage();
            float scale = minScale + ((maxScale - minScale) * zoomPercent / 100);
            transform.localScale = new Vector3(scale, scale, scale);
        }

        void LateUpdate()
        {
            transform.forward = Camera.main.transform.forward;
            show = false;
        }

        // PUBLIC

        public void SetText(string text)
        {
            hint.text = text;
        }

        public void EnableHint()
        {
            hint.enabled = true;
            show = true;
        }
    }
}