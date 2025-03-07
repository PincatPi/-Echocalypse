using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class EnemyBase : MonoBehaviour
{
    //敌人公共字段
    public float moveSpeed; //移动速度
    public float rotationSpeed; //旋转速度
    public float health; //血量
    public float defense; //防御力
    public float attack; //攻击力
    
    //战斗相关
    [SerializeField] protected Transform detectionCenter;
    [SerializeField] protected float detectionRadius;
    [SerializeField] protected LayerMask playerLayer;
    [SerializeField] protected LayerMask obstacleLayer;
    [SerializeField] protected Collider[] targets = new Collider[1];
    [SerializeField, Header("攻击目标")] protected Transform currentTarget = null;
    [SerializeField, Range(0f, 360f)] protected float detectAngle;
    [SerializeField] protected GameObject attacker;
    
    //巡逻
    public Transform[] patrolPoints;
    
    //组件
    protected Animator animator;

    //敌人公共方法
    //受到攻击
    public virtual void TakeDamage(GameObject other) { }
    
    //死亡
    public virtual void OnDeath() { }
    
    public virtual void Move() { }
    
    public virtual void Attack() { }
    
    //视野
    protected virtual void View()
    {
        int targetCount = Physics.OverlapSphereNonAlloc(detectionCenter.position, detectionRadius, targets, playerLayer);
        bool isInView = false;
        //若玩家在检测范围内
        if (targetCount > 0)
        {
            //射线检测障碍物
            if (IsInView(targets[0].transform))
            {
                //检测玩家是否在该对象面前一定角度的范围内
                if (Vector3.Dot(((targets[0].transform.position + new Vector3(0, 1f, 0)) - (transform.root.position + new Vector3(0, 1.2f, 0))).normalized,
                        transform.root.forward) > Mathf.Cos(Mathf.Deg2Rad * detectAngle / 2))
                {
                    currentTarget = targets[0].transform;
                    isInView = true;
                }
            }
        }
        if (!isInView)
        {
            currentTarget = null;
            targets[0] = null;
        }
    }
    
    /// <summary>
    /// 检测玩家对象在视野中是否可见
    /// </summary>
    /// <param name="target"></param>
    /// <returns>true为可见，false为不可见</returns>
    protected virtual bool IsInView(Transform target)
    {
        bool isInView = false;
        for (int i = 5; i <= 20; i += 5)
        {
            float offset = i / 10f;
            //若检测到了障碍物(只检测障碍物层)
            //从头部向target从root开始依次向上每隔0.5f发射一条射线，若有一条射线命中了（检测不到障碍物）则说明看得到，返回true
            if (Physics.Raycast((detectionCenter.position),
                    ((target.root.position + target.root.up * offset) - detectionCenter.position).normalized,
                    out RaycastHit hit, Vector3.Distance(detectionCenter.position, target.root.position + target.root.up * offset), obstacleLayer) == false)
            {
                return true;
            }
        }
        return false;
    }
}
