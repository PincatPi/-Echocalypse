using System.Collections;
using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttackController : MonoBehaviour
{
    #region 组件
    
    public Animator animator;
    public CharacterController controller;
    public ThirdPersonController thirdPersonController;
    
    #endregion

    public int attack = 100;
    public enum AttackType
    {
        Common,
        Skill,
        Ultimate,
    }
    private AttackType attackType = AttackType.Common;
    [SerializeField]
    private int attackCount = 0;

    private readonly float attackTime = 0.8f;
    [SerializeField]
    private float timeCounter = 0f;
    
    void Start()
    {
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        thirdPersonController = GetComponent<ThirdPersonController>();
    }
    
    void Update()
    {
        timeCounter -= Time.deltaTime;
        CommonAttack();
    }

    /// <summary>
    /// 普通攻击
    /// </summary>
    private void CommonAttack()
    {
        if (timeCounter <= 0f)
        {
            attackCount = 0;
        }
    }

    #region 玩家输入相关
    
    //判断是否接收玩家攻击输入
    private bool IsAttackValid()
    {
        if (thirdPersonController.armState != ThirdPersonController.ArmState.Equip)
        {
            return false;
        }
        //TODO: 增加其它判断是否接收的条件
        return true;
    }
    
    //获取玩家攻击输入
    public void GetAttackInput(InputAction.CallbackContext ctx)
    {
        if (ctx.started && IsAttackValid())
        {
            //重置连击倒数时间
            timeCounter = attackTime;
            //改变连击数，若连击数此时为4则会重置为1
            attackCount = attackCount < 4 ? attackCount + 1 : 1;
        }
    }
    
    #endregion
    
}
