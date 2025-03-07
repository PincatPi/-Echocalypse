using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class AttackCheckGizmos : MonoBehaviour
{
    //敌人层级
    [SerializeField] private LayerMask enemyLayer;
    //是否正在攻击
    [SerializeField] private bool isAttacking = false;
    //当前的武器类型
    [SerializeField] public PlayerAttackController.E_WeaponType weaponType = PlayerAttackController.E_WeaponType.Empty;
    //对应武器和攻击检测点组的字典
    private Dictionary<PlayerAttackController.E_WeaponType, Transform[]> attackCheckPointsOfWeapon = new Dictionary<PlayerAttackController.E_WeaponType, Transform[]>();
    //太刀攻击检测点
    public Transform[] katanaCheckPoints;
    //大剑攻击检测点
    public Transform[] greatSwordCheckPoints;
    
    //武器上的攻击检测点
    [SerializeField] private Transform[] attackCheckPoints;
    //上一次检测时检测点的位置
    [SerializeField] private Vector3[] lastCheckPointsPosition;
    //检测时间间隔
    public float timeBetweenCheck;
    //计时器
    private float timeCounter;
    //是否是第一次检测
    private bool isFirstCheck = true;
    private RaycastHit[] enemiesRaycastHits;

    private void Start()
    {
        //注册不同武器的攻击检测点
        attackCheckPointsOfWeapon.Add(PlayerAttackController.E_WeaponType.Katana, katanaCheckPoints);
        attackCheckPointsOfWeapon.Add(PlayerAttackController.E_WeaponType.GreatSword, greatSwordCheckPoints);
    }

    private void Update()
    {
        if (isAttacking)
        {
            timeCounter += Time.deltaTime;
        }
        SwitchAttackCheckPoints();
        AttackCheck();
    }

    private void SwitchAttackCheckPoints()
    {
        switch (weaponType)
        {
            case PlayerAttackController.E_WeaponType.Empty:
                break;
            
            case PlayerAttackController.E_WeaponType.Katana:
                attackCheckPoints = attackCheckPointsOfWeapon[PlayerAttackController.E_WeaponType.Katana];
                enemiesRaycastHits = new RaycastHit[attackCheckPoints.Length];
                break;
            
            case PlayerAttackController.E_WeaponType.GreatSword:
                attackCheckPoints = attackCheckPointsOfWeapon[PlayerAttackController.E_WeaponType.GreatSword];
                enemiesRaycastHits = new RaycastHit[attackCheckPoints.Length];
                break;
            
            case PlayerAttackController.E_WeaponType.Bow:
                //功能待添加
                break;
        }
    }
    
    private void AttackCheck()
    {
        if(weaponType == PlayerAttackController.E_WeaponType.Empty)
            return;
        
        //若当时处于攻击状态
        if (isAttacking)
        {
            if (timeCounter >= timeBetweenCheck)
            {
                //如果是第一次检查，则不进行检测
                if (isFirstCheck)
                {
                    lastCheckPointsPosition = new Vector3[attackCheckPoints.Length];
                    //将isFirstCheck置false
                    isFirstCheck = false;
                }
                //不是第一次检查，则进行检测
                else
                {
                    for (int i = 0; i < attackCheckPoints.Length; i++)
                    {
                        //进行射线检测
                        Ray ray = new Ray(lastCheckPointsPosition[i], (attackCheckPoints[i].position - lastCheckPointsPosition[i]).normalized);
                        int length = Physics.RaycastNonAlloc(ray, enemiesRaycastHits, Vector3.Distance(attackCheckPoints[i].position, lastCheckPointsPosition[i]), enemyLayer);
                        //若检测到了敌人
                        if (length > 0)
                        {
                            foreach (RaycastHit enemy in enemiesRaycastHits)
                            {
                                //TODO: 此处改为调用Enemy身上的受伤函数
                                EnemyBase enemyHit = enemy.transform.GetComponent<BossFSM>();
                                enemyHit.TakeDamage(this.transform.gameObject);
                            }
                        }
                        //绘制从上一次记录的该点的位置到当前该点的位置的线段
                        Debug.DrawRay(lastCheckPointsPosition[i], (attackCheckPoints[i].position - lastCheckPointsPosition[i]).normalized * Vector3.Distance(attackCheckPoints[i].position, lastCheckPointsPosition[i]), Color.red, 2f);
                    }
                }
                //记录上一次Check时攻击判定点的位置
                for (int i = 0; i < attackCheckPoints.Length; i++)
                {
                    lastCheckPointsPosition[i] = attackCheckPoints[i].position;
                }
                timeCounter = 0f; //计时器归零，重新开始计时
            }
        }
        else
        {
            isFirstCheck = true;
            lastCheckPointsPosition = null;
        }
    }

    public void StartAttacking()
    {
        isAttacking = true;
    }

    public void EndAttacking()
    {
        isAttacking = false;
    }
}
