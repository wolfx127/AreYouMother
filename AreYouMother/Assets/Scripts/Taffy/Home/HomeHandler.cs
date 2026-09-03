using System;
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
        public event Action ReplaceProp_AEvent;
        public event Action ReplaceProp_BEvent;
        public event Action UseProp_AEvent;
        public event Action UseProp_BEvent;

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
        private int GetMaxIndex()
        {
            if(place_A == HomeIndexPlace.BagA) return oapc.GetBag_A().Count;
            else if (OverAllStates.isInWarehouse && place_A == HomeIndexPlace.Warehouse)
                return WarehouseManager.GetWarehouseCount();
            else if (OverAllStates.isInDealer && place_A == HomeIndexPlace.Dealer)
                return DealerManager.store.Count;
            return 0;
        }

        private void ChooseProp_A(InputAction.CallbackContext ctx)
        {
            if (index_A == -1) return;
            Vector2 v = ctx.ReadValue<Vector2>();
            int count = GetMaxIndex();
            
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
            Debug.Log("更换checkingA");
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

            Debug.Log("A跳转道具栏成功");
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

            Debug.Log("B跳转道具栏成功");
        }

        private void ChooseProp_B(InputAction.CallbackContext ctx)
        {
            Vector2 v = ctx.ReadValue<Vector2>();
            Debug.Log($"[输入] ChooseProp_B: 方向=({v.x},{v.y}), place={place_B}, index={index_B}");
            int count;
            if(place_B == HomeIndexPlace.BagB) count = oapc.GetBag_B().Count;
            else if (OverAllStates.isInWarehouse && place_B == HomeIndexPlace.Warehouse)
                count = WarehouseManager.GetWarehouseCount();
            else if (OverAllStates.isInDealer && place_B == HomeIndexPlace.Dealer)
                count = DealerManager.store.Count;
            else return;

            bool trySwitch =
                (v.x < 0 && v.y == 0 && index_B <= 0) ||
                (v.x > 0 && v.y == 0 && index_B >= count - 1);

            if (trySwitch)
            {
                if (!TrySwitchPlace_B())
                {
                    place_B = HomeIndexPlace.BagB;
                    if (oapc.GetBag_B().Count == 0) index_B = -1;
                }
            }
            else
            {
                var n = UITools.GetNeighbors(index_B, count);
                if      (v.x == 0 && v.y > 0) index_B = n.up;
                else if (v.x == 0 && v.y < 0) index_B = n.down;
                else if (v.x < 0 && v.y == 0) index_B = n.left;
                else if (v.x > 0 && v.y == 0) index_B = n.right;
            }

            ChooseProp_BEvent?.Invoke();
            Debug.Log("更换checkingB");
        }

        private bool TrySwitchPlace_B()
        {
            if (place_B == HomeIndexPlace.BagB)
            {
                if (OverAllStates.isInWarehouse && WarehouseManager.GetWarehouseCount() > 0)
                {
                    place_B = HomeIndexPlace.Warehouse;
                    index_B = 0;
                    return true;
                }
                if (OverAllStates.isInDealer && DealerManager.store.Count > 0)
                {
                    place_B = HomeIndexPlace.Dealer;
                    index_B = 0;
                    return true;
                }
                return false;
            }

            if (oapc.GetBag_B().Count > 0)
            {
                place_B = HomeIndexPlace.BagB;
                index_B = 0;
                return true;
            }
            return false;
        }

        public void ResetIndex()
        {
            index_A = 0;
            index_B = 0;
            if(oapc.GetBag_A().Count == 0)
            {
                if (OverAllStates.isInWarehouse)
                {
                    place_A = HomeIndexPlace.Warehouse;
                }
                else if (OverAllStates.isInDealer)
                {
                    place_A = HomeIndexPlace.Dealer;
                }
            }
            else
            {
                place_A = HomeIndexPlace.BagA;
            }
            
            if(oapc.GetBag_B().Count == 0)
            {
                if (OverAllStates.isInWarehouse)
                {
                    place_B = HomeIndexPlace.Warehouse;
                }
                else if (OverAllStates.isInDealer)
                {
                    place_B = HomeIndexPlace.Dealer;
                }
            }
            else
            {
                place_B = HomeIndexPlace.BagB;
            }
        }
#endregion

/////////// 获取 与 使用选中的道具 //////////////////////////////////////////////////////////
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
                return oapc.GetBag_A()[index_B];
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
            Prop prop = GetChoosePropA();
            if (prop == null) return;

            prop.Execute();
            UseProp_AEvent?.Invoke();
            Debug.Log($"[输入] A使用了一个道具: place={place_A}, index={index_A}");
        }
        
        private void UseProp_B(InputAction.CallbackContext ctx)
        {
            Prop prop = GetChoosePropB();
            if (prop == null) return;
            
            prop.Execute();
            UseProp_BEvent?.Invoke();
            Debug.Log($"[输入] B使用了一个道具: place={place_B}, index={index_B}");
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
                OK = true;
            }
        //从背包卖到商人
            else if (place_A == HomeIndexPlace.BagA && OverAllStates.isInDealer)
            {
                int price = oapc.GetBag_A()[index_A].price;
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
                OK = true;
            }
        //从仓库放到背包
            else if (place_A == HomeIndexPlace.Warehouse)
            {
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
                ReplaceProp_AEvent?.Invoke();
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
                OK = true;
            }
            else if (place_B == HomeIndexPlace.BagB && OverAllStates.isInDealer)
            {
                int price = oapc.GetBag_B()[index_B].price;
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
                OK = true;
            }
            else if (place_B == HomeIndexPlace.Warehouse)
            {
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
                ReplaceProp_BEvent?.Invoke();
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
