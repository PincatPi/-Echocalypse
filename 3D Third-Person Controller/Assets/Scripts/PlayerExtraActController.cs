using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerExtraActController : MonoBehaviour
{
    #region 组件

    private ThirdPersonController thirdPersonController;
    private Animator animator;
    private PlayerAudioController playerAudioController;
    
    #endregion
    
    public enum ExtraActState
    {
        Empty = 0,
        Greet = 1,
        Agree = 2,
    }
    [SerializeField]
    private ExtraActState extraActState = ExtraActState.Empty;
    
    private string layerName = "ExtraLayer";

    private int extraActHash;

    [SerializeField]
    private bool isGreet;
    [SerializeField]
    private bool isAgree;
    [SerializeField]
    private bool isEmpty = true;
    
    void Start()
    {
        thirdPersonController = GetComponent<ThirdPersonController>();
        animator = thirdPersonController.gameObject.GetComponent<Animator>();
        playerAudioController = GetComponent<PlayerAudioController>();
        
        extraActHash = Animator.StringToHash("ExtraAct");
    }
    
    void Update()
    {
        SetExtraActAnimator();
        UpdateExtraActState();
    }

    /// <summary>
    /// 更新额外动作的状态
    /// </summary>
    private void UpdateExtraActState()
    {
        if ( !(thirdPersonController.playerPosture == ThirdPersonController.PlayerPosture.Stand &&
              thirdPersonController.locomotionState == ThirdPersonController.LocomotionState.Idle) )
        {
            isEmpty = true;
            isGreet = false;
            isAgree = false;
        }

        //当有动画在播放时
        if (!isEmpty)
        {
            //当前动画播放完毕且当前动画不是用于占位的空动画时
            if (Mathf.Clamp01(animator.GetCurrentAnimatorStateInfo(animator.GetLayerIndex(layerName)).normalizedTime) >= 0.9f && 
                !animator.GetCurrentAnimatorStateInfo(animator.GetLayerIndex(layerName)).IsTag("Empty"))
            {
                //将状态设置回Empty
                isEmpty = true;
                isGreet = false;
                isAgree = false;
            }
        }
    }

    /// <summary>
    /// 设置额外动作的状态
    /// </summary>
    private void SetExtraActAnimator()
    {
        //根据对应额外动作状态，设置动画状态机ExtraAct变量值
        if (isEmpty)
        {
            animator.SetFloat(extraActHash, (float)ExtraActState.Empty);
        }
        if (isGreet)
        {
            animator.SetFloat(extraActHash, (float)ExtraActState.Greet);
        }
        if (isAgree)
        {
            animator.SetFloat(extraActHash, (float)ExtraActState.Agree);
        }
    }
    
    #region 玩家输入相关

    private bool IsValidState()
    {
        if (isEmpty && thirdPersonController.playerPosture == ThirdPersonController.PlayerPosture.Stand &&
            thirdPersonController.locomotionState == ThirdPersonController.LocomotionState.Idle &&
            thirdPersonController.armState == ThirdPersonController.ArmState.Normal)
        {
            return true;
        }
        return false;
    }

    //获取玩家Greet输入
    public void GetGreetInput(InputAction.CallbackContext ctx)
    {
        if (ctx.started && IsValidState())
        {
            isGreet = true;
            isEmpty = false;
            playerAudioController.PlayGreetAudio();
        }
    }
    //获取玩家Agree输入
    public void GetAgreeInput(InputAction.CallbackContext ctx)
    {
        if (ctx.started && IsValidState())
        {
            isAgree = true;
            isEmpty = false;
            playerAudioController.PlayAgreeAudio();
        }
    }

    #endregion
}
