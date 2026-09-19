using TaffyFrame.EventBus;
using UnityEngine;

namespace Taffy.Play.Place
{
    /// <summary>
    /// 对局结算的计数中心：两个玩家各自"死亡或成功撤离"一次，两次都到齐就发结算事件。
    /// 由 PlayingHandler_A / PlayingHandler_B 在 Die 和 Evacuate 里各调一次 DieOrEvacuate()。
    /// 走 EventBus 而不是 C# 事件：UI 的 OnEnable 可能早于本组件的 Awake，用单例订阅会空引用。
    /// </summary>
    public class EvacuateManager : MonoBehaviour
    {
        private int count = 0;
        private bool hasEvacuated = false;

        public static EvacuateManager Instance = null;

        private void Awake()
        {
            if (!Instance)
            {
                Instance = this;
            }

            count = 0;
            hasEvacuated = false;
        }

        private void OnDestroy()
        {
            Instance = null;
        }

        public void DieOrEvacuate()
        {
            count++;
            Evacuate();
        }

        private void Evacuate()
        {
            if (hasEvacuated) return;
            if (count < 2) return;

            hasEvacuated = true;
            EventBus.Publish(new EvacuateEvent());   // 类型即频道
        }
    }
}
