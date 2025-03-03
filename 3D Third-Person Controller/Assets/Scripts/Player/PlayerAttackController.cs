using System.Collections;
using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
using UnityEngine.PlayerLoop;

public class PlayerAttackController : MonoBehaviour
{
    #region 组件
    
    private Animator animator;
    private CharacterController controller;
    private ThirdPersonController thirdPersonController;
    public Transform effectTransform;
    public TwoBoneIKConstraint[] rightHandIKConstraints;
    private TwoBoneIKConstraint currentRightHandIKConstraint;
    public TwoBoneIKConstraint[] leftHandIKConstraints;
    private TwoBoneIKConstraint currentLeftHandIKConstraint;
    
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
    public enum WeaponType
    {
        Empty,
        Katana,
        GreatSword,
        Bow
    }
    protected WeaponType weaponType = WeaponType.Empty;
    [SerializeField]
    private int attackCount = 0;

    private readonly float attackTime = 0.3f;
    [SerializeField]
    private float timeCounter = 0f;
    [SerializeField]
    public bool stunned = false;
    
    public GameObject[] weaponOnBack;
    public GameObject[] weaponInHand;

    private int equipHash;
    
    void Start()
    {
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        thirdPersonController = GetComponent<ThirdPersonController>();
        currentRightHandIKConstraint = rightHandIKConstraints[0];
        currentLeftHandIKConstraint = leftHandIKConstraints[0];
        
        equipHash = Animator.StringToHash("WeaponType");
    }
    
    void Update()
    {
        if (timeCounter >= 0f)
        {
            timeCounter -= Time.deltaTime;
        }
        //设置动画状态
        SetAnimator();
        //攻击计数
        CommonAttack();
    }

    private void SetAnimator()
    {
        //装备状态
        animator.SetInteger(equipHash, (int)weaponType);
        //控制掏出武器和收起武器时的右手IK权重
        currentRightHandIKConstraint.weight = animator.GetFloat("Right Hand Weight");
        currentLeftHandIKConstraint.weight = animator.GetFloat("Left Hand Weight");
    }
    
    /// <summary>
    /// 切换背部武器和手部武器的显示
    /// </summary>
    /// <param name="weaponType">表示武器的位置是在背上0还是手上1\2\3</param>
    public void PutGrabWeapon(int weaponType)
    {
        bool isOnBack = weaponOnBack[weaponType].activeSelf;
        weaponOnBack[weaponType].SetActive(!isOnBack);
        weaponInHand[weaponType].SetActive(isOnBack);
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
    private bool IsInputValid()
    {
        if (thirdPersonController.armState != ThirdPersonController.ArmState.Equip ||
            stunned == true)
        {
            return false;
        }
        return true;
    }
    
    //获取玩家武器装备输入
    public void GetKatanaInput(InputAction.CallbackContext ctx)
    {
        if (ctx.started && !stunned)
        {
            if (weaponType != WeaponType.Katana)
            {
                weaponType = WeaponType.Katana;
                thirdPersonController.isEquip = true;
                //将当前有效的IK约束设置为Katana的IK约束
                currentRightHandIKConstraint = rightHandIKConstraints[(int)WeaponType.Katana];
                currentLeftHandIKConstraint = leftHandIKConstraints[(int)WeaponType.Katana];
            }
            else
            {
                weaponType = WeaponType.Empty;
                thirdPersonController.isEquip = false;
            }
        }
        Debug.Log((int)weaponType);
    }
    public void GetGreatSwordInput(InputAction.CallbackContext ctx)
    {
        if (ctx.started && !stunned)
        {
            if (weaponType != WeaponType.GreatSword)
            {
                weaponType = WeaponType.GreatSword;
                thirdPersonController.isEquip = true;
                //将当前有效的IK约束设置为GreatSword的IK约束
                currentRightHandIKConstraint = rightHandIKConstraints[(int)WeaponType.GreatSword];
                currentLeftHandIKConstraint = leftHandIKConstraints[(int)WeaponType.GreatSword];
            }
            else
            {
                weaponType = WeaponType.Empty;
                thirdPersonController.isEquip = false;
            }
        }
        Debug.Log((int)weaponType);
    }
    public void GetBowInput(InputAction.CallbackContext ctx)
    {
        if (ctx.started && !stunned)
        {
            if (weaponType != WeaponType.Bow)
            {
                weaponType = WeaponType.Bow;
                thirdPersonController.isEquip = true;
                //将当前有效的IK约束设置为Bow的IK约束
                currentRightHandIKConstraint = rightHandIKConstraints[(int)WeaponType.Bow];
                currentLeftHandIKConstraint = leftHandIKConstraints[(int)WeaponType.Bow];
            }
            else
            {
                weaponType = WeaponType.Empty;
                thirdPersonController.isEquip = false;
            }
        }
        Debug.Log((int)weaponType);
    }
    
    //获取玩家攻击输入
    public void GetAttackInput(InputAction.CallbackContext ctx)
    {
        if (ctx.started && IsInputValid())
        {
            //改变连击数，若连击数此时为4则会重置为1
            attackCount = attackCount < 4 ? attackCount + 1 : 1;
            //阻断攻击输入的接受
            stunned = true;
        }
    }
    
    //获取玩家闪避输入
    public void GetSlideInput(InputAction.CallbackContext ctx)
    {
        if (ctx.interaction is TapInteraction && IsInputValid())
        {
            animator.SetTrigger("Roll");
        }
    }

    public void StopTime(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            if(Time.timeScale == 0f)
                Time.timeScale = 1f;
            else if(Time.timeScale == 1f)
                Time.timeScale = 0f;
        }
    }
    
    #endregion
    
}
