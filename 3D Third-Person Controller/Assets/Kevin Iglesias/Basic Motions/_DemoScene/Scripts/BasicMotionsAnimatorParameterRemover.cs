/*
=============================================================================================
BasicMotionsAnimatorParameterRemover.cs

This script is needed for the "BasicMotionsCharacterController.cs" script to work properly.

This script disables Animator Parameter when the Animator enters an Animator Controller State
with this script attached (simulates Trigger Parameter but with Bool Parameter) or resets
an Integer Parameter to its default value of 0.

https://www.keviniglesias.com/
support@keviniglesias.com
=============================================================================================
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KevinIglesias
{
    public class BasicMotionsAnimatorParameterRemover : StateMachineBehaviour
    {
        [SerializeField]
        private string removeParameter = ""; //ANIMATOR PARAMETER TO BE DISABLED OR RESET
        
        [SerializeField]
        private bool isInteger = false; //ENABLE THIS IN THE INSPECTOR IF THE PARAMETER IS AN INTEGER
        
        //THIS WILL BE CALLED RIGHT AFTER BEGIN THE TRANSITION TO THE NEW STATE
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if(isInteger)
            {
                animator.SetInteger(removeParameter, 0); //RESET ANIMATOR PARAMETER TO ZERO
            }else{
                animator.SetBool(removeParameter, false); //DISABLE ANIMATOR PARAMETER
            }
            
        }
    }
}
