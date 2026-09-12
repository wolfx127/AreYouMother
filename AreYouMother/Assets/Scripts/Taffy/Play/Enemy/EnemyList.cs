using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Taffy.Play.Enemy
{
    public static class EnemyList
    {
        private static readonly Dictionary<string, GameObject> enemyTable = new Dictionary<string, GameObject>();


        static EnemyList()
        {
            Build();
        }

        private static void Build()
        {
            var handle = Addressables.LoadAssetsAsync<GameObject>("EnemyPrefab");
            handle.WaitForCompletion();
            foreach (var go in handle.Result)
            {
                if (go.GetComponent<EnemyData>() != null)
                {
                    enemyTable.TryAdd(go.name, go);
                }
            }
        }

        public static GameObject GetEnemyByName(string name)
        {
            GameObject target = null;
            if (enemyTable.TryGetValue(name, out GameObject go))
            {
                target = GameObject.Instantiate(go);
            }

            return target;
        }

        public static void CopyInfosTo(string name, EnemyData target)
        {
            if (enemyTable.TryGetValue(name, out GameObject go))
            {
                go.GetComponent<EnemyData>().CopyInfosTo(target);
                return;
            }

            Debug.Log(name + "字段复制失败");
        }
    }
}
