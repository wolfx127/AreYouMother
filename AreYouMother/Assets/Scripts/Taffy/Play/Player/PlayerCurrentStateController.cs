////
//管理玩家进对局后的状态
////

using System;
using Taffy.Data;
using Taffy.OverAllManager;
using UnityEngine;

namespace Taffy.Play.Player
{
    public class PlayerCurrentStateController:MonoBehaviour
    {
        private PlayerCurrentState playerA =  new PlayerCurrentState();
        private PlayerCurrentState playerB =  new PlayerCurrentState();

        //只能看，不可调用
        #if UNITY_EDITOR
        [SerializeField] private int HP_PlayerA = 0;
        [SerializeField] private int HP_PlayerB = 0;
        #endif

        private void OnEnable()
        {
            EventBus.Subscribe<GetPlayersInfosEvent>(GetPlayersInfos);
        }

        //打包时把这个删了
        #if UNITY_EDITOR
        private void Update()
        {
            HP_PlayerA = playerA.curHP;
            HP_PlayerB = playerB.curHP;
        }
        #endif

        private void OnDisable()
        {
            EventBus.Unsubscribe<GetPlayersInfosEvent>(GetPlayersInfos);
        }

        private void GetPlayersInfos(GetPlayersInfosEvent evt)
        {
            playerA.curHP = evt.HP_playerA;
            playerA.curMP = evt.MP_playerA;
            playerA.bag = evt.bag_playerA;
            playerB.curHP = evt.HP_playerB;
            playerB.curMP = evt.MP_playerB;
            playerB.bag = evt.bag_playerB;
            Debug.Log("成功初始化对局中玩家状态");
        }
    }
}
