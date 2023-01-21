using UnityEngine;

namespace Core
{
    public class Randomizer : MonoBehaviour
    {
        // STATE

        [SerializeField] float randomPercent = 25f;
        [SerializeField] float criticalPercent = 50f;

        // PUBLIC

        public float GetBottom(float input)
        {
            return input - input * randomPercent / 100;
        }

        public float GetTop(float input)
        {
            return input + input * randomPercent / 100;
        }

        public float Randomize(float input)
        {
            float percent = Random.Range(-randomPercent, randomPercent);
            return input + input * percent / 100;
        }
        
        public float ApplyForCritical(float input, float chance)
        {
            return Random.Range(1f, 100f) < chance ? input * criticalPercent / 100f : 0f;
        }
    }
}