using UnityEngine;

/// <summary>
/// B类敌人追击状态 - 靠近到攻击范围才攻击
/// </summary>
public class EnemyB_ChaseState : ChaseState
{
    private EnemyDataBoard _board;
    private EnemyB_SO _meleeData;
    
    public EnemyB_ChaseState(Fsm fsm) : base(fsm)
    {
        _board = fsm.board as EnemyDataBoard;
        _meleeData = _board.Data as EnemyB_SO;
    }

    protected override bool CanAttack()
    {
        // B类敌人需要靠近到攻击范围内
        if (_board.DistanceToTarget <= _meleeData.AttackRange && !_board.HasAttacked)
        {
            base._fsm.SwitchState<MeleeAttackState>();
            return true;
        }
        return false;
    }
}
