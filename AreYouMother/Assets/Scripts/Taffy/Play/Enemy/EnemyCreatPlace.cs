using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Taffy.Play.Enemy
{
    [Serializable]
    public struct EnemyCreatProbability
    {
        [InspectorName("敌人预制体")]public GameObject enemy;
        [InspectorName("权重")]public float Weight;
    }

    public class EnemyCreatPlace : MonoBehaviour
    {
        [Header("生成敌人最少数量")] public int minEnemyCount = 1;
        [Header("生成敌人最多数量")] public int maxEnemyCount = 3;
        [Header("可生成的敌人列表")] public List<EnemyCreatProbability> list = new List<EnemyCreatProbability>();
        [Header("生成范围半径")][SerializeField]private float radius = 2;
        
        [Header("下面的不用看")]
        [SerializeField]private List<GameObject> enemyList = new List<GameObject>();
        
        [SerializeField]private int EnemyCount = 2;
        [SerializeField]private int aliveCount = 0;
        [SerializeField]private float sumWeight = 1;

        [SerializeField]private float createCD = 20f;
        [SerializeField]private float timeCounter = 20f;

        

        private void Awake()
        {
            sumWeight = list.Sum(w => w.Weight);
        }

        private void Update()
        {
            if(timeCounter <= createCD && aliveCount == 0) timeCounter += Time.deltaTime;
        }

        public void CreateEnemy()
        {
            if (timeCounter < createCD || aliveCount > 0) return;
            if (list.Count <= 0) return;
            
            timeCounter = 0f;
            Debug.Log($"开始生成敌人{transform.position}");

            for (int j = enemyList.Count - 1; j >= 0; j--)
            {
                if(enemyList[j] is not null) EnemyPool.Instance.RecycleEnemy(enemyList[j]);
                enemyList.RemoveAt(j);
            }

            EnemyCount = Random.Range(minEnemyCount,maxEnemyCount+1);
            
            int i = 0;
            int warningCount = 10;
            while (i < EnemyCount &&  warningCount > 0)
            {
                float Weight = Random.Range(0, sumWeight);
                float nowWeight = 0;
                int index = 0;
                for (; index < list.Count; index++)
                {
                    nowWeight += list[index].Weight;
                    if (nowWeight >= Weight)
                    {
                        break;
                    }
                }
                if (index >= list.Count) index = list.Count - 1;
                if (list[index].enemy is not null)
                {
                    float angle = i * (2f * Mathf.PI / EnemyCount);
                    float dist = Random.Range(0f, radius);
                    GameObject enemy = EnemyPool.Instance.GetEnemy(list[index].enemy.name, 
                        transform.position + new Vector3(Mathf.Cos(angle) * dist, 0f, Mathf.Sin(angle) * dist));
                    if(enemy.name != "Enemy_Default")
                    {
                        enemy.GetComponent<EnemyData>().DeadEvent += AliveLose;
                        enemyList.Add(enemy);
                        i++;
                        aliveCount++;
                        continue;
                    }
                    EnemyPool.Instance.RecycleEnemy(enemy);
                }
                warningCount--;
            }
        }

        private void AliveLose()
        {
            aliveCount--;
        }
        
        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(1f, 0.5f, 0.2f, 0.5f);
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}
