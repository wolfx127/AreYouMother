using System.Collections.Generic;
using UnityEngine;

namespace Taffy.Play.Algorithm
{
    /// <summary>
    /// A* 用的格子地图：把世界 xy 平面切成 CellSize 大小的格子，
    /// 每个格子“能不能走”由盖住它的障碍物高度是否超过跳跃高度决定。
    /// </summary>
    public static class PathGrid
    {
        struct Obstacle
        {
            public Rect rect;      // 世界 xy 包围盒
            public float height;   // 世界高度（y 方向尺寸）
        }

        static readonly List<Obstacle> obstacles = new List<Obstacle>();
        static readonly Dictionary<Vector2Int, bool> walkCache = new Dictionary<Vector2Int, bool>();
        static float nextRefreshTime = float.NegativeInfinity;

        /// <summary>世界坐标 → 格子坐标。</summary>
        public static Vector2Int WorldToCell(Vector2 world)
        {
            float cell = AlgorithmConfig.CellSize;
            return new Vector2Int(Mathf.FloorToInt(world.x / cell), Mathf.FloorToInt(world.y / cell));
        }

        /// <summary>格子坐标 → 格子中心的世界坐标。</summary>
        public static Vector2 CellCenter(Vector2Int cell)
        {
            float s = AlgorithmConfig.CellSize;
            return new Vector2((cell.x + 0.5f) * s, (cell.y + 0.5f) * s);
        }

        /// <summary>
        /// 这个格子能不能走：
        /// 被“高度 &gt; 跳跃高度”的障碍盖住 → 不能走（绕开）；
        /// 只被矮障碍盖住 → 能走（跳过去）。
        /// </summary>
        public static bool IsWalkable(Vector2Int cell)
        {
            EnsureObstacles();
            if (walkCache.TryGetValue(cell, out bool w)) return w;

            w = true;
            float s = AlgorithmConfig.CellSize;
            var cellRect = new Rect(cell.x * s, cell.y * s, s, s);
            for (int i = 0; i < obstacles.Count; i++)
            {
                if (obstacles[i].height <= AlgorithmConfig.JumpHeight) continue; // 跳得过去
                if (cellRect.Overlaps(obstacles[i].rect))
                {
                    w = false;
                    break;
                }
            }
            walkCache[cell] = w;
            return w;
        }

        /// <summary>目标格本身被墙挡住时，往外螺旋找一个最近的可走格。</summary>
        public static Vector2Int NearestWalkable(Vector2Int cell)
        {
            if (IsWalkable(cell)) return cell;
            for (int r = 1; r <= 8; r++)
            {
                for (int dx = -r; dx <= r; dx++)
                for (int dy = -r; dy <= r; dy++)
                {
                    if (Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dy)) != r) continue;
                    var c = new Vector2Int(cell.x + dx, cell.y + dy);
                    if (IsWalkable(c)) return c;
                }
            }
            return cell;
        }

        /// <summary>强制重新收集障碍物（清缓存）。</summary>
        public static void Refresh()
        {
            obstacles.Clear();
            walkCache.Clear();
            Collect();
        }

        static void EnsureObstacles()
        {
            if (Time.time < nextRefreshTime) return;
            nextRefreshTime = Time.time + AlgorithmConfig.ObstacleRefreshInterval;
            Refresh();
        }

        static void Collect()
        {
            // 1. 挂了 PathObstacle 标记的物体（推荐方式）
            PathObstacle[] markers = Object.FindObjectsByType<PathObstacle>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            for (int i = 0; i < markers.Length; i++)
            {
                PathObstacle m = markers[i];
                if (m == null) continue;
                obstacles.Add(new Obstacle { rect = m.GetWorldRect(), height = m.GetWorldHeight() });
            }

            // 2. 可选：把整个 Layer 上的碰撞体都当障碍（默认关，避免误伤玩家/敌人碰撞体）
            int mask = AlgorithmConfig.ObstacleLayerMask;
            if (mask == 0) return;

            Collider[] cols = Object.FindObjectsByType<Collider>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            for (int i = 0; i < cols.Length; i++)
            {
                Collider col = cols[i];
                if (col == null || col.isTrigger) continue;
                if ((mask & (1 << col.gameObject.layer)) == 0) continue;
                Bounds b = col.bounds;
                obstacles.Add(new Obstacle
                {
                    rect = new Rect(b.min.x, b.min.y, b.size.x, b.size.y),
                    height = b.size.y
                });
            }
        }
    }
}
