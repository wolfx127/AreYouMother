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
                    other.GetComponent<EnemyData>().isHateA = true;
                else if(playerName == 'B')
                    other.GetComponent<EnemyData>().isHateB = true;
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
            if (!table.Contains(other))
            {
                EnemyData e = other.GetComponent<EnemyData>();
                if(playerName == 'A')
                    e.isHateA = false;
                else if(playerName == 'B')
                    e.isHateB = false;
                e.StopPursue();
                table.Remove(other);
            }
        }
    }
}
