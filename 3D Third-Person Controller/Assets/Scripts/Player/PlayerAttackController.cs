using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.Burst.Intrinsics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
using UnityEngine.PlayerLoop;
using UnityEngine.UIElements;

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
    public Camera mainCamera;
    public CinemachineTargetGroup cinemachineTargetGroup;
    private AttackCheckGizmos attackCheck;
    
    #endregion

    public GameObject[] attackEffects;
    public int attack = 100;
    public enum E_AttackType
    {
        Common,
        Skill,
        Ultimate,
    }
    private E_AttackType attackType = E_AttackType.Common;
    public enum E_WeaponType
    {
        Empty,
        Katana,
        GreatSword,
        Bow
    }
    private E_WeaponType weaponType = E_WeaponType.Empty;
    private Dictionary<E_WeaponType, int> comboDic = new Dictionary<E_WeaponType, int>();
    [SerializeField]
    private int attackCount = 0;

    //锁定敌人目标
    [SerializeField]private bool isLockTarget = false;
    
    private readonly float attackTime = 0.4f;
    [SerializeField]
    private float timeCounter = 0f;
    [SerializeField]
    public bool stunned = false;
    
    public GameObject[] weaponOnBack;
    public GameObject[] weaponInHand;
    
    //玩家面前一定距离内的敌人数组
    private Collider[] enemies;
    [SerializeField] private Transform targetTransform = null;
    //[SerializeField] private Transform lastTargetTransform = null;
    //能够发现敌人的最远视线距离
    [SerializeField] private float distance = 30f;
    [SerializeField] private Vector3 offset;
    [SerializeField] private Vector3 size;
    [SerializeField] private Vector3 cubeCenter;
    [SerializeField] private Vector3 rotateEuler;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask playerSubLayer;

    private int equipHash;
    private int lockOnHash;
    
    void Start()
    {
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        thirdPersonController = GetComponent<ThirdPersonController>();
        currentRightHandIKConstraint = rightHandIKConstraints[0];
        currentLeftHandIKConstraint = leftHandIKConstraints[0];
        attackCheck = GetComponent<AttackCheckGizmos>();
        
        equipHash = Animator.StringToHash("WeaponType");
        lockOnHash = Animator.StringToHash("LockOn");
        
        //注册当前各种武器对应的Combo数
        comboDic.Add(E_WeaponType.Katana, 6);
        comboDic.Add(E_WeaponType.GreatSword, 4);
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

    void LateUpdate()
    {
        FindEnemyInFront();
        LockOnEnemy();
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
    
    /// <summary>
    /// 查找玩家面前一定距离内的敌人
    /// </summary>
    private void FindEnemyInFront()
    {
        //若不处在锁定状态，则不进行查找
        if (!isLockTarget)
            return;
        
        //检测相机面前的盒形碰撞体内是否有Enemy
        Vector3 cameraPos = mainCamera.transform.position;
        Vector3 cameraForward = mainCamera.transform.forward;
        cubeCenter = new Vector3(offset.x * cameraForward.x, offset.y * cameraForward.y, offset.z * cameraForward.z)+ cameraPos;
        enemies = Physics.OverlapBox(cubeCenter, size / 2, Quaternion.Euler(rotateEuler), enemyLayer);
        
        float minDistance = float.MaxValue;
        if (enemies.Length > 0)
        {
            //找到所有enemies中距离玩家最近的
            for (int i = 0; i < enemies.Length; i++)
            {
                float distance = Vector3.Distance(this.transform.position, enemies[i].transform.position);
                //TEST:
                Debug.Log("distance: " + distance);
                //若该敌人与玩家间的距离小于最小距离，且能够在摄像机中被看到
                if (distance < minDistance && IsVisableInCamera(mainCamera, enemies[i].transform))
                {
                    minDistance = distance;
                    targetTransform = enemies[i].transform;
                }
            }
            //若找到了这样的对象
            if (!Mathf.Approximately(minDistance, float.MaxValue) && targetTransform)
            {
                //将该对象添加到虚拟相机的targetGroup中
                //cinemachineTargetGroup每时刻最多应该只有2个对象（m_Targets[0]固定为玩家对象，m_Targets[1]为敌人对象）
                if (cinemachineTargetGroup.m_Targets.Length == 1)
                {
                    cinemachineTargetGroup.AddMember(targetTransform, 1, 1);
                }
                else if(cinemachineTargetGroup.m_Targets.Length == 2)
                {
                    CinemachineTargetGroup.Target newTarget = new CinemachineTargetGroup.Target
                    {
                        target = targetTransform, weight = 1f, radius = 1f
                    };
                    cinemachineTargetGroup.m_Targets[1] = newTarget;
                }
                cinemachineTargetGroup.DoUpdate();   
            }
        }
        //如果检测区内没有敌人 || 没有敌人是可以被相机看见的
        if(enemies.Length == 0 || !targetTransform || Mathf.Approximately(minDistance, float.MaxValue))
        {
            targetTransform = null; //目标对象置为空
            if (cinemachineTargetGroup.m_Targets.Length > 1)
            {
                cinemachineTargetGroup.m_Targets[1] = new CinemachineTargetGroup.Target();   
            }
            cinemachineTargetGroup.DoUpdate(); //更新
        }
    }
    
    /// <summary>
    /// 判断物体是否在相机中可见
    /// </summary>
    /// <param name="camera"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    private bool IsVisableInCamera(Camera camera, Transform target)
    {
        if (!camera || !target)
            return false;
        //将目标物体坐标转为屏幕坐标
        Vector3 screenPoint = camera.WorldToScreenPoint(target.position);
        //该物体坐标在屏幕外
        if(screenPoint.x < 0 || screenPoint.y < 0 || screenPoint.x > Screen.width || screenPoint.y > Screen.height)
            return false;
        //从摄像机向目标物体发射射线
        Ray ray = camera.ScreenPointToRay(screenPoint);
        //忽略检测Player和Player下子物体层
        if (Physics.Raycast(ray, out RaycastHit hit, distance, ~(playerLayer | playerSubLayer)))
        {
            return hit.collider.gameObject == target.gameObject;
        }
        return false;
    }
    
    //TEST: 绘制玩家视线范围
    private void OnDrawGizmos()
    {
        Vector3 cameraPos = mainCamera.transform.position;
        Vector3 cameraForward = mainCamera.transform.forward;
        cubeCenter = new Vector3(offset.x * cameraForward.x, offset.y * cameraForward.y, offset.z * cameraForward.z)+ cameraPos;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(cubeCenter, size);
    }
    private void DrawRay()
    {
        for (int i = 0; i < enemies.Length; i++)
        {
            Transform target = enemies[i].transform;
            
            Vector3 screenPoint = mainCamera.WorldToScreenPoint(target.position);
            Ray ray = mainCamera.ScreenPointToRay(screenPoint);
            Gizmos.DrawRay(ray.origin, ray.direction * distance);   
        }
    }

    /// <summary>
    /// //TODO: 锁定状态下的攻击，令玩家对象始终面朝敌人对象
    /// </summary>
    private void LockOnEnemy()
    {
        //若不处在锁定状态 || 找不到可以锁定的目标
        if (!isLockTarget || !targetTransform)
        {
            //切换为NormalCamera
            animator.SetBool(lockOnHash, false);
            targetTransform = null; //锁定目标置空（针对不处在锁定状态）
            isLockTarget = false; //退出锁定状态（针对找不到可以锁定的目标）
            return;   
        }
        //设状态为LockOn，切换至LockOnCamera
        animator.SetBool(lockOnHash, true);
        //目标旋转
        Quaternion targetRotation = Quaternion.LookRotation(targetTransform.position - transform.position);
        //当前旋转
        Quaternion currentRotation = transform.rotation;
        //对旋转进行平滑插值
        transform.rotation = Quaternion.Slerp(currentRotation, targetRotation, Time.deltaTime * 10f);
        transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
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
    
    /// <summary>
    /// 切换背部武器和手部武器的显示
    /// </summary>
    /// <param name="weaponType">表示武器的位置是在背上0还是手上1\2\3</param>
    public void PutGrabWeapon(int weaponType)
    {
        //isOnBack为true时是装备武器，为false时是收回武器
        bool isOnBack = weaponOnBack[weaponType].activeSelf;
        weaponOnBack[weaponType].SetActive(!isOnBack);
        weaponInHand[weaponType].SetActive(isOnBack);
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
            //若当前手上没有武器
            if (weaponType == E_WeaponType.Empty)
            {
                weaponType = E_WeaponType.Katana;
                thirdPersonController.isEquip = true;
                //将当前有效的IK约束设置为Katana的IK约束
                currentRightHandIKConstraint = rightHandIKConstraints[(int)E_WeaponType.Katana];
                currentLeftHandIKConstraint = leftHandIKConstraints[(int)E_WeaponType.Katana];
            }
            //若手上有武器，则收回该武器
            else
            {
                weaponType = E_WeaponType.Empty;
                thirdPersonController.isEquip = false;
            }
            attackCheck.weaponType = weaponType;
        }
    }
    public void GetGreatSwordInput(InputAction.CallbackContext ctx)
    {
        if (ctx.started && !stunned)
        {
            if (weaponType == E_WeaponType.Empty)
            {
                weaponType = E_WeaponType.GreatSword;
                thirdPersonController.isEquip = true;
                //将当前有效的IK约束设置为GreatSword的IK约束
                currentRightHandIKConstraint = rightHandIKConstraints[(int)E_WeaponType.GreatSword];
                currentLeftHandIKConstraint = leftHandIKConstraints[(int)E_WeaponType.GreatSword];
            }
            else
            {
                weaponType = E_WeaponType.Empty;
                thirdPersonController.isEquip = false;
            }
            attackCheck.weaponType = weaponType;
        }
    }
    public void GetBowInput(InputAction.CallbackContext ctx)
    {
        if (ctx.started && !stunned)
        {
            if (weaponType == E_WeaponType.Empty)
            {
                weaponType = E_WeaponType.Bow;
                thirdPersonController.isEquip = true;
                //将当前有效的IK约束设置为Bow的IK约束
                currentRightHandIKConstraint = rightHandIKConstraints[(int)E_WeaponType.Bow];
                currentLeftHandIKConstraint = leftHandIKConstraints[(int)E_WeaponType.Bow];
            }
            else
            {
                weaponType = E_WeaponType.Empty;
                thirdPersonController.isEquip = false;
            }
            attackCheck.weaponType = weaponType;
        }
    }
    
    //获取玩家攻击输入
    public void GetAttackInput(InputAction.CallbackContext ctx)
    {
        if (ctx.started && IsInputValid())
        {
            //改变连击数，若连击数此时为当前武器普通Combo最大值则会重置为1
            attackCount = attackCount < comboDic[weaponType] ? attackCount + 1 : 1;
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

    //获取锁定敌人输入
    public void GetLockTargetInput(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
            isLockTarget = !isLockTarget;
    }
    
    //TEST: 暂停时间（调试用，发布时删除）
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
