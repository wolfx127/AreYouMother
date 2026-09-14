using System.Runtime.ExceptionServices;
using Taffy.Play.Player;
using TaffyFrame.FSM;

namespace Taffy.Play.Player
{
    public class PlayerFSM : BaseFSM<PlayDataManager>
    {
        public PlayerFSM(PlayDataManager caller, Board_Player b, State state)
        {
            cur = state;
            this.b = b;
            this.caller = caller;
            stateTable[TaffyFrame.FSM.State.Idle_Player] = new State_Idle_Player();
        }


        public void SetBoard(int HP = 0, int MP = 0, bool attack = false, bool walk = false)
        {
            if (b is not Board_Player board) return;
            board.HP = HP;
            board.MP = MP;
            board.walk = walk;
            board.attack = attack;
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
        
    }

    public override void Update(PlayDataManager caller, IBoard b)
    {
        
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

        public bool attack;
        public bool walk;
        
//TODO: buff，用bool表示
    }
}
