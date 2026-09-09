using System.Collections.Generic;

namespace TaffyFrame.FSM
{
    /// <summary>
    /// 要在子类的构造函数里填: stateTable记录的映射关系、FSM持有的caller是谁、黑板b的类型
    /// 实现一个SetBoard()方法，填入黑板，需要 b as，因为参数各异就不写抽象方法了 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class BaseFSM<T> where T : class
    {
        public IBoard b;
        public State cur;
        public Dictionary<State,BaseState<T>> stateTable = new Dictionary<State,BaseState<T>>();
        public T caller;
        
        public void Tick()
        {
            if (cur != TaffyFrame.FSM.State.NULL)
            {
                foreach (var transition in stateTable[cur].transitionTable)
                {
                    if (transition.condition != null && transition.condition(b))
                    {
                        stateTable[cur].Exit(caller, b);
                        stateTable[transition.to].Enter(caller, b);
                        cur = transition.to;
                        return;
                    }
                    if (transition.trigger != null && transition.trigger(b))
                    {
                        stateTable[cur].Exit(caller, b);
                        stateTable[transition.to].Enter(caller, b);
                        cur = transition.to;
                        return;
                    }
                }
                stateTable[cur].Update(caller, b);
            }
        }
    }
}
