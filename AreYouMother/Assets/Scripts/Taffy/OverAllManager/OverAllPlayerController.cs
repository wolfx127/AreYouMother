////
//管理整个游戏下的两个玩家特有静态状态，也就是血量上限等，以及进对局前的背包
//// 
using System;
using System.Collections.Generic;
using NUnit.Framework;
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
        [SerializeField] private int bagSize_A;
        [SerializeField] private int bagSize_B;
        private List<Prop> bag_A = new List<Prop>();
        private List<Prop> bag_B = new List<Prop>();

        private void Awake()
        {
            var players = JsonData.Load();
            playerA_profile = players.player1;
            playerB_profile = players.player2;
            
            maxHP_A = playerA_profile.maxHP;
            maxHP_B = playerB_profile.maxHP;
            maxMP_A = playerA_profile.maxMP;
            maxMP_B = playerB_profile.maxMP;
            bagSize_A = playerA_profile.bagSize;
            bagSize_B = playerB_profile.bagSize;
            bag_A = playerA_profile.bag;
            bag_B = playerB_profile.bag;

            if (bag_A.Count == 0)
            {
                for(int i = 0;i<3;i++)
                {
                    bag_A.Add(new Coin());
                }
            }

            if (bag_B.Count == 0)
            {
                for(int i = 0;i<3;i++)
                {
                    bag_B.Add(new Coin());
                }
            }
        }

        private void OnEnable()
        {
            EventBus.Subscribe<InitialPlayingSceneEvent>(InitialPlayingScene);
            EventBus.Subscribe<ExitGameEvent>(ExitGame);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<InitialPlayingSceneEvent>(InitialPlayingScene);
            EventBus.Unsubscribe<ExitGameEvent>(ExitGame);
        }


        private void InitialPlayingScene(InitialPlayingSceneEvent evt)
        {
            EventBus.Publish(new GetPlayersInfosEvent(maxHP_A, maxHP_B, maxMP_A, maxMP_B,bag_A, bag_B,bagSize_A, bagSize_B));
        }

        private void ExitGame(ExitGameEvent evt)
        {
            JsonData.Save(playerA_profile,playerB_profile);
        }
    }
}
