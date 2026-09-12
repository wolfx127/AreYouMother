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
                GameObject temp = EnemyList.GetEnemyByName("Default");
                temp.name = "Default";
                temp.transform.SetParent(transform, false);
                pool.Enqueue(temp);
            }
        }

        public GameObject GetEnemy(string name)
        {
            if(pool.Count == 0) AddCapacity(20);
            GameObject go = pool.Dequeue();
            if (go != null)
            {
                EnemyList.CopyInfosTo(name,go.GetComponent<EnemyData>());
                go.name = go.GetComponent<EnemyData>().name;
                EnemyTool.LinkEntity(go.GetComponent<EnemyData>());
                go.SetActive(true);
            }
            return go;
        }

        public void RecycleEnemy(GameObject go)
        {
            if (go != null)
            {
                go.SetActive(false);
                EnemyList.CopyInfosTo("Default",go.GetComponent<EnemyData>());
                go.name = "Default";
                EnemyTool.UnlinkEntity(go.GetComponent<EnemyData>());
            }
        }
    }
}
