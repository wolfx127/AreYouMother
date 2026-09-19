using System;
using System.Collections.Generic;
using UnityEngine;

namespace Taffy.Play.Trigger
{
    /// <summary>
    /// 玩家的撤离判定，挂在玩家子物体 EvacuateTrigger 上（默认 inactive）。
    /// 流程：玩家按撤离键 → PlayingHandler 把这个子物体 SetActive(true) → 本组件开始工作
    ///  - 站在撤离点(tag = EvacuateZone)里才倒计时，倒计时满 stayTime 后发 EvacuateEvent
    ///  - 激活后有一小段宽限期，等物理步把 OnTriggerEnter 送过来，期间不判定失败
    ///  - 宽限期过了还没进撤离点 → 自己 SetActive(false)，玩家必须再按一次
    ///  - 倒计时中离开撤离点 → 取消计时并 SetActive(false)，再按一次从头计时
    /// 死亡中止撤离由 PlayingHandler 负责（Die 里会把本子物体关掉）
    /// </summary>
    [DisallowMultipleComponent]
    public class EvacuateTrigger : MonoBehaviour
    {
        /// <summary>撤离点的 Tag</summary>
        private const string ZoneTag = "EvacuateZone";

        [Header("站在撤离点里需要坚持的秒数")]
        [SerializeField] private float stayTime = 5f;

        [Header("激活后的宽限期(秒)：等物理步送来 OnTriggerEnter，期间不判定失败")]
        [SerializeField] private float graceTime = 0.15f;

        /// <summary>倒计时结束时触发（由 PlayingHandler 订阅）</summary>
        public event Action EvacuateEvent;

        /// <summary>是否正在倒计时</summary>
        public bool IsCounting => isCounting;

        private readonly HashSet<Collider> zones = new HashSet<Collider>();
        private float counter;
        private float grace;
        private bool isCounting;
        private bool hasEvacuated;

        private void OnEnable()
        {
            zones.Clear();
            counter = stayTime;
            grace = graceTime;
            isCounting = false;
            hasEvacuated = false;
        }

        private void OnDisable()
        {
            isCounting = false;
            zones.Clear();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other && other.CompareTag(ZoneTag))
            {
                zones.Add(other);
                Debug.Log("开始撤离");
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other != null) zones.Remove(other);
        }

        private void Update()
        {
            if (hasEvacuated) return;

            if (grace > 0f) grace -= Time.deltaTime;

            if (!InZone)
            {
                // 宽限期内先不判定：激活当帧物理还没跑，OnTriggerEnter 还没送到
                if (grace > 0f) return;
                Cancel();
                return;
            }

            isCounting = true;
            counter -= Time.deltaTime;
            if (counter > 0f) return;

            // 计时满：只发一次
            isCounting = false;
            hasEvacuated = true;
            EvacuateEvent?.Invoke();
        }

        /// <summary>取消计时并关掉自己：玩家必须再按一次撤离键</summary>
        private void Cancel()
        {
            isCounting = false;
            counter = stayTime;
            gameObject.SetActive(false);
        }

        /// <summary>当前是否站在撤离点里（顺手清理被销毁的 Collider）</summary>
        private bool InZone
        {
            get
            {
                zones.RemoveWhere(c => !c);
                return zones.Count > 0;
            }
        }
    }
}
