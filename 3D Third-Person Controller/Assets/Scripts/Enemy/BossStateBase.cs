using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossStateBase : IState
{
    private BossFSM fsm;
    private Boss boss;
    //初始化函数
    public void Init(BossFSM bossFSM)
    {
        fsm = bossFSM;
        boss = bossFSM.parameters;
    }
    public virtual void OnEnter()
    {
    }

    public virtual void OnUpdate()
    {
    }

    public virtual void OnExit()
    {
    }
}
