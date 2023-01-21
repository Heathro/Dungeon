using UnityEngine;

namespace Core
{
    public class PauseHub : MonoBehaviour
    {
        // STATE

        bool isPaused = false;
        bool canPause = true;
        bool goingToMain = false;

        // PUBLIC

        public void SetPauseEnable(bool isEnabled)
        {
            canPause = isEnabled;
        }

        public void GoingToMainMenu()
        {
            goingToMain = true;
        }

        public bool CanPause()
        {
            return canPause && !goingToMain;
        }

        public void SetPause(bool isPaused)
        {
            Time.timeScale = isPaused ? 0f : 1f;
            this.isPaused = isPaused;
        }

        public bool IsPaused()
        {
            return isPaused;
        }
    }
}