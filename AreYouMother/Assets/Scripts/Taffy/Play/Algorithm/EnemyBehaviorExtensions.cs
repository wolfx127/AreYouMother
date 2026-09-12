using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Taffy.Play.Enemy;
using Taffy.Play.Player;
using Unity.Entities;
using UnityEngine;

namespace Taffy.Play.Algorithm
{
    /// <summary>
    /// 追击算法的两个入口（扩展方法）：
    ///
    /// ① 玩家 GameObject 的扩展方法 CallPursueOnEnemiesInHateRadius：
    ///    仇恨半径（CombatData.HateRadius）内所有 Tag=Enemy 的敌人调用 Pursue()。
    ///    用四叉树查范围，不遍历全场。建议玩家每帧调用一次。
    ///
    /// ② 敌人 GameObject 的扩展方法 GetPursueDirection：
    ///    内部 A* 寻路（绕开跳不过去的碰撞箱），返回敌人“现在该走”的 xy 归一化方向。
    ///    玩家进入攻击半径时停下（返回 zero）攻击一下（按攻速冷却），出了半径继续追。
    ///    建议在追击状态里每帧调用，把返回值喂给敌人的移动逻辑（比如 EnemySystem 的 PursueSystem）。
    /// </summary>
    public static class EnemyBehaviorExtensions
    {
        static readonly List<EnemyData> enemyBuffer = new List<EnemyData>(64);

        /// <summary>
        /// 每个敌人一份的运行时数据。
        /// 不修改 EnemyData 脚本，用 ConditionalWeakTable 挂在 EnemyData 实例上（敌人销毁时自动回收）。
        /// </summary>
        sealed class EnemyPursueState
        {
            public float LastAttackTime = float.NegativeInfinity;
            public readonly List<Vector2> Path = new List<Vector2>();
            public int PathIndex;
            public Vector2 LastTarget = new Vector2(float.NaN, float.NaN);
            public float LastPathTime = float.NegativeInfinity;
            public Entity LastEntity = Entity.Null; // 检测对象池复用：实体变了说明是“新一世”
        }

        static readonly ConditionalWeakTable<EnemyData, EnemyPursueState> runtimeTable =
            new ConditionalWeakTable<EnemyData, EnemyPursueState>();

        /// <summary>
        /// ① 玩家 GameObject 的扩展方法：
        /// 仇恨半径内所有 Tag=Enemy 的敌人的 EnemyData 组件调用 Pursue()。
        ///
        /// 【仇恨避障·已定方案】不改四叉树，在仇恨判定时补一次“寻路长度”检查（见下方注释掉的代码）。
        /// </summary>
        public static void CallPursueOnEnemiesInHateRadius(this GameObject player)
        {
            if (player == null || QuadtreeManager.Instance == null) return;

            QuadtreeManager.Instance.QueryEnemiesInRadius(
                player.transform.position, CombatData.HateRadius, enemyBuffer);

            for (int i = 0; i < enemyBuffer.Count; i++)
            {
                EnemyData enemy = enemyBuffer[i];
                if (enemy == null) continue;
                if (!enemy.gameObject.CompareTag("Enemy")) continue;

                // ── 仇恨避障（选后者：不改四叉树，仇恨判定时算路径长度）──────────────
                // 为什么不动四叉树：四叉树只是纯空间索引，感知不了墙——隔着墙的敌人
                // 直线距离一样落在仇恨半径里。把寻路塞进四叉树会让每次范围查询都跑 A*，
                // 而且破坏“空间分区”和“寻路”两个职责。
                // 正确做法 = 粗筛 + 精筛：四叉树先按直线距离圈出候选，再对每个候选敌人
                // 算一遍路径；路径长度 &gt; 仇恨半径 → 被墙隔开，不拉仇恨。
                // 等 Pursue()/追击系统落地后，把下面注释掉的代码恢复即可；算出来的路径
                // 可以顺手写进 runtimeTable 缓存，GetPursueDirection 直接复用，不白算。
                //
                // if (!enemy.isPursue)   // 已经在追的不用重复判定
                // {
                //     List<Vector2> path = AStarPathfinder.FindPath(
                //         enemy.transform.position, player.transform.position);
                //     if (path == null) continue;   // 完全被墙堵死：不拉仇恨
                //     float pathLen = 0f;
                //     for (int p = 1; p < path.Count; p++)
                //         pathLen += Vector2.Distance(path[p - 1], path[p]);
                //     if (pathLen > CombatData.HateRadius) continue; // 绕路超过仇恨半径：不拉仇恨
                // }
                // ────────────────────────────────────────────────────────────────

                enemy.Pursue(); // 项目现有 API（目前是空方法，状态切换逻辑以后填在它里面）
            }
        }

        /// <summary>
        /// ② 敌人 GameObject 的扩展方法：
        /// 内部寻路，返回归一化的 xy 方向（Vector2）。
        /// 玩家进入攻击半径 → 停下（返回 zero）攻击一下 → 出半径继续追击，如此往复。
        /// </summary>
        public static Vector2 GetPursueDirection(this GameObject enemyGO)
        {
            if (enemyGO == null) return Vector2.zero;

            EnemyData enemy = enemyGO.GetComponent<EnemyData>();
            Vector2 from = enemyGO.transform.position;

            // 场景里没有玩家：原地待机（TODO：等玩家控制器落地后换成真实目标来源）
            if (!PursueTargetProvider.TryGetTargetPosition(enemyGO, out Vector2 to))
                return Vector2.zero;

            if (enemy != null)
            {
                // 玩家进了攻击半径：停下，冷却好了就攻击一下，然后继续追击
                float atkRadius = enemy.AttackRadius;
                if (atkRadius > 0f && (to - from).sqrMagnitude <= atkRadius * atkRadius)
                {
                    TryAttackOnce(enemy, runtimeTable.GetOrCreateValue(enemy));
                    return Vector2.zero;
                }
            }

            Vector2 dir = SteerAlongPath(enemy, from, to);
            if (enemy != null) SyncEntityDirection(enemy, dir);
            return dir;
        }

        /// <summary>攻击一下：攻速按“每秒攻击次数”算成冷却时间。</summary>
        static void TryAttackOnce(EnemyData enemy, EnemyPursueState state)
        {
            // AttackSpeed <= 0 视为 1 次/秒，避免攻击半径内每帧都出手
            float cooldown = enemy.AttackSpeed > 0f ? 1f / enemy.AttackSpeed : 1f;
            if (Time.time - state.LastAttackTime < cooldown) return;

            state.LastAttackTime = Time.time;
            EnemyAttackBridge.AttackOnce(enemy); // TODO 空壳：等攻击系统落地后接真实 API
        }

        /// <summary>
        /// 沿缓存路径走：
        /// 目标动远了 / 路走完了 / 超时 → 重新寻路；
        /// 到达路点就切下一个；路径走完或没路就朝目标直走兜底。
        /// </summary>
        static Vector2 SteerAlongPath(EnemyData enemy, Vector2 from, Vector2 to)
        {
            if ((to - from).sqrMagnitude <= AlgorithmConfig.ArriveRadius * AlgorithmConfig.ArriveRadius)
                return Vector2.zero; // 已经贴脸，不用动

            if (enemy == null)
                return (to - from).normalized;

            EnemyPursueState state = runtimeTable.GetOrCreateValue(enemy);

            // 对象池复用同一组件实例：实体变了说明是“新一世”，清掉上一世的路径和攻击冷却
            if (state.LastEntity != enemy.entity)
            {
                state.LastEntity = enemy.entity;
                state.Path.Clear();
                state.PathIndex = 0;
                state.LastTarget = new Vector2(float.NaN, float.NaN);
                state.LastAttackTime = float.NegativeInfinity;
            }

            bool targetMoved = float.IsNaN(state.LastTarget.x) ||
                (to - state.LastTarget).sqrMagnitude > AlgorithmConfig.RepathDistance * AlgorithmConfig.RepathDistance;
            bool timedOut = Time.time - state.LastPathTime > AlgorithmConfig.RepathInterval;
            bool pathExhausted = state.Path.Count > 0 && state.PathIndex >= state.Path.Count;

            // 注意：没路时 Path 为空，不能用“路走完”当重寻路条件，否则每帧白跑一次 A*。
            // 没路只按超时/目标移动重试，期间朝目标直走兜底。
            if (targetMoved || timedOut || pathExhausted)
            {
                List<Vector2> path = AStarPathfinder.FindPath(from, to);
                state.Path.Clear();
                if (path != null) state.Path.AddRange(path);
                state.PathIndex = 0;
                state.LastTarget = to;
                state.LastPathTime = Time.time;
            }

            float arrive = AlgorithmConfig.WaypointArriveDistance;
            while (state.PathIndex < state.Path.Count &&
                   (state.Path[state.PathIndex] - from).sqrMagnitude <= arrive * arrive)
                state.PathIndex++;

            Vector2 dir;
            if (state.PathIndex < state.Path.Count) dir = state.Path[state.PathIndex] - from;
            else dir = to - from; // 路径走完或没路：直走兜底

            return dir.sqrMagnitude < 1e-8f ? Vector2.zero : dir.normalized;
        }

        /// <summary>
        /// 把方向同步回 ECS 实体：
        /// 现有 DirectionComp（x/y 轴走向）更新后，EnemyData.Update 里的贴图翻转和
        /// 以后的 PursueSystem 移动都能直接拿到方向。
        /// </summary>
        static void SyncEntityDirection(EnemyData enemy, Vector2 dir)
        {
            if (enemy.entity == Entity.Null) return;
            World world = World.DefaultGameObjectInjectionWorld;
            if (world == null) return;
            EntityManager em = world.EntityManager;
            if (!em.Exists(enemy.entity) || !em.HasComponent<DirectionComp>(enemy.entity)) return;

            em.SetComponentData(enemy.entity, new DirectionComp { x = dir.x >= 0f, y = dir.y >= 0f });
        }
    }
}
