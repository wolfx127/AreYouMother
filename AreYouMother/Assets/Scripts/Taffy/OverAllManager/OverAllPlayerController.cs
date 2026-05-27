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

        public event Action UpdateInfo_AEvent;
        public event Action UpdateInfo_BEvent;
        
        public int maxHP_A
        {
            get { return playerA.maxHP; }
            set { playerA.maxHP = value; }
        }

        public int maxHP_B
        {
            get { return playerB.maxHP; }
            set { playerB.maxHP = value; }
        }

        public int maxMP_A
        {
            get { return playerA.maxMP; }
            set { playerA.maxMP = value; }
        }

        public int maxMP_B
        {
            get { return playerB.maxMP; }
            set { playerB.maxMP = value; }
        }

        public int bagSize_A
        {
            get { return playerA.bagSize; }
            set { playerA.bagSize = value; }
        }

        public int bagSize_B
        {
            get { return playerB.bagSize; }
            set { playerB.bagSize = value; }
        }

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


        public List<Prop> GetBag_A()
        {
            return playerA.bag;
        }

        public List<Prop> GetBag_B()
        {
            return playerB.bag;
        }

        public void RemoveProp_A(Prop a)
        {
            playerA.bag.Remove(a);
            JsonData.SavePlayer(playerA,playerB);
        }

        public void RemoveProp_B(Prop b)
        {
            playerB.bag.Remove(b);
            JsonData.SavePlayer(playerA,playerB);
        }

        public void RemovePropByIndex_A(int i)
        {
            playerA.bag.RemoveAt(i);
            JsonData.SavePlayer(playerA,playerB);
        }

        public void RemovePropByIndex_B(int i)
        {
            playerB.bag.RemoveAt(i);
            JsonData.SavePlayer(playerA,playerB);
        }

        public void AddProp_A(Prop a)
        {
            playerA.bag.Add(a);
            JsonData.SavePlayer(playerA,playerB);
        }

        public void AddProp_B(Prop b)
        {
            playerB.bag.Add(b);
            JsonData.SavePlayer(playerA,playerB);
        }

        public void AddMaxHP_A(int value)
        {
            playerA.maxHP += value;
            UpdateInfo_AEvent?.Invoke();
            JsonData.SavePlayer(playerA,playerB);
        }
        public void AddMaxHP_B(int value)
        {
            playerB.maxHP += value;
            UpdateInfo_BEvent?.Invoke();
            JsonData.SavePlayer(playerA,playerB);
        }
        public void AddMaxMP_A(int value)
        {
            playerA.maxMP += value;
            UpdateInfo_AEvent?.Invoke();
            JsonData.SavePlayer(playerA,playerB);
        }
        public void AddMaxMP_B(int value)
        {
            playerB.maxMP += value;
            UpdateInfo_BEvent?.Invoke();
            JsonData.SavePlayer(playerA,playerB);
        }
    }
}
