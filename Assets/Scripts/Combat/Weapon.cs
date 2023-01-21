using UnityEngine;

namespace Combat
{
    public class Weapon : MonoBehaviour
    {
        // PUBLIC

        public void OnHit()
        {
            AudioSource audio = GetComponent<AudioSource>();
            if (audio == null) return;
            
            audio.Play();
        }
    }
}