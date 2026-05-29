using UnityEngine;
using Taffy.Data;

/// <summary>
/// 中毒Buff - 扣蓝（MP）
/// </summary>
public class PoisonBuff : Buff
{
    private int _damagePerTick;
    private PropOwner _target;

    public PoisonBuff(PropOwner target, int damagePerTick, float duration, int stacks = 1)
    {
        BuffType = BuffType.Poison;
        _target = target;
        _damagePerTick = damagePerTick;
        Duration = duration;
        Stacks = stacks;
        TickInterval = 1f; // 每秒触发一次
    }

    public override void OnApply()
    {
        Debug.Log($"玩家 {_target} 中毒了！层数: {Stacks}");
    }

    protected override void OnTick()
    {
        // 中毒扣蓝
        int totalDamage = _damagePerTick * Stacks;
        Debug.Log($"玩家 {_target} 中毒触发，扣除 {totalDamage} 点MP");

        // TODO: 通过EventBus发送扣蓝事件
        // EventBus.Publish(new DamageMPEvent { Target = _target, Amount = totalDamage });
    }

    public override void OnRemove()
    {
        Debug.Log($"玩家 {_target} 中毒效果结束");
    }
}

/// <summary>
/// 流血Buff - 扣血（HP）
/// </summary>
public class BleedBuff : Buff
{
    private int _damagePerTick;
    private int _minHpThreshold;
    private PropOwner _target;

    public BleedBuff(PropOwner target, int damagePerTick, float duration, int minHpThreshold = 1, int stacks = 1)
    {
        BuffType = BuffType.Bleed;
        _target = target;
        _damagePerTick = damagePerTick;
        _minHpThreshold = minHpThreshold;
        Duration = duration;
        Stacks = stacks;
        TickInterval = 1f; // 每秒触发一次
    }

    public override void OnApply()
    {
        Debug.Log($"玩家 {_target} 流血了！层数: {Stacks}");
    }

    protected override void OnTick()
    {
        // TODO: 获取玩家当前血量，检查是否低于阈值
        // 这里简化处理，假设可以扣血
        int totalDamage = _damagePerTick * Stacks;
        Debug.Log($"玩家 {_target} 流血触发，扣除 {totalDamage} 点HP");

        // TODO: 通过EventBus发送扣血事件
        // EventBus.Publish(new DamageHPEvent { Target = _target, Amount = totalDamage });
    }

    public override void OnRemove()
    {
        Debug.Log($"玩家 {_target} 流血效果结束");
    }
}
