using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Taffy.Play.Enemy
{
    public class EnemyPool : MonoBehaviour
    {
        private Queue<GameObject> pool = new Queue<GameObject>();

        /// <summary>已在池里的对象，用来挡住重复回收（同一个对象进队两次会被两个生成点同时拿到）</summary>
        private readonly HashSet<int> inPool = new HashSet<int>();
        
        public static EnemyPool Instance = null;

        public GameObject playerA;
        public GameObject playerB;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            if (Instance != this)
            {
                Destroy(Instance);
                Instance = this;
            }
            AddCapacity(50);
        }

        private void AddCapacity(int count)
        {
            if (pool.Count != 0) return;
            for (int i = 0; i < count; i++)
            {
                GameObject temp = EnemyList.GetEnemyByName("Enemy_Default");
                if (temp is null)
                {
                    Debug.LogError("[EnemyPool] 取不到 Enemy_Default 预制体，池扩容失败（检查 Addressables 的 EnemyPrefab 标签）");
                    return;
                }
                temp.SetActive(false);
                temp.name = "Enemy_Default";
                temp.transform.SetParent(transform, false);
                pool.Enqueue(temp);
                inPool.Add(temp.GetInstanceID());
            }
        }

        public GameObject GetEnemy(string name, Vector3 position)
        {
            if(pool.Count == 0) AddCapacity(20);
            if (pool.Count == 0) return null;

            GameObject go = pool.Dequeue();
            inPool.Remove(go.GetInstanceID());
            if (go is not null)
            {
                Debug.Log("敌人被从池中取出");
                go.name = name;
                EnemyData enemy = go.GetComponent<EnemyData>();
                EnemyList.CopyInfosTo(name,go);
                enemy.isAlive = true;
                enemy.dead = false;
                enemy.goA = playerA;
                enemy.goB = playerB;
                // 上一次死亡时 collider 被关掉了，而 Awake 只跑一次，必须在这里恢复
                if (enemy.collider != null) enemy.collider.enabled = true;
                // 清掉上一轮的仇恨，否则复用出来的敌人一出生就带着旧目标
                enemy.isHateA = false;
                enemy.isHateB = false;
                
                enemy.LinkEntity(position);
                
                go.transform.position = position;
                go.SetActive(true);
                Debug.Log("敌人生成完毕");
            }
            return go;
        }

        public void RecycleEnemy(GameObject go)
        {
            if (go is null) return;
            // 已在池里说明是重复回收（死亡回收 + 生成点清理），直接忽略，避免同一条记录排队两次
            if (inPool.Contains(go.GetInstanceID())) return;

            go.SetActive(false);
            EnemyData enemy = go.GetComponent<EnemyData>();
            EnemyList.CopyInfosTo("Enemy_Default",go);
            go.name = "Enemy_Default";
            enemy.UnlinkEntity();
            pool.Enqueue(go);
            inPool.Add(go.GetInstanceID());
        }
    }
}
