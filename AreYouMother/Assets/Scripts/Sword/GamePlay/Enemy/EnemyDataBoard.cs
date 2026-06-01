using UnityEngine;
using Taffy.Play.Player;

/// <summary>
/// 敌人数据黑板 - 存储FSM需要的共享数据
/// </summary>
public class EnemyDataBoard : IDataBoard
{
    // 配置数据
    public EnemySOBase Data { get; private set; }

    // 运行时数据
    public Transform SelfTransform { get; private set; }
    public int CurrentHP { get; set; }
    public bool IsDead => CurrentHP <= 0;

    // 目标相关
    public Transform TargetPlayer { get; set; }
    public float DistanceToTarget => TargetPlayer != null ?
        Vector3.Distance(SelfTransform.position, TargetPlayer.position) : float.MaxValue;

    // 游荡相关
    public Vector3 WanderDirection { get; private set; }
    private float _wanderTimer;

    // 攻击相关
    public float LastAttackTime { get; set; } = -999f;
    public bool HasAttacked { get; set; } = false;

    // 组件缓存
    public EnemyBase EnemyComponent { get; private set; }

    public EnemyDataBoard(EnemySOBase data, Transform selfTransform, EnemyBase enemyComponent)
    {
        Data = data;
        SelfTransform = selfTransform;
        EnemyComponent = enemyComponent;
        CurrentHP = data.hp;
        InitWander();
    }

    public void UpdateBoard()
    {
        // 更新目标：优先找距离最近的玩家
        UpdateTargetPlayer();
    }

    /// <summary>
    /// 更新目标玩家 - 优先攻击距离最近的（已死亡的玩家不会被仇恨）
    /// </summary>
    private void UpdateTargetPlayer()
    {
        var stateCtrl = PlayerCurrentStateController.Instance;

        Transform playerA = PlayingHandler_A.Instance?.transform;
        Transform playerB = PlayingHandler_B.Instance?.transform;

        // 死亡的玩家视为不可索敌
        bool deadA = stateCtrl != null && stateCtrl.GetIsDead_A();
        bool deadB = stateCtrl != null && stateCtrl.GetIsDead_B();

        float distA = (playerA != null && !deadA) ?
            Vector3.Distance(SelfTransform.position, playerA.position) : float.MaxValue;
        float distB = (playerB != null && !deadB) ?
            Vector3.Distance(SelfTransform.position, playerB.position) : float.MaxValue;

        // 两个玩家都不可索敌时清空目标，避免继续仇恨
        if (distA == float.MaxValue && distB == float.MaxValue)
        {
            TargetPlayer = null;
            return;
        }

        // 选择距离最近的玩家
        TargetPlayer = distA <= distB ? playerA : playerB;
    }

    /// <summary>
    /// 根据出生点XY哈希决定游荡轴向和初始方向，初始化游荡计时器
    /// </summary>
    private void InitWander()
    {
        Vector3 pos = SelfTransform.position;
        int hash = Mathf.Abs(pos.x.GetHashCode() + pos.y.GetHashCode());

        // bit0 决定轴向：0=水平(X), 1=垂直(Z)
        bool isHorizontal = (hash & 1) == 0;
        // bit1 决定正负方向
        bool positive = (hash & 2) == 0;

        WanderDirection = isHorizontal
            ? (positive ? Vector3.right : Vector3.left)
            : (positive ? Vector3.forward : Vector3.back);

        _wanderTimer = Data.wanderDuration;
    }

    /// <summary>
    /// 游荡计时器更新：倒计时到零后反向并重置
    /// </summary>
    public void UpdateWanderTimer(float deltaTime)
    {
        _wanderTimer -= deltaTime;
        if (_wanderTimer <= 0f)
        {
            WanderDirection = -WanderDirection;
            _wanderTimer = Data.wanderDuration;
        }
    }

    /// <summary>
    /// 检查是否在仇恨范围内
    /// </summary>
    public bool IsInAggroRange()
    {
        return TargetPlayer != null && DistanceToTarget <= Data.aggroRadius;
    }

    /// <summary>
    /// 检查是否丢失仇恨
    /// </summary>
    public bool IsOutOfAggroRange()
    {
        return TargetPlayer == null || DistanceToTarget > Data.loseAggroRadius;
    }
}
