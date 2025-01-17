using System.Collections;
using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.PlayerLoop;

public class PlayerAttackController : MonoBehaviour
{
    #region 组件
    
    public Animator animator;
    public CharacterController controller;
    public ThirdPersonController thirdPersonController;
    public Transform effectTransform;
    
    #endregion

    public GameObject[] attackEffects;
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

    private readonly float attackTime = 0.3f;
    [SerializeField]
    private float timeCounter = 0f;
    [SerializeField]
    private bool stunned = false;
    
    void Start()
    {
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        thirdPersonController = GetComponent<ThirdPersonController>();
    }
    
    void Update()
    {
        if (timeCounter >= 0f)
        {
            timeCounter -= Time.deltaTime;
        }
        CommonAttack();
    }

    /// <summary>
    /// 普通攻击
    /// </summary>
    private void CommonAttack()
    {
        if (timeCounter <= 0f && !stunned)
        {
            attackCount = 0;
        }
        
        animator.SetFloat("AttackCount", attackCount, 0.1f, Time.deltaTime);
    }

    #region 动画片段调用函数
    
    /// <summary>
    /// 从动画片段的时间中接收是否允许攻击输入的判断
    /// </summary>
    /// <param name="attack"></param>
    public void AllowAttack()
    {
        //重置连击倒数时间
        timeCounter = attackTime;
        stunned = false;
    }

    /// <summary>
    /// 播放攻击特效
    /// </summary>
    public void PlayAttackEffect()
    {
        //实例化特效对象
        GameObject effect = GameObject.Instantiate<GameObject>(attackEffects[0], Vector3.zero, Quaternion.identity, effectTransform);
        
        //获取父物体旋转
        Quaternion parentRotation = effect.transform.parent.rotation;
        effect.transform.SetParent(null);
        //将物体的旋转转换为世界坐标系下的值
        Quaternion worldRotation = effect.transform.rotation;
        //将物体的旋转设置为父物体的相对坐标系下的值
        effect.transform.rotation = parentRotation * worldRotation;

        effect.GetComponent<ParticleSystem>().Play();
        Destroy(effect, 1f);
    }
    
    #endregion

    #region 玩家输入相关
    
    //判断是否接收玩家攻击输入
    private bool IsAttackValid()
    {
        if (thirdPersonController.armState != ThirdPersonController.ArmState.Equip ||
            stunned == true)
        {
            return false;
        }
        return true;
    }
    
    //获取玩家攻击输入
    public void GetAttackInput(InputAction.CallbackContext ctx)
    {
        if (ctx.started && IsAttackValid())
        {
            //改变连击数，若连击数此时为4则会重置为1
            attackCount = attackCount < 4 ? attackCount + 1 : 1;
            //阻断攻击输入的接受
            stunned = true;
        }
    }
    
    #endregion
    
}
