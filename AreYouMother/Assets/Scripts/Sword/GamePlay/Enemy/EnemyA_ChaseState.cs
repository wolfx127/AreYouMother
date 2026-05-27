using UnityEngine;

/// <summary>
/// A类敌人追击状态 - 进入视野就切换到远程攻击状态
/// </summary>
public class EnemyA_ChaseState : ChaseState
{
    private EnemyDataBoard _board;
    private EnemyA_SO _rangedData;

    public EnemyA_ChaseState(Fsm fsm) : base(fsm)
    {
        _board = fsm.board as EnemyDataBoard;
       
    }

    protected override bool CanAttack()
    {
        // A类敌人只要进入视野范围就开始攻击（不需要距离很近）
        if (_board.IsInAggroRange())
        {
            _fsm.SwitchState<RangedAttackState>();
            return true;
        }
        return false;
    }
}
