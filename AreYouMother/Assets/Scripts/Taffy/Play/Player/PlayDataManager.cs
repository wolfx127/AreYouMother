////
//管理玩家进对局后的状态
////

using System;
using System.Collections.Generic;
using Taffy.Data;
using Taffy.Data.PropData;
using Taffy.OverAllManager;
using TaffyFrame.EventBus;
using TaffyFrame.FSM;
using UnityEngine;

namespace Taffy.Play.Player
{
    public enum PlayIndexPlace
    {
        Bag,
        Container
    }

    public class PlayDataManager
    {
        public CombatData combatData = null;
        [InspectorName("背包")]public List<Prop> bag = new List<Prop>();
        [InspectorName("背包容量")]public int bagSize = 0;
        [InspectorName("武器")]public Prop weapon = null;
        [InspectorName("防具")]public Prop defense = null;
        
        [InspectorName("移动速度")]public float speed = 0;
        
        private PlayerFSM FSM = null;

        public event Action UpdateHP_AEvent;
        public event Action UpdateMP_AEvent;
        public event Action UpdateMP_BEvent;
        public event Action UpdateHP_BEvent;
        public event Action Dead_AEvent;
        public event Action Dead_BEvent;

        public PlayDataManager(int HP, int MP, List<Prop> bag, int bagSize, Prop weapon, Prop defense)
        {
            this.weapon = weapon;
            this.defense = defense;
            combatData = new CombatData(HP, MP, weapon.GetATK(), defense.GetDEF());
            this.bag = bag;
            this.bagSize = bagSize;
            this.weapon = weapon;
            this.defense = defense;
            
            FSM =  new PlayerFSM(this, new Board_Player());
        }

        public void Subscribe()
        {
            
        }

        public void Unsubscribe()
        {
            
        }

///////// 状态机 //////////////////////////////////////////////////////////////////////
#region 状态

        public void SetBoard()
        {
            FSM.SetBoard(combatData.HP, combatData.MP);
        }
        public void TickFSM() => FSM.Tick();
        
////////// 和状态匹配的方法 /////////////////////////////////////////////////////////////////////////
        public void Idle()
        {
            
        }



        #endregion
    }
}
