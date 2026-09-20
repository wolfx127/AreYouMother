using System;
using System.Collections.Generic;
using Taffy.Play.Enemy;
using TaffyFrame.Trigger;
using UnityEngine;

namespace Taffy.Play.Trigger
{
    public class PursueTrigger : MonoBehaviour
    {
        public char playerName = 'A';
        
        HashSet<Collider> table = new HashSet<Collider>();

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Enemy")) return;
            if(table.Add(other))
            {
                if(playerName == 'A')
                {
                    other.GetComponent<EnemyData>().isHateA = true;
                    Debug.Log($"[寻路] 玩家A的仇恨圈进入敌人 {other.name}");
                }
                else if(playerName == 'B')
                {
                    other.GetComponent<EnemyData>().isHateB = true;
                    Debug.Log($"[寻路] 玩家B的仇恨圈进入敌人 {other.name}");
                }
            }
        }

        public void OnTriggerStay(Collider other)
        {
            if (table.Contains(other))
            {
                other.gameObject.GetComponent<EnemyData>().StartPursue();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            // 只有真正进过圈的敌人（在 table 里）才需要清仇恨；
            // 写成 !table.Contains 会让敌人永远脱不了仇恨，还会对非敌人碰撞体取 EnemyData 抛空引用
            if (!table.Contains(other)) return;

            EnemyData e = other.GetComponent<EnemyData>();
            if (e != null)
            {
                if(playerName == 'A')
                    e.isHateA = false;
                else if(playerName == 'B')
                    e.isHateB = false;
                e.StopPursue();
            }
            Debug.Log($"[寻路] 玩家{playerName}的仇恨圈退出敌人 {other.name}");
            table.Remove(other);
        }
    }
}
