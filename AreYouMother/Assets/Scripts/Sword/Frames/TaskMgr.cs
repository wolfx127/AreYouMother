using Cysharp.Threading.Tasks;
using System;
using System.Threading;
public static class TaskMgr
{
    // <summary>
    /// 添加一个定时器
    /// </summary>
    /// <param name="callBack">回调函数</param>
    /// <param name="delaySeconds">延迟时间（秒）</param>
    /// <param name="cancellationToken">取消令牌（可选）</param> 这个东西 可以提前 取消任务 不让他执行了
    public static async UniTask AddTask(Action callBack, float delaySeconds, CancellationToken cancellationToken = default)
    {
        await UniTask.Delay((int)(delaySeconds * 1000), cancellationToken: cancellationToken);//指定参数名称传递 就不用把可选项补全了
        callBack?.Invoke();
    }

    //这是它的 参数列表 如果 不用指定名称的话 前面的默认参数 都得补充
    //public static UniTask Delay(int millisecondsDelay, bool ignoreTimeScale = false,
    //PlayerLoopTiming delayTiming = PlayerLoopTiming.Update,
    //CancellationToken cancellationToken = default(CancellationToken),
    //bool cancelImmediately = false)



    // 等待 frameCount 帧
    public static async UniTask AddFrameDelay(Action callBack, int frameCount, CancellationToken cancellationToken = default)
    {
        await UniTask.DelayFrame(frameCount, cancellationToken: cancellationToken);
        callBack?.Invoke();
    }





    /// <summary>
    /// 循环任务
    /// </summary>
    /// <param name="callBackPerTimes"></param> 每次进行完 回调
    /// <param name="callBackWhenFinal"></param> 全部进行完 回调
    /// <param name="delaySeconds"></param> 循环次数
    /// <param name="Times"></param> 每次Delay时间 单位秒
    /// <param name="cancellationToken"></param> 取消令牌
    /// <returns></returns>
    public static async UniTask AddLoopTask(Action callBackPerTimes, Action callBackWhenFinal, float delaySeconds, int Times,  CancellationToken cancellationToken = default)
    {
        for (int i = 0; i < Times; i++)
        {
            await UniTask.Delay((int)(delaySeconds * 1000), cancellationToken: cancellationToken);//指定参数名称传递 就不用把可选项补全了
            callBackPerTimes?.Invoke();
        }
        callBackWhenFinal?.Invoke();
    }



    /// <summary>
    /// 循环任务 无限循环
    /// </summary>
    /// <param name="callBackPerTimes"></param> 每次进行完 回调
    /// <param name="delaySeconds"></param> 每次Delay时间 单位秒
    /// <param name="cancellationToken"></param> 取消令牌
    /// <returns></returns>
    public static async UniTask AddLoopTask(Action callBackPerTimes, float delaySeconds, CancellationToken cancellationToken = default)
    {
        while (true)
        {
            await UniTask.Delay((int)(delaySeconds * 1000), cancellationToken: cancellationToken);//指定参数名称传递 就不用把可选项补全了
            callBackPerTimes?.Invoke();
        }


    }


}
