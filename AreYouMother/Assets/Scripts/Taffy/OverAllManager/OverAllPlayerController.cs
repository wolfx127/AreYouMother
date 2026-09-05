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
            }
        }

        public int ATK_B
        {
            get => atk_B;
            set
            {
                atk_B = value;
            }
        }

        public int DEF_A
        {
            get => def_A;
            set
            {
                def_A = value;
            }
        }

        public int DEF_B
        {
            get => def_B;
            set
            {
                def_B = value;
            }
        }

        private void Awake()
        {
            if (Instance == null) Instance = this;
            var players = JsonData.LoadPlayer();
            playerA = players.player1;
            playerB = players.player2;
            Debug.Log($"[初始化] OverAllPlayerController.Awake, 所在场景:{gameObject.scene.name}, Instance置好:{(Instance == this)}, playerA:{(playerA == null ? "null" : "OK")}, playerB:{(playerB == null ? "null" : "OK")}");
        }

        private void OnDestroy()
        {
            Debug.Log("[销毁] OverAllPlayerController 被销毁了! 若开局即出现=没挂DontDestroyOnLoad");
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

        public void RemovePropAt_A(int index)
        {
            playerA.bag.RemoveAt(index);
            JsonData.SavePlayer(playerA, playerB);
        }

        public void RemovePropAt_B(int index)
        {
            playerB.bag.RemoveAt(index);
            JsonData.SavePlayer(playerA, playerB);
        }

        public void AddProp_A(Prop prop)
        {
            playerA.bag.Add(prop);
            JsonData.SavePlayer(playerA, playerB);
        }

        public void AddProp_B(Prop prop)
        {
            playerB.bag.Add(prop);
            JsonData.SavePlayer(playerA, playerB);
        }
    }
}
