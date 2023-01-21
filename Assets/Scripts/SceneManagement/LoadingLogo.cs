using UnityEngine;

namespace SceneManagement
{
    public class LoadingLogo : MonoBehaviour
    {        
        // CACHE
        
        CanvasGroup canvasGroup;

        // LIFECYCLE

        void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        // PUBLIC

        public void SetLogoActive(bool isActive)
        {
            canvasGroup.alpha = isActive ? 1f : 0f;
        }
    }
}