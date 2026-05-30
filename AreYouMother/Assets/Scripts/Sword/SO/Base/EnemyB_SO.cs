using UnityEngine;
using Taffy.Data;

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
