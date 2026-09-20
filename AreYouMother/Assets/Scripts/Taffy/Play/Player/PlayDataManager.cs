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

        public int sumInjury = 0;
        public float injuryCD = 0.5f;
        
        private PlayerFSM FSM = null;

        public event Action<float> InjuryAnimEvent;
        public event Action<float> AttackAnimEvent;
        public event Action WalkAnimEvent;
        public event Action IdleAnimEvent;
        public event Action DeadEvent;

        /// <summary>HP/MP 变化的对外事件：由 combatData 转发过来，换 CombatData 也不会断订阅</summary>
        public event Action<int> UpdateHPEvent;
        public event Action<int> UpdateMPEvent;

        public PlayDataManager(int HP, int MP, List<Prop> bag, int bagSize, Prop weapon, Prop defense)
        {
            this.weapon = weapon;
            this.defense = defense;
            int atk = weapon is not null ? weapon.GetATK() : 0;
            int def = defense is not null ? defense.GetDEF() : 0;
            SetCombatData(new CombatData(HP, MP, atk, def));
            this.bag = bag == null ? new List<Prop>() : new List<Prop>(bag);
            this.bagSize = bagSize;
            
            FSM =  new PlayerFSM(this, new Board_Player(),State.Idle);
        }

        public void WriteInfo(int HP, int MP, List<Prop> bag, int bagSize, Prop weapon, Prop defense)
        {
            this.weapon = weapon;
            this.defense = defense;
            int atk = weapon is not null ? weapon.GetATK() : 0;
            int def = defense is not null ? defense.GetDEF() : 0;

            // combatData 可能是第一次赋值，也可能被整个替换掉，两种情况都要把事件转接好，
            // 否则 UI 订阅的是旧实例，玩家掉血时界面不会刷新
            SetCombatData(new CombatData(HP, MP, atk, def));

            this.bag = bag == null ? new List<Prop>() : new List<Prop>(bag);
            this.bagSize = bagSize;
        }

        /// <summary>
        /// 换一份 CombatData 并把它的 HP/MP 事件转接到本对象的事件上，订阅方不用重新订阅。
        /// </summary>
        private void SetCombatData(CombatData data)
        {
            if (combatData != null)
            {
                combatData.UpdateHPEvent -= ForwardHP;
                combatData.UpdateMPEvent -= ForwardMP;
            }

            combatData = data;

            combatData.UpdateHPEvent += ForwardHP;
            combatData.UpdateMPEvent += ForwardMP;

            // 换完立刻推一次当前值，UI 能马上显示成新数据
            combatData.HP = combatData.HP;
            combatData.MP = combatData.MP;
        }

        private void ForwardHP(int hp) => UpdateHPEvent?.Invoke(hp);
        private void ForwardMP(int mp) => UpdateMPEvent?.Invoke(mp);

        /// <summary>
        /// 把当前 HP/MP 重新推一次给 UI（进场初始化完数据后调用）。
        /// </summary>
        public void SyncUI()
        {
            if (combatData == null) return;
            combatData.HP = combatData.HP;
            combatData.MP = combatData.MP;
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

            // 调试用：把每秒动画状态打出来，排查"挨打后动画消失"
            if (debugAnim && Time.time - lastAnimDebugTime >= 0.5f)
            {
                lastAnimDebugTime = Time.time;
                bool hasInjurySub = InjuryAnimEvent != null;
                bool hasIdleSub   = IdleAnimEvent != null;
                bool hasWalkSub   = WalkAnimEvent != null;
                Debug.Log($"[动画] HP={combatData.HP} isIdle={isIdle} isWalk={isWalk} " +
                          $"isAttack={isAttack} isInjury={isInjury} " +
                          $"受伤CD={InjuryCounter:F2} 攻击CD={AttackCounter:F2} " +
                          $"订阅(受伤={hasInjurySub},待机={hasIdleSub},行走={hasWalkSub})");
            }
        }

        /// <summary>调试：每 0.5 秒打印一次玩家动画状态</summary>
        public bool debugAnim = false;
        private float lastAnimDebugTime = -999f;

        public void Injury(int damage)
        {
            if (InjuryCounter > 0) return;
            isInjury = true;
            sumInjury += damage * 100 / (100 + combatData.DEF);
            InjuryCounter = injuryCD;
        }

        public void Damage()
        {
            combatData.HP -= sumInjury;
            sumInjury = 0;
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
