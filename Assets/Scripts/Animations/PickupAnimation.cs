using System;
using UnityEngine;

namespace Animations
{
    public class PickupAnimation : StateMachineBehaviour
    {
        // STATE

        Action onFinish = null;

        // PUBLIC

        public void SetOnFinishCallBack(Action onFinish)
        {
            this.onFinish = onFinish;
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (onFinish != null)
            {
                onFinish();
            }
        }
    }
}