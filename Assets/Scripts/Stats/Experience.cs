using System;
using System.Collections;
using UnityEngine;
using Saving;
using UI.Ingame;
using Utils;
using Audio;

namespace Stats
{
    public class Experience : MonoBehaviour, ISaveable
    {
        // CACHE

        OverheadUI overheadUI;
        AudioHub audioHub;

        // STATE

        public event Action onExperienceGained;

        float experiencePoints = 0f;
        static bool soundPlaying = false;

        // LIFECYCLE

        void Awake()
        {
            overheadUI = GetComponentInChildren<OverheadUI>();
            audioHub = FindObjectOfType<AudioHub>();
        }

        // PUBLIC

        public float GetPoints()
        {
            return experiencePoints;
        }

        public void GainExperience(float amount)
        {
            if (!soundPlaying) audioHub.StartCoroutine(PlaySound());

            overheadUI.AddPopUp(amount, DamageType.Experience, true, "XP");

            experiencePoints += amount;
            if (onExperienceGained != null)
            {
                onExperienceGained();
            }
        }

        // PRIVATE

        IEnumerator PlaySound()
        {
            soundPlaying = true;
            audioHub.PlayExperience();
            yield return new WaitForSeconds(1f);
            soundPlaying = false;
        }
        
        object ISaveable.CaptureState()
        {
            return experiencePoints;
        }

        void ISaveable.RestoreState(object state)
        {
            experiencePoints = (float)state;
            GetComponent<BaseStats>().InitLevel();
        }
    }
}