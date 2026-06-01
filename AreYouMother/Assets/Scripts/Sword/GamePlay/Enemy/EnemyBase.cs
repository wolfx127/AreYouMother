using UnityEngine;

/// <summary>
/// 敌人基类 - FSM驱动
/// </summary>
public abstract class EnemyBase : MonoBehaviour
{
    [SerializeField] protected EnemySOBase enemyData;

    [Header("【视觉子物体】")]
    [SerializeField] protected Transform visualRoot;   // 拖入挂 SpriteRenderer+Animator 的子物体，不设则默认为自身

    public Transform VisualRoot => visualRoot != null ? visualRoot : transform;

    protected Fsm _fsm;
    protected EnemyDataBoard _dataBoard;
    protected Animator _animator;
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int AttackHash = Animator.StringToHash("Attack");
    private static readonly int IsDeadHash = Animator.StringToHash("IsDead");

    protected virtual void Start()
    {
        _animator = VisualRoot.GetComponent<Animator>();
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

    /// <summary>
    /// 设置视觉朝向（仅旋转视觉子物体，保持根节点和碰撞箱垂直）
    /// </summary>
    public void SetFacingDirection(Vector3 direction)
    {
        if (direction.sqrMagnitude > 0.0001f)
            VisualRoot.rotation = Quaternion.LookRotation(direction);
    }

    /// <summary>
    /// 设置动画速度参数（0=Idle, >0=移动）
    /// </summary>
    public void SetAnimSpeed(float speed)
    {
        if (_animator != null)
            _animator.SetFloat(SpeedHash, speed);
    }

    /// <summary>
    /// 触发攻击动画
    /// </summary>
    public void TriggerAnimAttack()
    {
        if (_animator != null)
            _animator.SetTrigger(AttackHash);
    }

    /// <summary>
    /// 设置死亡动画状态
    /// </summary>
    public void SetAnimDead(bool isDead)
    {
        if (_animator != null)
            _animator.SetBool(IsDeadHash, isDead);
    }
}
