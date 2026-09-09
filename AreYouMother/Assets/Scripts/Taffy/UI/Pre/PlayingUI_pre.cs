using System;
using System.Collections.Generic;
using Taffy.Data;
using Taffy.Data.PropData;
using Taffy.OverAllManager;
using Taffy.Play.Container;
using Taffy.Play.Player;
using TaffyFrame.EventBus;
using UnityEngine;

namespace Taffy.UI.Pro
{
    public struct Index
    {
        public  int  index;
        public bool isInContainer;
        public Index(int index , bool isInContainer = false)
        {
            this.index = index;
            this.isInContainer = isInContainer;
        }
        public bool GetisInContainer() => isInContainer;
        public void ChangePlace()
        {
            isInContainer = !isInContainer;
            index = 0;
        }
        /// <summary>
        /// 把参数赋给调用者
        /// </summary>
        /// <param name="other"></param>
        public void EqualAs(Index other)
        {
            this.index = other.index;
            this.isInContainer = other.GetisInContainer();
        }
    }
    public class PlayingUI_pro
    {
        /// <summary>
        /// 外部类使用时应仅用作事件注册
        /// </summary>
        /// <summary>
        /// 外部类使用时应仅用作事件注册
        /// </summary>
        public PlayingHandler_A handlerA => PlayingHandler_A.Instance;
        /// <summary>
        /// 外部类使用时应仅用作事件注册
        /// </summary>

        public event Action CheckingProp_AEvent;
        public event Action DiscardProp_AEvent;
        public event Action ReplaceProp_AEvent;
        public event Action CheckingProp_BEvent;
        public event Action DiscardProp_BEvent;
        public event Action ReplaceProp_BEvent;
        public event Action RefreshBag_AEvent;
        public event Action RefreshBag_BEvent;

        /// <summary>
        /// playerA:包含一个int成员和一个bool成员|
        /// int是索引，bool假 是在背包，真 是在箱子
        /// </summary>
        private Index propIndex_A = new Index(0);
        private Index prevPropIndex_A = new Index(0);
        /// <summary>
        /// playerA:包含一个int成员和一个bool成员|
        /// int是索引，bool假 是在背包，真 是在箱子
        /// </summary>
        private Index propIndex_B = new Index(0);
        private Index prevPropIndex_B = new Index(0);

        public ContainerData container_A;
        public ContainerData container_B;
        

        public void Subscribe()
        {
            handlerA.ChoosePropArrowEvent += ObtainPropIndex_A;//上下左右输入->invoke->更新索引()
            handlerA.DiscardPropEvent += DiscardProp_A;//丢弃道具输入->invoke->丢弃道具()
            handlerA.ReplacePropEvent += ReplaceProp_A;//更换道具输入->invoke->更换道具()
            handlerA.CloseBagEvent += ResetIndex_A;//关闭查看背包输入->invoke->重置索引()
            EventBus.Subscribe<GiveContainer_AEvent>(ObtainContainer_A);//trigger碰撞(Enter返回other,Exit返回null)->获取箱子event->获取碰到的的箱子()
//TODO            handlerA.UsePropEvent += UseProp_A;

            EventBus.Subscribe<GiveContainer_BEvent>(ObtainContainer_B);//trigger碰撞(Enter返回other,Exit返回null)->获取箱子event->获取碰到的的箱子()
//TODO            handlerB.UsePropEvent += UseProp_B;
        }
        public void Unsubscribe()
        {
            handlerA.ChoosePropArrowEvent -= ObtainPropIndex_A;
            handlerA.DiscardPropEvent -= DiscardProp_A;
            handlerA.ReplacePropEvent -= ReplaceProp_A;
            handlerA.CloseBagEvent -= ResetIndex_A;
            EventBus.Unsubscribe<GiveContainer_AEvent>(ObtainContainer_A);
//TODO            handlerA.UsePropEvent -= UseProp_A;

            EventBus.Unsubscribe<GiveContainer_BEvent>(ObtainContainer_B);
//TODO            handlerB.UsePropEvent -= UseProp_B;
        }
        

        /// <summary>
        /// 获取playerA的HPmax/HP  MPmax/MP的字符串
        /// </summary>
        /// <returns></returns>
        public string InfoNum_playerA()
        {
            return "";
        }

        /// <summary>
        /// 获取playerB的HPmax/HP  MPmax/MP的字符串
        /// </summary>
        /// <returns></returns>
        public string InfoNum_playerB()
        {
            return "";
        }

        /// <summary>
        /// playerA:pro类内闭包保存checking的索引，返回该索引代表的道具
        /// </summary>
        /// <returns></returns>
        public Prop GetCurrentProp_A()
        {
            return null;
        }
        /// <summary>
        /// playerB:pro类内闭包保存checking的索引，返回该索引代表的道具
        /// </summary>
        /// <returns></returns>
        public Prop GetCurrentProp_B()
        {
            return null;
        }

        public string GetCurrentPropName_A()
        {
            return GetCurrentProp_A().name;
        }
        public string GetCurrentPropName_B()
        {
            return GetCurrentProp_B().name;
        }
        
        /// <summary>
        /// playerA:获取道具描述字符串，包括价值、数值、稀有度
        /// </summary>
        /// <returns></returns> 
        //public string GetCurrentPropDescribe_A()
        //{
//TODO
        //    return $"价值:{GetCurrentProp_A().value} | {GetCurrentProp_A().rarity}" + '\n' +
        //           GetCurrentProp_A().description;
        //}
        /// <summary>
        /// playerB:获取道具描述字符串，包括价值、数值、稀有度
        /// </summary>
        /// <returns></returns> 
        //public string GetCurrentPropDescribe_B()
        //{
//TODO
        //    return $"价值:{GetCurrentProp_B().value} | {GetCurrentProp_B().rarity}" + '\n' +
        //          GetCurrentProp_B().description;
        //}

        public string GetBagInfo_A()
        {
            return "$\"背包上限/现存道具数:{pcsc.GetBagSize_A()}/{pcsc.GetBag_A().Count}\";";
        }
        public string GetBagInfo_B()
        {
            return "";
        }
        
        public string GetContainerName_A() => GetLocalizedContainerName(container_A?.name);
        public string GetContainerName_B() => GetLocalizedContainerName(container_B?.name);

        private string GetLocalizedContainerName(string rawName) => rawName switch
        {
            string s when s != null && s.Contains("Weapon")    => "武器箱",
            string s when s != null && s.Contains("Treat")     => "医疗箱",
            string s when s != null && s.Contains("Defence")   => "防具箱",
            string s when s != null && s.Contains("Insurance") => "保险箱",
            string s when s != null && s.Contains("Case")      => "普通箱",
            _ => rawName
        };

        public Index GetPropIndex_A() => propIndex_A;
        public Index GetPrevPropIndex_A() => prevPropIndex_A;
        /// <summary>
        /// playerA:设置prevIndex以便跟随Index
        /// </summary>
        /// <param name="i"></param>
        public void SetPrevPropIndex_A(Index i) => prevPropIndex_A.EqualAs(i);

        /// <summary>
        /// playerA:Index的Setter,为了每次write都触发一下check事件
        /// </summary>
        /// <param name="i"></param>
        private void SetPropIndex_A(Index i)
        {
            propIndex_A.EqualAs(i);
            CheckingProp_AEvent?.Invoke();
        }

        /// <summary>
        /// playerA:重置checking索引
        /// </summary>
        public void ResetIndex_A()
        {
            propIndex_A =  new Index(0);
            prevPropIndex_A = new Index(0);
        }

        /// <summary>
        /// playerA丢弃道具
        /// </summary>
        private void DiscardProp_A()
        {
        }

        public Index GetPropIndex_B() => propIndex_B;
        public Index GetPrevPropIndex_B() => prevPropIndex_B;
        /// <summary>
        /// playerB:设置prevIndex，以便跟随Index
        /// </summary>
        /// <param name="i"></param>
        public void SetPrevPropIndex_B(Index i) => prevPropIndex_B.EqualAs(i);

        /// <summary>
        /// playerB:Index的Setter,为了每次write都触发一下check事件
        /// </summary>
        /// <param name="i"></param>
        private void SetPropIndex_B(Index i)
        {
            propIndex_B.EqualAs(i);
            CheckingProp_BEvent?.Invoke();
        }

        /// <summary>
        /// playerB:重置checking索引
        /// </summary>
        public void ResetIndex_B()
        {
            propIndex_B =  new Index(0);
            prevPropIndex_B = new Index(0);
        }

        private void DiscardProp_B()
        {
        }

        /// <summary>
        /// playerA:更新checking道具的index|
        /// 接受输入事件，内部算法处理index上下左右的变化|
        /// 内部使用index的setter，因为setter有checking事件，通知ui层更新checking光标|
        /// 整个checking系统的精髓，采用拼接数组，临时矩阵的算法换算index
        /// </summary>
        /// <param name="dir"></param>
        private void ObtainPropIndex_A(Vector2Int dir)
        {
        }
        
        private void ObtainPropIndex_B(Vector2Int dir)
        {
        }

        private void ReplaceProp_B()
        {
        }

        private void ObtainContainer_B(GiveContainer_BEvent evt)
        {
            container_B = evt.containerData;
        }


        public List<Prop> GetContainerProps_B()
        {
            if (container_B is null || container_B.GetAllProps().Count <= 0) return new List<Prop>();
            return container_B.GetAllProps();
        }

        // public void UseProp_A()
        // {
        //     if (pcsc.GetBag_A().Count == 0) return;
        //     Prop prop = pcsc.GetBag_A()[propIndex_A.index];
        //     if (prop is IUsable buff)
        //     {
        //         buff.UseEffect(PropOwner.A);
        //         pcsc.RemovePropFromBagByIndex_A(propIndex_A.index);
        //         int count = pcsc.GetBag_A().Count;
        //         if (propIndex_A.index >= count && count > 0) propIndex_A.index = count - 1;
        //         RefreshBag_AEvent?.Invoke();
        //     }
        //     else if (prop is IWeapon weapon)
        //     {
        //         pcsc.Weapon_A = prop;
        //         weapon.AssignATK(PropOwner.A);
        //     }
        //     else if (prop is IDefend defend)
        //     {
        //         pcsc.Defense_A = prop;
        //         defend.AssignDEF(PropOwner.A);
        //     }
        // }
        //
        // public void UseProp_B()
        // {
        //     if (pcsc.GetBag_B().Count == 0) return;
        //     Prop prop = pcsc.GetBag_B()[propIndex_B.index];
        //     if (prop is IUsable buff)
        //     {
        //         buff.UseEffect(PropOwner.B);
        //         pcsc.RemovePropFromBagByIndex_B(propIndex_B.index);
        //         int count = pcsc.GetBag_B().Count;
        //         if (propIndex_B.index >= count && count > 0) propIndex_B.index = count - 1;
        //         RefreshBag_BEvent?.Invoke();
        //     }
        //     else if (prop is IWeapon weapon)
        //     {
        //         pcsc.Weapon_B = prop;
        //         weapon.AssignATK(PropOwner.B);
        //     }
        //     else if (prop is IDefend defend)
        //     {
        //         pcsc.Defense_B = prop;
        //         defend.AssignDEF(PropOwner.B);
        //     }
        // }

        private void ReplaceProp_A()
        {
        }

        private void ObtainContainer_A(GiveContainer_AEvent evt)
        {
            container_A = evt.containerData;
        }
        
        public List<Prop> GetContainerProps_A()
        {
            if (container_A is null || container_A.GetAllProps().Count <= 0) return new List<Prop>();
            return container_A.GetAllProps();
        }
    }
}
