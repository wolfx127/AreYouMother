using UnityEngine;

/// <summary>
/// 示例 FSM 用法 可以照着这个思路写
/// </summary>

/// <summary>
/// ⚠️ 示例 FSM 用法 - 仅供学习参考，请勿在正式代码中使用 ⚠️
/// </summary>
 #if UNITY_EDITOR
namespace _Examples
{
 
    public class FSM_Template : MonoBehaviour
    {
        Fsm boarFsm;

        BoarDataBoard board;

        private void Start()
        {
            //FSM初始化
            board = new BoarDataBoard(this.gameObject);
            boarFsm = new Fsm(board);
           



            //添加状态 切换到初始状态  //工厂版本
            boarFsm.AddState(fsm => new BoarChaseState(fsm));
            boarFsm.SwitchState<BoarChaseState>();





        }
    }
    public class BoarDataBoard : IDataBoard
    {
        public GameObject go;
        public Rigidbody rb;

        public BoarDataBoard(GameObject g)
        {
            go = g;
            rb = go.GetComponent<Rigidbody>();
        }
        public void UpdateBoard()
        {

        }


    }

    public class BoarChaseState : IState
    {

        public Fsm fsm;
        public BoarDataBoard board;
        Rigidbody rb;
        public BoarChaseState(Fsm fsm)
        {
            this.fsm = fsm;
            this.board = fsm.board as BoarDataBoard;
            rb = board.rb;
        }
        public void OnEnter()
        {
            Debug.Log("BoarChase ONENTER");
        }

        public void OnExit()
        {

        }

        public void OnFixUpdate()
        {

        }

        public void OnUpdate()
        {
         
        }
    }

}
#endif
