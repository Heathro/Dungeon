using System.Collections;
using UnityEngine;

namespace Audio
{
    public class AudioPlayer : MonoBehaviour
    {
        // CONFIG

        [SerializeField] AudioClip menu;
        [SerializeField] AudioClip civil;
        [SerializeField] AudioClip[] battle;
        [SerializeField] AudioClip fight;

        // CACHE

        AudioSource audioSource = null;

        // LIFECYCLE

        void Awake()
        {
            audioSource = GetComponent<AudioSource>();
        }

        // PUBLIC

        public void PlayMenuTheme()
        {
            audioSource.clip = menu;
            audioSource.Play();
        }

        public void PlayCivilTheme()
        {
            if (audioSource.clip == civil) return;

            audioSource.Stop();
            audioSource.clip = civil;
            audioSource.Play();
        }

        public void PlayBattleTheme()
        {
            StartCoroutine(SwitchToBattleTheme());
        }

        public void StopMusic()
        {
            audioSource.Stop();
        }

        // PRIVATE

        IEnumerator SwitchToBattleTheme()
        {
            audioSource.Stop();
            audioSource.PlayOneShot(fight);

            yield return new WaitForSeconds(2f);

            audioSource.clip = battle[UnityEngine.Random.Range(0, battle.Length)];
            audioSource.Play();
        }
    }
}