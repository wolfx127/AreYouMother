using UnityEngine;

/// <summary>
/// Buff配置数据 - 定义Buff的各项参数
/// 用于BuffMgr创建Buff实例时读取配置
/// 创建路径：Assets/Create/GameSO/BuffConfig
/// </summary>
[CreateAssetMenu(fileName = "BuffConfig", menuName = "GameSO/BuffConfig")]
public class BuffConfig : ScriptableObject
{
    [Header("【基础信息】")]
    [Tooltip("Buff类型 - 中毒(Poison)扣MP，流血(Bleed)扣HP")]
    public BuffType BuffType;

    [Tooltip("基础扣减值 - 每次触发时扣除的HP或MP数值")]
    public int BaseValue;

    [Header("【时间设置】")]
    [Tooltip("持续时间 - Buff总共持续的时间（秒），结束后自动移除")]
    public float Duration;

    [Tooltip("触发间隔 - 每隔多少秒触发一次效果（如每秒扣血）")]
    public float TickInterval;

    [Header("【叠加设置】")]
    [Tooltip("是否可叠加 - true=同类型Buff可以叠加层数，false=新Buff会刷新旧Buff")]
    public bool CanStack;

    [Tooltip("最大层数 - 该Buff最多可以叠加到多少层")]
    public int MaxStacks;

    [Header("【特殊设置】")]
    [Tooltip("最低HP阈值 - 仅流血(Bleed)有效，HP不会扣到此数值以下（保护机制）")]
    public int MinHpThreshold;
}
