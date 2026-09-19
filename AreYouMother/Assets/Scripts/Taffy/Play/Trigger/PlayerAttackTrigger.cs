using System;
using System.Collections.Generic;
using Taffy.Play.Enemy;
using TaffyFrame.Trigger;
using UnityEngine;

namespace Taffy.Play.Trigger
{
    public class PlayerAttackTrigger : MonoBehaviour
    {
        public int ATK = 0;
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Enemy"))
            {
                Vector2 v = new Vector2(other.transform.position.x - transform.position.x,
                    other.transform.position.z - transform.position.z).normalized;
                other.GetComponent<EnemyData>().Injury(ATK,v);
            }
        }
    }
}
