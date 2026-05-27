using UnityEngine;

/// <summary>
/// 敌人基类 - FSM驱动
/// </summary>
public abstract class EnemyBase : MonoBehaviour
{
    [SerializeField] protected EnemySOBase enemyData;

    protected Fsm _fsm;
    protected EnemyDataBoard _dataBoard;

    protected virtual void Start()
    {
        InitFSM();
    }

    protected virtual void Update()
    {
        if (_fsm == null) return;

        _fsm.UpdateDataBoard();
        _fsm.FsmUpdate();

        // 检查死亡
        if (_dataBoard.IsDead && !(_fsm.GetCurrState() is DeathState))
        {
            _fsm.SwitchState<DeathState>();
        }
    }

    protected virtual void FixedUpdate()
    {
        _fsm?.FsmFixUpdate();
    }

    /// <summary>
    /// 初始化FSM - 子类重写
    /// </summary>
    protected abstract void InitFSM();

    /// <summary>
    /// 远程攻击回调 - A类敌人实现
    /// </summary>
    public virtual void OnRangedAttack() { }

    /// <summary>
    /// 近战攻击回调 - B类敌人实现
    /// </summary>
    public virtual void OnMeleeAttack() { }

    /// <summary>
    /// 死亡回调
    /// </summary>
    public virtual void OnDeath()
    {
        // 播放死亡动画、掉落物品等
        Debug.Log($"{gameObject.name} 死亡");
    }

    /// <summary>
    /// 受到伤害
    /// </summary>
    public virtual void TakeDamage(int damage)
    {
        _dataBoard.CurrentHP -= damage;
        Debug.Log($"{gameObject.name} 受到 {damage} 点伤害，剩余血量: {_dataBoard.CurrentHP}");
    }

    /// <summary>
    /// 获取数据黑板
    /// </summary>
    public EnemyDataBoard GetDataBoard() => _dataBoard;
}
