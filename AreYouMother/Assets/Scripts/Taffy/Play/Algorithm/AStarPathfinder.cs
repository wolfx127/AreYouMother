using System;
using System.Collections.Generic;
using Taffy.Play.Trigger;
using UnityEngine;

namespace Taffy.Play.Algorithm
{
    /// <summary>
    /// 基于 3D Collider 的地表 A*。只查询路线，不负责移动、施加重力或执行跳跃。
    /// 目标是角色 pivot 的世界坐标；起点始终取本物体的 transform.position。
    /// 地面必须有非 Trigger Collider；悬空区域没有落脚面时不可通行。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class AStarPathfinder : MonoBehaviour
    {
        // 单次允许的下落深度（世界单位，严格小于此值）；按游戏需要修改。
        private const float MaxDeep = 2f;
        private const float Skin = 0.01f;
        private const float HeightEpsilon = 0.0001f;
        //跳跃高度
        private const float maxHeight = 0.5f;

        public enum HeightAxis
        {
            NegativeZ,
            PositiveY,
            PositiveZ
        }

        [Header("坐标与碰撞")]
        [Tooltip("默认 XZ 寻路、Y 为上，与项目重力一致。若使用 XY 移动，可选择 PositiveZ 或 NegativeZ。")]
        [SerializeField] private HeightAxis heightAxis = HeightAxis.PositiveY;
        [Tooltip("可站立地面的层。barrier 会自动加入；请排除其他角色所在的层。")]
        [SerializeField] private LayerMask groundLayers = 1;
        [Tooltip("角色的实体碰撞体；留空时自动查找非 Trigger Collider。使用世界包围盒保守避障。")]
        [SerializeField] private Collider bodyCollider;
        [Tooltip("没有实体 Collider 时，使用此尺寸（寻路横轴、竖直高度、寻路纵轴），pivot 视为中心。")]
        [SerializeField] private Vector3 fallbackBodySize = new Vector3(0.6f, 1.8f, 0.6f);

        [Header("搜索精度与范围")]
        [Min(0.05f)] [SerializeField] private float cellSize = 0.5f;
        [Tooltip("沿边地面采样的最大间距。窄沟需要减小此值；同时会按角色宽度限制采样间距。")]
        [Min(0.01f)] [SerializeField] private float surfaceSampleSpacing = 0.1f;
        [Tooltip("起点、终点包围矩形向四周扩展的搜索距离。")]
        [Min(0f)] [SerializeField] private float searchMargin = 10f;
        [Min(1f)] [SerializeField] private float maxTargetDistance = 80f;
        [Min(16)] [SerializeField] private int maxExpandedNodes = 4096;
        [Range(0f, 80f)] [SerializeField] private float maxSlopeAngle = 50f;
        [SerializeField] private bool drawPath = true;

        private readonly Dictionary<Vector3Int, Node> nodes = new Dictionary<Vector3Int, Node>();
        private readonly List<OpenEntry> open = new List<OpenEntry>();
        private readonly List<Vector3> path = new List<Vector3>();
        private readonly List<Vector3> edge = new List<Vector3>();
        private readonly List<Node> reverseNodes = new List<Node>();
        private RaycastHit[] castHits = new RaycastHit[32];
        private Collider[] overlaps = new Collider[32];

        private Vector3 up, axisX, axisZ, extents, probeExtents, feetOffset;
        private float halfHeight, sampleSpacing, slopeCos;
        private int environmentMask;
        private int cachedFrame = -1;
        private Vector3 cachedStart, cachedTarget;
        private float cachedLength = float.PositiveInfinity;
        private Vector3 cachedDirection;
        private bool warnedMissingLayer;

        /// <summary>
        /// 返回地面/台阶折线路径的长度（世界单位，包含升降，但不模拟跳跃抛物线）。
        /// 无路、未配置 barrier、目标超出搜索范围或预算耗尽且未找到路时返回 PositiveInfinity。
        /// 已到达返回 0。同帧、相同起终点的两个接口共用一次搜索。
        /// </summary>
        public float GetPathLength(Vector3 targetPosition)
        {
            EnsurePath(targetPosition);
            return cachedLength;
        }

        /// <summary>
        /// 返回当前沿路径行走的世界空间单位方向（投影到寻路平面）。
        /// 已到达或无路返回 Vector3.zero；跳跃和下落由外部移动逻辑执行。
        /// </summary>
        public Vector3 GetPathDirection(Vector3 targetPosition)
        {
            EnsurePath(targetPosition);
            return cachedDirection;
        }

        private void OnEnable() { cachedFrame = -1; }

        private void OnValidate()
        {
            cellSize = Mathf.Max(0.05f, cellSize);
            surfaceSampleSpacing = Mathf.Max(0.01f, surfaceSampleSpacing);
            searchMargin = Mathf.Max(0f, searchMargin);
            maxTargetDistance = Mathf.Max(1f, maxTargetDistance);
            maxExpandedNodes = Mathf.Max(16, maxExpandedNodes);
            fallbackBodySize = Vector3.Max(fallbackBodySize, Vector3.one * 0.05f);
            cachedFrame = -1;
        }

        private void EnsurePath(Vector3 target)
        {
            Vector3 start = transform.position;
            if (cachedFrame == Time.frameCount && cachedStart.Equals(start) && cachedTarget.Equals(target))
                return;

            cachedFrame = Time.frameCount;
            cachedStart = start;
            cachedTarget = target;
            cachedLength = float.PositiveInfinity;
            cachedDirection = Vector3.zero;
            path.Clear();
            if (!IsFinite(start) || !IsFinite(target)) return;

            int barrierLayer = LayerMask.NameToLayer("barrier");
            if (barrierLayer < 0)
            {
                if (!warnedMissingLayer)
                {
                    Debug.LogWarning("AStarPathfinder：请创建名为 barrier 的 Layer，并设置地面层 Ground Layers。", this);
                    warnedMissingLayer = true;
                }
                return;
            }
            warnedMissingLayer = false;
            environmentMask = groundLayers.value | (1 << barrierLayer);

            // 项目直接修改 Transform 且关闭了 AutoSyncTransforms，查询前同步物理世界。
            Physics.SyncTransforms();
            PrepareBody();
            if (Planar(target - start).magnitude > maxTargetDistance) return;

            Vector3 actualFeet = start + feetOffset;
            Vector3 requestedFeet = target + feetOffset;
            // 起终点只能向下贴地，不能把桥下的目标吸附到桥顶。
            if (!TryFindSurface(actualFeet, Skin * 2f, MaxDeep, out Vector3 startFeet) ||
                !SweepClear(actualFeet, startFeet) ||
                !TryFindSurface(requestedFeet, Skin * 2f, MaxDeep, out Vector3 goalFeet)) return;

            if (!Search(startFeet, goalFeet)) return;
            if ((actualFeet - path[0]).sqrMagnitude > HeightEpsilon * HeightEpsilon)
                path.Insert(0, actualFeet);

            cachedLength = 0f;
            for (int i = 1; i < path.Count; i++)
                cachedLength += Vector3.Distance(path[i - 1], path[i]);
            for (int i = 1; i < path.Count; i++)
            {
                Vector3 direction = Planar(path[i] - actualFeet);
                if (direction.sqrMagnitude <= HeightEpsilon * HeightEpsilon) continue;
                cachedDirection = direction.normalized;
                break;
            }
            if (cachedLength <= HeightEpsilon) cachedLength = 0f;
        }

        private void PrepareBody()
        {
            up = heightAxis == HeightAxis.PositiveY ? Vector3.up :
                heightAxis == HeightAxis.PositiveZ ? Vector3.forward : Vector3.back;
            axisX = Vector3.right;
            axisZ = heightAxis == HeightAxis.PositiveY ? Vector3.forward : Vector3.up;
            if (bodyCollider == null || !bodyCollider.enabled || bodyCollider.isTrigger)
            {
                bodyCollider = null;
                foreach (Collider candidate in GetComponentsInChildren<Collider>())
                {
                    if (!candidate.enabled || candidate.isTrigger) continue;
                    bodyCollider = candidate;
                    break;
                }
            }

            Vector3 center;
            Vector3 fullExtents;
            if (bodyCollider != null)
            {
                Bounds bounds = bodyCollider.bounds;
                center = bounds.center;
                fullExtents = Vector3.Max(bounds.extents, Vector3.one * 0.025f);
            }
            else
            {
                center = transform.position;
                Vector3 size = Vector3.Max(fallbackBodySize, Vector3.one * 0.05f);
                fullExtents = (Abs(axisX) * size.x + Abs(up) * size.y + Abs(axisZ) * size.z) * 0.5f;
            }
            halfHeight = Vector3.Dot(fullExtents, Abs(up));
            feetOffset = center - up * halfHeight - transform.position;
            extents = Vector3.Max(fullExtents - Vector3.one * Skin, Vector3.one * Skin);
            probeExtents = Planar(extents) + Abs(up) * Skin;
            // Planar 对 -Z 仍会去掉 z 分量，probeExtents 始终是正半尺寸。
            float narrowHalfWidth = Mathf.Min(Vector3.Dot(extents, axisX), Vector3.Dot(extents, axisZ));
            sampleSpacing = Mathf.Max(0.005f, Mathf.Min(surfaceSampleSpacing, cellSize * 0.25f, narrowHalfWidth * 0.5f));
            slopeCos = Mathf.Cos(maxSlopeAngle * Mathf.Deg2Rad);
        }

        private bool Search(Vector3 start, Vector3 goal)
        {
            nodes.Clear();
            open.Clear();
            reverseNodes.Clear();
            Node first = new Node(Vector2Int.zero, start) { G = 0f };
            nodes.Add(Key(first.Cell, start), first);
            Push(first, Vector3.Distance(start, goal));

            Node bestGoal = null;
            float bestCost = float.PositiveInfinity;
            // 平地直达时无需建立整张网格；有升降时保留候选，再用 A* 尝试更短路线。
            if (TryConnect(start, goal, true, out Vector3 end, out float directCost))
            {
                bestGoal = MakeGoal(first, end, directCost);
                bestCost = directCost;
                if (directCost <= Vector3.Distance(start, goal) + HeightEpsilon)
                {
                    BuildPath(bestGoal);
                    return true;
                }
            }

            Vector3 goalDelta = goal - start;
            float gx = Vector3.Dot(goalDelta, axisX);
            float gz = Vector3.Dot(goalDelta, axisZ);
            float minX = Mathf.Min(0f, gx) - searchMargin, maxX = Mathf.Max(0f, gx) + searchMargin;
            float minZ = Mathf.Min(0f, gz) - searchMargin, maxZ = Mathf.Max(0f, gz) + searchMargin;
            int expanded = 0;
            while (open.Count > 0 && expanded < maxExpandedNodes)
            {
                OpenEntry entry = Pop();
                Node current = entry.Node;
                if (current.Closed || entry.G != current.G) continue;
                if (entry.F >= bestCost) break;
                current.Closed = true;
                expanded++;

                if (Planar(goal - current.Feet).magnitude <= cellSize * 1.5f &&
                    TryConnect(current.Feet, goal, true, out end, out float goalCost) &&
                    current.G + goalCost < bestCost)
                {
                    bestCost = current.G + goalCost;
                    bestGoal = MakeGoal(current, end, bestCost);
                }

                for (int x = -1; x <= 1; x++)
                for (int z = -1; z <= 1; z++)
                {
                    if (x == 0 && z == 0) continue;
                    Vector2Int cell = current.Cell + new Vector2Int(x, z);
                    float px = cell.x * cellSize, pz = cell.y * cellSize;
                    if (px < minX || px > maxX || pz < minZ || pz > maxZ) continue;
                    Vector3 desired = start + axisX * px + axisZ * pz;
                    desired += up * Vector3.Dot(current.Feet - desired, up);
                    if (!TryConnect(current.Feet, desired, false, out end, out float cost)) continue;

                    float g = current.G + cost;
                    if (g + Vector3.Distance(end, goal) >= bestCost) continue;
                    Vector3Int key = Key(cell, end);
                    if (!nodes.TryGetValue(key, out Node next))
                    {
                        next = new Node(cell, end);
                        nodes.Add(key, next);
                    }
                    if (g >= next.G) continue;
                    next.Feet = end;
                    next.G = g;
                    next.Parent = current;
                    next.Edge = new List<Vector3>(edge);
                    next.Closed = false;
                    Push(next, g + Vector3.Distance(end, goal));
                }
            }
            // 达到预算时只返回已完整验证的路线，绝不返回未连到终点的部分路径。
            if (bestGoal == null) return false;
            BuildPath(bestGoal);
            return true;
        }

        private Node MakeGoal(Node parent, Vector3 feet, float cost)
        {
            return new Node(parent.Cell, feet) { G = cost, Parent = parent, Edge = new List<Vector3>(edge) };
        }

        private bool TryConnect(Vector3 from, Vector3 desired, bool exactHeight, out Vector3 end, out float cost)
        {
            edge.Clear();
            end = from;
            cost = 0f;
            Vector3 horizontal = Planar(desired - from);
            int samples = Mathf.Max(1, Mathf.CeilToInt(horizontal.magnitude / sampleSpacing));
            for (int i = 1; i <= samples; i++)
            {
                Vector3 probe = from + horizontal * ((float)i / samples);
                probe += up * Vector3.Dot(end - probe, up);
                if (!TryFindSurface(probe, maxHeight, MaxDeep, out Vector3 next))
                    return false;
                if (!TryStep(end, next, out Vector3 corner)) return false;
                AppendEdge(end, corner, ref cost);
                AppendEdge(corner, next, ref cost);
                end = next;
            }
            return !exactHeight || Mathf.Abs(Vector3.Dot(end - desired, up)) <= Skin;
        }

        private void AppendEdge(Vector3 from, Vector3 to, ref float cost)
        {
            float distance = Vector3.Distance(from, to);
            if (distance <= HeightEpsilon) return;
            edge.Add(to);
            cost += distance;
        }

        private bool TryFindSurface(Vector3 referenceFeet, float maxRise, float maxDrop, out Vector3 feet)
        {
            feet = default;
            Vector3 origin = referenceFeet + up * (maxRise + Skin * 2f);
            int count = Cast(origin, probeExtents, -up, maxRise + maxDrop + Skin * 4f);
            float bestDelta = float.PositiveInfinity;
            // 体积探测覆盖整个脚底，可以提早发现比网格更薄的台阶。
            // 优先选离当前脚底最近的可站立表面，避免桥下路线被吸到桥顶。
            for (int i = 0; i < count; i++)
            {
                RaycastHit hit = castHits[i];
                if (IsSelf(hit.collider) || hit.distance <= HeightEpsilon || Vector3.Dot(hit.normal, up) < slopeCos)
                    continue;
                float delta = maxRise + Skin - hit.distance;
                if (delta > maxRise + HeightEpsilon || -delta >= maxDrop - HeightEpsilon) continue;
                if (maxRise > Skin * 2f && delta >= maxRise - HeightEpsilon) continue;
                if (Mathf.Abs(delta) >= bestDelta) continue;
                Vector3 candidate = referenceFeet + up * delta;
                if (!BodyClear(candidate)) continue;
                feet = candidate;
                bestDelta = Mathf.Abs(delta);
            }
            return !float.IsPositiveInfinity(bestDelta);
        }

        private bool TryStep(Vector3 from, Vector3 to, out Vector3 corner)
        {
            float rise = Vector3.Dot(to - from, up);
            corner = rise > 0f ? from + up * rise : to - up * rise;
            if (rise >= maxHeight - HeightEpsilon || -rise >= MaxDeep - HeightEpsilon)
                return false;
            // 上台阶先抬升再前进；下台阶先走出边缘再下落。两段都检查头顶和侧面。
            return SweepClear(from, corner) && SweepClear(corner, to);
        }

        private bool BodyClear(Vector3 feet)
        {
            Vector3 center = feet + up * (halfHeight + Skin);
            int count;
            while (true)
            {
                count = Physics.OverlapBoxNonAlloc(center, extents, overlaps, Quaternion.identity,
                    environmentMask, QueryTriggerInteraction.Ignore);
                if (count < overlaps.Length) break;
                Array.Resize(ref overlaps, overlaps.Length * 2);
            }
            for (int i = 0; i < count; i++)
                if (!IsSelf(overlaps[i])) return false;
            return true;
        }

        private bool SweepClear(Vector3 from, Vector3 to)
        {
            if (!BodyClear(from) || !BodyClear(to)) return false;
            Vector3 delta = to - from;
            float distance = delta.magnitude;
            if (distance <= HeightEpsilon) return true;
            int count = Cast(from + up * (halfHeight + Skin), extents, delta / distance, distance);
            for (int i = 0; i < count; i++)
                if (!IsSelf(castHits[i].collider)) return false;
            return true;
        }

        private int Cast(Vector3 center, Vector3 halfExtents, Vector3 direction, float distance)
        {
            while (true)
            {
                int count = Physics.BoxCastNonAlloc(center, halfExtents, direction, castHits,
                    Quaternion.identity, distance, environmentMask, QueryTriggerInteraction.Ignore);
                if (count < castHits.Length) return count;
                // NonAlloc 缓冲区满时扩大后重查，不能把被截断的结果当成无障碍。
                Array.Resize(ref castHits, castHits.Length * 2);
            }
        }

        private bool IsSelf(Collider collider)
        {
            return collider == null || collider == bodyCollider || collider.transform.IsChildOf(transform);
        }

        private Vector3 Planar(Vector3 value) { return value - up * Vector3.Dot(value, up); }
        private static Vector3 Abs(Vector3 value) { return new Vector3(Mathf.Abs(value.x), Mathf.Abs(value.y), Mathf.Abs(value.z)); }
        private static bool IsFinite(Vector3 v)
        {
            return !float.IsNaN(v.x) && !float.IsInfinity(v.x) &&
                   !float.IsNaN(v.y) && !float.IsInfinity(v.y) &&
                   !float.IsNaN(v.z) && !float.IsInfinity(v.z);
        }

        private Vector3Int Key(Vector2Int cell, Vector3 feet)
        {
            // 高度属于节点状态，同一平面格子的桥上、桥下不能合并。
            return new Vector3Int(cell.x, cell.y, Mathf.RoundToInt(Vector3.Dot(feet, up) / Skin));
        }

        private void BuildPath(Node goal)
        {
            reverseNodes.Clear();
            for (Node node = goal; node != null; node = node.Parent) reverseNodes.Add(node);
            path.Add(reverseNodes[reverseNodes.Count - 1].Feet);
            for (int i = reverseNodes.Count - 2; i >= 0; i--)
                if (reverseNodes[i].Edge != null) path.AddRange(reverseNodes[i].Edge);
        }

        private sealed class Node
        {
            public readonly Vector2Int Cell;
            public Vector3 Feet;
            public float G = float.PositiveInfinity;
            public bool Closed;
            public Node Parent;
            public List<Vector3> Edge;
            public Node(Vector2Int cell, Vector3 feet) { Cell = cell; Feet = feet; }
        }

        private struct OpenEntry
        {
            public Node Node;
            public float G, F;
        }

        // 内置二叉小根堆，保持单文件，不依赖额外的网格、堆或配置脚本。
        private void Push(Node node, float f)
        {
            OpenEntry value = new OpenEntry { Node = node, G = node.G, F = f };
            int index = open.Count;
            open.Add(value);
            while (index > 0)
            {
                int parent = (index - 1) / 2;
                if (open[parent].F <= value.F) break;
                open[index] = open[parent];
                index = parent;
            }
            open[index] = value;
        }

        private OpenEntry Pop()
        {
            OpenEntry result = open[0];
            OpenEntry last = open[open.Count - 1];
            open.RemoveAt(open.Count - 1);
            if (open.Count == 0) return result;
            int index = 0;
            while (index * 2 + 1 < open.Count)
            {
                int child = index * 2 + 1;
                if (child + 1 < open.Count && open[child + 1].F < open[child].F) child++;
                if (open[child].F >= last.F) break;
                open[index] = open[child];
                index = child;
            }
            open[index] = last;
            return result;
        }

        private void OnDrawGizmosSelected()
        {
            if (!drawPath || path.Count < 2) return;
            Gizmos.color = Color.cyan;
            for (int i = 1; i < path.Count; i++)
                Gizmos.DrawLine(path[i - 1] - feetOffset, path[i] - feetOffset);
        }
    }
}
