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
    public Vector3 SpawnPosition { get; private set; }
    public Vector3 WanderTarget { get; set; }
    public bool IsWanderingRight { get; set; }

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
        SpawnPosition = selfTransform.position;
        IsWanderingRight = true;
        GenerateNextWanderTarget();
    }

    public void UpdateBoard()
    {
        // 更新目标：优先找距离最近的玩家
        UpdateTargetPlayer();
    }

    /// <summary>
    /// 更新目标玩家 - 优先攻击距离最近的
    /// </summary>
    private void UpdateTargetPlayer()
    {
        Transform playerA = PlayingHandler_A.Instance?.transform;
        Transform playerB = PlayingHandler_B.Instance?.transform;

        float distA = playerA != null ?
            Vector3.Distance(SelfTransform.position, playerA.position) : float.MaxValue;
        float distB = playerB != null ?
            Vector3.Distance(SelfTransform.position, playerB.position) : float.MaxValue;

        // 选择距离最近的玩家
        if (distA <= distB)
        {
            TargetPlayer = playerA;
        }
        else
        {
            TargetPlayer = playerB;
        }
    }

    /// <summary>
    /// 生成下一个游荡目标点
    /// </summary>
    public void GenerateNextWanderTarget()
    {
        Vector3 offset;
        if (Data.wanderHorizontal)
        {
            offset = IsWanderingRight ?
                new Vector3(Data.wanderDistance, 0, 0) :
                new Vector3(-Data.wanderDistance, 0, 0);
        }
        else
        {
            offset = IsWanderingRight ?
                new Vector3(0, 0, Data.wanderDistance) :
                new Vector3(0, 0, -Data.wanderDistance);
        }

        WanderTarget = SpawnPosition + offset;
        IsWanderingRight = !IsWanderingRight;
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
