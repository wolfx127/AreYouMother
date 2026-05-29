using Cysharp.Threading.Tasks;
using System;

/// <summary>
/// Buff 类型枚举
/// Poison - 中毒效果：周期性扣除MP
/// Bleed - 流血效果：周期性扣除HP
/// </summary>
public enum BuffType
{
    Poison,     // 中毒（扣蓝）
    Bleed       // 流血（扣血）
}

/// <summary>
/// Buff 基类 - 使用TaskMgr驱动定时触发
/// 子类需要实现OnTick方法来定义具体的Buff效果
/// </summary>
public abstract class Buff
{
    // ========== 属性 ==========
    /// <summary>Buff类型</summary>
    public BuffType BuffType { get; protected set; }

    /// <summary>当前层数，可叠加</summary>
    public int Stacks { get; set; }

    /// <summary>总持续时间（秒）</summary>
    public float Duration { get; set; }

    /// <summary>触发间隔（秒），每间隔这段时间触发一次OnTick</summary>
    public float TickInterval { get; set; }

    /// <summary>Buff是否已过期，过期后会被BuffMgr清理</summary>
    public bool IsExpired { get;  set; } = false;

    // ========== 私有字段 ==========
    private bool _isApplied = false;  // 是否已启动，防止重复启动
    private bool _isRunning = false;  // 是否正在运行，用于停止时标记

    // ========== 生命周期 ==========

    /// <summary>
    /// 启动Buff，开始定时触发和倒计时
    /// 由BuffMgr在添加Buff时调用
    /// </summary>
    public void Start()
    {
        if (_isApplied) return;  // 防止重复启动
        _isApplied = true;
        _isRunning = true;

        OnApply();              // 首次应用效果
        StartTickLoop();        // 启动周期性触发
        StartDurationTimer();   // 启动过期倒计时
    }

    /// <summary>
    /// 停止Buff，标记为过期并触发移除回调
    /// 由BuffMgr在手动移除Buff时调用
    /// </summary>
    public void Stop()
    {
        _isRunning = false;
        if (!IsExpired)
        {
            IsExpired = true;
            OnRemove();
        }
    }

    // ========== 定时驱动（使用TaskMgr） ==========

    /// <summary>
    /// 启动周期性触发循环
    /// 使用TaskMgr.AddLoopTask实现无限循环触发
    /// </summary>
    private void StartTickLoop()
    {
        if (TickInterval <= 0) return;

        TaskMgr.AddLoopTask(
            () =>
            {
                // 只有运行中且未过期时才触发
                if (_isRunning && !IsExpired)
                {
                    OnTick();
                }
            },
            TickInterval  // 每次触发间隔
        ).Forget();       // Forget表示不等待任务完成
    }

    /// <summary>
    /// 启动持续时间倒计时
    /// 使用TaskMgr.AddTask实现延迟执行
    /// </summary>
    private void StartDurationTimer()
    {
        if (Duration <= 0) return;

        TaskMgr.AddTask(
            () =>
            {
                // 时间到，标记过期并触发移除
                if (_isRunning && !IsExpired)
                {
                    IsExpired = true;
                    OnRemove();
                }
            },
            Duration  // 延迟时间
        ).Forget();
    }

    // ========== 子类需要实现的方法 ==========

    /// <summary>
    /// 周期性触发的效果，子类必须实现
    /// 例如：中毒扣MP、流血扣HP
    /// </summary>
    protected abstract void OnTick();

    /// <summary>
    /// Buff首次应用时的回调，可选实现
    /// 例如：播放特效、显示UI提示
    /// </summary>
    public virtual void OnApply() { }

    /// <summary>
    /// Buff移除时的回调，可选实现
    /// 例如：停止特效、清理资源
    /// </summary>
    public virtual void OnRemove() { }
}
