using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;
using UnityEngine.Animations.Rigging;
using UnityEngine.InputSystem.Interactions;
using UnityEngine.Serialization;

public class ThirdPersonController : MonoBehaviour
{
    #region 组件
    
    private Transform playerTransform;
    private Animator animator;
    private Transform cameraTransform;
    private CharacterController characterController;
    private PlayerSoundController playerSoundController;
    
    #endregion
    
    //角色姿态枚举
    public enum PlayerPosture
    {
        Crouch,
        Stand,
        Falling,
        Jumping,
        Landing,
    }
    [HideInInspector]
    public PlayerPosture playerPosture = PlayerPosture.Stand;
    
    #region 动画状态机阈值
    
    private float standThreshold = 0f;
    private float crouchThreshold = 1f;
    private float midairThreshold = 2.2f;
    private float landingThreshold = 1f;
    
    #endregion
    
    //角色运动状态枚举
    public enum LocomotionState
    {
        Idle,
        Walk,
        Run,
    }
    [HideInInspector]
    public LocomotionState locomotionState = LocomotionState.Idle;
   
    #region 角色手持装备
    
    //角色手持装备状态枚举
    public enum ArmState
    {
        Normal = 0,
        Equip = 1
    }
    [HideInInspector]
    public ArmState armState = ArmState.Normal;

    public GameObject weaponOnBack;
    public GameObject weaponInHand;
    
    //TODO:检查此处修改
    // public TwoBoneIKConstraint rightHandIKConstraint;
    
    #endregion
    
    
    #region 角色速度
    
    float crouchSpeed = 0.8f;
    float walkSpeed = 1.27f;
    float runSpeed = 4.2f;
    
    #endregion
    
    //角色移动输入
    private Vector2 moveInput;
    //玩家实际移动方向
    private Vector3 playerMovement = Vector3.zero;
    //角色是否可以急停
    [SerializeField]
    private bool canStop = false;
    
    #region 角色状态
    
    //角色状态输入
    private bool isRunPressed = false;
    private bool isCrouchPressed = false;
    [FormerlySerializedAs("isEquipPressed")] public bool isEquip = false;
    private bool isKatana = false;
    private bool isGrateSword = false;
    private bool isBow = false;
    private bool isJumpPressed = false;
    
    #endregion
    
    #region Animator中动画状态机的哈希值
    
    private int postureHash;
    private int moveSpeedHash;
    private int turnSpeedHash;
    private int verticalSpeedHash;
    private int jumpTypeHash;
    //private int equipHash;
    
    #endregion
    
    #region 角色跳跃相关
    
    //重力
    public float gravity = -9.81f;
    //角色垂直方向速度
    private float verticalVelocity;
    //角色跳跃高度
    public float maxHeight = 1.5f;
    //玩家空中水平移动速度的缓存值
    private static readonly int CACHE_SIZE = 3;
    //缓存池
    private Vector3[] velCache = new Vector3[CACHE_SIZE];
    //缓存池中最老的向量的索引值
    private int currentCacheIndex = 0;
    //平均速度变量
    private Vector3 averageVelocity = Vector3.zero;
    //下坠时的加速度是上升时加速度的倍数
    public float fallMultiplier = 1.5f;
    //角色是否落地
    [SerializeField]
    private bool isGrounded;
    //射线检测偏移量
    private float groundCheckOffset = 0.5f;
    //角色跳跃时的左右脚
    private float footTween;
    //角色跳跃的CD时间
    private float jumpCD = 0.15f;
    //角色是否处于跳跃CD中
    [SerializeField]
    private bool isLanding = false;
    //角色长按跳跃键时的加速度是普通加速度的倍数
    public float longJumpMultiplier = 2.5f;
    [SerializeField]
    //角色是否可以跌落
    private bool couldFall = false;
    //角色跌落的最小高度，小于此高度则不会切换到跌落姿态
    private float fallHeight = 0.5f;
    //角色跌落时，能够被判定为Landing状态的最小速度
    private float landdingMinVelocity;
    
    #endregion

    #region 角色音效相关
    
    //上一帧的动画nornalized时间
    private float lastFootCycle = 0f;
    
    #endregion

    #region 角色急停相关

    private float currentFootCycle = 0f;

    #endregion
    
    void Start()
    {
        playerTransform = this.transform;
        animator = this.GetComponent<Animator>();
        cameraTransform = Camera.main.transform;
        characterController = this.GetComponent<CharacterController>();
        playerSoundController = this.GetComponent<PlayerSoundController>();
        
        //获取哈希值
        postureHash = Animator.StringToHash("Posture");
        moveSpeedHash = Animator.StringToHash("MoveSpeed");
        turnSpeedHash = Animator.StringToHash("TurnSpeed");
        verticalSpeedHash = Animator.StringToHash("VerticalSpeed");
        jumpTypeHash = Animator.StringToHash("JumpType");
        //TODO: 检查此处是否存在问题
        // equipHash = Animator.StringToHash("WeaponType");
        
        //锁定鼠标
        Cursor.lockState = CursorLockMode.Locked;
        
        //根据最小的跌落高度来计算落地CD速度的阈值（绝对值低于该速度则不计算落地CD）
        landdingMinVelocity = -Mathf.Sqrt(-2 * gravity * fallHeight);
        landdingMinVelocity -= 1f;
    }

    void Update()
    {
        //地面检测
        CheckGrounded();
        //切换角色姿态
        SwitchPlayerStates();
        
        //先计算角色重力
        CaculateGravity();
        //再执行跳跃
        Jump();
        
        //计算输入方向
        CaculateInputDirection();
        //设置动画状态
        SetupAnimator();
        //播放脚步声
        PlayFootStepSound();
        
        //TEST: 测试急停
        //急停
        NoEquipEmergencyStop();
    }

    /// <summary>
    /// 更改玩家状态
    /// </summary>
    private void SwitchPlayerStates()
    {
        //玩家姿态
        switch (playerPosture)
        {
            case PlayerPosture.Stand:
                //垂直速度大于0，说明此时为跳跃状态
                if (verticalVelocity > 0)
                {
                    playerPosture = PlayerPosture.Jumping;
                }
                //站立状态下跌落
                else if (!isGrounded && couldFall)
                {
                    playerPosture = PlayerPosture.Falling;
                }
                else if (isCrouchPressed)
                {
                    playerPosture = PlayerPosture.Crouch;
                }
                break;
            
            case PlayerPosture.Crouch:
                //从下蹲状态跌落
                if (!isGrounded && couldFall)
                {
                    playerPosture = PlayerPosture.Falling;
                }
                else if (!isCrouchPressed)
                {
                    playerPosture = PlayerPosture.Stand;
                }
                break;
            //坠落状态下
            case PlayerPosture.Falling:
                //落地
                //TODO: 有没有比判断当前垂直速度更好的方法，来预防角色在墙体边缘可能出现的卡Landing状态的情况
                if (verticalVelocity <= landdingMinVelocity && isGrounded)
                {
                    //计算跳跃冷却时间=
                    StartCoroutine(CoolDownJump());
                }
                //冷却状态
                if (isLanding)
                {
                    playerPosture = PlayerPosture.Landing;
                }
                break;
            //跳跃状态下
            case PlayerPosture.Jumping:
                //落地
                if (verticalVelocity < 0 && isGrounded)
                {
                    //计算跳跃冷却时间
                    StartCoroutine(CoolDownJump());
                }
                //落地冷却状态
                if (isLanding)
                {
                    playerPosture = PlayerPosture.Landing;
                }
                break;
            //落地冷却状态
            case PlayerPosture.Landing:
                //落地冷却状态下，设为站立姿态，保证此状态下玩家能够行走
                if (!isLanding)
                {
                    playerPosture = PlayerPosture.Stand;
                }
                break;
        }
        
        //装备状态
        if (!isEquip)
        {
            armState = ArmState.Normal;
        }
        else
        {
            if (isEquip)
                armState = ArmState.Equip;
        }
        
        //玩家输入
        if (moveInput.magnitude == 0)
        {
            locomotionState = LocomotionState.Idle;
        }
        else if(isRunPressed)
        {
            locomotionState = LocomotionState.Run;
        }
        else
        {
            locomotionState = LocomotionState.Walk;
        }
    }

    
    /// <summary>
    /// 用于计算跳跃cd时间的协程函数
    /// </summary>
    /// <returns></returns>
    private IEnumerator CoolDownJump()
    {
        //根据下落速度来计算落地后跳跃CD状态的阈值，以此设置下蹲动画状态的程度
        //去掉小于-10和大于0的速度
        landingThreshold = Mathf.Clamp(verticalVelocity, -10, 0);
        //限制landingThreshold在[-0.5, 0]
        landingThreshold /= 20f;
        //将landingThreshold变为[0.5, 1]
        //TEST: 将landingThreshold变为[0, 0.5]
        landingThreshold += 0.5f;
        isLanding = true;
        
        playerPosture = PlayerPosture.Landing;
        //等待CD时间后
        yield return new WaitForSeconds(jumpCD);
        //将CD状态设为false
        isLanding = false;
    }

    /// <summary>
    /// 计算移动方向
    /// </summary>
    private void CaculateInputDirection()
    {
        //获取相机在水平平面（XZ平面）上的投影，并做归一化
        Vector3 cameraForwardProjection = new Vector3(cameraTransform.forward.x, 0, cameraTransform.forward.z).normalized;
        //根据玩家输入moveInput和相机XZ投影，计算玩家移动的三维向量值
        //输入的Y分量乘以投影的方向，输入的X分量乘以相机右方向
        //TODO: 没看懂
        playerMovement = cameraForwardProjection * moveInput.y + cameraTransform.right * moveInput.x;
        //将该向量转换到玩家本地坐标系下，得到玩家Y方向和输入方向的夹角
        playerMovement = playerTransform.InverseTransformVector(playerMovement);
    }

    /// <summary>
    /// 设置动画状态机
    /// </summary>
    private void SetupAnimator()
    {
        //TODO: 检查此处修改是否存在问题
        // //装备状态
        // animator.SetInteger(equipHash, );
        // //控制掏出武器和收起武器时的右手IK权重
        // rightHandIKConstraint.weight = animator.GetFloat("Right Hand Weight");
        
        //站立状态
        if (playerPosture == PlayerPosture.Stand)
        {
            //线性插值地改变动画状态机中的Posture变量为Stand值
            animator.SetFloat(postureHash, standThreshold, 0.1f, Time.deltaTime);
            switch (locomotionState)
            {
                case LocomotionState.Idle:
                    animator.SetFloat(moveSpeedHash, 0, 0.1f, Time.deltaTime);
                    break;
                case LocomotionState.Walk:
                    animator.SetFloat(moveSpeedHash, playerMovement.magnitude * walkSpeed, 0.1f, Time.deltaTime);
                    break;
                case LocomotionState.Run:
                    animator.SetFloat(moveSpeedHash, playerMovement.magnitude * runSpeed, 0.1f, Time.deltaTime);
                    break;
            }
        }
        //下蹲状态
        else if (playerPosture == PlayerPosture.Crouch)
        {
            //线性插值地改变动画状态机中的Posture为Crouch值
            animator.SetFloat(postureHash, crouchThreshold, 0.15f, Time.deltaTime);
            switch (locomotionState)
            {
                case LocomotionState.Idle:
                    animator.SetFloat(moveSpeedHash, 0, 0.1f, Time.deltaTime);
                    break;
                default:
                    animator.SetFloat(moveSpeedHash, playerMovement.magnitude * crouchSpeed, 0.1f, Time.deltaTime);
                    break;
            }
        }
        //滞空状态
        else if (playerPosture == PlayerPosture.Jumping)
        {
            //线性插值地改变动画状态机中的Posture为Midair值
            animator.SetFloat(postureHash, midairThreshold, 0.1f, Time.deltaTime);
            //设置状态机中VerticalSpeed的值
            animator.SetFloat(verticalSpeedHash, verticalVelocity, 0.1f, Time.deltaTime);
            animator.SetFloat(jumpTypeHash, footTween);
        }
        //跳跃CD状态
        else if (playerPosture == PlayerPosture.Landing)
        {
            //线性插值地改变动画状态机中的Posture变量为Stand值
            animator.SetFloat(postureHash, landingThreshold, 0.1f, Time.deltaTime);
            switch (locomotionState)
            {
                case LocomotionState.Idle:
                    animator.SetFloat(moveSpeedHash, 0, 0.1f, Time.deltaTime);
                    break;
                case LocomotionState.Walk:
                    animator.SetFloat(moveSpeedHash, playerMovement.magnitude * walkSpeed, 0.1f, Time.deltaTime);
                    break;
                case LocomotionState.Run:
                    animator.SetFloat(moveSpeedHash, playerMovement.magnitude * runSpeed, 0.1f, Time.deltaTime);
                    break;
            }
        }
        //跌落状态
        else if (playerPosture == PlayerPosture.Falling)
        {
            //线性插值地改变动画状态机中的Posture为Midair值
            //TODO:检查此处的dampTime是否有问题
            animator.SetFloat(postureHash, midairThreshold, 0.5f, Time.deltaTime);
            //设置状态机中VerticalSpeed的值
            animator.SetFloat(verticalSpeedHash, verticalVelocity, 0.1f, Time.deltaTime);
            //TODO:检查此处的footTween是否正确
            animator.SetFloat(jumpTypeHash, footTween);
        }
        
        //若不处于瞄准状态
        if (armState == ArmState.Normal || armState == ArmState.Equip)
        {
            //得到玩家当前运动方向playerMovement.x和玩家当前正前方playerMovement.z的夹角（弧度制）
            float rad = Mathf.Atan2(playerMovement.x, playerMovement.z);
            animator.SetFloat(turnSpeedHash, rad * 1.3f, 0.1f, Time.deltaTime);
            
            //人为添加Y轴上的旋转，令转向速度加快
            playerTransform.Rotate(0, rad * 240 * Time.deltaTime, 0);
        }
    }

    /// <summary>
    /// 玩家跳跃
    /// </summary>
    private void Jump()
    {
        //若当前角色姿态为站立或下蹲，且按下了跳跃键，且角色竖直速度小于一个阈值（为防止按下跳跃后角色还没进入Jump状态时，该部分代码被重复执行）
        if ((playerPosture == PlayerPosture.Stand || playerPosture == PlayerPosture.Crouch) && isJumpPressed && verticalVelocity < 2f)
        {
            //播放跳跃音效
            playerSoundController.PlayJumpEffortSound();
            
            //根据跳跃最大高度，计算起跳初速度
            verticalVelocity = Mathf.Sqrt(-2 * gravity * maxHeight);
            
            //为保证站立到跳跃的动画切换流畅，此处立即将速度设为起跳初速度
            animator.SetFloat(verticalSpeedHash, verticalVelocity);
            
            //随机左右脚跳跃动画
            footTween = Random.value > 0.5 ? 1f : -1f;
        }
    }

    /// <summary>
    /// 计算玩家离地前3帧的平均水平移动速度
    /// </summary>
    /// <param name="newVel">当前帧的速度</param>
    /// <returns>计算出的平均速度</returns>
    //TODO: 该方法常用于游戏开发中的各种平滑和去噪场景
    private Vector3 AverageVelocity(Vector3 newVel)
    {
        //缓存池设计为循环队列
        //新速度替换缓存池中最老的速度
        velCache[currentCacheIndex] = newVel;
        currentCacheIndex++;
        //取模，防止索引越界
        currentCacheIndex %= CACHE_SIZE;
        
        //计算缓存池中速度的平均值
        Vector3 average = Vector3.zero;
        foreach (Vector3 vel in velCache)
        {
            average += vel;
        }
        return average / CACHE_SIZE;
    }
    
    /// <summary>
    /// 代码控制角色移动
    /// </summary>
    private void OnAnimatorMove()
    {
        if (playerPosture != PlayerPosture.Jumping)
        {
            //每个deltaTime时间内，角色的移动位置(注意：deltaPosition会受到帧率大小的影响）
            Vector3 playerDeltaMovement = animator.deltaPosition;
            //垂直方向上位移 = 垂直方向上速度 * 间隔时间deltaTime
            playerDeltaMovement.y = verticalVelocity * Time.deltaTime;
            characterController.Move(playerDeltaMovement);
            
            //计算前三帧的水平平均速度
            averageVelocity = AverageVelocity(animator.velocity);
        }
        else
        {
            //沿用角色在地面时的水平移动速度
            //使用角色离地前几帧的平均速度，避免不确定因素对玩家空中移动速度的影响
            //此处使用速度，而不使用deltaPosition，是为了避免deltaPosition受到帧率大小的影响而得到不准确的值
            averageVelocity.y = verticalVelocity;
            Vector3 playerDeltaMovement = averageVelocity * Time.deltaTime;
            characterController.Move(playerDeltaMovement);
        }
    }

    /// <summary>
    /// 计算角色重力
    /// </summary>
    private void CaculateGravity()
    {
        //若角色状态不是跳跃也不是跌落
        if (playerPosture != PlayerPosture.Jumping && playerPosture != PlayerPosture.Falling)
        {
            //不在跳跃状态和跌落状态，但也没有在地面上时（如在斜坡上时）
            if (!isGrounded)
            {
                //添加向下的速度
                verticalVelocity += gravity * fallMultiplier * Time.deltaTime;
            }
            //在地面上时
            else
            {
                //CharacterController的isGrounded要求角色必须持续有向下的速度
                //站在地面上时，重力不会累加
                verticalVelocity = gravity * Time.deltaTime;
            }
        }
        else
        {
            //重力加速度公式，模拟重力
            //下降时
            if (verticalVelocity <= 0)
            {
                verticalVelocity += gravity * fallMultiplier * Time.deltaTime;
            }
            //上升时
            else
            {
                if (isJumpPressed)
                {
                    verticalVelocity += gravity * Time.deltaTime;
                }
                else
                {
                    //若玩家没有长按跳跃键，则加快玩家的下落速度
                    verticalVelocity += gravity * longJumpMultiplier * Time.deltaTime;
                }
            }
        }
    }

    /// <summary>
    /// 检测玩家是否落地
    /// </summary>
    private void CheckGrounded()
    {
        //射线检测到了碰撞体
        //TODO: 1.5f个characterController.skinWidth的距离不会引起误判
        if (Physics.SphereCast(playerTransform.position + (Vector3.up * groundCheckOffset), characterController.radius,
                Vector3.down, out RaycastHit hit,
                groundCheckOffset - characterController.radius + 1.5f * characterController.skinWidth))
        {
            //碰撞体是地面
            if (hit.collider.gameObject.CompareTag("Ground"))
            {
                isGrounded = true;
                couldFall = false;
            }
        }
        //未检测到碰撞体
        else
        {
            isGrounded = false;
            //再发射一条射线，检测地面是否离角色脚底fallHeight的高度
            couldFall = !Physics.Raycast(playerTransform.position, Vector3.down, fallHeight);
        }
    }

    /// <summary>
    /// 播放脚步音效
    /// </summary>
    private void PlayFootStepSound()
    {
        //角色状态不是跳跃也不是跌落
        if (playerPosture != PlayerPosture.Jumping && playerPosture != PlayerPosture.Falling)
        {
            //当角色正在行走或奔跑
            if (locomotionState == LocomotionState.Walk || locomotionState == LocomotionState.Run)
            {
                //currentFootCycle为动画在0-1之间的循环值
                float currentFootCycle = Mathf.Repeat(animator.GetCurrentAnimatorStateInfo(0).normalizedTime, 1f);
                //在动画播放到0.1或0.6时（即角色动画脚着地时）播放音效
                if ((lastFootCycle < 0.1f && currentFootCycle >= 0.1f) ||
                    (lastFootCycle < 0.6f && currentFootCycle >= 0.6f))
                {
                    //播放脚步声音效
                    playerSoundController.PlayFootStepSound();
                }
                lastFootCycle = currentFootCycle;
            }
        }
    }

    //TODO:检查此方法删除后是否有影响
    /// <summary>
    /// 切换背部武器和手部武器的显示
    /// </summary>
    /// <param name="weaponPosition">表示武器的位置是在背上0还是手上1</param>
    // public void PutGrabWeapon(int weaponPosition)
    // {
    //     bool onBack = (weaponPosition != (int)ArmState.Equip);
    //     weaponOnBack.SetActive(onBack);
    //     weaponInHand.SetActive(!onBack);
    // }

    /// <summary>
    /// 没有装备武器时，奔跑下的急停
    /// </summary>
    public void NoEquipEmergencyStop()
    {
        //控制是否可以急停的条件
        if (locomotionState == LocomotionState.Run && armState == ArmState.Normal)
        {
            canStop = true;
        }
        else if (animator.velocity.magnitude < 3.5f)
        {
            canStop = false;
        }
        //TODO: 优化奔跑时急停动画的切换
        //控制奔跑时的急停
        if (canStop && moveInput is { x: 0, y: 0 })
        {
            currentFootCycle = Mathf.Repeat(animator.GetCurrentAnimatorStateInfo(0).normalizedTime, 1f);
            //根据动画此时迈的是哪只脚来播放不同脚的急停动画
            if (currentFootCycle >= 0f && currentFootCycle < 0.5f)
            {
                //TODO: 急停动画的过渡需要优化
                animator.CrossFade("NormalStopRight", 0.1f);
            }
            else if (currentFootCycle >= 0.5f && currentFootCycle < 1f)
            {
                //TODO: 急停动画的过渡需要优化
                animator.CrossFade("NormalStopLeft", 0.1f);
            }
            canStop = false;
        }
    }
    
    #region 玩家输入相关
    
    //获取玩家移动输入
    public void GetMoveInput(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>();
    }
    //获取玩家奔跑状态和空手状态下闪避的输入
    public void GetRunInput(InputAction.CallbackContext ctx)
    {
        if(ctx.interaction is HoldInteraction)
            isRunPressed = ctx.ReadValueAsButton();
        else if (ctx.interaction is TapInteraction)
        {
            //在地面上 且 处于空手状态下
            if(isGrounded && armState == ArmState.Normal)
                animator.SetTrigger("Roll"); 
        }
    }
    //获取玩家下蹲状态输入
    public void GetCrouchInput(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            isCrouchPressed = !isCrouchPressed;
        }
    }
    //获取玩家装备武器状态输入
    //TODO: 该方法有待移除
    public void GetEquipInput(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            //isEquipPressed = !isEquipPressed;
        }
    }
    //获取玩家跳跃输入
    public void GetJumpInput(InputAction.CallbackContext ctx)
    {
        isJumpPressed = ctx.ReadValueAsButton();
    }
    
    #endregion
}
