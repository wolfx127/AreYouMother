using System;
using System.Collections.Generic;
using Taffy.Data.PropData;
using Taffy.OverAllManager;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Taffy.Home
{
    public enum HomeIndexPlace
    {
        BagA,
        BagB,
        Warehouse,
        Dealer
    }

    public class HomeHandler:MonoBehaviour
    {
        private PlayingInputAction playerInputAction;
        public static HomeHandler Instance { get; private set; }
        public int index_A = 0;
        public int index_B = 0;
        public HomeIndexPlace place_A = HomeIndexPlace.BagA;
        public HomeIndexPlace place_B = HomeIndexPlace.BagB;
        
        private OverAllPlayerController oapc = OverAllPlayerController.Instance;
        
        public event Action ChooseProp_AEvent;
        public event Action ChooseProp_BEvent;
        public event Action ReplacePropEvent;
        public event Action UpdatePropertyEvent;
        public event Action Delete_AEvent;
        public event Action Delete_BEvent;
        public event Action UsingProp_AEvent;
        public event Action UsingProp_BEvent;
        public event Action UpdateState_AEvent;
        public event Action UpdateState_BEvent;

        private void Awake()
        {
            Instance = this;
            playerInputAction = new PlayingInputAction();
            Debug.Log($"[初始化] HomeHandler.Awake, 所在场景:{gameObject.scene.name}, Instance置好:{(Instance == this)}, playerInputAction:{(playerInputAction == null ? "null" : "OK")}");
        }

        private void OnEnable()
        {
            playerInputAction.PlayerA.Enable();
            playerInputAction.PlayerB.Enable();
            
            playerInputAction.PlayerA.ChooseProp.Enable();
            playerInputAction.PlayerA.ReplaceProp.Enable();
            playerInputAction.PlayerA.UseProp.Enable();
            playerInputAction.PlayerA.SwitchIndex.Enable();
            playerInputAction.PlayerA.ChooseProp.performed += ChooseProp_A;
            playerInputAction.PlayerA.ReplaceProp.performed += ReplaceProp_A;
            playerInputAction.PlayerA.UseProp.performed += UseProp_A;
            playerInputAction.PlayerA.SwitchIndex.performed += SwitchIndex_A;
            
            playerInputAction.PlayerB.ChooseProp.Enable();
            playerInputAction.PlayerB.ReplaceProp.Enable();
            playerInputAction.PlayerB.UseProp.Enable();
            playerInputAction.PlayerB.SwitchIndex.Enable();
            playerInputAction.PlayerB.ChooseProp.performed += ChooseProp_B;
            playerInputAction.PlayerB.ReplaceProp.performed += ReplaceProp_B;
            playerInputAction.PlayerB.UseProp.performed += UseProp_B;
            playerInputAction.PlayerB.SwitchIndex.performed += SwitchIndex_B;
            
            EventBus.Subscribe<ChangeSceneHomeToPlayingEvent>(DisposeInputAction);
            
            ResetIndex();
        }

        private void OnDisable()
        {

            EventBus.Unsubscribe<ChangeSceneHomeToPlayingEvent>(DisposeInputAction);
            playerInputAction.PlayerA.Disable();
            playerInputAction.PlayerB.Disable();
            
            playerInputAction.PlayerA.ChooseProp.performed -= ChooseProp_A;
            playerInputAction.PlayerA.ReplaceProp.performed -= ReplaceProp_A;
            playerInputAction.PlayerA.UseProp.performed -= UseProp_A;
            playerInputAction.PlayerA.SwitchIndex.performed -= SwitchIndex_A;
            playerInputAction.PlayerA.ChooseProp.Disable();
            playerInputAction.PlayerA.ReplaceProp.Disable();
            playerInputAction.PlayerA.UseProp.Disable();
            playerInputAction.PlayerA.SwitchIndex.Disable();
            
            playerInputAction.PlayerB.ChooseProp.performed -= ChooseProp_B;
            playerInputAction.PlayerB.ReplaceProp.performed -= ReplaceProp_B;
            playerInputAction.PlayerB.UseProp.performed -= UseProp_B;
            playerInputAction.PlayerB.SwitchIndex.performed -= SwitchIndex_B;
            playerInputAction.PlayerB.ChooseProp.Disable();
            playerInputAction.PlayerB.ReplaceProp.Disable();
            playerInputAction.PlayerB.UseProp.Disable();
            playerInputAction.PlayerB.SwitchIndex.Disable();
        }
        
/////////// 维护选中的索引 与 变换索引 /////////////////////////////////////////////////////////////
#region        

        private void ChooseProp_A(InputAction.CallbackContext ctx)
        {
            if (index_A == -1) return;
            Vector2 v = ctx.ReadValue<Vector2>();
            int count = GetMaxIndex_A();
            
            var n = UITools.GetNeighbors(index_A, count);
            if (v.x == 0 && v.y > 0)
            {
                index_A = n.up;
            }
            else if (v.x == 0 && v.y < 0)
            {
                index_A = n.down;
            }
            else if (v.x < 0 && v.y == 0)
            {
                index_A = n.left;
            }
            else if (v.x > 0 && v.y == 0)
            {
                index_A = n.right;
            }
            
            ChooseProp_AEvent?.Invoke();
            Debug.Log($"更换A索引至{place_A}{index_A}");
        }
        
        private void SwitchIndex_A(InputAction.CallbackContext ctx)
        {
            if (place_A != HomeIndexPlace.BagA)
            {
                if(oapc.GetBag_A().Count == 0)
                {
                    Debug.Log("A跳转背包失败，背包是空的");
                    return;
                }
                place_A = HomeIndexPlace.BagA;
                index_A = 0;
            }
            else if (OverAllStates.isInWarehouse)
            {
                if(WarehouseManager.GetWarehouseCount() == 0)
                {
                    Debug.Log("A跳转仓库失败，仓库是空的");
                    return;
                }
                place_A = HomeIndexPlace.Warehouse;
                index_A = 0;
            }
            else if (OverAllStates.isInDealer)
            {
                if(DealerManager.store.Count == 0)
                {
                    Debug.Log("A跳转商店失败，商店是空的");
                    return;
                }
                place_A = HomeIndexPlace.Dealer;
                index_A = 0;
            }
            else Debug.Log("A跳转道具栏失败");

            ChooseProp_AEvent?.Invoke();
            Debug.Log("A跳转道具栏成功");
        }
        
        private int GetMaxIndex_A()
        {
            if(place_A == HomeIndexPlace.BagA) return oapc.GetBag_A().Count;
            else if (OverAllStates.isInWarehouse && place_A == HomeIndexPlace.Warehouse)
                return WarehouseManager.GetWarehouseCount();
            else if (OverAllStates.isInDealer && place_A == HomeIndexPlace.Dealer)
                return DealerManager.store.Count;
            return 0;
        }

        
        private void ChooseProp_B(InputAction.CallbackContext ctx)
        {
            if (index_B == -1) return;
            Vector2 v = ctx.ReadValue<Vector2>();
            int count = GetMaxIndex_B();
            
            var n = UITools.GetNeighbors(index_B, count);
            if (v.x == 0 && v.y > 0)
            {
                index_B = n.up;
            }
            else if (v.x == 0 && v.y < 0)
            {
                index_B = n.down;
            }
            else if (v.x < 0 && v.y == 0)
            {
                index_B = n.left;
            }
            else if (v.x > 0 && v.y == 0)
            {
                index_B = n.right;
            }

            ChooseProp_BEvent?.Invoke();
            Debug.Log($"更换B索引至{place_B}{index_B}");
        }

        private void SwitchIndex_B(InputAction.CallbackContext ctx)
        {
            if (place_B != HomeIndexPlace.BagB)
            {
                if (oapc.GetBag_B().Count == 0)
                {
                    Debug.Log("B跳转背包失败，背包是空的");
                    return;
                }
                place_B = HomeIndexPlace.BagB;
                index_B = 0;
            }
            else if (OverAllStates.isInWarehouse)
            {
                if (WarehouseManager.GetWarehouseCount() == 0)
                {
                    Debug.Log("B跳转仓库失败，仓库是空的");
                    return;
                }
                place_B = HomeIndexPlace.Warehouse;
                index_B = 0;
            }
            else if (OverAllStates.isInDealer)
            {
                if (DealerManager.store.Count == 0)
                {
                    Debug.Log("B跳转商店失败，商店是空的");
                    return;
                }
                place_B = HomeIndexPlace.Dealer;
                index_B = 0;
            }
            else Debug.Log("B跳转道具栏失败");

            ChooseProp_BEvent?.Invoke();
            Debug.Log("B跳转道具栏成功");
        }
        
        private int GetMaxIndex_B()
        {
            if(place_B == HomeIndexPlace.BagB) return oapc.GetBag_B().Count;
            else if (OverAllStates.isInWarehouse && place_B == HomeIndexPlace.Warehouse)
                return WarehouseManager.GetWarehouseCount();
            else if (OverAllStates.isInDealer && place_B == HomeIndexPlace.Dealer)
                return DealerManager.store.Count;
            return 0;
        }


        public void ResetIndex()
        {
            index_A = 0;
            index_B = 0;
            if(oapc.GetBag_A().Count == 0)
            {
                if (OverAllStates.isInWarehouse)
                    place_A = WarehouseManager.GetWarehouseCount() == 0 ? HomeIndexPlace.BagA : HomeIndexPlace.Warehouse;
                
                else if (OverAllStates.isInDealer)
                    place_A = DealerManager.store.Count == 0 ? HomeIndexPlace.BagA : HomeIndexPlace.Dealer;
            }
            else place_A = HomeIndexPlace.BagA;
            
            
            if(oapc.GetBag_B().Count == 0)
            {
                if (OverAllStates.isInWarehouse)
                {
                    place_B = WarehouseManager.GetWarehouseCount() == 0 ? HomeIndexPlace.BagB : HomeIndexPlace.Warehouse;
                }
                else if (OverAllStates.isInDealer)
                {
                    place_B = DealerManager.store.Count == 0 ? HomeIndexPlace.BagB : HomeIndexPlace.Dealer;
                }
            }
            else place_B = HomeIndexPlace.BagB;
            
            ChooseProp_AEvent?.Invoke();
            ChooseProp_BEvent?.Invoke();
        }
#endregion

/////////// 获取 与 检测是否已使用 与 使用选中的道具  //////////////////////////////////////////////////////////
#region
        private Prop GetChoosePropA()
        {
            if (index_A == -1) return null;
            if (place_A == HomeIndexPlace.BagA)
            {
                return oapc.GetBag_A()[index_A];
            }
            if (place_A == HomeIndexPlace.Warehouse)
            {
                return WarehouseManager.GetWarehouse()[index_A];
            } 
            if (place_A == HomeIndexPlace.Dealer)
            {
                return DealerManager.store[index_A];
            }
            return null;
        }
        private Prop GetChoosePropB()
        {
            if (index_B == -1) return null;
            if (place_B == HomeIndexPlace.BagB)
            {
                return oapc.GetBag_B()[index_B];
            }
            if (place_B == HomeIndexPlace.Warehouse)
            {
                return WarehouseManager.GetWarehouse()[index_B];
            }
            if (place_B == HomeIndexPlace.Dealer)
            {
                return DealerManager.store[index_B];
            }
            return null;
        }

        private void UseProp_A(InputAction.CallbackContext ctx)
        {
            if (place_A != HomeIndexPlace.BagA) return;
            Prop prop = GetChoosePropA();
            if (prop == null) return;

            bool shouldDelete = prop.Execute(prop,'A');
            
            if (shouldDelete)
            {
                oapc.RemovePropAt_A(index_A);
                if (oapc.GetBag_A().Count == 0)
                {
                    if (OverAllStates.isInWarehouse && WarehouseManager.GetWarehouseCount() != 0)
                        place_A = HomeIndexPlace.Warehouse;
                    else if (OverAllStates.isInDealer && DealerManager.store.Count != 0)
                        place_A = HomeIndexPlace.Dealer;
                    index_A = 0;
                }
                else index_A--;
                Delete_AEvent?.Invoke();
            }
            else
            {
                bool OK = false;
                foreach (var b in prop.behavior_value)
                {
                    if (b.type == PropType.Remote_Attack)
                    {
                        if (prop.Equals(oapc.tempWeapon_A)) break;
                        oapc.tempWeapon_A = prop;
                        OK = true;
                        Debug.Log($"已确认当前A武器为 {prop.name}");
                    }
                    else if (b.type == PropType.Defend)
                    {
                        if (prop.Equals(oapc.tempDefense_A)) break;
                        oapc.tempDefense_A = prop;
                        OK = true;
                        Debug.Log($"已确认当前A防具为 {prop.name}");
                    }
                }
                if(OK) UsingProp_AEvent?.Invoke();
            }

            Debug.Log($"[输入] A使用了一个道具: place={place_A}, index={index_A}");
        }
        
        private void UseProp_B(InputAction.CallbackContext ctx)
        {
            if (place_B != HomeIndexPlace.BagB) return;
            Prop prop = GetChoosePropB();
            if (prop == null) return;

            bool shouldDelete = prop.Execute(prop, 'B');

            if (shouldDelete)
            {
                oapc.RemovePropAt_B(index_B);
                if (oapc.GetBag_B().Count == 0)
                {
                    if (OverAllStates.isInWarehouse && WarehouseManager.GetWarehouseCount() != 0)
                        place_B = HomeIndexPlace.Warehouse;
                    else if (OverAllStates.isInDealer && DealerManager.store.Count != 0)
                        place_B = HomeIndexPlace.Dealer;
                    index_B = 0;
                }
                else index_B--;
                Delete_BEvent?.Invoke();
            }
            else
            {
                bool OK = false;
                foreach (var b in prop.behavior_value)
                {
                    if (b.type == PropType.Close_Attack)
                    {
                        if (prop.Equals(oapc.tempWeapon_B)) break;
                        oapc.tempWeapon_B = prop;
                        OK = true;
                        Debug.Log($"已确认当前B武器为 {prop.name}");
                    }
                    else if (b.type == PropType.Defend)
                    {
                        if (prop.Equals(oapc.tempDefense_B)) break;
                        oapc.tempDefense_B = prop;
                        OK = true;
                        Debug.Log($"已确认当前B防具为 {prop.name}");
                    }
                }
                if(OK) UsingProp_BEvent?.Invoke();
            }

            Debug.Log($"[输入] B使用了一个道具: place={place_B}, index={index_B}");
        }

        public bool isUsing_A(int index, HomeIndexPlace place)
        {
            if (place != HomeIndexPlace.BagA) return false;
            if (index < 0 || index >= oapc.GetBag_A().Count) return false;
            Prop p = oapc.GetBag_A()[index];
            return p != null && (p.Equals(oapc.tempWeapon_A) || p.Equals(oapc.tempDefense_A));
        }
        
        public bool isUsing_B(int index, HomeIndexPlace place)
        {
            if (place != HomeIndexPlace.BagB) return false;
            if (index < 0 || index >= oapc.GetBag_B().Count) return false;
            Prop p = oapc.GetBag_B()[index];
            return p != null && (p.Equals(oapc.tempWeapon_B) || p.Equals(oapc.tempDefense_B));
        }

        #endregion
        
/////////// 交换、卖物品 ///////////////////////////////////////////////////////////////////////        
#region
        private void ReplaceProp_A(InputAction.CallbackContext ctx)
        {
            if (index_A < 0) return;
            bool OK = false;
        //从背包放进仓库
            if(place_A == HomeIndexPlace.BagA && OverAllStates.isInWarehouse)
            {
                Prop temp = oapc.GetBag_A()[index_A];
                oapc.RemovePropAt_A(index_A);
                WarehouseManager.AddProp(temp);
                if (oapc.GetBag_A().Count == 0)
                {
                    index_A = 0;
                    place_A = HomeIndexPlace.Warehouse;
                }
                else if (oapc.GetBag_A().Count <= index_A)
                {
                    index_A = oapc.GetBag_A().Count-1;
                }

                if (temp.Equals(oapc.tempWeapon_A))
                {
                    oapc.tempWeapon_A = null;
                    oapc.ATK_A = 0;
                    UpdateState_AEvent?.Invoke();
                }

                if (temp.Equals(oapc.tempDefense_A))
                {
                    oapc.tempDefense_A = null;
                    oapc.DEF_A = 0;
                    UpdateState_AEvent?.Invoke();
                }

                OK = true;
            }
        //从背包卖到商人
            else if (place_A == HomeIndexPlace.BagA && OverAllStates.isInDealer)
            {
                Prop temp = oapc.GetBag_A()[index_A];
                int price = temp.price;
                oapc.RemovePropAt_A(index_A);
                WarehouseManager.AddProperty(price);
                if (oapc.GetBag_A().Count == 0)
                {
                    index_A = 0;
                    place_A = HomeIndexPlace.Dealer;
                }
                else if (oapc.GetBag_A().Count <= index_A)
                {
                    index_A = oapc.GetBag_A().Count-1;
                }
                
                if (temp.Equals(oapc.tempWeapon_A))
                {
                    oapc.tempWeapon_A = null;
                    oapc.ATK_A = 0;
                    UpdateState_AEvent?.Invoke();
                }
                if (temp.Equals(oapc.tempDefense_A))
                {
                    oapc.tempDefense_A = null;
                    oapc.DEF_A = 0;
                    UpdateState_AEvent?.Invoke();
                }
                
                OK = true;
            }
        //从仓库放到背包
            else if (place_A == HomeIndexPlace.Warehouse)
            {
                if (oapc.isBagFull_A()) return;
                Prop temp = WarehouseManager.GetWarehouse()[index_A];
                WarehouseManager.RemovePropByIndex(index_A);
                oapc.AddProp_A(temp);
                if (WarehouseManager.GetWarehouseCount() == 0)
                {
                    index_A = 0;
                    place_A = HomeIndexPlace.BagA;
                    if (place_B == HomeIndexPlace.Warehouse)
                    {
                        index_B = 0;
                        place_B = HomeIndexPlace.BagB;
                    }
                }
                else if (WarehouseManager.GetWarehouseCount() <= index_A)
                {
                    index_A = WarehouseManager.GetWarehouseCount()-1;
                }

                if (WarehouseManager.GetWarehouse().Count != 0 && place_B == HomeIndexPlace.Warehouse && WarehouseManager.GetWarehouse().Count <= index_B)
                {
                    index_B = WarehouseManager.GetWarehouse().Count-1;
                }
                
                OK = true;
            }
        //从商人买进背包
            else if (place_A == HomeIndexPlace.Dealer)
            {
                if (oapc.isBagFull_A()) return;
                Prop temp = DealerManager.BuyProp(index_A);
                if (temp != null)
                {
                    oapc.AddProp_A(temp);
                    if (DealerManager.store.Count == 0)
                    {
                        index_A = 0;
                        place_A = HomeIndexPlace.BagA;
                        if (place_B == HomeIndexPlace.Dealer)
                        {
                            index_B = 0;
                            place_B = HomeIndexPlace.BagB;
                        }
                    }
                    else if (DealerManager.store.Count <= index_A)
                    {
                        index_A = DealerManager.store.Count-1;
                    }
                    if (DealerManager.store.Count != 0 && place_B == HomeIndexPlace.Dealer && DealerManager.store.Count <= index_B)
                    {
                        index_B = DealerManager.store.Count-1;
                    }
                    
                    
                    OK = true;
                }
            }

            if (OK)
            {
                ReplacePropEvent?.Invoke();
                Debug.Log($"换道具位置A  place:{place_A} ; index:{index_A}");
            }
        }

        private void ReplaceProp_B(InputAction.CallbackContext ctx)
        {
            if (index_B < 0) return;
            bool OK = false;
            if(place_B == HomeIndexPlace.BagB && OverAllStates.isInWarehouse)
            {
                Prop temp = oapc.GetBag_B()[index_B];
                oapc.RemovePropAt_B(index_B);
                WarehouseManager.AddProp(temp);
                if (oapc.GetBag_B().Count == 0)
                {
                    index_B = 0;
                    place_B = HomeIndexPlace.Warehouse;
                }
                else if (oapc.GetBag_B().Count <= index_B)
                {
                    index_B = oapc.GetBag_B().Count-1;
                }
                
                if (temp.Equals(oapc.tempWeapon_B))
                {
                    oapc.tempWeapon_B = null;
                    oapc.ATK_B = 0;
                    UpdateState_BEvent?.Invoke();
                }
                if (temp.Equals(oapc.tempDefense_B))
                {
                    oapc.tempDefense_B = null;
                    oapc.DEF_B = 0;
                    UpdateState_BEvent?.Invoke();
                }
                
                OK = true;
            }
            else if (place_B == HomeIndexPlace.BagB && OverAllStates.isInDealer)
            {
                Prop temp = oapc.GetBag_B()[index_B];
                int price = temp.price;
                oapc.RemovePropAt_B(index_B);
                WarehouseManager.AddProperty(price);
                if (oapc.GetBag_B().Count == 0)
                {
                    index_B = 0;
                    place_B = HomeIndexPlace.Dealer;
                }
                else if (oapc.GetBag_B().Count <= index_B)
                {
                    index_B = oapc.GetBag_B().Count-1;
                }
                
                if (temp.Equals(oapc.tempWeapon_B))
                {
                    oapc.tempWeapon_B = null;
                    oapc.ATK_B = 0;
                    UpdateState_BEvent?.Invoke();
                }
                if (temp.Equals(oapc.tempDefense_B))
                {
                    oapc.tempDefense_B = null;
                    oapc.DEF_B = 0;
                    UpdateState_BEvent?.Invoke();
                }
                
                OK = true;
            }
            else if (place_B == HomeIndexPlace.Warehouse)
            {
                if (oapc.isBagFull_B()) return;
                Prop temp = WarehouseManager.GetWarehouse()[index_B];
                WarehouseManager.RemovePropByIndex(index_B);
                oapc.AddProp_B(temp);
                if (WarehouseManager.GetWarehouseCount() == 0)
                {
                    index_B = 0;
                    place_B = HomeIndexPlace.BagB;
                    if (place_A == HomeIndexPlace.Warehouse)
                    {
                        index_A = 0;
                        place_A = HomeIndexPlace.BagA;
                    }
                }
                else if (WarehouseManager.GetWarehouseCount() <= index_B)
                {
                    index_B = WarehouseManager.GetWarehouseCount()-1;
                }

                if (WarehouseManager.GetWarehouse().Count != 0 && place_A == HomeIndexPlace.Warehouse && WarehouseManager.GetWarehouse().Count <= index_A)
                {
                    index_A = WarehouseManager.GetWarehouse().Count-1;
                }
                OK = true;
            }
            else if (place_B == HomeIndexPlace.Dealer)
            {
                if (oapc.isBagFull_B()) return;
                Prop temp = DealerManager.BuyProp(index_B);
                if (temp != null)
                {
                    oapc.AddProp_B(temp);
                    if (DealerManager.store.Count == 0)
                    {
                        index_B = 0;
                        place_B = HomeIndexPlace.BagB;
                        if (place_A == HomeIndexPlace.Dealer)
                        {
                            index_A = 0;
                            place_A = HomeIndexPlace.BagA;
                        }
                    }
                    else if (DealerManager.store.Count <= index_B)
                    {
                        index_B = DealerManager.store.Count-1;
                    }
                    if (DealerManager.store.Count != 0 && place_A == HomeIndexPlace.Dealer && DealerManager.store.Count <= index_A)
                    {
                        index_A = DealerManager.store.Count-1;
                    }
                    OK = true;
                }
            }

            if (OK)
            {
                ReplacePropEvent?.Invoke();
                Debug.Log($"换道具位置B  place:{place_B} ; index:{index_B}");
            }
        }
        
#endregion

        
        private void DisposeInputAction(ChangeSceneHomeToPlayingEvent evt)
        {
            playerInputAction.Dispose();
        }

        private void OnDestroy()
        {
            Debug.Log("[销毁] HomeHandler 被销毁了");
        }
    }
}
