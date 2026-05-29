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

        public Prop Weapon_A;
        public Prop Weapon_B;
        public Prop Defense_A; 
        public Prop Defense_B;

        public event Action UpdateHP_AEvent;
        public event Action UpdateMP_AEvent;
        public event Action UpdateMP_BEvent;
        public event Action UpdateHP_BEvent;
        public event Action Dead_AEvent;
        public event Action Dead_BEvent;

        public PlayerCurrentStateController()
        {
            UpdateHP_AEvent += () => Debug.Log("playerA的血量发生变化");
            UpdateMP_AEvent += () => Debug.Log("playerA的法力值发生变化");
            UpdateHP_BEvent += () => Debug.Log("playerB的血量发生变化");
            UpdateMP_BEvent += () => Debug.Log("playerB的法力值发生变化");
        }

        //只能看，不可调用
        #if UNITY_EDITOR
        [SerializeField] private int HP_Areadonly => playerA.curHP;
        [SerializeField] private int HP_Breadonly => playerB.curHP;
        [SerializeField] private int MP_Areadonly => playerA.curMP;
        [SerializeField] private int MP_Breadonly => playerB.curMP;
        [SerializeField] private int ATK_Areadonly => playerA.ATK;
        [SerializeField] private int ATK_Breadonly => playerB.ATK;
        [SerializeField] private int DEF_Areadonly => playerA.DEF;
        [SerializeField] private int DEF_Breadonly => playerB.DEF;
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
            playerA.ATK = evt.ATK_A;
            playerB.ATK = evt.ATK_B;
            playerA.DEF = evt.DEF_A;
            playerB.DEF = evt.DEF_B;
            Weapon_A = evt.tempWeapon_A;
            Weapon_B = evt.tempWeapon_B;
            Defense_A = evt.tempDefense_A;
            Defense_B = evt.tempDefense_B;

            playerA.bag = evt.bag_playerA;
            playerB.bag = evt.bag_playerB;
            Debug.Log("成功初始化对局中玩家状态");
        }
        #endregion

#region 基本业务逻辑
        public int GetCurHP_A() => playerA.curHP;

        public void SetCurHP_A(int HP)
        {
            playerA.curHP = HP;
            UpdateHP_AEvent?.Invoke();
        }
        public int GetCurMP_A() => playerA.curMP;

        public void SetCurMP_A(int MP)
        {
            playerA.curMP = MP; 
            UpdateMP_AEvent?.Invoke();
        }
        public int GetMaxHP_A() => maxHP_A;
        public void SetMaxHP_A(int HP) => maxHP_A = HP;
        public int GetMaxMP_A() => maxMP_A;
        public void SetMaxMP_A(int MP) => maxMP_A = MP;
        public bool GetIsDead_A() => playerA.isDead;
        public List<Prop> GetBag_A() => playerA.bag;
        public Prop GetPropByIndex_A(int index) => playerA.bag[index];
        public int GetBagSize_A() => bagSize_A;
        public bool AddPropToBag_A(Prop prop)
        {
            if (!CanOwn(prop, PropOwner.A)) return false;
            if (playerA.bag.Count + 1 > bagSize_A) return false;
            playerA.bag.Add(prop);
            return true;
        }

        public bool AddPropsToBag_A(List<Prop> prop)
        {
            foreach (var p in prop) if (!CanOwn(p, PropOwner.A)) return false;
            foreach (Prop p in prop) AddPropToBag_A(p);
            return true;
        }
        public void RemovePropFromBag_A(Prop prop) => playerA.bag.Remove(prop);
        public void RemovePropFromBagByIndex_A(int index) => playerA.bag.RemoveAt(index);
        public void clearBag_A() => playerA.bag.Clear();


        public int GetCurHP_B() => playerB.curHP;

        public void SetCurHP_B(int HP)
        {
            playerB.curHP = HP;

            UpdateHP_BEvent?.Invoke();
        }
        public int GetCurMP_B() => playerB.curMP;

        public void SetCurMP_B(int MP)
        {
            playerB.curMP -= MP; 
            UpdateMP_BEvent?.Invoke();
        }
        public int GetMaxHP_B() => maxHP_B;
        public void SetMaxHP_B(int HP) => maxHP_B = HP;
        public int GetMaxMP_B() => maxMP_B;
        public void SetMaxMP_B(int MP) => maxMP_B = MP;
        public bool GetIsDead_B() => playerB.isDead;
        public List<Prop> GetBag_B() => playerB.bag;
        public Prop GetPropByIndex_B(int index) => playerB.bag[index];
        public int GetBagSize_B() => bagSize_B;
        public bool AddPropToBag_B(Prop prop)
        {
            if (!CanOwn(prop, PropOwner.B)) return false;
            if (playerB.bag.Count + 1 > bagSize_B) return false;
            playerB.bag.Add(prop);
            return true;
        }

        public bool AddPropsToBag_B(List<Prop> prop)
        {
            foreach (var p in prop) if (!CanOwn(p, PropOwner.B)) return false;
            foreach (Prop p in prop) AddPropToBag_B(p);
            return true;
        }
        public void RemovePropFromBag_B(Prop prop) => playerB.bag.Remove(prop);
        public void RemovePropFromBagByIndex_B(int index) => playerB.bag.RemoveAt(index);
        public void clearBag_B() => playerB.bag.Clear();
        #endregion

        #region 归属与堆叠的辅助
        private static bool CanOwn(Prop prop, PropOwner who)
        {
            if (prop == null) return false;
            return prop.owner == PropOwner.Public || prop.owner == who;
        }
        
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
            UpdateHP_AEvent?.Invoke();
        }

        public void Cure_B(int givenHP)
        {
            if(playerB.curHP + givenHP >= maxHP_B) playerB.curHP = maxHP_B;
            else playerB.curHP += givenHP;
            UpdateHP_BEvent?.Invoke();
        }

        public void Injury_A(int takeHP)
        {
            playerA.curHP -= takeHP;
            if (playerA.curHP <= 0)
            {
                playerA.isDead = true;
                Dead_AEvent?.Invoke();
            }
            UpdateHP_AEvent?.Invoke();
        }

        public void Injury_B(int takeHP)
        {
            playerB.curHP -= takeHP;
            if (playerB.curHP <= 0)
            {
                playerB.isDead = true;
                Dead_BEvent?.Invoke();
            }
            UpdateHP_BEvent?.Invoke();
        }

        public void RestoreMP_A(int givenMP)
        {
            if(playerA.curMP + givenMP >= maxMP_A) playerA.curMP = maxMP_A;
            else playerA.curMP += givenMP;
            UpdateMP_AEvent?.Invoke();
        }

        public void RestoreMP_B(int givenMP)
        {
            if(playerB.curMP + givenMP >= maxMP_B) playerB.curMP = maxMP_B;
            else playerB.curMP += givenMP;
            UpdateMP_BEvent?.Invoke();
        }

        public bool ConsumeMP_A(int takeMP)
        {
            if(takeMP > playerA.curMP) return false;
            playerA.curMP -= takeMP;
            UpdateMP_AEvent?.Invoke();
            return true;
        }

        public bool ConsumeMP_B(int takeMP)
        {
            if(takeMP > playerB.curMP) return false;
            playerB.curMP -= takeMP;
            UpdateMP_BEvent?.Invoke();
            return true;
        }
        #endregion

        #region 交换道具
        public bool ExchangePropFromContainerToBag_A(Prop prop,List<Prop> container)
        {
            if (!CanOwn(prop, PropOwner.A)) return false;
            if (playerA.bag.Count + 1 > bagSize_A) return false;
            playerA.bag.Add(prop);
            container.Remove(prop);
            return true;
        }

        public bool ExchangePropFromContainerToBag_B(Prop prop, List<Prop> container)
        {
            if (!CanOwn(prop, PropOwner.B)) return false;
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

        public void ResetWeapon_A(Prop prop)
        {
            Weapon_A = prop;
        }

        public void ResetWeapon_B(Prop prop)
        {
            Weapon_B = prop;
        }

        public void ResetDefense(Prop prop)
        {
            Defense_A = prop;
        }

        public void ResetDefense_B(Prop prop)
        {
            Defense_B = prop;
        }

        public void SetAtk_A(int value)
        {
            playerA.ATK = value;
        }
        public void SetAtk_B(int value)
        {
            playerB.ATK = value;
        }
        public void SetDef_A(int value)
        {
            playerA.DEF = value;
        }
        public void SetDef_B(int value)
        {
            playerB.DEF = value;
        }

        #endregion
        
        

        public int GetBagProperty_A()
        {
            int property = 0;
            foreach (Prop prop in playerA.bag)
            {
                property += prop.value;
            }
            return property;
        }
        public int GetBagProperty_B()
        {
            int property = 0;
            foreach (Prop prop in playerB.bag)
            {
                property += prop.value;
            }
            return property;
        }

        public int GetAllProperty()
        {
            int property = 0;
            property += GetBagProperty_A();
            property += GetBagProperty_B();
            return property;
        }

        //把对局内的两个背包回传给对局外的 OverAllPlayerController
        public void GiveBags()
        {
            EventBus.Publish(new GiveBagsEvent(playerA.bag, playerB.bag));
        }

        #endregion

    }
}
