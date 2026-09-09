using System.Runtime.ExceptionServices;
using Taffy.Play.Player;
using TaffyFrame.FSM;

namespace Taffy.Play.Player
{
    public class PlayerFSM : BaseFSM<PlayDataManager>
    {
        public PlayerFSM(PlayDataManager caller, Board_Player b)
        {
            this.b = b;
            this.caller = caller;
            stateTable[TaffyFrame.FSM.State.Idle_Player] = new State_Idle_Player();
        }

        public void SetBoard(int HP, int MP)
        {
            if (b is not Board_Player board) return;
            board.HP = HP;
            board.MP = MP;
        }
    }
    
/////// 状态 /////////////////////////////////////////////////////////
#region 状态

public class State_Idle_Player : BaseState<PlayDataManager>
{
    public State_Idle_Player()
    {
        
    }
    
    public override void Enter(PlayDataManager caller, IBoard b)
    {
        caller.Idle();
    }

    public override void Update(PlayDataManager caller, IBoard b)
    {
        caller.Idle();
    }

    public override void Exit(PlayDataManager caller, IBoard b)
    {
        
    }
}
#endregion

///////// 黑板 ////////////////////////////////////////////
    public class Board_Player : IBoard
    {
        public int HP = 0;
        public int MP = 0;
        
//TODO: buff，用bool表示
        public void SetBoard(int HP, int MP)
        {
            this.HP = HP;
            this.MP = MP;
        }
    }
}
