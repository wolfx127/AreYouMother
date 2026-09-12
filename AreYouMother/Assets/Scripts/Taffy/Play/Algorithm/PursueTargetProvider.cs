using System.Collections.Generic;
using UnityEngine;

namespace Taffy.Play.Algorithm
{
    /// <summary>
    /// 追击目标（玩家）位置提供者。
    ///
    /// TODO：项目里还没有现成的“玩家位置”公共 API——
    ///   ① ECS 里已有 PursueTargetComp（存 Char player），但 Char 内部的 Entity 是 internal，托管侧读不出来；
    ///   ② 托管侧目前只有 PlayingHandler_A.Instance 单例（B 玩家连 Handler 都还是空的）。
    /// 所以这里先做“尽力而为”的默认实现：找最近的 Tag=Player_Tag 玩家。
    /// 等项目的玩家控制器/仇恨表做好后，把 GetTargetPosition 换成真实接口即可。
    /// </summary>
    public static class PursueTargetProvider
    {
        static readonly List<GameObject> players = new List<GameObject>(2);
        static float nextScanTime = float.NegativeInfinity;

        /// <summary>
        /// 返回这个敌人当前该追的玩家位置（世界 xy）。
        /// 场景里没有任何存活玩家时返回 false（调用方应让敌人原地待机，而不是“攻击空气”）。
        /// </summary>
        public static bool TryGetTargetPosition(GameObject enemy, out Vector2 pos)
        {
            RefreshIfNeeded();

            Vector2 from = enemy.transform.position;
            pos = from;
            float bestSqr = float.MaxValue;

            for (int i = 0; i < players.Count; i++)
            {
                GameObject p = players[i];
                if (p == null || !p.activeInHierarchy) continue;
                Vector2 pPos = p.transform.position;
                float d = (pPos - from).sqrMagnitude;
                if (d < bestSqr)
                {
                    bestSqr = d;
                    pos = pPos;
                }
            }

            return bestSqr < float.MaxValue;
        }

        /// <summary>兼容包装：没有玩家时返回敌人自身位置（调用方自己决定怎么办）。</summary>
        public static Vector2 GetTargetPosition(GameObject enemy)
        {
            TryGetTargetPosition(enemy, out Vector2 pos);
            return pos;
        }

        static void RefreshIfNeeded()
        {
            if (Time.time < nextScanTime) return;
            nextScanTime = Time.time + 0.5f;

            players.Clear();
            GameObject[] found = GameObject.FindGameObjectsWithTag("Player_Tag");
            players.AddRange(found);
        }
    }
}
