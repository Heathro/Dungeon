using UnityEngine;

namespace Audio
{
    public class AudioController : MonoBehaviour
    {
        // CONFIG

        [SerializeField] Transform rightLegTransform;
        [SerializeField] Transform leftLegTransform;
        [SerializeField] Transform rightLegAudioModule;
        [SerializeField] Transform leftLegAudioModule;
        [SerializeField] AudioClip[] footsteps;

        [SerializeField] AudioSource mouth;

        [SerializeField] AudioClip[] hits;
        [SerializeField] AudioClip[] deaths;
        [SerializeField] AudioClip[] pains;

        [SerializeField] AudioSource body;

        [SerializeField] AudioClip unsheath;
        [SerializeField] AudioClip sheath;

        // CACHE

        AudioSource rightLegAudioSource = null;
        AudioSource leftLegAudioSource = null;

        // LIFECYCLE

        void Awake()
        {
            rightLegAudioSource = rightLegAudioModule.GetComponent<AudioSource>();
            leftLegAudioSource = leftLegAudioModule.GetComponent<AudioSource>();
        }

        void Update()
        {
            rightLegAudioModule.position = rightLegTransform.position;
            leftLegAudioModule.position = leftLegTransform.position;
        }

        // PUBLIC

        public void PlayFootstep(bool isRightStep)
        {
            int index = Random.Range(0, footsteps.Length);

            if (isRightStep)
            {
                rightLegAudioSource.PlayOneShot(footsteps[index]);
            }
            else
            {
                leftLegAudioSource.PlayOneShot(footsteps[index]);
            }
        }

        public void PlayHit()
        {
            mouth.PlayOneShot(hits[Random.Range(0, hits.Length)]);
        }

        public void PlayDeath()
        {
            mouth.PlayOneShot(deaths[Random.Range(0, deaths.Length)]);
        }

        public void PlayPain()
        {
            mouth.PlayOneShot(pains[Random.Range(0, pains.Length)]);
        }

        public void PlaySound(AudioClip sound)
        {
            body.PlayOneShot(sound);
        }

        public void PlaySheath(bool isUnsheath)
        {
            if (isUnsheath)
            {
                body.PlayOneShot(unsheath);
            }
            else
            {
                body.PlayOneShot(sheath);
            }
        }
    }
}