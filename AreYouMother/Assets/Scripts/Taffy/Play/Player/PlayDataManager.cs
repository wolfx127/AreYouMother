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
        public CombatData combatData = new CombatData(1, 1, 1, 1);
        [InspectorName("背包")]public List<Prop> bag = new List<Prop>();
        [InspectorName("背包容量")]public int bagSize = 20;
        [InspectorName("武器")]public Prop weapon = null;
        [InspectorName("防具")]public Prop defense = null;
        
        private PlayerFSM FSM = null;

        public PlayDataManager(int HP, int MP, List<Prop> bag, int bagSize, Prop weapon, Prop defense)
        {
            this.weapon = weapon;
            this.defense = defense;
            if(weapon is not null && defense is not null) 
                combatData.WriteInfo(HP, MP, weapon.GetATK(), defense.GetDEF());
            else 
                combatData.WriteInfo(HP, MP, 0, 0);
            this.bag = bag == null ? new List<Prop>() : new List<Prop>(bag);
            this.bagSize = bagSize;
            this.weapon = weapon;
            this.defense = defense;
            
            FSM =  new PlayerFSM(this, new Board_Player(),State.Idle_Player);
        }

        public void WriteInfo(int HP, int MP, List<Prop> bag, int bagSize, Prop weapon, Prop defense)
        {
            this.weapon = weapon;
            this.defense = defense;
            if(weapon is not null && defense is not null) 
                combatData.WriteInfo(HP, MP, weapon.GetATK(), defense.GetDEF());
            else 
                combatData.WriteInfo(HP, MP, 0, 0);
            this.bag = bag == null ? new List<Prop>() : new List<Prop>(bag);
            this.bagSize = bagSize;
            this.weapon = weapon;
            this.defense = defense;
        }

///////// 状态机 //////////////////////////////////////////////////////////////////////
#region 状态

        public void SetBoard()
        {
            FSM.SetBoard(combatData.HP, combatData.MP);
        }
        public void TickFSM() => FSM.Tick();
        
////////// 和状态匹配的方法 /////////////////////////////////////////////////////////////////////////
        

        public void UpdateHP()
        {
            
        }

        public void UpdateMP()
        {
            
        }

        #endregion
    }
}
