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
            if (!other.CompareTag("Player")) return;

            // 不比对对象名字：能拿到 A 就用 A，拿不到再试 B
            if (other.TryGetComponent(out PlayingHandler_A handlerA))
            {
                handlerA.player.Injury(ATK);
            }
            else if (other.TryGetComponent(out PlayingHandler_B handlerB))
            {
                handlerB.player.Injury(ATK);
            }
            else
            {
                Debug.LogWarning($"[EnemyAttackTrigger] 命中 Player 但取不到 PlayingHandler: {other.name}");
            }
        }
    }
}
