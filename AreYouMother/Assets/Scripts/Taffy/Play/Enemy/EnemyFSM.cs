using TaffyFrame.FSM;

namespace Taffy.Play.Enemy
{
    public class EnemyFSM : BaseFSM<Enemy>
    {
        public EnemyFSM(Enemy caller,EnemyBoard b)
        {
            this.caller = caller;
            this.b = b;

            stateTable[State.Idel_Enemy] = new State_Idel_Enemy();
        }

        public void SetBoard(int HP, bool IsPursue, bool IsWalk, bool IsIdle)
        {
            if (b is not EnemyBoard board) return;
            board.HP = HP;
            board.isPursue = IsPursue;
            board.isWalk = IsWalk;
            board.isIdle = IsIdle;
        }
    }

////////// 状态 ///////////////////////////////////////////////////////////////    

#region 状态

    public class State_Idel_Enemy : BaseState<Enemy>
    {
        public State_Idel_Enemy()
        {
            
        }

        public override void Enter(Enemy caller, IBoard b)
        {
            
        }

        public override void Update(Enemy caller, IBoard b)
        {
            caller.Idle();
        }

        public override void Exit(Enemy caller, IBoard b)
        {
            
        }
    }
    
    public class State_Walk_Enemy : BaseState<Enemy>
    {
        public State_Walk_Enemy()
        {
            
        }

        public override void Enter(Enemy caller, IBoard b)
        {
            
        }

        public override void Update(Enemy caller, IBoard b)
        {
            caller.Walk();
        }

        public override void Exit(Enemy caller, IBoard b)
        {
            
        }
    }
    
    public class State_Pursue_Enemy : BaseState<Enemy>
    {
        public State_Pursue_Enemy()
        {
            
        }

        public override void Enter(Enemy caller, IBoard b)
        {
            
        }

        public override void Update(Enemy caller, IBoard b)
        {
            caller.Pursue();
        }

        public override void Exit(Enemy caller, IBoard b)
        {
            
        }
    }

    #endregion
    
/////// 黑板 ///////////////////////////////////////////    
    
    public class EnemyBoard : IBoard
    {
        public int HP = 0;
        public bool isPursue = false;
        public bool isWalk = false;
        public bool isIdle = false;
    }
}
