using System;

namespace Taffy.Play.Algorithm
{
    /// <summary>
    /// 手搓小顶堆（二叉堆），当 A* 的开放列表。
    /// 允许重复元素：A* 找到更短的 g 时会重复压同一个格子，Pop 出来的老条目由 closed 集跳过。
    /// </summary>
    public class MinHeap<T>
    {
        T[] items;
        float[] priorities;
        int count;

        public MinHeap(int capacity = 256)
        {
            items = new T[capacity];
            priorities = new float[capacity];
        }

        /// <summary>当前元素个数。</summary>
        public int Count => count;

        /// <summary>只清计数不缩容量，避免反复分配。</summary>
        public void Clear()
        {
            count = 0;
        }

        /// <summary>压入（元素, 优先级），优先级越小越先出。</summary>
        public void Push(T item, float priority)
        {
            if (count == items.Length) Grow();

            items[count] = item;
            priorities[count] = priority;
            SiftUp(count);
            count++;
        }

        /// <summary>弹出优先级最小的元素；堆空返回 false。</summary>
        public bool TryPop(out T item, out float priority)
        {
            if (count == 0)
            {
                item = default;
                priority = default;
                return false;
            }

            item = items[0];
            priority = priorities[0];
            count--;
            if (count > 0)
            {
                items[0] = items[count];
                priorities[0] = priorities[count];
                SiftDown(0);
            }
            items[count] = default;
            priorities[count] = default;
            return true;
        }

        /// <summary>自下而上：新元素上浮。</summary>
        void SiftUp(int i)
        {
            T item = items[i];
            float p = priorities[i];

            while (i > 0)
            {
                int parent = (i - 1) >> 1;
                if (priorities[parent] <= p) break;
                items[i] = items[parent];
                priorities[i] = priorities[parent];
                i = parent;
            }
            items[i] = item;
            priorities[i] = p;
        }

        /// <summary>自上而下：堆顶元素下沉。</summary>
        void SiftDown(int i)
        {
            T item = items[i];
            float p = priorities[i];
            int half = count >> 1;

            while (i < half)
            {
                int child = (i << 1) + 1;
                if (child + 1 < count && priorities[child + 1] < priorities[child]) child++;
                if (priorities[child] >= p) break;
                items[i] = items[child];
                priorities[i] = priorities[child];
                i = child;
            }
            items[i] = item;
            priorities[i] = p;
        }

        void Grow()
        {
            int newCapacity = items.Length * 2;
            Array.Resize(ref items, newCapacity);
            Array.Resize(ref priorities, newCapacity);
        }
    }
}
