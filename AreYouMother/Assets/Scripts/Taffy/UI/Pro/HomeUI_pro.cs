using System;
using System.Collections;
using System.Collections.Generic;
using Taffy.Data;
using Taffy.Home;
using Taffy.OverAllManager;
using Taffy.Play.Player;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using EventBus = Taffy.OverAllManager.EventBus;

namespace Taffy.UI.Pro
{
    public enum Place {bagA,warehouse,dealer,bagB }

    public class HomeUI_pro
    {
        private OverAllPlayerController oapc;
        public HomeHandler homeHandler;
        public event Action CheckProp_AEvent;
        public event Action CheckProp_BEvent;
        public event Action RefreshBag_AEvent;
        public event Action RefreshBag_BEvent;
        public event Action RefreshWarehouseEvent;
        public event Action RefreshDealerEvent;
        public event Action UpdatePropertyEvent;
        public event Action UpdateRuleTipsEvent;
        
        //有关于索引
        #region 关于索引的字段和方法
        public int bagCols = 5;
        public int centerCols = 5;
        public int index_A = 0;
        public int prevIndex_A = 0;
        public int index_B = 0;
        public int prevIndex_B = 0;
        public int count_BagA => oapc.GetBag_A().Count;
        public int count_BagB => oapc.GetBag_B().Count;
        public int count_warehouse => WarehouseManager.GetWarehouse().Count;
        public int count_dealer => DealerManager.GetStore().Count;
        public Place centerPlace = Place.warehouse;
        public Place indexPlace_A = Place.bagA;
        public Place indexPlace_B = Place.bagB;
        public Place prevIndexPlace_A = Place.bagA;
        public Place prevIndexPlace_B = Place.bagB;
        public int count_A { get => GetCount_A(); }
        public int count_B { get => GetCount_B(); }
        private int GetCount_A()
        {
            if(indexPlace_A == Place.bagA) return count_BagA;
            if(indexPlace_A == Place.warehouse) return count_warehouse;
            if(indexPlace_A == Place.dealer) return count_dealer;
            return 0;
        }
        private int GetCount_B()
        {
            if(indexPlace_B == Place.bagB) return count_BagB;
            if(indexPlace_B == Place.warehouse) return count_warehouse;
            if(indexPlace_B == Place.dealer) return count_dealer;
            return 0;
        }
        private int GetCenterCount()
        {
            if (centerPlace == Place.warehouse) return count_warehouse;
            if (centerPlace == Place.dealer)    return count_dealer;
            return 0;
        }

        private void IndexRightOne_A()
        {
            index_A++;
            if (index_A >= count_A)
            {
                index_A = 0;
                if(indexPlace_A == Place.bagA) indexPlace_A = centerPlace;
                else if(indexPlace_A == centerPlace) indexPlace_A = Place.bagA; 
            }

            if (indexPlace_A == centerPlace && count_A <= 0) ResetIndexA();

            Debug.Log($"indexA从 {prevIndexPlace_A}{prevIndex_A} 移动到 {indexPlace_A}{index_A}");
        }
        private void IndexRightOne_B()
        {
            index_B++;
            if (index_B >= count_B)
            {
                index_B = 0;
                if (indexPlace_B == Place.bagB) indexPlace_B = centerPlace;
                else if (indexPlace_B == centerPlace) indexPlace_B = Place.bagB;
            }

            if (indexPlace_B == centerPlace && count_B <= 0) ResetIndexB();

            Debug.Log($"indexB从 {prevIndexPlace_B}{prevIndex_B} 移动到 {indexPlace_B}{index_B}");
        }

        private void IndexLeftOne_A()
        {
            index_A--;
            if (index_A < 0)
            {
                if (indexPlace_A == Place.bagA) indexPlace_A = centerPlace;
                else if (indexPlace_A == centerPlace) indexPlace_A = Place.bagA;
                index_A = count_A - 1;
                if (index_A < 0) index_A = 0;
            }

            if (indexPlace_A == centerPlace && count_A <= 0) ResetIndexA();

            Debug.Log($"indexA从 {prevIndexPlace_A}{prevIndex_A} 移动到 {indexPlace_A}{index_A}");
        }
        private void IndexLeftOne_B()
        {
            index_B--;
            if (index_B < 0)
            {
                if (indexPlace_B == Place.bagB) indexPlace_B = centerPlace;
                else if (indexPlace_B == centerPlace) indexPlace_B = Place.bagB;
                index_B = count_B - 1;
                if (index_B < 0) index_B = 0;
            }

            if (indexPlace_B == centerPlace && count_B <= 0) ResetIndexB();

            Debug.Log($"indexB从 {prevIndexPlace_B}{prevIndex_B} 移动到 {indexPlace_B}{index_B}");
        }

        private void IndexUpOne_A()
        {
            if (indexPlace_A == Place.bagA)
            {
                index_A -= bagCols;
                if (index_A < 0)
                {
                    index_A = ((index_A % bagCols) + bagCols) % bagCols;
                }
            }
            else
            {
                index_A -= centerCols;
                if (index_A < 0)
                {
                    index_A = ((index_A % centerCols) + centerCols) % centerCols;
                }
            }

            if (indexPlace_A == centerPlace && count_A <= 0) ResetIndexA();

            Debug.Log($"indexA从 {prevIndexPlace_A}{prevIndex_A} 移动到 {indexPlace_A}{index_A}");
        }
        private void IndexDownOne_A()
        {
            if (indexPlace_A == Place.bagA)
            {
                index_A += bagCols;
                if (index_A >= count_A)
                {
                    index_A %= bagCols;
                }
            }
            else
            {
                index_A += centerCols;
                if (index_A >= count_A)
                {
                    index_A %= centerCols;
                }
            }

            if (indexPlace_A == centerPlace && count_A <= 0) ResetIndexA();

            Debug.Log($"indexA从 {prevIndexPlace_A}{prevIndex_A} 移动到 {indexPlace_A}{index_A}");
        }
        private void IndexUpOne_B()
        {
            if (indexPlace_B == Place.bagB)
            {
                index_B -= bagCols;
                if (index_B < 0)
                {
                    index_B = ((index_B % bagCols) + bagCols) % bagCols;
                }
            }
            else
            {
                index_B -= centerCols;
                if (index_B < 0)
                {
                    index_B = ((index_B % centerCols) + centerCols) % centerCols;
                }
            }

            if (indexPlace_B == centerPlace && count_B <= 0) ResetIndexB();

            Debug.Log($"indexB从 {prevIndexPlace_B}{prevIndex_B} 移动到 {indexPlace_B}{index_B}");
        }
        private void IndexDownOne_B()
        {
            if (indexPlace_B == Place.bagB)
            {
                index_B += bagCols;
                if (index_B >= count_B)
                {
                    index_B %= bagCols;
                }
            }
            else
            {
                index_B += centerCols;
                if (index_B >= count_B)
                {
                    index_B %= centerCols;
                }
            }

            if (indexPlace_B == centerPlace && count_B <= 0) ResetIndexB();

            Debug.Log($"indexB从 {prevIndexPlace_B}{prevIndex_B} 移动到 {indexPlace_B}{index_B}");
        }


        public void KeepUpWithIndex_A()
        {
            prevIndex_A = index_A;
            prevIndexPlace_A = indexPlace_A;
        }
        public void KeepUpWithIndex_B()
        {
            prevIndex_B = index_B;
            prevIndexPlace_B = indexPlace_B;
        }

        public void ResetIndexA()
        {
            index_A = 0;
            indexPlace_A = Place.bagA;
        }
        public void ResetIndexB()
        {
            index_B = 0;
            indexPlace_B = Place.bagB;
        }
        public void ResetIndex()
        {
            ResetIndexA();
            ResetIndexB();
        }

        #endregion 
        ////////////////////////////////////////////
        
        public List<Prop> GetBag_A() =>  oapc.GetBag_A();
        public List<Prop> GetBag_B() =>  oapc.GetBag_B();
        public List<Prop> GetWarehouse() => WarehouseManager.GetWarehouse();
        public List<Prop> GetDealerStore() => DealerManager.GetStore();

        public Prop GetCheckingProp_A()
        {
            if (indexPlace_A == Place.bagA)
            { var bag = oapc.GetBag_A(); return index_A < bag.Count ? bag[index_A] : null; }
            else if (indexPlace_A == Place.warehouse)
            { var wh = WarehouseManager.GetWarehouse(); return index_A < wh.Count ? wh[index_A] : null; }
            else if (indexPlace_A == Place.dealer)
            { var store = DealerManager.GetStore(); return index_A < store.Count ? store[index_A] : null; }
            return null;
        }
        public Prop GetCheckingProp_B()
        {
            if (indexPlace_B == Place.bagB)
            { var bag = oapc.GetBag_B(); return index_B < bag.Count ? bag[index_B] : null; }
            else if (indexPlace_B == Place.warehouse)
            { var wh = WarehouseManager.GetWarehouse(); return index_B < wh.Count ? wh[index_B] : null; }
            else if (indexPlace_B == Place.dealer)
            { var store = DealerManager.GetStore(); return index_B < store.Count ? store[index_B] : null; }
            return null;
        }
        public string GetCheckingPropName_A()
        {
            return GetCheckingProp_A()?.name ?? "";
        }
        public string GetCheckingPropName_B()
        {
            return GetCheckingProp_B()?.name ?? "";
        }
        public string GetCheckingPropDescribe_A()
        {
            var p = GetCheckingProp_A();
            if (p == null) return "";
            return $"价值:{p.value} | 数值:{p.playingQuantity} | 消耗法力值:{p.costMP} | {p.rarity.ToLocalizedString()}" + '\n' + p.description;
        }
        public string GetCheckingPropDescribe_B()
        {
            var p = GetCheckingProp_B();
            if (p == null) return "";
            return $"价值:{p.value} | 数值:{p.playingQuantity} | 消耗法力值:{p.costMP} | {p.rarity.ToLocalizedString()}" + '\n' + p.description;
        }

        public string GetBagInfo_A()
        {
            return $"背包上限/现存道具数:{oapc.bagSize_A}/{oapc.GetBag_A().Count}";
        }
        public string GetBagInfo_B()
        {
            return $"背包上限/现存道具数:{oapc.bagSize_B}/{oapc.GetBag_B().Count}";
        }

        public void ChangeCenter()
        {
            if (centerPlace == Place.warehouse)
            {
                centerPlace = Place.dealer;
            }
            else if (centerPlace == Place.dealer)
            {
                centerPlace = Place.warehouse;
            }
            else return;
            
            ResetIndex();
            UpdateRuleTipsEvent?.Invoke();
        }

        public string GetStateInfo_A()
        {
            return $"HP上限:{oapc.maxHP_A}" + '\n' + $"MP上限:{oapc.maxMP_A}" + '\n' 
                   + $"当前攻击力:{oapc.ATK_A}  当前防御力:{oapc.DEF_A}";
        }
        public string GetStateInfo_B()
        {
            return $"HP上限:{oapc.maxHP_B}" + '\n' + $"MP上限:{oapc.maxMP_B}" + '\n' 
                   + $"当前攻击力:{oapc.ATK_B}  当前防御力:{oapc.DEF_B}";
        }

        public string GetRuleTips()
        {
            if (centerPlace == Place.warehouse)
            {
                return "这里是仓库，带出来的道具都可以放在这里，只能在背包里使用道具" + '\n' 
                                                        + "使用道具说明：首先它必须是能使用的道具。武器和护甲也可以使用，数值将用于战斗" + '\n'
                                                        + "玩家无法从仓库拿取不属于自己的道具" + '\n'
                                                        + "玩家1按'F'交换仓库道具，按'Z'使用道具" + '\n' 
                                                        + "玩家2按'小键盘0'交换仓库道具,按'小键盘9'使用道具";
            }
            else if (centerPlace == Place.dealer)
            {
                return "这里是商人，可以用总资产和他交易物品，好感度越高，卖的品质越高。买卖一次成交概不退货" + '\n' 
                                                                    + "玩家无法购买不属于自己的道具" + '\n'
                                                                    + "玩家1按'F'买卖道具" + '\n'
                                                                    + "玩家2按'小键盘0'买卖道具";
            }

            return "";
        }

        /// ////////////////////////////////////////

        public void Subscribe()
        {
            oapc = OverAllPlayerController.Instance;
            homeHandler = HomeHandler.Instance;
            homeHandler.ReplaceProp_AEvent += Replace_A;
            homeHandler.ReplaceProp_BEvent += Replace_B;
            homeHandler.ChooseProp_AEvent += ChangeIndex_A;
            homeHandler.ChooseProp_BEvent += ChangeIndex_B;
            homeHandler.UseProp_AEvent += UseProp_A;
            homeHandler.UseProp_BEvent += UseProp_B;
        }

        public void Unsubscribe()
        {
            homeHandler.ReplaceProp_AEvent -= Replace_A;
            homeHandler.ReplaceProp_BEvent -= Replace_B;
            homeHandler.ChooseProp_AEvent -= ChangeIndex_A;
            homeHandler.ChooseProp_BEvent -= ChangeIndex_B;
            homeHandler.UseProp_AEvent -= UseProp_A;
            homeHandler.UseProp_BEvent -= UseProp_B;
        }

        public void ChangeSceneToPlaying()
        {
            EventBus.Publish(new ChangeSceneHomeToPlayingEvent());
        }
        
        public void ExitGame()
        {
            EventBus.Publish(new ExitGameEvent());
            JsonData.SaveWarehouse();
            Application.Quit();
            Debug.Log("退出游戏");
        }

        public void ChangeIndex_A(Vector2Int vec)
        {
            if(vec == Vector2Int.up) IndexUpOne_A();
            else if(vec == Vector2Int.down) IndexDownOne_A();
            else if(vec == Vector2Int.left) IndexLeftOne_A();
            else if(vec == Vector2Int.right) IndexRightOne_A();
            CheckProp_AEvent?.Invoke();
        }

        public void ChangeIndex_B(Vector2Int vec)
        {
            if(vec == Vector2Int.up) IndexUpOne_B();
            else if(vec == Vector2Int.down) IndexDownOne_B();
            else if(vec == Vector2Int.left) IndexLeftOne_B();
            else if(vec == Vector2Int.right) IndexRightOne_B();
            CheckProp_BEvent?.Invoke();
        }

        public void Replace_A()
        {
            if (indexPlace_A == Place.bagA)//从背包向外交换
            {
                Prop temp = oapc.GetBag_A()[index_A];
                if (centerPlace == Place.warehouse)//存
                {
                    oapc.RemovePropByIndex_A(index_A);
                    WarehouseManager.AddProp(temp);
                    IndexLeftOne_A();
                    RefreshWarehouseEvent?.Invoke();
                }
                else if (centerPlace == Place.dealer)//卖
                {
                    oapc.RemovePropByIndex_A(index_A);
                    WarehouseManager.AddProperty(temp.value);
                    UpdatePropertyEvent?.Invoke();
                    IndexLeftOne_A();
                    RefreshDealerEvent?.Invoke();
                }

                if (ReferenceEquals(temp, oapc.tempWeapon_A))
                {
                    oapc.tempWeapon_A = null;
                    oapc.ATK_A = 0;
                }
                if (ReferenceEquals(temp, oapc.tempDefense_A))
                {
                    oapc.tempDefense_A = null;
                    oapc.DEF_A = 0;
                }
            }
            else if (indexPlace_A == Place.warehouse)//取
            {
                Prop temp = WarehouseManager.GetPropByIndex(index_A);
                if (temp.owner == PropOwner.B) return;
                WarehouseManager.RemovePropByIndex(index_A);
                oapc.AddProp_A(temp);
                IndexLeftOne_A();
                RefreshWarehouseEvent?.Invoke();
            }
            else if (indexPlace_A == Place.dealer)//买
            {
                Prop temp = DealerManager.GetStore()[index_A];
                if (temp.owner == PropOwner.B) return;
                if (!WarehouseManager.CanMinusProperty(temp.value)) return;
                DealerManager.RemoveStoreByIndex(index_A);
                oapc.AddProp_A(temp);
                WarehouseManager.MinusProperty(temp.value);
                UpdatePropertyEvent?.Invoke();
                IndexLeftOne_A();
                RefreshDealerEvent?.Invoke();
            }
            RefreshBag_AEvent?.Invoke();
        }

        public void Replace_B()
        {
            if (indexPlace_B == Place.bagB)
            {
                Prop temp = oapc.GetBag_B()[index_B];
                if (centerPlace == Place.warehouse)
                {
                    oapc.RemovePropByIndex_B(index_B);
                    WarehouseManager.AddProp(temp);
                    IndexLeftOne_B();
                    RefreshWarehouseEvent?.Invoke();
                }
                else if (centerPlace == Place.dealer)
                {
                    oapc.RemovePropByIndex_B(index_B);
                    WarehouseManager.AddProperty(temp.value);
                    UpdatePropertyEvent?.Invoke();
                    IndexLeftOne_B();
                    RefreshDealerEvent?.Invoke();
                }
                if (ReferenceEquals(temp, oapc.tempWeapon_B))
                {
                    oapc.tempWeapon_B = null;
                    oapc.ATK_B = 0;
                }
                if (ReferenceEquals(temp, oapc.tempDefense_B))
                {
                    oapc.tempDefense_B = null;
                    oapc.DEF_B = 0;
                }
            }
            else if (indexPlace_B == Place.warehouse)
            {
                Prop temp = WarehouseManager.GetPropByIndex(index_B);
                if (temp.owner == PropOwner.A) return;
                WarehouseManager.RemovePropByIndex(index_B);
                oapc.AddProp_B(temp);
                IndexLeftOne_B();
                RefreshWarehouseEvent?.Invoke();
            }
            else if (indexPlace_B == Place.dealer)
            {
                Prop temp = DealerManager.GetStore()[index_B];
                if (temp.owner == PropOwner.A) return;
                if (!WarehouseManager.CanMinusProperty(temp.value)) return;
                DealerManager.RemoveStoreByIndex(index_B);
                oapc.AddProp_B(temp);
                WarehouseManager.MinusProperty(temp.value);
                UpdatePropertyEvent?.Invoke();
                IndexLeftOne_B();
                RefreshDealerEvent?.Invoke();
            }
            RefreshBag_BEvent?.Invoke();
        }

        public string PropertyDescribe()
        {
            return $"总资产: {WarehouseManager.property}";
        }

        public void UseProp_A()
        {
            if (indexPlace_A == Place.bagA)
            {
                Prop prop = oapc.GetBag_A()[index_A];
                if (prop is ICultivate cultivate)
                {
                    cultivate.BonusEffect(PropOwner.A);

                    oapc.RemovePropByIndex_A(index_A);
                    KeepUpWithIndex_A();
                    RefreshBag_AEvent?.Invoke();
                }
                else if (prop is IWeapon weapon)
                {
                    weapon.AssignATK(PropOwner.A);
                }
                else if (prop is IDefend defend)
                {
                    defend.AssignDEF(PropOwner.A);
                    oapc.tempDefense_A = prop;
                }
            }
        }
        public void UseProp_B()
        {
            if (indexPlace_B == Place.bagB)
            {
                Prop prop = oapc.GetBag_B()[index_B];
                if (prop is ICultivate cultivate)
                {
                    cultivate.BonusEffect(PropOwner.B);

                    oapc.RemovePropByIndex_B(index_B);
                    KeepUpWithIndex_B();
                    RefreshBag_BEvent?.Invoke();
                }
                else if (prop is IWeapon weapon)
                {
                    weapon.AssignATK(PropOwner.B);
                    oapc.tempWeapon_B = prop;
                }
                else if (prop is IDefend defend)
                {
                    defend.AssignDEF(PropOwner.B);
                    oapc.tempDefense_B = prop;
                }
            }
        }
    }
}
