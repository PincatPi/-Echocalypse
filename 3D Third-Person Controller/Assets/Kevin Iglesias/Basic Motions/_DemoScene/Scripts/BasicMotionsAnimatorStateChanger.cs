/*
=============================================================================================
BasicMotionsAnimatorStateChanger.cs

This script is needed for the "BasicMotionsCharacterController.cs" script to work properly.

This script changes CharacterState value in BasicMotionsCharacterController.cs script when
the Animator enters an Animator Controller State with this script.

https://www.keviniglesias.com/
support@keviniglesias.com
=============================================================================================
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KevinIglesias
{
    public class BasicMotionsAnimatorStateChanger : StateMachineBehaviour
    {
        [SerializeField]
        private CharacterState newState; //NEW STATE TO CHANGE
        
        //THIS WILL BE CALLED EVERY FRAME WHILE IN THE STATE, HIGHER LAYERS WILL HAVE HIGHER PRIORITY AND WILL OVERRIDE LOWER LAYERS CALLS
        override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            //CALL CHANGE STATE FUNCTION FROM MAIN SCRIPT
            animator.transform.parent.GetComponent<BasicMotionsCharacterController>().ChangeState(newState); 
        }
    }
}
