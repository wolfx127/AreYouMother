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

        public bool isAttack = false;
        public bool isIdle = false;
        public bool isWalk = false;
        public bool isInjury = false;
        public bool isDead = false;
        public bool isEvacuate  = false;

        public float AttackAnimTime = 0.2f;
        public float AttackCounter = 0;
        public float InjuryAnimTime = 0.5f;
        public float InjuryCounter = 0;
        
        private PlayerFSM FSM = null;

        public event Action<float> InjuryAnimEvent;
        public event Action<float> AttackAnimEvent;
        public event Action WalkAnimEvent;
        public event Action IdleAnimEvent;
        public event Action DeadEvent;

        public PlayDataManager(int HP, int MP, List<Prop> bag, int bagSize, Prop weapon, Prop defense)
        {
            this.weapon = weapon;
            this.defense = defense;
            int atk = weapon is not null ? weapon.GetATK() : 0;
            int def = defense is not null ? defense.GetDEF() : 0;
            combatData.WriteInfo(HP, MP, atk, def);
            this.bag = bag == null ? new List<Prop>() : new List<Prop>(bag);
            this.bagSize = bagSize;
            this.weapon = weapon;
            this.defense = defense;
            
            FSM =  new PlayerFSM(this, new Board_Player(),State.Idle);
        }

        public void WriteInfo(int HP, int MP, List<Prop> bag, int bagSize, Prop weapon, Prop defense)
        {
            this.weapon = weapon;
            this.defense = defense;
            int atk = weapon is not null ? weapon.GetATK() : 0;
            int def = defense is not null ? defense.GetDEF() : 0;
            combatData.WriteInfo(HP, MP, atk, def);
            this.bag = bag == null ? new List<Prop>() : new List<Prop>(bag);
            this.bagSize = bagSize;
            this.weapon = weapon;
            this.defense = defense;
        }

///////// 状态机 //////////////////////////////////////////////////////////////////////
#region 状态

public void SetBoard(int HP, int MP, bool idle, bool walk, bool attack, bool injury)
{
    FSM.SetBoard(HP, MP, idle, walk, attack, injury);
}
        public void TickFSM() => FSM.Tick();
        
////////// 和状态匹配的方法 /////////////////////////////////////////////////////////////////////////

        public void AnimPlay()
        {
            if (AttackCounter > 0)
            {
                AttackCounter -= Time.deltaTime;
                AttackAnimEvent?.Invoke(AttackAnimTime);
            }
            else isAttack = false;
            
            if (InjuryCounter > 0)
            {
                InjuryCounter -= Time.deltaTime;
                InjuryAnimEvent?.Invoke(InjuryAnimTime);
            }
            else isInjury = false;
            
            if(!isAttack)
            {
                if(isWalk) WalkAnimEvent?.Invoke();
                if(isIdle) IdleAnimEvent?.Invoke();
            }
        }

        public void Die()
        {
            isDead = true;
            combatData.HP = 0;
            combatData.MP = 0;
            combatData.ATK = 0;
            combatData.DEF = 0;
            weapon = null;
            defense = null;
            bag.Clear();
            DeadEvent?.Invoke();
        }

        #endregion
    }
}
