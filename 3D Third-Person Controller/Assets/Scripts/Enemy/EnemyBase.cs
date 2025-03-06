using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    //敌人公共字段
    public float moveSpeed; //移动速度
    public float rotationSpeed; //旋转速度
    public float health; //血量
    public float defense; //防御力
    public float attack; //攻击力
    
    //敌人公共方法
    //受到攻击
    public virtual void OnHit() { }
    //死亡
    public virtual void OnDeath() { }
}
