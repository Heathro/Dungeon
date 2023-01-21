using System.Collections;
using UnityEngine;

namespace Audio
{
    public class AudioHub : MonoBehaviour
    {
        // CONFIG

        [SerializeField] AudioClip experience;
        [SerializeField] AudioClip doorOpen;
        [SerializeField] AudioClip doorClose;

        [SerializeField] AudioClip click;
        [SerializeField] AudioClip equip;
        [SerializeField] AudioClip pickup;
        [SerializeField] AudioClip drop;
        [SerializeField] AudioClip move;
        [SerializeField] AudioClip error;
        [SerializeField] AudioClip money;
        [SerializeField] AudioClip level;
        [SerializeField] AudioClip open;
        [SerializeField] AudioClip collect;

        // CACHE

        AudioSource audioSource = null;

        // STATE

        bool equipmentRunning = false;

        // LIFECYCLE

        void Awake()
        {
            audioSource = GetComponent<AudioSource>();
        }

        // PUBLIC

        public void PlayClick()
        {
            audioSource.PlayOneShot(click);
        }

        public void PlayEquip()
        {
            if (equipmentRunning) return;
            StartCoroutine(EquipmentRoutine());
        }

        public void PlayPickup()
        {
            audioSource.PlayOneShot(pickup);
        }

        public void PlayDrop()
        {
            audioSource.PlayOneShot(drop);
        }

        public void PlayMove()
        {
            audioSource.PlayOneShot(move);
        }

        public void PlayError()
        {
            audioSource.PlayOneShot(error);
        }

        public void PlayMoney()
        {
            audioSource.PlayOneShot(money);
        }

        public void PlayLevel()
        {
            audioSource.PlayOneShot(level);
        }

        public void PlayCollect()
        {
            audioSource.PlayOneShot(collect);
        }

        public void PlayOpen()
        {
            audioSource.PlayOneShot(open);
        }

        public void PlayExperience()
        {
            audioSource.PlayOneShot(experience);
        }

        public void PlayDoor(bool isOpening)
        {
            if (isOpening)
            {
                audioSource.PlayOneShot(doorOpen);
            }
            else
            {
                audioSource.PlayOneShot(doorClose);
            }
        }

        // PRIVATE

        IEnumerator EquipmentRoutine()
        {
            yield return new WaitForEndOfFrame();
            equipmentRunning = false;
            audioSource.PlayOneShot(equip);
        }
    }
}