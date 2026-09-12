using System.Collections.Generic;
using Taffy.Play.Enemy;
using UnityEngine;

namespace Taffy.Play.Algorithm
{
    /// <summary>
    /// 敌人四叉树管理器。
    /// 每隔 QuadtreeRebuildInterval 秒把场景里所有活的敌人重新插进四叉树，
    /// 别处用 QuadtreeManager.Instance.QueryEnemiesInRadius(...) 查仇恨范围。
    ///
    /// 不用挂场景：进 PlayMode 后会自动创建一个 [Algorithm] QuadtreeManager 常驻对象。
    /// </summary>
    public class QuadtreeManager : MonoBehaviour
    {
        public static QuadtreeManager Instance { get; private set; }

        Quadtree<EnemyData> tree;
        float nextRebuildTime = float.NegativeInfinity;

        static readonly List<EnemyData> scanBuffer = new List<EnemyData>(128);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void AutoBoot()
        {
            if (Instance != null) return;
            var go = new GameObject("[Algorithm] QuadtreeManager");
            DontDestroyOnLoad(go);
            go.AddComponent<QuadtreeManager>();
        }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            Rebuild();
        }

        void Update()
        {
            if (Time.time < nextRebuildTime) return;
            Rebuild();
            nextRebuildTime = Time.time + AlgorithmConfig.QuadtreeRebuildInterval;
        }

        /// <summary>立刻重建四叉树（扫描全场敌人，重算范围）。</summary>
        public void Rebuild()
        {
            scanBuffer.Clear();
            EnemyData[] found = Object.FindObjectsByType<EnemyData>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            scanBuffer.AddRange(found);

            tree = new Quadtree<EnemyData>(ComputeBounds());
            for (int i = 0; i < scanBuffer.Count; i++)
            {
                EnemyData e = scanBuffer[i];
                // 对象池回收的敌人是 SetActive(false) 的，不参与仇恨查询
                if (e == null || !e.isActiveAndEnabled || !e.gameObject.activeInHierarchy) continue;
                tree.Insert(e.transform.position, e);
            }
        }

        Rect ComputeBounds()
        {
            Vector2 min = Vector2.one * float.MaxValue;
            Vector2 max = Vector2.one * float.MinValue;
            bool any = false;

            for (int i = 0; i < scanBuffer.Count; i++)
            {
                EnemyData e = scanBuffer[i];
                if (e == null || !e.gameObject.activeInHierarchy) continue;
                Vector2 p = e.transform.position;
                min = Vector2.Min(min, p);
                max = Vector2.Max(max, p);
                any = true;
            }

            if (!any)
                return new Rect(-512f, -512f, 1024f, 1024f); // 场景里暂时没有敌人时的兜底范围

            float margin = AlgorithmConfig.QuadtreeBoundsMargin;
            min -= Vector2.one * margin;
            max += Vector2.one * margin;
            return new Rect(min.x, min.y, max.x - min.x, max.y - min.y);
        }

        /// <summary>
        /// 查一个圆形范围内的所有敌人。会先清空 results 再填。
        /// </summary>
        public void QueryEnemiesInRadius(Vector2 center, float radius, List<EnemyData> results)
        {
            results.Clear();
            if (tree == null) return;
            tree.QueryCircle(center, radius, results);
        }

        void OnDrawGizmosSelected()
        {
            if (!AlgorithmConfig.DebugDraw || tree == null) return;
            Gizmos.color = new Color(0.2f, 0.9f, 0.4f, 0.4f);
            Gizmos.DrawWireCube(tree.Bounds.center, new Vector3(tree.Bounds.width, tree.Bounds.height, 0f));
        }
    }
}
