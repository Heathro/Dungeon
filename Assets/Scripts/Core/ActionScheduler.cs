using UnityEngine;

namespace Core
{
    public class ActionScheduler : MonoBehaviour
    {
        // CACHE

        IAction currentAction;

        // PUBLIC

        public void StartAction(IAction action)
        {
            if (currentAction == action) return;

            if (currentAction != null)
            {
                currentAction.CancelAction();
            }
            currentAction = action;
        }

        public void CancelCurrentAction()
        {
            StartAction(null);
        }    
    }
}