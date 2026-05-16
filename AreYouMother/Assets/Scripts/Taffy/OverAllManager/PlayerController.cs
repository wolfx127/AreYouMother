using System;
using Taffy.Data;
using UnityEngine;

namespace Taffy.OverAllManager
{
    public class OverAllPlayerController:MonoBehaviour
    {
        private PlayerProfile playerA_profile = new PlayerProfile();
        private PlayerProfile playerB_profile = new PlayerProfile();
        
        [SerializeField] private int maxHP_A;
        [SerializeField] private int maxHP_B;
        [SerializeField] private int maxMP_A;
        [SerializeField] private int maxMP_B;

        private void Awake()
        {
            var players = JsonData.Load();
            playerA_profile = players.player1;
            playerB_profile = players.player2;
            maxHP_A = playerA_profile.maxHP;
            maxHP_B = playerB_profile.maxHP;
            maxMP_A = playerA_profile.maxMP;
            maxMP_B = playerB_profile.maxMP;
        }

        private void OnEnable()
        {
            EventBus.Subscribe<GiveHPandMPEvent>(GiveHPandMP);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<GiveHPandMPEvent>(GiveHPandMP);
        }

        private void GiveHPandMP(GiveHPandMPEvent evt)
        {
            EventBus.Publish(new GetPlayersHPandMPEvent(maxHP_A, maxHP_B, maxMP_A, maxMP_B));
        }
    }
}
