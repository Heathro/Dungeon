using System.Collections;
using UnityEngine;
using Control;
using Attributes;
using Inventories;

namespace SceneManagement
{
    public class Fader : MonoBehaviour
    {
        // CONFIG

        [SerializeField] float fadeInTime = 0.5f;
        [SerializeField] float fadeOutTime = 0.5f;
        [SerializeField] float controlDelay = 0.5f;
        [SerializeField] float expDelayAmount = 2.5f;

        // CACHE

        LoadingLogo loadingLogo;
        CanvasGroup canvasGroup;
        Coroutine runningFade;

        // LIFECYCLE

        void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        void Start()
        {
            loadingLogo = FindObjectOfType<LoadingLogo>();
        }

        // PUBLIC

        public IEnumerator FadeOut()
        {
            return FadeRoutine(1, fadeOutTime);
        }

        public IEnumerator FadeIn(bool expDelay = false)
        {
            return FadeRoutine(0, fadeInTime, expDelay);
        }

        public void FadeOutImmediate()
        {
            canvasGroup.alpha = 1;
        }

        // PRIVATE

        IEnumerator FadeRoutine(float target, float time, bool expDelay = false)
        {
            bool fadeIn = target == 0;

            if (runningFade != null) StopCoroutine(runningFade);

            ControlSwitcher controlSwitcher = FindObjectOfType<ControlSwitcher>();

            if (fadeIn && controlSwitcher != null)
            {
                foreach (PlayerController player in controlSwitcher.GetPlayers())
                {
                    player.GetComponent<Health>().RestoreActualHealthPoints();
                    player.GetComponent<MagicArmor>().RestoreActualMagicPoints();
                    player.GetComponent<PhysicArmor>().RestoreActualPhysicPoints();
                }
            }

            if (fadeIn) loadingLogo.SetLogoActive(false);
            while (!Mathf.Approximately(canvasGroup.alpha, target))
            {
                canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, target, Time.deltaTime / time);
                yield return null;
            }
            if (!fadeIn) loadingLogo.SetLogoActive(true);

            if (fadeIn)
            {
                yield return new WaitForSeconds(controlDelay);
                if (expDelay) yield return new WaitForSeconds(expDelayAmount);

                if (controlSwitcher != null) controlSwitcher.SwitchPlayerWithDelay();
            }
        }
    }
}