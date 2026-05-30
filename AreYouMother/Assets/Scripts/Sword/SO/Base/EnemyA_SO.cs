using UnityEngine;
using Taffy.Data;

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
