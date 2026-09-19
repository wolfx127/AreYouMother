using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Taffy.Play.Enemy
{
    public class EnemyPool : MonoBehaviour
    {
        private Queue<GameObject> pool = new Queue<GameObject>();
        
        public static EnemyPool Instance = null;

        private void Awake()
        {
            AddCapacity(50);
            if(Instance == null) Instance = this;
            if (Instance != this)
            {
                Destroy(Instance);
                Instance = this;
            }
        }

        private void AddCapacity(int count)
        {
            if (pool.Count != 0) return;
            for (int i = 0; i < count; i++)
            {
                GameObject temp = EnemyList.GetEnemyByName("Enemy_Default");
                temp.SetActive(false);
                temp.name = "Enemy_Default";
                temp.transform.SetParent(transform, false);
                pool.Enqueue(temp);
            }
        }

        public GameObject GetEnemy(string name, Vector3 position)
        {
            if(pool.Count == 0) AddCapacity(20);
            GameObject go = pool.Dequeue();
            if (go is not null)
            {
                Debug.Log("敌人被从池中取出");
                go.name = name;
                EnemyData enemy = go.GetComponent<EnemyData>();
                EnemyList.CopyInfosTo(name,go);
                enemy.isAlive = true;
                enemy.dead = false;
                
                enemy.LinkEntity(position);
                
                go.transform.position = position;
                go.SetActive(true);
                Debug.Log("敌人生成完毕");
            }
            return go;
        }

        public void RecycleEnemy(GameObject go)
        {
            if (go is not null)
            {
                go.SetActive(false);
                EnemyData enemy = go.GetComponent<EnemyData>();
                EnemyList.CopyInfosTo("Enemy_Default",go);
                go.name = "Enemy_Default";
                enemy.UnlinkEntity();
                pool.Enqueue(go);
            }
        }
    }
}
