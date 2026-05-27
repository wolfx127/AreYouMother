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
        private PlayerProfile playerA = new PlayerProfile();
        private PlayerProfile playerB = new PlayerProfile();
        public static OverAllPlayerController Instance;
        
        public int maxHP_A { get => playerA.maxHP; set => playerA.maxHP = value; }
        public int maxHP_B { get => playerB.maxHP; set => playerB.maxHP = value; }
        public int maxMP_A { get => playerA.maxMP; set =>  playerA.maxMP = value; }
        public int maxMP_B { get => playerB.maxMP; set =>  playerB.maxMP = value; }
        public int bagSize_A { get => playerA.bagSize; set => playerA.bagSize = value; }
        public int bagSize_B { get => playerB.bagSize; set => playerB.bagSize = value; }

        private void Awake()
        {
            if (Instance == null) Instance = this;
            var players = JsonData.LoadPlayer();
            playerA = players.player1;
            playerB = players.player2;
            
            playerA.bag.Add(new Coin());
            playerB.bag.Add(new Coin());
            playerA.bag.Add(new Coin());
            playerB.bag.Add(new Coin());
            playerA.bag.Add(new Coin());
            playerB.bag.Add(new Coin());
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
            EventBus.Publish(new GetPlayersInfosEvent(maxHP_A, maxHP_B, maxMP_A, maxMP_B, GetBag_A(), GetBag_B(), bagSize_A, bagSize_B));
        }

        private void ExitGame(ExitGameEvent evt)
        {
            JsonData.SavePlayer(playerA,playerB);
        }


        public List<Prop> GetBag_A() => playerA.bag;
        public List<Prop> GetBag_B() => playerB.bag;
        public void RemoveProp_A(Prop a) => playerA.bag.Remove(a);
        public void RemoveProp_B(Prop b) => playerB.bag.Remove(b);
        public void RemovePropByIndex_A(int i) => playerA.bag.RemoveAt(i);
        public void RemovePropByIndex_B(int i) => playerB.bag.RemoveAt(i);
        public void AddProp_A(Prop a) => playerA.bag.Add(a);
        public void AddProp_B(Prop b) => playerB.bag.Add(b);
    }
}
