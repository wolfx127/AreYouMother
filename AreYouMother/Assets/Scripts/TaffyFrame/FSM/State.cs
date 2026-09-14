using System;
using System.Collections.Generic;
using UnityEngine;

namespace TaffyFrame.FSM
{
    public enum State
    {
        NULL,
        Idle_Player,
        
        Idel_Enemy,
    }

    /// <summary>
    /// 需要在构造函数里填: 转换表
    /// 使用board时，需要 b as target 目标类型
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class BaseState<T> where T : class
    {
        public State state;
        public List<Transition> transitionTable =  new List<Transition>();
        public abstract void Enter(T caller, IBoard b);
        public abstract void Update(T caller, IBoard b);
        public abstract void Exit(T caller, IBoard b);
    }
}
