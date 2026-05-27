using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
#if UNITY_EDITOR
namespace _Examples
{
    public class TaskMgr_Template
    {
        /// <summary>
        /// UniTaskVoid 就是 没返回值 但是 可以 await 
        /// </summary>
        /// <returns></returns>
        async UniTaskVoid Test()
        {
            // 基本用法
            await TaskMgr.AddTask(() => Debug.Log("2秒后执行"), 2f); //await 表示 等待 也就是说 开一个 计时器  等待它干完之后 才进行 后边的 

            // 不等待的用法（Fire and Forget）
            TaskMgr.AddTask(() => Debug.Log("1秒后执行，不等待"), 1f).Forget();//不等待 直接进行后面的  比如 技能CD等 这个比较常用

            // 支持取消
            var cts = new CancellationTokenSource(); //这个东西 叫取消令牌  可以 终止任务 这个东西 可以提前 取消任务 不让他执行了
            TaskMgr.AddTask(() => Debug.Log("这行不会执行"), 5f, cts.Token).Forget();

            await UniTask.Delay(2000);
            cts.Cancel();  // 2秒后取消，5秒的回调不会执行



            // 3. 循环5次
            await TaskMgr.AddLoopTask(
                () => Debug.Log("Tick"),
                () => Debug.Log("完成"),
                5f,
                1
            );

            // 4. 无限循环（配合取消令牌）
            var cts1 = new CancellationTokenSource();
            TaskMgr.AddLoopTask(
                () => Debug.Log("无限循环"),
                1f,
                cts1.Token
            ).Forget();

            await UniTask.Delay(5000);
            cts.Cancel();  // 5秒后停止
        }



        /// <summary>
        /// 注意 如果 用 await 等待的话 你这个 函数 也要变成 async 如果要避免 这样子的话 可以用 forget 然后 就可以正常写了 
        /// </summary>
        void TestVoid()
        {
            

            // 不等待的用法（Fire and Forget）
            TaskMgr.AddTask(() => Debug.Log("1秒后执行，不等待"), 1f).Forget();//不等待 直接进行后面的  比如 技能CD等 这个比较常用

            // 支持取消
            var cts = new CancellationTokenSource(); //这个东西 叫取消令牌  可以 终止任务 这个东西 可以提前 取消任务 不让他执行了
            TaskMgr.AddTask(() => Debug.Log("这行不会执行"), 5f, cts.Token).Forget();

        
        }
    }
}
#endif

