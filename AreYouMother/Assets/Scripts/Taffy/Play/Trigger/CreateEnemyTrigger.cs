using System;
using System.Collections.Generic;
using Taffy.Play.Enemy;
using TaffyFrame.Trigger;
using UnityEngine;

namespace Taffy.Play.Trigger
{
    public class CreateEnemyTrigger : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            try
            {
                if (!other.CompareTag("EnemyCreatePlace")) return;
                other.gameObject.GetComponent<EnemyCreatPlace>().CreateEnemy();
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
            }
        }
    }
}
