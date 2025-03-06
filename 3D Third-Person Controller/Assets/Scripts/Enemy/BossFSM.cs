using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.Compilation;
using UnityEngine;
using Assembly = System.Reflection.Assembly;

public enum E_BossState
{
    Idle, Patrol, Dead
}
/// <summary>
/// Boss的有限状态机
/// </summary>
public class BossFSM : EnemyBase
{
    private IState currentState;
    public Boss parameters;
    private Dictionary<E_BossState, IState> stateDic = new Dictionary<E_BossState, IState>();
    
    void Start()
    {
        parameters = GetComponent<Boss>();
        //注册状态
        //获取所有继承自IState的派生类
        var stateTypes = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.IsSubclassOf(typeof(BossStateBase)));
        //遍历stateTypes数组，对每个类依次实例化
        foreach (var stateType in stateTypes)
        {
            //通过反射构造对象
            BossStateBase state = Activator.CreateInstance(stateType) as BossStateBase;
            //对对象进行初始化
            if (state != null)
            {
                state.Init(this);
                //将构造出的对象加入到字典中进行状态注册
                stateDic.Add(GetStateEnumType(stateType), state);
            }
            else
            {
                Debug.LogError(string.Format("BossFSM state '{0}' not found", stateType.Name));
            }
        }
        //设置初始状态为Idle状态
        SwitchState(E_BossState.Idle);
    }
    
    void Update()
    {
        this.OnUpdate(); //所有状态共有的逻辑
        currentState.OnUpdate(); //每个状态特有的逻辑
    }
    
    /// <summary>
    /// 执行所有状态共有的逻辑
    /// </summary>
    private void OnUpdate()
    {
        View();
    }

    /// <summary>
    /// 切换状态机当前状态
    /// </summary>
    /// <param name="newState"></param>
    private void SwitchState(E_BossState newState)
    {
        //退出旧状态
        if (currentState != null)
            currentState.OnExit();
        //切换新状态
        currentState = stateDic[newState];
        //执行新状态的进入逻辑
        currentState.OnEnter();
    }
    
    /// <summary>
    /// 根据类型获得对应的枚举
    /// </summary>
    /// <param name="stateType"></param>
    /// <returns></returns>
    private E_BossState GetStateEnumType(Type stateType)
    {
        //获取状态的名字字符串，并去掉其中的“State”字段
        string stateName = stateType.Name.Replace("State", "");
        return Enum.Parse<E_BossState>(stateName);
    }

    #region 重写方法

    protected override void View()
    {
        base.View();
    }

    #endregion
}
