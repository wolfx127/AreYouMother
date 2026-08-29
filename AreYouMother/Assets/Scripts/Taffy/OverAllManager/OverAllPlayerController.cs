////
//管理整个游戏下的两个玩家特有静态状态，也就是血量上限等，以及进对局前的背包
//// 
using System;
using System.Collections.Generic;
using NUnit.Framework;
using Taffy.Data;
using Taffy.Data.PropData;
using UnityEngine;

namespace Taffy.OverAllManager
{
    public class OverAllPlayerController:MonoBehaviour
    {
        private PlayerProfile playerA = new PlayerProfile();
        private PlayerProfile playerB = new PlayerProfile();
        public static OverAllPlayerController Instance;

        private int atk_A = 0;
        private int atk_B = 0;
        private int def_A = 0;
        private int def_B = 0;

        public Prop tempWeapon_A;
        public Prop tempWeapon_B;
        public Prop tempDefense_A;
        public Prop tempDefense_B;

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

        public int ATK_A
        {
            get => atk_A;
            set
            {
                atk_A = value;
                UpdateInfo_AEvent?.Invoke();
            }
        }

        public int ATK_B
        {
            get => atk_B;
            set { atk_B = value;
                UpdateInfo_BEvent?.Invoke(); }
        }

        public int DEF_A
        {
            get => def_A;
            set { def_A = value;
                UpdateInfo_AEvent?.Invoke(); }
        }

        public int DEF_B
        {
            get => def_B;
            set { def_B = value;
                UpdateInfo_BEvent?.Invoke(); }
        }

        private void Awake()
        {
            if (Instance == null) Instance = this;
            var players = JsonData.LoadPlayer();
            playerA = players.player1;
            playerB = players.player2;
        }

        private void OnEnable()
        {
            EventBus.Subscribe<InitialPlayingSceneEvent>(InitialPlayingScene);
            EventBus.Subscribe<ExitGameEvent>(ExitGame);
            EventBus.Subscribe<GiveBagsEvent>(OnGiveBags);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<InitialPlayingSceneEvent>(InitialPlayingScene);
            EventBus.Unsubscribe<ExitGameEvent>(ExitGame);
            EventBus.Unsubscribe<GiveBagsEvent>(OnGiveBags);
        }

        //接收对局内回传的两个背包，重新赋值给对局外的背包
        private void OnGiveBags(GiveBagsEvent evt)
        {
            playerA.bag = evt.bagA;
            playerB.bag = evt.bagB;
            JsonData.SavePlayer(playerA, playerB);
        }
        
        private void InitialPlayingScene(InitialPlayingSceneEvent evt)
        {
            EventBus.Publish(new GetPlayersInfosEvent(maxHP_A, maxHP_B, maxMP_A, maxMP_B, GetBag_A(), GetBag_B(), bagSize_A, bagSize_B,
                ATK_A,ATK_B,DEF_A,DEF_B,tempWeapon_A,tempWeapon_B,tempDefense_A,tempDefense_B));
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

        public void AssignATK_A(int ATK)
        {
            atk_A = ATK;
            UpdateInfo_AEvent?.Invoke();
        }

        public void AssignATK_B(int ATK)
        {
            atk_B = ATK;
            UpdateInfo_BEvent?.Invoke();
        }

        public void AssignDEF_A(int DEF)
        {
            def_A = DEF;
            UpdateInfo_AEvent?.Invoke();
        }

        public void AssignDEF_B(int DEF)
        {
            def_B = DEF;
            UpdateInfo_BEvent?.Invoke();
        }
    }
}
