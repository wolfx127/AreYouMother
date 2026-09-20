using System;
using System.Collections.Generic;
using Taffy.Play.Player;
using TaffyFrame.Trigger;
using UnityEngine;

namespace Taffy.Play.Trigger
{
    public class EnemyAttackTrigger : MonoBehaviour
    {
        public int ATK = 0;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                if (other.name == "playerA") other.gameObject.GetComponent<PlayingHandler_A>().player.Injury(ATK);
                else other.gameObject.GetComponent<PlayingHandler_B>().player.Injury(ATK);
            }
        }
    }
}
