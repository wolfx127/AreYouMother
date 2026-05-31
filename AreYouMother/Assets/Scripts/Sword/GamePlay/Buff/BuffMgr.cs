using System.Collections.Generic;
using UnityEngine;
using Taffy.Data;

/// <summary>
/// Buff管理器 - 管理所有玩家的Buff（使用TaskMgr驱动）
/// </summary>
public class BuffMgr : MonoBehaviour
{
    public static BuffMgr Instance { get; private set; }

    // 每个玩家的Buff列表
    private Dictionary<PropOwner, List<Buff>> _playerBuffs;

    [Header("Buff配置")]
    [SerializeField] private BuffConfig poisonConfig;
    [SerializeField] private BuffConfig bleedConfig;

    [Header("清理设置")]
    [SerializeField] private float cleanupInterval = 2f;   // 每隔多少秒清理一次过期 Buff
    private float _lastCleanupTime;

    private void Awake()
    {
        Instance = this;
        _playerBuffs = new Dictionary<PropOwner, List<Buff>>
        {
            { PropOwner.A, new List<Buff>() },
            { PropOwner.B, new List<Buff>() }
        };

        // 检测配置
        if (poisonConfig == null)
        {
            Debug.LogWarning("BuffMgr: PoisonConfig 未配置，将使用默认值");
        }
        if (bleedConfig == null)
        {
            Debug.LogWarning("BuffMgr: BleedConfig 未配置，将使用默认值");
        }
    }

    private void Update()
    {
        // 定期清理过期 Buff，避免内存泄漏
        if (Time.time - _lastCleanupTime >= cleanupInterval)
        {
            _lastCleanupTime = Time.time;
            CleanupExpiredBuffs();
        }
    }

    /// <summary>
    /// 添加Buff到指定玩家
    /// </summary>
    public void AddBuff(PropOwner target, BuffType buffType, int stacks = 1)
    {
        if (!_playerBuffs.ContainsKey(target))
        {
            Debug.LogWarning($"无效的目标: {target}");
            return;
        }

        var buffs = _playerBuffs[target];

        // 检查是否已存在同类型Buff
        var existingBuff = buffs.Find(b => b.BuffType == buffType);
        if (existingBuff != null)
        {
            // 叠加层数
            var config = GetConfig(buffType);
            if (config != null && config.CanStack && existingBuff.Stacks < config.MaxStacks)
            {
                existingBuff.Stacks = Mathf.Min(existingBuff.Stacks + stacks, config.MaxStacks);
                // 刷新持续时间（通过Stop+重新Start）
                RefreshBuffDuration(existingBuff, config);
                Debug.Log($"玩家 {target} 的 {buffType} 叠加到 {existingBuff.Stacks} 层");
            }
            else
            {
                // 不可叠加或已达最大层数，刷新持续时间
                if (config != null)
                {
                    RefreshBuffDuration(existingBuff, config);
                }
            }
        }
        else
        {
            // 创建新Buff
            Buff newBuff = CreateBuff(target, buffType, stacks);
            if (newBuff != null)
            {
                buffs.Add(newBuff);
                newBuff.Start(); // 启动Buff（使用TaskMgr驱动）
            }
        }
    }

    /// <summary>
    /// 刷新Buff持续时间
    /// </summary>
    private void RefreshBuffDuration(Buff buff, BuffConfig config)
    {
        // 停止旧的定时器
        buff.Stop();
        // 重置状态
        buff.IsExpired = false;
        buff.Duration = config.Duration;
        // 重新启动
        buff.Start();
    }

    /// <summary>
    /// 移除指定玩家的指定类型Buff
    /// </summary>
    public void RemoveBuff(PropOwner target, BuffType buffType)
    {
        if (!_playerBuffs.ContainsKey(target)) return;

        var buffs = _playerBuffs[target];
        var buff = buffs.Find(b => b.BuffType == buffType);
        if (buff != null)
        {
            buff.Stop();
            buffs.Remove(buff);
        }
    }

    /// <summary>
    /// 移除指定玩家的所有Buff
    /// </summary>
    public void RemoveAllBuffs(PropOwner target)
    {
        if (!_playerBuffs.ContainsKey(target)) return;

        var buffs = _playerBuffs[target];
        foreach (var buff in buffs)
        {
            buff.Stop();
        }
        buffs.Clear();
    }

    /// <summary>
    /// 获取指定玩家的指定类型Buff
    /// </summary>
    public Buff GetBuff(PropOwner target, BuffType buffType)
    {
        if (!_playerBuffs.ContainsKey(target)) return null;
        return _playerBuffs[target].Find(b => b.BuffType == buffType);
    }

    /// <summary>
    /// 检查指定玩家是否有指定类型的Buff
    /// </summary>
    public bool HasBuff(PropOwner target, BuffType buffType)
    {
        return GetBuff(target, buffType) != null;
    }

    /// <summary>
    /// 获取指定玩家的所有Buff
    /// </summary>
    public List<Buff> GetAllBuffs(PropOwner target)
    {
        if (!_playerBuffs.ContainsKey(target)) return new List<Buff>();
        return new List<Buff>(_playerBuffs[target]);
    }

    /// <summary>
    /// 清理已过期的Buff（可选，用于内存优化）
    /// </summary>
    public void CleanupExpiredBuffs()
    {
        foreach (var kvp in _playerBuffs)
        {
            kvp.Value.RemoveAll(b => b.IsExpired);
        }
    }

    /// <summary>
    /// 创建Buff实例
    /// </summary>
    private Buff CreateBuff(PropOwner target, BuffType buffType, int stacks)
    {
        switch (buffType)
        {
            case BuffType.Poison:
                if (poisonConfig != null)
                {
                    return new PoisonBuff(target, poisonConfig.BaseValue, poisonConfig.Duration, stacks);
                }
                return new PoisonBuff(target, 5, 5f, stacks); // 默认值

            case BuffType.Bleed:
                if (bleedConfig != null)
                {
                    return new BleedBuff(target, bleedConfig.BaseValue, bleedConfig.Duration, bleedConfig.MinHpThreshold, stacks);
                }
                return new BleedBuff(target, 5, 5f, 1, stacks); // 默认值

            default:
                Debug.LogWarning($"未知的Buff类型: {buffType}");
                return null;
        }
    }

    /// <summary>
    /// 获取Buff配置
    /// </summary>
    private BuffConfig GetConfig(BuffType buffType)
    {
        switch (buffType)
        {
            case BuffType.Poison: return poisonConfig;
            case BuffType.Bleed: return bleedConfig;
            default: return null;
        }
    }

    /// <summary>
    /// 清除所有Buff（用于场景切换等）
    /// </summary>
    public void ClearAll()
    {
        foreach (var kvp in _playerBuffs)
        {
            RemoveAllBuffs(kvp.Key);
        }
    }
}
