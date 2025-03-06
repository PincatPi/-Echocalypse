using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 有限状态机的状态类接口
/// </summary>
public interface IState
{
    public void OnEnter();

    public void OnUpdate();
    
    public void OnExit();
}
