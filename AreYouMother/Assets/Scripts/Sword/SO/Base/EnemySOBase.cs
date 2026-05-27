using UnityEngine;
using Taffy.Data;

/// <summary>
/// 攻击类型接口基类
/// </summary>
public interface IAttackType { }

/// <summary>
/// 远程攻击接口 - A类敌人实现此接口
/// </summary>
public interface IRemoteAttack : IAttackType
{
    /// <summary>发射物预制体在Resources文件夹中的路径</summary>
    string ProjectilePrefabPath { get; }

    /// <summary>发射物飞行速度</summary>
    float ProjectileSpeed { get; }

    /// <summary>每次攻击之间的间隔时间（秒）</summary>
    float AttackInterval { get; }

    /// <summary>特殊效果触发概率，范围0-1（如中毒20% = 0.2）</summary>
    float SpecialEffectChance { get; }

    /// <summary>特殊效果类型（中毒/流血等）</summary>
    BuffType SpecialEffectType { get; }
}

/// <summary>
/// 近战攻击接口 - B类敌人实现此接口
/// </summary>
public interface IMeleeAttack : IAttackType
{
    /// <summary>近战攻击的有效范围（米）</summary>
    float AttackRange { get; }

    /// <summary>攻击后进入冷却的时间（秒）</summary>
    float CooldownTime { get; }

    /// <summary>特殊效果触发概率，范围0-1（如流血20% = 0.2）</summary>
    float SpecialEffectChance { get; }

    /// <summary>特殊效果类型（中毒/流血等）</summary>
    BuffType SpecialEffectType { get; }
}

/// <summary>
/// 敌人数据基类 - 定义所有敌人的通用属性
/// </summary>
public abstract class EnemySOBase : ScriptableObject
{
    [Header("【基础属性】")]
    [Tooltip("攻击力 - 每次攻击造成的伤害值")]
    public int atk = 10;

    [Tooltip("生命值 - 敌人的总血量")]
    public int hp = 100;

    [Tooltip("移动速度 - 敌人的移动速率")]
    public float moveSpeed = 3f;

    [Header("【游荡设置】")]
    [Tooltip("游荡距离 - 在出生点左右/上下游荡的范围")]
    public float wanderDistance = 5f;

    [Tooltip("游荡方向 - true=水平左右游荡，false=垂直上下游荡")]
    public bool wanderHorizontal = true;

    [Header("【仇恨设置】")]
    [Tooltip("仇恨范围 - 发现玩家并开始追击的距离")]
    public float aggroRadius = 10f;

    [Tooltip("丢失仇恨范围 - 超出此距离后停止追击")]
    public float loseAggroRadius = 15f;

    [Header("【归属设置】")]
    [Tooltip("从属关系 - A=只攻击玩家A，B=只攻击玩家B，Public=攻击所有玩家")]
    public PropOwner owner = PropOwner.Public;
}

/// <summary>
/// A类敌人数据 - 远程攻击型，命中玩家有概率使其中毒
/// 创建路径：Assets/Create/GameSO/EnemyA_Data
/// </summary>
[CreateAssetMenu(fileName = "EnemyA_Data", menuName = "GameSO/EnemyA_Data")]
public class EnemyA_SO : EnemySOBase, IRemoteAttack
{
    [Header("【远程攻击】")]
    [Tooltip("发射物预制体路径 - 相对于Resources文件夹的路径，如：Prefabs/Projectiles/EnemyProjectile")]
    public string projectilePrefabPath = "Prefabs/Projectiles/EnemyProjectile";

    [Tooltip("发射物速度 - 投射物的飞行速度")]
    public float projectileSpeed = 10f;

    [Tooltip("攻击间隔 - 每次射击之间的间隔时间（秒）")]
    public float attackInterval = 3f;

    [Tooltip("特殊效果触发概率 - 0到1之间，0.2表示20%概率触发中毒")]
    public float specialEffectChance = 0.2f;

    [Tooltip("特殊效果类型 - 通常设置为Poison（中毒）")]
    public BuffType specialEffectType = BuffType.Poison;

    // 接口实现
    public string ProjectilePrefabPath => projectilePrefabPath;
    public float ProjectileSpeed => projectileSpeed;
    public float AttackInterval => attackInterval;
    public float SpecialEffectChance => specialEffectChance;
    public BuffType SpecialEffectType => specialEffectType;
}

/// <summary>
/// B类敌人数据 - 近战攻击型，命中玩家有概率使其流血
/// 创建路径：Assets/Create/GameSO/EnemyB_Data
/// </summary>
[CreateAssetMenu(fileName = "EnemyB_Data", menuName = "GameSO/EnemyB_Data")]
public class EnemyB_SO : EnemySOBase, IMeleeAttack
{
    [Header("【近战攻击】")]
    [Tooltip("攻击范围 - 近战攻击的有效距离（米）")]
    public float attackRange = 1.5f;

    [Tooltip("冷却时间 - 攻击后原地停留的时间（秒）")]
    public float cooldownTime = 2f;

    [Tooltip("特殊效果触发概率 - 0到1之间，0.2表示20%概率触发流血")]
    public float specialEffectChance = 0.2f;

    [Tooltip("特殊效果类型 - 通常设置为Bleed（流血）")]
    public BuffType specialEffectType = BuffType.Bleed;

    // 接口实现
    public float AttackRange => attackRange;
    public float CooldownTime => cooldownTime;
    public float SpecialEffectChance => specialEffectChance;
    public BuffType SpecialEffectType => specialEffectType;
}
