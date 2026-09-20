using System.Runtime.ExceptionServices;
using Taffy.Play.Player;
using TaffyFrame.FSM;
using UnityEngine;

namespace Taffy.Play.Player
{
    public class PlayerFSM : BaseFSM<PlayDataManager>
    {
        public PlayerFSM(PlayDataManager caller, Board_Player b, State state)
        {
            cur = state;
            this.b = b;
            this.caller = caller;
            stateTable[State.Idle] = new State_Idle_Player();
            stateTable[State.Walk]   = new State_Walk_Player();
            stateTable[State.Attack] = new State_Attack_Player();
            stateTable[State.Injury] = new State_Injury_Player();
            stateTable[State.Dead] = new State_Dead_Player();

            foreach (var s in stateTable.Values)
            {
                s.caller =  caller;
                s.b = b;
            }
            Debug.Log($"{caller} 加载状态机成功");
        }


        public void SetBoard(int HP, int MP, bool idle, bool walk, bool attack, bool injury)
        {
            if (b is not Board_Player board) return;
            board.HP = HP;
            board.MP = MP;
            board.idle = idle;
            board.walk = walk;
            board.attack = attack;
            board.injury = injury;
        }
    }
    
/////// 状态 /////////////////////////////////////////////////////////
#region 状态

public class State_Idle_Player : BaseState<PlayDataManager>
{
    public State_Idle_Player()
    {
        transitionTable.Add(new Transition(State.Dead, b=> ((Board_Player)b).HP <= 0));
        transitionTable.Add(new Transition(State.Attack, b => ((Board_Player)b).attack));
        transitionTable.Add(new Transition(State.Injury, b => ((Board_Player)b).injury));
        transitionTable.Add(new Transition( State.Walk, b => ((Board_Player)b).walk));
    }
    
    public override void Enter()
    {
        
    }

    public override void Update()
    {
        // InjuryCounter 由 AnimPlay() 内部递减，这里不要重复减（否则硬直/无敌时间只有配置值的一半）
        caller.AnimPlay();
    }

    public override void Exit()
    {
        
    }
}

public class State_Walk_Player : BaseState<PlayDataManager>
{
    public State_Walk_Player()
    {
        transitionTable.Add(new Transition(State.Dead, b=> ((Board_Player)b).HP <= 0));
        transitionTable.Add(new Transition(State.Attack, b => ((Board_Player)b).attack));
        transitionTable.Add(new Transition(State.Injury, b => ((Board_Player)b).injury));
        transitionTable.Add(new Transition( State.Idle, b => ((Board_Player)b).idle));
    }

    public override void Enter()
    {
        
    }

    public override void Update()
    {
        // InjuryCounter 由 AnimPlay() 内部递减，这里不要重复减（否则硬直/无敌时间只有配置值的一半）
        caller.AnimPlay();
    }

    public override void Exit()
    {
        
    }
}

public class State_Attack_Player : BaseState<PlayDataManager>
{
    public State_Attack_Player()
    {
        transitionTable.Add(new Transition(State.Dead, b=> ((Board_Player)b).HP <= 0));
        transitionTable.Add(new Transition(State.Injury, b => ((Board_Player)b).injury));
        // 攻击状态要一直待到 isAttack 被清掉（异步攻击协程或 AnimPlay 里 AttackCounter 走完），
        // 不能按 walk/idle 退出 —— 因为 Move() 每帧都会把 walk/idle 其中一个置 true，
        // 那样攻击状态会立刻退出，和 Idle/Walk 来回横跳，AttackAnim 永远播不出来。
        transitionTable.Add(new Transition( State.Idle, b => !((Board_Player)b).attack));
    }

    public override void Enter()
    {
        caller.AttackCounter = caller.AttackAnimTime;
    }

    public override void Update()
    {
        // InjuryCounter 由 AnimPlay() 内部递减，这里不要重复减（否则硬直/无敌时间只有配置值的一半）
        caller.AnimPlay();
    }

    public override void Exit()
    {
        
    }
}

public class State_Injury_Player : BaseState<PlayDataManager>
{
    public State_Injury_Player()
    {
        transitionTable.Add(new Transition(State.Dead, b=> ((Board_Player)b).HP <= 0));
        // 受伤状态要一直待到 isInjury 被 AnimPlay 清零（受伤硬直结束），
        // 不能按 walk/idle 退出 —— walk/idle 永远有一个为 true，会导致本状态一进来就被踢出去，
        // Update()（Damage + AnimPlay）永远不执行：血不掉、UI 不刷新、动画全卡死。
        transitionTable.Add(new Transition( State.Idle, b => !((Board_Player)b).injury));
    }

    public override void Enter()
    {
        caller.InjuryCounter =  caller.InjuryAnimTime;
        caller.Damage();   // 进受伤状态时一次性结算伤害（原来放 Update，横跳时永远执行不到）
    }

    public override void Update()
    {
        caller.AnimPlay();
    }

    public override void Exit()
    {
    }
}

public class State_Dead_Player : BaseState<PlayDataManager>
{
    public override void Enter()
    {
        caller.isDead = true;
        caller.Die();
    }

    public override void Update()
    {
        
    }

    public override void Exit()
    {
        
    }
}

#endregion

///////// 黑板 ////////////////////////////////////////////
    public class Board_Player : IBoard
    {
        public int HP = 0;
        public int MP = 0;

        public bool attack = false;
        public bool injury = false;
        public bool idle = false;
        public bool walk = false;

//TODO: buff，用bool表示
    }
}
