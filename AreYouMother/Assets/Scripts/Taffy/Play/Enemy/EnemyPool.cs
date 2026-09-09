using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Taffy.Play.Enemy
{
    public class EnemyPool : MonoBehaviour
    {
        private Queue<GameObject> pool = new Queue<GameObject>();

        private void Awake()
        {
            AddCapacity(20);
        }

        private void AddCapacity(int count)
        {
            if (pool.Count != 0) return;
            for (int i = 0; i < count; i++)
            {
                pool.Enqueue(EnemyList.GetEnemyByName("Default"));
            }
        }

        public GameObject GetEnemy(string name)
        {
            if(pool.Count == 0) AddCapacity(20);
            GameObject go = pool.Dequeue();
            if (go != null)
            {
                EnemyList.CopyInfosTo(name,go.GetComponent<Enemy>());
                go.SetActive(true);
            }
            return go;
        }

        public void RecycleEnemy(GameObject go)
        {
            if (go != null)
            {
                go.SetActive(false);
                EnemyList.CopyInfosTo("Default",go.GetComponent<Enemy>());
            }
        }
    }
}
