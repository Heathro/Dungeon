using UnityEngine;

namespace UI.HUD
{
    public class APBar : MonoBehaviour
    {
        // CONFIG

        [SerializeField] CircleIndicator[] indicators;

        // PUBLIC

        public void SetPoints(int available, int inUse)
        {
            if (available < 0) available = 0;

            for (int i = 0; i < available - inUse; i++)
            {
                indicators[i].SetIndicator(3);
            }

            for (int i = available - inUse; i < available; i++)
            {
                indicators[i].SetIndicator(2);
            }

            for (int i = available; i < 6; i++)
            {
                indicators[i].SetIndicator(1);
            }
        }
    }
}