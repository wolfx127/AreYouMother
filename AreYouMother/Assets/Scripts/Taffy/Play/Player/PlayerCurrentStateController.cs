////
//管理玩家进对局后的状态
////

using System;
using System.Collections.Generic;
using Taffy.Data;
using Taffy.OverAllManager;
using UnityEngine;

namespace Taffy.Play.Player
{
    public class PlayerCurrentStateController:MonoBehaviour
    {
        public static PlayerCurrentStateController Instance;

        #region 字段
        private PlayerCurrentState playerA =  new PlayerCurrentState();
        private PlayerCurrentState playerB =  new PlayerCurrentState();

        private int maxHP_A = 0;
        private int maxHP_B = 0;
        private int maxMP_A = 0;
        private int maxMP_B = 0;
        private int bagSize_A = 0;
        private int bagSize_B = 0;

        public event Action UpdateHP_A;
        public event Action UpdateMP_A;
        public event Action UpdateMP_B;
        public event Action UpdateHP_B;

        //只能看，不可调用
        #if UNITY_EDITOR
        [SerializeField] private int HP_Areadonly = 0;
        [SerializeField] private int HP_Breadonly = 0;
        [SerializeField] private int MP_Areadonly = 0;
        [SerializeField] private int MP_Breadonly = 0;
        #endif
        #endregion

#region 周期函数
        private void Awake()
        {
            if (Instance is not null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void OnEnable()
        {
            EventBus.Subscribe<GetPlayersInfosEvent>(GetPlayersInitInfos);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<GetPlayersInfosEvent>(GetPlayersInitInfos);
        }

        //打包时把这个删了
#if UNITY_EDITOR
        private void Update()
        {
            HP_Areadonly = playerA.curHP;
            HP_Breadonly = playerB.curHP;
            MP_Areadonly = playerA.curMP;
            MP_Breadonly = playerB.curMP;
        }
#endif
        #endregion

        #region 初始化
        private void GetPlayersInitInfos(GetPlayersInfosEvent evt)
        {
            //静态状态
            maxHP_A = evt.HP_playerA;
            maxHP_B = evt.HP_playerB;
            maxMP_A = evt.MP_playerA;
            maxMP_B = evt.MP_playerB;
            bagSize_A = evt.bagSize_playerA;
            bagSize_B = evt.bagSize_playerB;
            //动态状态
            playerA.curHP = evt.HP_playerA;
            playerA.curMP = evt.MP_playerA;
            playerB.curHP = evt.HP_playerB;
            playerB.curMP = evt.MP_playerB;

            playerA.bag = new List<Prop>(bagSize_A);
            playerB.bag = new List<Prop>(bagSize_B);
            Debug.Log("成功初始化对局中玩家状态");
        }
        #endregion

#region 基本业务逻辑
        public int GetCurHP_A() => playerA.curHP;
        public void SetCurHP_A(int HP) { playerA.curHP = HP; UpdateHP_A?.Invoke(); }
        public int GetCurMP_A() => playerA.curMP;
        public void SetCurMP_A(int MP) { playerA.curMP = MP; UpdateMP_A?.Invoke(); }
        public int GetMaxHP_A() => maxHP_A;
        public void SetMaxHP_A(int HP) => maxHP_A = HP;
        public int GetMaxMP_A() => maxMP_A;
        public void SetMaxMP_A(int MP) => maxMP_A = MP;
        public bool GetIsDead_A() => playerA.isDead;
        public List<Prop> GetBag_A() => playerA.bag;
        public Prop GetPropByIndex_A(int index) => playerA.bag[index];
        public int GetBagSize_A() => playerA.bag.Count;
        public bool AddPropToBag_A(Prop prop)
        {
            if (playerA.bag.Count + 1 > bagSize_A) return false;
            playerA.bag.Add(prop);
            return true;
        }

        public bool AddPropsToBag_A(List<Prop> prop)
        {
            if (playerA.bag.Count + prop.Count > bagSize_A) return false;
            foreach(Prop p in prop) playerA.bag.Add(p);
            return true;
        }
        public void RemovePropFromBag_A(Prop prop) => playerA.bag.Remove(prop);
        public void RemovePropFromBagByIndex_A(int index) => playerA.bag.RemoveAt(index);
        public void clearBag_A() => playerA.bag.Clear();


        public int GetCurHP_B() => playerB.curHP;
        public void SetCurHP_B(int HP) { playerB.curHP = HP; UpdateHP_B?.Invoke(); }
        public int GetCurMP_B() => playerB.curMP;
        public void SetCurMP_B(int MP) { playerB.curMP = MP; UpdateMP_B?.Invoke(); }
        public int GetMaxHP_B() => maxHP_B;
        public void SetMaxHP_B(int HP) => maxHP_B = HP;
        public int GetMaxMP_B() => maxMP_B;
        public void SetMaxMP_B(int MP) => maxMP_B = MP;
        public bool GetIsDead_B() => playerB.isDead;
        public List<Prop> GetBag_B() => playerB.bag;
        public Prop GetPropByIndex_B(int index) => playerB.bag[index];
        public int GetBagSize_B() => playerB.bag.Count;
        public bool AddPropToBag_B(Prop prop)
        {
            if (playerB.bag.Count + 1 > bagSize_B) return false;
            playerB.bag.Add(prop);
            return true;
        }

        public bool AddPropsToBag_B(List<Prop> prop)
        {
            if (playerB.bag.Count + prop.Count > bagSize_B) return false;
            foreach(Prop p in prop) playerB.bag.Add(p);
            return true;
        }
        public void RemovePropFromBag_B(Prop prop) => playerB.bag.Remove(prop);
        public void RemovePropFromBagByIndex_B(int index) => playerB.bag.RemoveAt(index);
        public void clearBag_B() => playerB.bag.Clear();
        #endregion
        
#region 复杂需求逻辑

        #region 获取血、蓝的额外方法
        public float GetHPPercent_A() => (float)playerA.curHP / maxHP_A;                                                                                                                           
        public float GetHPPercent_B() => (float)playerB.curHP / maxHP_B; 
        public float GetMPPercent_A() => (float)playerA.curMP / maxMP_A;                                                                                                                           
        public float GetMPPercent_B() => (float)playerB.curMP / maxMP_B;
        #endregion
        
        #region 回血扣血、回蓝扣蓝
        public void Cure_A(int givenHP)
        {
            if(playerA.curHP + givenHP >= maxHP_A) playerA.curHP = maxHP_A;
            else playerA.curHP += givenHP;
            UpdateHP_A?.Invoke();
        }

        public void Cure_B(int givenHP)
        {
            if(playerB.curHP + givenHP >= maxHP_B) playerB.curHP = maxHP_B;
            else playerB.curHP += givenHP;
            UpdateHP_B?.Invoke();
        }

        public void Injury_A(int takeHP)
        {
            if(playerA.curHP - takeHP <= 0) { playerA.isDead = true; playerA.curHP = 0; }
            else playerA.curHP -= takeHP;
            UpdateHP_A?.Invoke();
        }

        public void Injury_B(int takeHP)
        {
            if(playerB.curHP - takeHP <= 0) { playerB.isDead = true; playerB.curHP = 0; }
            else playerB.curHP -= takeHP;
            UpdateHP_B?.Invoke();
        }

        public void RestoreMP_A(int givenMP)
        {
            if(playerA.curMP + givenMP >= maxMP_A) playerA.curMP = maxMP_A;
            else playerA.curMP += givenMP;
            UpdateMP_A?.Invoke();
        }

        public void RestoreMP_B(int givenMP)
        {
            if(playerB.curMP + givenMP >= maxMP_B) playerB.curMP = maxMP_B;
            else playerB.curMP += givenMP;
            UpdateMP_B?.Invoke();
        }

        public void ConsumeMP_A(int takeMP)
        {
            if(playerA.curMP - takeMP <= 0) playerA.curMP = 0;
            else playerA.curMP -= takeMP;
            UpdateMP_A?.Invoke();
        }

        public void ConsumeMP_B(int takeMP)
        {
            if(playerB.curMP - takeMP <= 0) playerB.curMP = 0;
            else playerB.curMP -= takeMP;
            UpdateMP_B?.Invoke();
        }
        #endregion

        #region 交换道具
        public bool ExchangePropFromContainerToBag_A(Prop prop,List<Prop> container)
        {
            if (playerA.bag.Count + 1 > bagSize_A) return false;
            playerA.bag.Add(prop);
            container.Remove(prop);
            return true;
        }

        public bool ExchangePropFromContainerToBag_B(Prop prop, List<Prop> container)
        {
            if (playerB.bag.Count + 1 > bagSize_B) return false;
            playerB.bag.Add(prop);
            container.Remove(prop);
            return true;
        }

        public void ExchangePropFromBagToContainer_A(Prop prop, List<Prop> container)
        {
            container.Add(prop);
            playerA.bag.Remove(prop);
        }

        public void ExchangePropFromBagToContainer_B(Prop prop, List<Prop> container)
        {
            container.Add(prop);
            playerB.bag.Remove(prop);
        }
        #endregion

        #region 扔道具
        public void DiscardProp_A(Prop prop)
        {
            playerA.bag.Remove(prop);
        }

        public void DiscardProp_B(Prop prop)
        {
            playerB.bag.Remove(prop);
        }

        public void DiscardPropByIndex_A(int index)
        {
            playerA.bag.RemoveAt(index);
        }

        public void DiscardPropByIndex_B(int index)
        {
            playerB.bag.RemoveAt(index);
        }
        #endregion

        #endregion

    }
}
