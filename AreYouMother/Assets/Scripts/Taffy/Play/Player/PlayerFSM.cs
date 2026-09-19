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
        transitionTable.Add(new Transition( State.Idle, b => ((Board_Player)b).idle));
        transitionTable.Add(new Transition( State.Walk, b => ((Board_Player)b).walk));
    }

    public override void Enter()
    {
        caller.AttackCounter = caller.AttackAnimTime;
    }

    public override void Update()
    {
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
        transitionTable.Add(new Transition(State.Attack, b => ((Board_Player)b).attack));
        transitionTable.Add(new Transition( State.Idle, b => ((Board_Player)b).idle));
        transitionTable.Add(new Transition( State.Walk, b => ((Board_Player)b).walk));
    }

    public override void Enter()
    {
        caller.InjuryCounter =  caller.InjuryAnimTime;
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
