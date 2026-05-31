using UnityEngine;

/// <summary>
/// 巡逻状态 - 在出生点附近游荡
/// </summary>
public class PatrolState : IState
{
    protected Fsm _fsm;
    private EnemyDataBoard _board;

    public PatrolState(Fsm fsm)
    {
        _fsm = fsm;
        _board = fsm.board as EnemyDataBoard;
    }

    public void OnEnter()
    {
        // 进入巡逻状态
    }

    public void OnUpdate()
    {
        // 检查是否发现玩家
        if (_board.IsInAggroRange())
        {
            _fsm.SwitchState<ChaseState>();
            return;
        }

        // 游荡移动
        MoveToWanderTarget();
    }

    public void OnFixUpdate() { }

    public void OnExit() { }

    private void MoveToWanderTarget()
    {
        Vector3 direction = (_board.WanderTarget - _board.SelfTransform.position).normalized;
        direction.y = 0;

        float distanceToTarget = Vector3.Distance(_board.SelfTransform.position, _board.WanderTarget);

        if (distanceToTarget < 0.1f)
        {
            // 到达目标点，生成下一个游荡点
            _board.GenerateNextWanderTarget();
        }
        else
        {
            // 继续移动
            _board.SelfTransform.position += direction * _board.Data.moveSpeed * Time.deltaTime;

            // 面向移动方向
            if (direction != Vector3.zero)
            {
                _board.SelfTransform.rotation = Quaternion.LookRotation(direction);
            }
        }
    }
}

/// <summary>
/// 追击状态 - 向玩家移动
/// </summary>
public class ChaseState : IState
{
    protected Fsm _fsm;
    private EnemyDataBoard _board;

    public ChaseState(Fsm fsm)
    {
        _fsm = fsm;
        _board = fsm.board as EnemyDataBoard;
    }

    public void OnEnter()
    {
        // 进入追击状态
    }

    public void OnUpdate()
    {
        // 检查是否丢失仇恨
        if (_board.IsOutOfAggroRange())
        {
            _fsm.SwitchState<PatrolState>();
            return;
        }

        // 检查是否可以攻击（子类会重写这个逻辑）
        if (CanAttack())
        {
            return;
        }

        // 追击玩家
        MoveToTarget();
    }

    public void OnFixUpdate() { }

    public void OnExit() { }

    /// <summary>
    /// 检查是否可以攻击 - 子类可重写
    /// </summary>
    protected virtual bool CanAttack()
    {
        return false;
    }

    private void MoveToTarget()
    {
        if (_board.TargetPlayer == null) return;

        Vector3 direction = (_board.TargetPlayer.position - _board.SelfTransform.position).normalized;
        direction.y = 0;

        _board.SelfTransform.position += direction * _board.Data.moveSpeed * Time.deltaTime;

        // 面向目标
        if (direction != Vector3.zero)
        {
            _board.SelfTransform.rotation = Quaternion.LookRotation(direction);
        }
    }
}

/// <summary>
/// 远程攻击状态 - A类敌人专用
/// </summary>
public class RangedAttackState : IState
{
    protected Fsm _fsm;
    private EnemyDataBoard _board;
    private EnemyA_SO _rangedData;
    private float _stateEnterTime;
    private const float MinLingerTime = 0.5f;  // 进入状态后至少停留的时间，避免在仇恨边界反复横跳

    public RangedAttackState(Fsm fsm)
    {
        _fsm = fsm;
        _board = fsm.board as EnemyDataBoard;
        _rangedData = _board.Data as EnemyA_SO;
    }

    public void OnEnter()
    {
        _stateEnterTime = Time.time;
    }

    public void OnUpdate()
    {
        // 检查是否丢失仇恨（进入状态后至少停留 MinLingerTime，避免在边界反复横跳）
        if (Time.time - _stateEnterTime >= MinLingerTime && _board.IsOutOfAggroRange())
        {
            _fsm.SwitchState<PatrolState>();
            return;
        }

        // 面向玩家
        if (_board.TargetPlayer != null)
        {
            Vector3 direction = (_board.TargetPlayer.position - _board.SelfTransform.position).normalized;
            direction.y = 0;
            if (direction != Vector3.zero)
            {
                _board.SelfTransform.rotation = Quaternion.LookRotation(direction);
            }
        }

        // 检查攻击间隔
        if (Time.time - _board.LastAttackTime >= _rangedData.AttackInterval)
        {
            // 发射投射物
            FireProjectile();
            _board.LastAttackTime = Time.time;
        }
    }

    public void OnFixUpdate() { }

    public void OnExit() { }

    private void FireProjectile()
    {
        // 通知EnemyBase发射投射物
        _board.EnemyComponent.OnRangedAttack();
    }
}

/// <summary>
/// 近战攻击状态 - B类敌人专用
/// </summary>
public class MeleeAttackState : IState
{
    private Fsm _fsm;
    private EnemyDataBoard _board;
    private EnemyB_SO _meleeData;

    public MeleeAttackState(Fsm fsm)
    {
        _fsm = fsm;
        _board = fsm.board as EnemyDataBoard;
        _meleeData = _board.Data as EnemyB_SO;
    }

    public void OnEnter()
    {
        // 执行攻击
        PerformAttack();
        _board.HasAttacked = true;
        _board.LastAttackTime = Time.time;
    }

    public void OnUpdate()
    {
        // 攻击完成后立即切换到冷却状态
        _fsm.SwitchState<IdleCooldownState>();
    }

    public void OnFixUpdate() { }

    public void OnExit() { }

    private void PerformAttack()
    {
        // 通知EnemyBase执行近战攻击
        _board.EnemyComponent.OnMeleeAttack();
    }
}

/// <summary>
/// 空闲冷却状态 - B类敌人攻击后使用
/// </summary>
public class IdleCooldownState : IState
{
    private Fsm _fsm;
    private EnemyDataBoard _board;
    private EnemyB_SO _meleeData;
    private float _stateEnterTime;
    private const float MinLingerTime = 0.5f;  // 进入状态后至少停留的时间，避免在仇恨边界反复横跳

    public IdleCooldownState(Fsm fsm)
    {
        _fsm = fsm;
        _board = fsm.board as EnemyDataBoard;
        _meleeData = _board.Data as EnemyB_SO;
    }

    public void OnEnter()
    {
        _stateEnterTime = Time.time;
    }

    public void OnUpdate()
    {
        // 检查是否丢失仇恨（进入状态后至少停留 MinLingerTime，避免在边界反复横跳）
        if (Time.time - _stateEnterTime >= MinLingerTime && _board.IsOutOfAggroRange())
        {
            _fsm.SwitchState<PatrolState>();
            return;
        }

        // 检查冷却是否结束
        if (Time.time - _stateEnterTime >= _meleeData.CooldownTime)
        {
            _board.HasAttacked = false;

            // 检查是否还在攻击范围内
            if (_board.DistanceToTarget <= _meleeData.AttackRange)
            {
                _fsm.SwitchState<MeleeAttackState>();
            }
            else
            {
                _fsm.SwitchState<ChaseState>();
            }
        }
    }

    public void OnFixUpdate() { }

    public void OnExit() { }
}

/// <summary>
/// 死亡状态
/// </summary>
public class DeathState : IState
{
    private Fsm _fsm;
    private EnemyDataBoard _board;
    private float _deathTimer;

    public DeathState(Fsm fsm)
    {
        _fsm = fsm;
        _board = fsm.board as EnemyDataBoard;
    }

    public void OnEnter()
    {
        _deathTimer = 0f;
        // 播放死亡动画
        _board.EnemyComponent.OnDeath();
    }

    public void OnUpdate()
    {
        _deathTimer += Time.deltaTime;

        // 延迟销毁
        if (_deathTimer >= 2f)
        {
            Object.Destroy(_board.SelfTransform.gameObject);
        }
    }

    public void OnFixUpdate() { }

    public void OnExit() { }
}
