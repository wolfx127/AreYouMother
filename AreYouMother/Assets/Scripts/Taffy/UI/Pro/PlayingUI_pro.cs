using System;
using System.Collections.Generic;
using Taffy.Data;
using Taffy.OverAllManager;
using Taffy.Play.Player;
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
        public PlayerCurrentStateController pcsc => PlayerCurrentStateController.Instance;
        /// <summary>
        /// 外部类使用时应仅用作事件注册
        /// </summary>
        public PlayingHandler_A handlerA => PlayingHandler_A.Instance;
        /// <summary>
        /// 外部类使用时应仅用作事件注册
        /// </summary>
        public PlayingHandler_B handlerB => PlayingHandler_B.Instance;

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
        public bool isBagClosed_A => handlerA.isBagClosed;
        public bool isBagClosed_B => handlerB.isBagClosed;

        public ContainerData container_A;
        public bool isContainerClosed_A => handlerA.isContainerClosed;
        public ContainerData container_B;
        public bool isContainerClosed_B => handlerB.isContainerClosed;
        

        public void Subscribe()
        {
            handlerA.ChoosePropArrowEvent += ObtainPropIndex_A;//上下左右输入->invoke->更新索引()
            handlerA.DiscardPropEvent += DiscardProp_A;//丢弃道具输入->invoke->丢弃道具()
            handlerA.ReplacePropEvent += ReplaceProp_A;//更换道具输入->invoke->更换道具()
            handlerA.CloseBagEvent += ResetIndex_A;//关闭查看背包输入->invoke->重置索引()
            EventBus.Subscribe<GiveContainer_AEvent>(ObtainContainer_A);//trigger碰撞(Enter返回other,Exit返回null)->获取箱子event->获取碰到的的箱子()
            handlerA.UsePropEvent += UseProp_A;

            handlerB.ChoosePropArrowEvent += ObtainPropIndex_B;//上下左右输入->invoke->更新索引()
            handlerB.DiscardPropEvent += DiscardProp_B;//丢弃道具输入->invoke->丢弃道具()
            handlerB.ReplacePropEvent += ReplaceProp_B;//更换道具输入->invoke->更换道具()
            handlerB.CloseBagEvent += ResetIndex_B;//关闭查看背包输入->invoke->重置索引()
            EventBus.Subscribe<GiveContainer_BEvent>(ObtainContainer_B);//trigger碰撞(Enter返回other,Exit返回null)->获取箱子event->获取碰到的的箱子()
            handlerB.UsePropEvent += UseProp_B;
        }
        public void Unsubscribe()
        {
            handlerA.ChoosePropArrowEvent -= ObtainPropIndex_A;
            handlerA.DiscardPropEvent -= DiscardProp_A;
            handlerA.ReplacePropEvent -= ReplaceProp_A;

            handlerB.ChoosePropArrowEvent -= ObtainPropIndex_B;
            handlerB.DiscardPropEvent -= DiscardProp_B;
            handlerB.ReplacePropEvent -= ReplaceProp_B;
        }
        

        /// <summary>
        /// 获取playerA的HPmax/HP  MPmax/MP的字符串
        /// </summary>
        /// <returns></returns>
        public string InfoNum_playerA()
        {
            return $"HP:{pcsc.GetCurHP_A()}/{pcsc.GetMaxHP_A()}"+'\n'+$"MP:{pcsc.GetCurMP_A()}/{pcsc.GetMaxMP_A()}";
        }

        /// <summary>
        /// 获取playerB的HPmax/HP  MPmax/MP的字符串
        /// </summary>
        /// <returns></returns>
        public string InfoNum_playerB()
        {
            return $"HP:{pcsc.GetCurHP_B()}/{pcsc.GetMaxHP_B()}"+'\n'+$"MP:{pcsc.GetCurMP_B()}/{pcsc.GetMaxMP_B()}";
        }

        public float HPPercent_A() => pcsc.GetHPPercent_A() * 100f;
        public float HPPercent_B() => pcsc.GetHPPercent_B() * 100f;
        public float MPPercent_A() => pcsc.GetMPPercent_A() * 100f;
        public float MPPercent_B() => pcsc.GetMPPercent_B() * 100f;

        public int GetBagCount_A() => pcsc.GetBag_A().Count;
        public int GetBagCount_B() => pcsc.GetBag_B().Count;
        
        public List<Prop> GetBag_A() => pcsc.GetBag_A();
        public List<Prop> GetBag_B() => pcsc.GetBag_B();

        /// <summary>
        /// playerA:pro类内闭包保存checking的索引，返回该索引代表的道具
        /// </summary>
        /// <returns></returns>
        public Prop GetCurrentProp_A()
        {
            if (propIndex_A.GetisInContainer()) return container_A?.GetPropByIndex(propIndex_A.index);
            return pcsc.GetBag_A()[propIndex_A.index];
        }
        /// <summary>
        /// playerB:pro类内闭包保存checking的索引，返回该索引代表的道具
        /// </summary>
        /// <returns></returns>
        public Prop GetCurrentProp_B()
        {
            if (propIndex_B.GetisInContainer())
                return container_B?.GetPropByIndex(propIndex_B.index);
            return pcsc.GetBag_B()[propIndex_B.index];
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
        public string GetCurrentPropDescribe_A()
        {
            return $"价值:{GetCurrentProp_A().value} | 数值:{GetCurrentProp_A().playingQuantity} | 消耗法力值:{GetCurrentProp_A().costMP} | {GetCurrentProp_A().rarity.ToLocalizedString()}" + '\n' +
                   GetCurrentProp_A().description;
        }
        /// <summary>
        /// playerB:获取道具描述字符串，包括价值、数值、稀有度
        /// </summary>
        /// <returns></returns> 
        public string GetCurrentPropDescribe_B()
        {
            return $"价值:{GetCurrentProp_B().value} | 数值:{GetCurrentProp_B().playingQuantity} | 消耗法力值:{GetCurrentProp_B().costMP} | {GetCurrentProp_B().rarity.ToLocalizedString()}" + '\n' +
                   GetCurrentProp_B().description;
        }

        public string GetBagInfo_A()
        {
            return $"背包上限/现存道具数:{pcsc.GetBagSize_A()}/{pcsc.GetBag_A().Count}";
        }
        public string GetBagInfo_B()
        {
            return $"背包上限/现存道具数:{pcsc.GetBagSize_B()}/{pcsc.GetBag_B().Count}";
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
            if (pcsc.GetBag_A().Count == 0) return;
            pcsc.DiscardPropByIndex_A(propIndex_A.index);
            int count = pcsc.GetBag_A().Count;
            if (count == 0) propIndex_A.index = 0;
            else if (propIndex_A.index >= count) propIndex_A.index = count - 1;
            prevPropIndex_A = propIndex_A;
            DiscardProp_AEvent?.Invoke();
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
            if (pcsc.GetBag_B().Count == 0) return;
            pcsc.DiscardPropByIndex_B(propIndex_B.index);
            int count = pcsc.GetBag_B().Count;
            if (count == 0) propIndex_B.index = 0;
            else if (propIndex_B.index >= count) propIndex_B.index = count - 1;
            prevPropIndex_B = propIndex_B;
            DiscardProp_BEvent?.Invoke();
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
            int count = pcsc.GetBag_A().Count;
            int bagCount = count;
            if (count == 0) return;
            if (!isContainerClosed_A) count += container_A.GetCount();
            Index ans = new Index(propIndex_A.index, propIndex_A.isInContainer);

            int cols = 5;
            int containerCount = (!isContainerClosed_A && container_A.GetCount() > 0)
                ? container_A.GetCount() : 0;
            int bagRows = (bagCount + cols - 1) / cols;

            if (dir == Vector2Int.left)
            {
                if (propIndex_A.index > 0)
                {
                    ans.index = propIndex_A.index - 1;
                }
                else if (!propIndex_A.isInContainer && containerCount > 0)
                {
                    ans.index = containerCount - 1;
                    ans.isInContainer = true;
                }
                else
                {
                    ans.index = bagCount - 1;
                    ans.isInContainer = false;
                }
            }
            else if (dir == Vector2Int.right)
            {
                int areaCount = propIndex_A.isInContainer ? containerCount : bagCount;
                if (propIndex_A.index < areaCount - 1)
                {
                    ans.index = propIndex_A.index + 1;
                }
                else if (!propIndex_A.isInContainer && containerCount > 0)
                {
                    ans.index = 0;
                    ans.isInContainer = true;
                }
                else
                {
                    ans.index = 0;
                    ans.isInContainer = false;
                }
            }
            else if (dir == Vector2Int.up || dir == Vector2Int.down)
            {
                int containerRows = (containerCount + cols - 1) / cols;
                int totalRows = bagRows + containerRows;
                int curRow = propIndex_A.isInContainer
                    ? bagRows + propIndex_A.index / cols
                    : propIndex_A.index / cols;
                int col = propIndex_A.index % cols;
                int step = dir == Vector2Int.up ? -1 : 1;

                int targetRow = curRow + step;
                int checked_ = 0;
                while (checked_ < totalRows - 1)
                {
                    if (targetRow < 0) targetRow = totalRows - 1;
                    else if (targetRow >= totalRows) targetRow = 0;

                    bool inBag = targetRow < bagRows;
                    int areaLocalIdx = targetRow * cols + col - (inBag ? 0 : bagRows * cols);
                    int countInArea = inBag ? bagCount : containerCount;

                    if (areaLocalIdx < countInArea)
                    {
                        ans.index = areaLocalIdx;
                        ans.isInContainer = !inBag;
                        break;
                    }
                    targetRow += step;
                    checked_++;
                }
            }
            SetPropIndex_A(ans);
        }
        
        private void ObtainPropIndex_B(Vector2Int dir)
        {
            int count = pcsc.GetBag_B().Count;
            int bagCount = count;
            if (count == 0) return;
            if (!isContainerClosed_B) count += container_B.GetCount();
            Index ans = new Index(propIndex_B.index, propIndex_B.isInContainer);

            int cols = 5;
            int containerCount = (!isContainerClosed_B && container_B.GetCount() > 0)
                ? container_B.GetCount() : 0;
            int bagRows = (bagCount + cols - 1) / cols;

            if (dir == Vector2Int.left)
            {
                if (propIndex_B.index > 0)
                {
                    ans.index = propIndex_B.index - 1;
                }
                else if (!propIndex_B.isInContainer && containerCount > 0)
                {
                    ans.index = containerCount - 1;
                    ans.isInContainer = true;
                }
                else
                {
                    ans.index = bagCount - 1;
                    ans.isInContainer = false;
                }
            }
            else if (dir == Vector2Int.right)
            {
                int areaCount = propIndex_B.isInContainer ? containerCount : bagCount;
                if (propIndex_B.index < areaCount - 1)
                {
                    ans.index = propIndex_B.index + 1;
                }
                else if (!propIndex_B.isInContainer && containerCount > 0)
                {
                    ans.index = 0;
                    ans.isInContainer = true;
                }
                else
                {
                    ans.index = 0;
                    ans.isInContainer = false;
                }
            }
            else if (dir == Vector2Int.up || dir == Vector2Int.down)
            {
                int containerRows = (containerCount + cols - 1) / cols;
                int totalRows = bagRows + containerRows;
                int curRow = propIndex_B.isInContainer
                    ? bagRows + propIndex_B.index / cols
                    : propIndex_B.index / cols;
                int col = propIndex_B.index % cols;
                int step = dir == Vector2Int.up ? -1 : 1;

                int targetRow = curRow + step;
                int checked_ = 0;
                while (checked_ < totalRows - 1)
                {
                    if (targetRow < 0) targetRow = totalRows - 1;
                    else if (targetRow >= totalRows) targetRow = 0;

                    bool inBag = targetRow < bagRows;
                    int areaLocalIdx = targetRow * cols + col - (inBag ? 0 : bagRows * cols);
                    int countInArea = inBag ? bagCount : containerCount;

                    if (areaLocalIdx < countInArea)
                    {
                        ans.index = areaLocalIdx;
                        ans.isInContainer = !inBag;
                        break;
                    }
                    targetRow += step;
                    checked_++;
                }
            }
            SetPropIndex_B(ans);
        }

        private void ReplaceProp_B()
        {
            if (pcsc.GetBag_B().Count == 0 && container_B.GetCount() == 0) return;
            Prop temp;
            if (propIndex_B.isInContainer)
            {
                if (GetBagCount_B() == pcsc.GetBagSize_B()) return;
                temp = container_B.GetPropByIndex(propIndex_B.index);
                container_B.RemovePropByIndex(propIndex_B.index);
                pcsc.AddPropToBag_B(temp);
                if (propIndex_B.index == 0)
                {
                    propIndex_B.index = pcsc.GetBag_B().Count-1;
                    propIndex_B.isInContainer = false;
                }
                if(propIndex_B.index >= container_B.GetCount()) propIndex_B.index =  container_B.GetCount() - 1;
            }
            else
            {
                if (container_B.GetCount() == container_B.length) return;
                temp = GetBag_B()[propIndex_B.index];
                pcsc.RemovePropFromBagByIndex_B(propIndex_B.index);
                container_B.AddProp(temp);
                if (propIndex_B.index == 0)
                {
                    propIndex_B.index = container_B.GetCount()-1;
                    propIndex_B.isInContainer = true;
                }
                if(propIndex_B.index >= GetBagCount_B()) propIndex_B.index = GetBagCount_B()-1;

                if (ReferenceEquals(temp, pcsc.Weapon_B))
                {
                    pcsc.Weapon_B = null;
                    pcsc.SetAtk_B(0);
                }
                if (ReferenceEquals(temp, pcsc.Defense_B))
                {
                    pcsc.Defense_B = null;
                    pcsc.SetDef_B(0);
                }
            }

            if(GetBagCount_B() == 0||container_B.GetCount() ==0 ) ResetIndex_B();
            prevPropIndex_B = propIndex_B;
            ReplaceProp_BEvent?.Invoke();
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

        public void UseProp_A()
        {
            if (pcsc.GetBag_A().Count == 0) return;
            Prop prop = pcsc.GetBag_A()[propIndex_A.index];
            if (prop is IUsable buff)
            {
                buff.UseEffect(PropOwner.A);
                pcsc.RemovePropFromBagByIndex_A(propIndex_A.index);
                int count = pcsc.GetBag_A().Count;
                if (propIndex_A.index >= count && count > 0) propIndex_A.index = count - 1;
                RefreshBag_AEvent?.Invoke();
            }
            else if (prop is IWeapon weapon)
            {
                pcsc.Weapon_A = prop;
                weapon.AssignATK(PropOwner.A);
            }
            else if (prop is IDefend defend)
            {
                pcsc.Defense_A = prop;
                defend.AssignDEF(PropOwner.A);
            }
        }

        public void UseProp_B()
        {
            if (pcsc.GetBag_B().Count == 0) return;
            Prop prop = pcsc.GetBag_B()[propIndex_B.index];
            if (prop is IUsable buff)
            {
                buff.UseEffect(PropOwner.B);
                pcsc.RemovePropFromBagByIndex_B(propIndex_B.index);
                int count = pcsc.GetBag_B().Count;
                if (propIndex_B.index >= count && count > 0) propIndex_B.index = count - 1;
                RefreshBag_BEvent?.Invoke();
            }
            else if (prop is IWeapon weapon)
            {
                pcsc.Weapon_B = prop;
                weapon.AssignATK(PropOwner.B);
            }
            else if (prop is IDefend defend)
            {
                pcsc.Defense_B = prop;
                defend.AssignDEF(PropOwner.B);
            }
        }

        private void ReplaceProp_A()
        {
            if (pcsc.GetBag_A().Count == 0 && container_A.GetCount() == 0) return;
            Prop temp;
            if (propIndex_A.isInContainer)
            {
                if (GetBagCount_A() == pcsc.GetBagSize_A()) return;
                temp = container_A.GetPropByIndex(propIndex_A.index);
                container_A.RemovePropByIndex(propIndex_A.index);
                pcsc.AddPropToBag_A(temp);
                if (propIndex_A.index == 0)
                {
                    propIndex_A.index = pcsc.GetBag_A().Count-1;
                    propIndex_A.isInContainer = false;
                }
                if(propIndex_A.index >= container_A.GetCount()) propIndex_A.index =  container_A.GetCount() - 1;
            }
            else
            {
                if (container_A.GetCount() == container_A.length) return;
                temp = GetBag_A()[propIndex_A.index];
                pcsc.RemovePropFromBagByIndex_A(propIndex_A.index);
                container_A.AddProp(temp);
                if (propIndex_A.index == 0)
                {
                    propIndex_A.index = container_A.GetCount()-1;
                    propIndex_A.isInContainer = true;
                }
                if(propIndex_A.index >= GetBagCount_A()) propIndex_A.index = GetBagCount_A()-1;

                if (ReferenceEquals(temp, pcsc.Weapon_A))
                {
                    pcsc.Weapon_A = null;
                    pcsc.SetAtk_A(0);
                }
                if (ReferenceEquals(temp, pcsc.Defense_A))
                {
                    pcsc.Defense_A = null;
                    pcsc.SetDef_A(0);
                }
            }
            
            if(GetBagCount_A() == 0||container_A.GetCount() ==0 ) ResetIndex_A();
            prevPropIndex_A = propIndex_A;
            ReplaceProp_AEvent?.Invoke();
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
