using System;
using System.Threading;
using Cysharp.Threading.Tasks;

/// <summary>
/// UniTask 计时器工具类：延迟 delaySeconds 秒后执行一次 callback。
/// 零 GC、可取消；传 GameObject 销毁令牌可自动作废（this.GetCancellationTokenOnDestroy()）。
/// </summary>
public static class TaskMgr
{
    /// <summary>
    /// 延迟 delaySeconds 秒后执行一次 callback（时间在前，委托在后）。
    /// 例：TaskMgr.AddTask(0.2f, () => canAttack = true).Forget();
    /// </summary>
    public static UniTask AddTask(float delaySeconds, Action callback, CancellationToken cancellationToken = default)
    {
        return DelayAction(delaySeconds, callback, cancellationToken);
    }

    private static async UniTask DelayAction(float delaySeconds, Action callback, CancellationToken ct)
    {
        await UniTask.Delay(TimeSpan.FromSeconds(delaySeconds), cancellationToken: ct);
        callback?.Invoke();
    }
}
