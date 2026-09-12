using System.Collections.Generic;
using UnityEngine;

namespace Taffy.Play.Algorithm
{
    /// <summary>
    /// 手搓四叉树：把点按 xy 平面分区存起来，用圆形/矩形范围快速查询。
    /// 用法：Rebuild 时把所有敌人 Insert 进来，查询时 QueryCircle。
    /// </summary>
    public class Quadtree<T>
    {
        readonly Rect bounds;
        readonly int capacity;
        readonly int maxDepth;
        Node root;

        public Quadtree(Rect bounds, int capacity = 8, int maxDepth = 6)
        {
            this.bounds = bounds;
            this.capacity = capacity;
            this.maxDepth = maxDepth;
            root = new Node(bounds, 0, capacity, maxDepth);
        }

        public Rect Bounds => bounds;

        /// <summary>清空整棵树（每帧重建时用）。</summary>
        public void Clear()
        {
            root = new Node(bounds, 0, capacity, maxDepth);
        }

        /// <summary>插入一个点。越界的点直接丢弃（外部保证范围够大）。</summary>
        public void Insert(Vector2 pos, T item)
        {
            if (!bounds.Contains(pos)) return;
            root.Insert(pos, item);
        }

        /// <summary>查一个圆内的所有点，结果追加进 results（不自动清空）。</summary>
        public void QueryCircle(Vector2 center, float radius, List<T> results)
        {
            root.QueryCircle(center, radius, results);
        }

        /// <summary>查一个矩形内的所有点，结果追加进 results（不自动清空）。</summary>
        public void QueryRect(Rect rect, List<T> results)
        {
            root.QueryRect(rect, results);
        }

        class Node
        {
            readonly Rect bounds;
            readonly int depth;
            readonly int capacity;
            readonly int maxDepth;

            Node[] children;            // 4 个子节点：SW/SE/NW/NE
            readonly List<Vector2> positions = new List<Vector2>();
            readonly List<T> items = new List<T>();
            bool subdivided;

            public Node(Rect bounds, int depth, int capacity, int maxDepth)
            {
                this.bounds = bounds;
                this.depth = depth;
                this.capacity = capacity;
                this.maxDepth = maxDepth;
            }

            public void Insert(Vector2 pos, T item)
            {
                // 还没满：直接塞进自己的桶
                if (!subdivided && positions.Count < capacity)
                {
                    positions.Add(pos);
                    items.Add(item);
                    return;
                }

                if (!subdivided)
                {
                    // 深度到底了就不再细分，桶允许溢出
                    if (depth >= maxDepth)
                    {
                        positions.Add(pos);
                        items.Add(item);
                        return;
                    }
                    Subdivide();
                }

                int idx = ChildIndex(pos);
                if (idx >= 0) children[idx].Insert(pos, item);
                else { positions.Add(pos); items.Add(item); } // 压在分割线上的点留在本节点
            }

            void Subdivide()
            {
                float halfW = bounds.width * 0.5f;
                float halfH = bounds.height * 0.5f;
                Vector2 c = bounds.center;

                children = new Node[4];
                children[0] = new Node(new Rect(bounds.xMin, bounds.yMin, halfW, halfH), depth + 1, capacity, maxDepth); // 左下
                children[1] = new Node(new Rect(c.x,        bounds.yMin, halfW, halfH), depth + 1, capacity, maxDepth); // 右下
                children[2] = new Node(new Rect(bounds.xMin, c.y,         halfW, halfH), depth + 1, capacity, maxDepth); // 左上
                children[3] = new Node(new Rect(c.x,        c.y,         halfW, halfH), depth + 1, capacity, maxDepth); // 右上

                // 把本节点已有的点下沉到子节点（压线的留下）
                for (int i = positions.Count - 1; i >= 0; i--)
                {
                    int idx = ChildIndex(positions[i]);
                    if (idx >= 0)
                    {
                        children[idx].Insert(positions[i], items[i]);
                        positions.RemoveAt(i);
                        items.RemoveAt(i);
                    }
                }
                subdivided = true;
            }

            int ChildIndex(Vector2 p)
            {
                Vector2 c = bounds.center;
                if (p.x == c.x || p.y == c.y) return -1; // 压分割线：留给父节点
                int idx = 0;
                if (p.x > c.x) idx += 1;
                if (p.y > c.y) idx += 2;
                return idx;
            }

            public void QueryCircle(Vector2 center, float radius, List<T> results)
            {
                if (!OverlapsCircle(bounds, center, radius)) return;

                float r2 = radius * radius;
                for (int i = 0; i < items.Count; i++)
                {
                    if ((positions[i] - center).sqrMagnitude <= r2)
                        results.Add(items[i]);
                }

                if (children == null) return;
                for (int i = 0; i < 4; i++)
                    children[i].QueryCircle(center, radius, results);
            }

            public void QueryRect(Rect rect, List<T> results)
            {
                if (!bounds.Overlaps(rect)) return;

                for (int i = 0; i < items.Count; i++)
                {
                    if (rect.Contains(positions[i]))
                        results.Add(items[i]);
                }

                if (children == null) return;
                for (int i = 0; i < 4; i++)
                    children[i].QueryRect(rect, results);
            }

            /// <summary>圆和矩形是否相交：矩形上离圆心最近的点到圆心的距离 &lt;= r。</summary>
            static bool OverlapsCircle(Rect r, Vector2 c, float radius)
            {
                float cx = Mathf.Clamp(c.x, r.xMin, r.xMax);
                float cy = Mathf.Clamp(c.y, r.yMin, r.yMax);
                float dx = c.x - cx;
                float dy = c.y - cy;
                return dx * dx + dy * dy <= radius * radius;
            }
        }
    }
}
