using System.Collections.Generic;
using UnityEngine;

namespace Taffy.Play.Algorithm
{
    /// <summary>
    /// 手搓 A*：8 方向网格寻路。
    /// 开放列表用手搓的 MinHeap（小顶堆，见 MinHeap.cs）。
    /// 只绕开“跳不过去”的障碍（高度 &gt; JumpHeight），跳得过去的直接穿。
    /// </summary>
    public static class AStarPathfinder
    {
        // 8 个邻居：前 4 个直线，后 4 个斜线
        static readonly Vector2Int[] Neighbors8 =
        {
            new Vector2Int(1, 0), new Vector2Int(-1, 0), new Vector2Int(0, 1), new Vector2Int(0, -1),
            new Vector2Int(1, 1), new Vector2Int(1, -1), new Vector2Int(-1, 1), new Vector2Int(-1, -1),
        };

        const float DiagonalCost = 1.41421356f; // sqrt(2)
        const float StraightCost = 1f;

        /// <summary>
        /// 从 from 到 to 的世界坐标路径（含起终点）。没路返回 null。
        /// </summary>
        public static List<Vector2> FindPath(Vector2 from, Vector2 to)
        {
            Vector2Int start = PathGrid.WorldToCell(from);
            Vector2Int goal = PathGrid.WorldToCell(to);

            if (start == goal)
                return new List<Vector2> { from, to };

            // 目标格被墙盖住（比如玩家贴着墙根站）：挪到最近的可走格
            if (!PathGrid.IsWalkable(goal))
                goal = PathGrid.NearestWalkable(goal);

            List<Vector2Int> cells = Search(start, goal);
            if (cells == null) return null;

            var path = new List<Vector2>(cells.Count + 2) { from };
            for (int i = 0; i < cells.Count; i++)
                path.Add(PathGrid.CellCenter(cells[i]));
            path.Add(to);
            return Smooth(path);
        }

        /// <summary>只算“第一步该往哪走”的归一化方向。</summary>
        public static Vector2 GetDirection(Vector2 from, Vector2 to)
        {
            Vector2 delta = to - from;
            if (delta.sqrMagnitude < 1e-8f) return Vector2.zero;

            List<Vector2> path = FindPath(from, to);
            if (path == null || path.Count < 2)
                return delta.normalized; // 没路：直走兜底（可能被物理挡住，但不会呆住）

            Vector2 dir = path[1] - from;
            return dir.sqrMagnitude < 1e-8f ? Vector2.zero : dir.normalized;
        }

        /// <summary>核心搜索：返回格子路径（不含 start，含 goal）。</summary>
        static List<Vector2Int> Search(Vector2Int start, Vector2Int goal)
        {
            var open = new MinHeap<Vector2Int>();
            var gCost = new Dictionary<Vector2Int, float>();
            var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
            var closed = new HashSet<Vector2Int>();

            gCost[start] = 0f;
            open.Push(start, Heuristic(start, goal));
            int popped = 0;

            while (open.TryPop(out Vector2Int cur, out float _))
            {
                // 小顶堆不支持改优先级，同一个格子可能被压多次，老条目直接跳过
                if (closed.Contains(cur)) continue;

                if (++popped > AlgorithmConfig.MaxSearchNodes) return null;
                if (cur == goal)
                {
                    var cells = new List<Vector2Int>();
                    for (Vector2Int c = cur; c != start; c = cameFrom[c])
                        cells.Add(c);
                    cells.Reverse();
                    return cells;
                }
                closed.Add(cur);

                for (int i = 0; i < Neighbors8.Length; i++)
                {
                    Vector2Int next = cur + Neighbors8[i];
                    if (closed.Contains(next)) continue;
                    if (!PathGrid.IsWalkable(next)) continue;

                    bool diagonal = Neighbors8[i].x != 0 && Neighbors8[i].y != 0;
                    if (diagonal)
                    {
                        // 防斜穿墙角：斜着走的两个贴边格子必须都能走
                        if (!PathGrid.IsWalkable(cur + new Vector2Int(Neighbors8[i].x, 0)) ||
                            !PathGrid.IsWalkable(cur + new Vector2Int(0, Neighbors8[i].y)))
                            continue;
                    }

                    float ng = gCost[cur] + (diagonal ? DiagonalCost : StraightCost);
                    if (gCost.TryGetValue(next, out float old) && old <= ng) continue;

                    gCost[next] = ng;
                    cameFrom[next] = cur;
                    open.Push(next, ng + Heuristic(next, goal));
                }
            }
            return null;
        }

        /// <summary>8 方向的 octile 启发式：max + (sqrt2-1)*min，保证不高估。</summary>
        static float Heuristic(Vector2Int a, Vector2Int b)
        {
            int dx = Mathf.Abs(a.x - b.x);
            int dy = Mathf.Abs(a.y - b.y);
            return Mathf.Max(dx, dy) + 0.41421356f * Mathf.Min(dx, dy);
        }

        /// <summary>拉直路径：能直线走通就删掉中间拐点。</summary>
        static List<Vector2> Smooth(List<Vector2> path)
        {
            if (path.Count <= 3) return path;

            var result = new List<Vector2> { path[0] };
            int i = 1;
            while (i < path.Count - 1)
            {
                if (!LineWalkable(result[result.Count - 1], path[i + 1]))
                    result.Add(path[i]);
                i++;
            }
            result.Add(path[path.Count - 1]);
            return result;
        }

        /// <summary>两点之间沿线段采样，看每个经过的格子是不是都能走。</summary>
        static bool LineWalkable(Vector2 a, Vector2 b)
        {
            float dist = Vector2.Distance(a, b);
            if (dist <= 1e-4f) return true;

            int steps = Mathf.CeilToInt(dist / (AlgorithmConfig.CellSize * 0.5f));
            for (int s = 1; s <= steps; s++)
            {
                Vector2 p = Vector2.Lerp(a, b, (float)s / steps);
                if (!PathGrid.IsWalkable(PathGrid.WorldToCell(p))) return false;
            }
            return true;
        }
    }
}
