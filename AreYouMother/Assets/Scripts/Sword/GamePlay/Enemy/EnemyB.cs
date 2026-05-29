using Taffy.Data;
using UnityEngine;

/// <summary>
/// B类敌人 - 近战攻击，20%概率流血
/// </summary>
public class EnemyB : EnemyBase
{
    [SerializeField] private Transform attackPoint;

    private EnemyB_SO _meleeData;

    protected override void Start()
    {
        // 转换数据类型并检测配置
        _meleeData = enemyData as EnemyB_SO;
        if (_meleeData == null)
        {
            Debug.LogError($"{gameObject.name} 的数据不是 EnemyB_SO 类型！请检查Inspector中拖拽的SO文件。");
            return;
        }

        base.Start();
    }

    protected override void InitFSM()
    {
        // 创建数据黑板
        _dataBoard = new EnemyDataBoard(enemyData, transform, this);

        // 创建FSM
        _fsm = new Fsm(_dataBoard);

        // 注册状态（追击状态注册到基类 ChaseState 的 key 下，
        // 这样 PatrolState/IdleCooldownState 里的 SwitchState<ChaseState>() 才能命中）
        _fsm.AddState<PatrolState>(fsm => new PatrolState(fsm));
        _fsm.AddState<ChaseState>(fsm => new EnemyB_ChaseState(fsm));
        _fsm.AddState<MeleeAttackState>(fsm => new MeleeAttackState(fsm));
        _fsm.AddState<IdleCooldownState>(fsm => new IdleCooldownState(fsm));
        _fsm.AddState<DeathState>(fsm => new DeathState(fsm));

        // 初始状态
        _fsm.SwitchState<PatrolState>();
    }

    public override void OnMeleeAttack()
    {
        if (_meleeData == null) return;

        // 执行近战攻击检测
        PerformMeleeAttack();
    }

    private void PerformMeleeAttack()
    {
        Vector3 attackCenter = GetAttackPosition();

        // 检测攻击范围内的玩家
        Collider[] hitColliders = Physics.OverlapSphere(attackCenter, _meleeData.AttackRange);

        foreach (var hitCollider in hitColliders)
        {
            // 检查是否击中玩家A
            var playerA = hitCollider.GetComponent<Taffy.Play.Player.PlayingHandler_A>();
            if (playerA != null && CanAttackPlayer(PropOwner.A))
            {
                DamagePlayer(PropOwner.A, _meleeData.atk);
                TryApplySpecialEffect(PropOwner.A);
                continue;
            }

            // 检查是否击中玩家B
            var playerB = hitCollider.GetComponent<Taffy.Play.Player.PlayingHandler_B>();
            if (playerB != null && CanAttackPlayer(PropOwner.B))
            {
                DamagePlayer(PropOwner.B, _meleeData.atk);
                TryApplySpecialEffect(PropOwner.B);
            }
        }

        // 播放攻击动画/特效
        PlayAttackEffect();
    }

    /// <summary>
    /// 检查是否可以攻击该玩家（根据从属关系）
    /// </summary>
    private bool CanAttackPlayer(PropOwner playerOwner)
    {
        // Public敌人可以攻击所有玩家
        if (_meleeData.owner == PropOwner.Public)
            return true;

        // 否则只能攻击对应归属的玩家
        return _meleeData.owner == playerOwner;
    }

    /// <summary>
    /// 对玩家造成伤害
    /// </summary>
    private void DamagePlayer(PropOwner playerOwner, int damage)
    {
        var stateCtrl = Taffy.Play.Player.PlayerCurrentStateController.Instance;
        if (stateCtrl == null)
        {
            Debug.LogWarning("PlayerCurrentStateController.Instance 为空，无法对玩家造成伤害");
            return;
        }

        switch (playerOwner)
        {
            case PropOwner.A:
                stateCtrl.Injury_A(damage);
                break;
            case PropOwner.B:
                stateCtrl.Injury_B(damage);
                break;
        }

        Debug.Log($"B类敌人对玩家 {playerOwner} 造成 {damage} 点近战伤害");
    }

    /// <summary>
    /// 尝试施加特殊效果（流血）
    /// </summary>
    private void TryApplySpecialEffect(PropOwner playerOwner)
    {
        if (Random.value < _meleeData.SpecialEffectChance)
        {
            Debug.Log($"对玩家 {playerOwner} 施加 {_meleeData.SpecialEffectType} 效果");

            // TODO: 通过EventBus发送添加Buff事件
            // EventBus.Publish(new ApplyBuffEvent { Target = playerOwner, BuffType = _meleeData.SpecialEffectType });
        }
    }

    /// <summary>
    /// 播放攻击特效
    /// </summary>
    private void PlayAttackEffect()
    {
        // TODO: 播放攻击动画、音效、粒子效果等
        Debug.Log($"{gameObject.name} 执行近战攻击");
    }

    private Vector3 GetAttackPosition()
    {
        if (attackPoint != null)
        {
            return attackPoint.position;
        }
        return transform.position + transform.forward * 0.5f;
    }

    private void OnDrawGizmosSelected()
    {
        // 绘制视野范围
        if (enemyData != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, enemyData.aggroRadius);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, enemyData.loseAggroRadius);
        }

        // 绘制攻击范围
        if (_meleeData != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(GetAttackPosition(), _meleeData.AttackRange);
        }

        // 绘制攻击点
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(attackPoint.position, 0.1f);
        }
    }
}
