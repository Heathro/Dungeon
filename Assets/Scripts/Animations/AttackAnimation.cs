using System;
using UnityEngine;

namespace Animations
{
    public class AttackAnimation : StateMachineBehaviour
    {
        // STATE

        Action finish = null;

        // PUBLIC

        public void SetAnimationCallback(Action finish)
        {
            this.finish = finish; 
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (finish != null)
            {
                finish();
            }
        }
    }
}