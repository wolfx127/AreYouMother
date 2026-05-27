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
    public enum Place {bagA, warehouse , dealer , bagB}

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
            if (indexPlace_A == Place.bagA)
            {
                bool atRowEnd  = (index_A + 1) % bagCols == 0;
                bool atListEnd = index_A == count_BagA - 1;
                if (atRowEnd || atListEnd)
                {
                    int centerRowStart = (index_A / bagCols) * centerCols;
                    if (GetCenterCount() > 0 && centerRowStart < GetCenterCount())
                    {
                        indexPlace_A = centerPlace;
                        index_A = centerRowStart;
                    }
                    else if (atListEnd)
                        index_A = 0;
                    else
                        index_A += 1;
                }
                else
                    index_A += 1;
            }
            else
            {
                bool atRowEnd  = (index_A + 1) % centerCols == 0;
                bool atListEnd = index_A == GetCenterCount() - 1;
                if (atRowEnd || atListEnd)
                {
                    if (count_BagA > 0)
                    {
                        indexPlace_A = Place.bagA;
                        index_A = 0;
                    }
                    else
                        index_A = 0;
                }
                else
                    index_A += 1;
            }
            Debug.Log($"indexA从 {prevIndexPlace_A}{prevIndex_A} 移动到 {indexPlace_A}{index_A}");
        }
        private void IndexRightOne_B()
        {
            if (indexPlace_B != Place.bagB)
            {
                bool atRowEnd  = (index_B + 1) % centerCols == 0;
                bool atListEnd = index_B == GetCenterCount() - 1;
                if (atRowEnd || atListEnd)
                {
                    int bagRowStart = (index_B / centerCols) * bagCols;
                    if (bagRowStart < count_BagB)
                    {
                        indexPlace_B = Place.bagB;
                        index_B = bagRowStart;
                    }
                    else
                        index_B = 0;
                }
                else
                    index_B += 1;
            }
            else
            {
                bool atRowEnd  = (index_B + 1) % bagCols == 0;
                bool atListEnd = index_B == count_BagB - 1;
                if (atRowEnd || atListEnd)
                {
                    if (GetCenterCount() > 0)
                    {
                        indexPlace_B = centerPlace;
                        index_B = 0;
                    }
                    else if (atListEnd)
                        index_B = 0;
                    else
                        index_B += 1;
                }
                else
                    index_B += 1;
            }
            Debug.Log($"indexB从 {prevIndexPlace_B}{prevIndex_B} 移动到 {indexPlace_B}{index_B}");
        }

        private void IndexLeftOne_A()
        {
            if (indexPlace_A == Place.bagA)
            {
                bool atRowStart = index_A % bagCols == 0;
                bool atListStart = index_A == 0;
                if (atRowStart || atListStart)
                {
                    int centerRowStart = (index_A / bagCols) * centerCols;
                    int centerRowEnd = Mathf.Min(centerRowStart + centerCols - 1, GetCenterCount() - 1);
                    if (GetCenterCount() > 0 && centerRowStart < GetCenterCount())
                    {
                        indexPlace_A = centerPlace;
                        index_A = centerRowEnd;
                    }
                    else if (atListStart)
                        index_A = count_BagA - 1;
                    else
                        index_A -= 1;
                }
                else
                    index_A -= 1;
            }
            else
            {
                bool atRowStart = index_A % centerCols == 0;
                bool atListStart = index_A == 0;
                if (atRowStart || atListStart)
                {
                    int bagRowStart = (index_A / centerCols) * bagCols;
                    int bagRowEnd = Mathf.Min(bagRowStart + bagCols - 1, count_BagA - 1);
                    if (bagRowStart < count_BagA)
                    {
                        indexPlace_A = Place.bagA;
                        index_A = bagRowEnd;
                    }
                    else
                        index_A = GetCenterCount() - 1;
                }
                else
                    index_A -= 1;
            }
            Debug.Log($"indexA从 {prevIndexPlace_A}{prevIndex_A} 移动到 {indexPlace_A}{index_A}");
        }
        private void IndexLeftOne_B()
        {
            if (indexPlace_B != Place.bagB)
            {
                bool atRowStart = index_B % centerCols == 0;
                bool atListStart = index_B == 0;
                if (atRowStart || atListStart)
                {
                    if (count_BagB > 0)
                    {
                        indexPlace_B = Place.bagB;
                        index_B = count_BagB - 1;
                    }
                    else
                        index_B = 0;
                }
                else
                    index_B -= 1;
            }
            else
            {
                bool atRowStart = index_B % bagCols == 0;
                bool atListStart = index_B == 0;
                if (atRowStart || atListStart)
                {
                    int centerRowStart = (index_B / bagCols) * centerCols;
                    int centerRowEnd = Mathf.Min(centerRowStart + centerCols - 1, GetCenterCount() - 1);
                    if (GetCenterCount() > 0 && centerRowStart < GetCenterCount())
                    {
                        indexPlace_B = centerPlace;
                        index_B = centerRowEnd;
                    }
                    else if (atListStart)
                        index_B = count_BagB - 1;
                    else
                        index_B -= 1;
                }
                else
                    index_B -= 1;
            }
            Debug.Log($"indexB从 {prevIndexPlace_B}{prevIndex_B} 移动到 {indexPlace_B}{index_B}");
        }

        private void IndexUpOne_A()
        {
            int cols = indexPlace_A == Place.bagA ? bagCols : centerCols;
            int count = count_A;
            int col = index_A % cols;
            if (index_A - cols < 0)
            {
                int lastRowStart = (count - 1) / cols * cols;
                int lastRowIndex = lastRowStart + col;
                index_A = lastRowIndex < count ? lastRowIndex : lastRowIndex - cols;
            }
            else
                index_A -= cols;
            Debug.Log($"indexA从 {prevIndexPlace_A}{prevIndex_A} 移动到 {indexPlace_A}{index_A}");
        }
        private void IndexDownOne_A()
        {
            int cols = indexPlace_A == Place.bagA ? bagCols : centerCols;
            int count = count_A;
            int col = index_A % cols;
            if (index_A + cols >= count)
                index_A = col < count ? col : index_A;
            else
                index_A += cols;
            Debug.Log($"indexA从 {prevIndexPlace_A}{prevIndex_A} 移动到 {indexPlace_A}{index_A}");
        }
        private void IndexUpOne_B()
        {
            int cols = indexPlace_B == Place.bagB ? bagCols : centerCols;
            int count = count_B;
            int col = index_B % cols;
            if (index_B - cols < 0)
            {
                int lastRowStart = (count - 1) / cols * cols;
                int lastRowIndex = lastRowStart + col;
                index_B = lastRowIndex < count ? lastRowIndex : lastRowIndex - cols;
            }
            else
                index_B -= cols;
            Debug.Log($"indexB从 {prevIndexPlace_B}{prevIndex_B} 移动到 {indexPlace_B}{index_B}");
        }
        private void IndexDownOne_B()
        {
            int cols = indexPlace_B == Place.bagB ? bagCols : centerCols;
            int count = count_B;
            int col = index_B % cols;
            if (index_B + cols >= count)
                index_B = col < count ? col : index_B;
            else
                index_B += cols;
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

#endregion 
        ////////////////////////////////////////////
        
        public List<Prop> GetBag_A() =>  oapc.GetBag_A();
        public List<Prop> GetBag_B() =>  oapc.GetBag_B();
        public List<Prop> GetWarehouse() => WarehouseManager.GetWarehouse();
        public List<Prop> GetDealerStore() => DealerManager.GetStore();

        public Prop GetCheckingProp_A()
        {
            if (indexPlace_A == Place.bagA) return oapc.GetBag_A()[index_A];
            else if (indexPlace_A == Place.warehouse) return WarehouseManager.GetPropByIndex(index_A);
            else if (indexPlace_A == Place.dealer) return DealerManager.GetStore()[index_A];
            return null;
        }
        public Prop GetCheckingProp_B()
        {
            if (indexPlace_B == Place.bagB) return oapc.GetBag_B()[index_B];
            else if (indexPlace_B == Place.warehouse) return WarehouseManager.GetPropByIndex(index_B);
            else if (indexPlace_A == Place.dealer) return DealerManager.GetStore()[index_B];
            return null;
        }
        public string GetCheckingPropName_A()
        {
            return GetCheckingProp_A().name;
        }
        public string GetCheckingPropName_B()
        {
            return GetCheckingProp_B().name;
        }
        public string GetCheckingPropDescribe_A()
        {
            return $"价值:{GetCheckingProp_A().value} | 数值:{GetCheckingProp_A().playingQuantity} | {GetCheckingProp_A().rarity}" + '\n' +
                   GetCheckingProp_A().description; 
        }
        public string GetCheckingPropDescribe_B()
        {
            return $"价值:{GetCheckingProp_B().value} | 数值:{GetCheckingProp_B().playingQuantity} | {GetCheckingProp_B().rarity}" + '\n' +
                   GetCheckingProp_B().description; 
        }

        public string GetBagInfo_A()
        {
            return $"背包上限/现存道具数:{oapc.bagSize_A}/{oapc.GetBag_A().Count}";
        }
        public string GetBagInfo_B()
        {
            return $"背包上限/现存道具数:{oapc.bagSize_B}/{oapc.GetBag_B().Count}";
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
        }

        public void Unsubscribe()
        {
            homeHandler.ReplaceProp_AEvent -= Replace_A;
            homeHandler.ReplaceProp_BEvent -= Replace_B;
            homeHandler.ChooseProp_AEvent -= ChangeIndex_A;
            homeHandler.ChooseProp_BEvent -= ChangeIndex_B;
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
            if (indexPlace_A == Place.bagA)
            {
                if (centerPlace == Place.warehouse)
                {
                    Prop temp = oapc.GetBag_A()[index_A];
                    oapc.RemovePropByIndex_A(index_A);
                    WarehouseManager.AddProp(temp);
                    IndexLeftOne_A();
                    RefreshWarehouseEvent?.Invoke();
                }
                else if (centerPlace == Place.dealer)
                {
                    Prop temp = oapc.GetBag_A()[index_A];
                    oapc.RemovePropByIndex_A(index_A);
                    IndexLeftOne_A();
                    RefreshDealerEvent?.Invoke();
                }
            }
            else if (indexPlace_A == Place.warehouse)
            {
                Prop temp = WarehouseManager.GetPropByIndex(index_A);
                WarehouseManager.RemovePropByIndex(index_A);
                oapc.AddProp_A(temp);
                IndexLeftOne_A();
                RefreshWarehouseEvent?.Invoke();
            }
            else if (indexPlace_A == Place.dealer)
            {
                Prop temp = DealerManager.GetStore()[index_A];
                DealerManager.RemoveStoreByIndex(index_A);
                oapc.AddProp_A(temp);
                IndexLeftOne_A();
                RefreshDealerEvent?.Invoke();
            }
            RefreshBag_AEvent?.Invoke();
        }

        public void Replace_B()
        {
            if (indexPlace_B == Place.bagB)
            {
                if (centerPlace == Place.warehouse)
                {
                    Prop temp = oapc.GetBag_B()[index_B];
                    oapc.RemovePropByIndex_B(index_B);
                    WarehouseManager.AddProp(temp);
                    IndexLeftOne_B();
                    RefreshWarehouseEvent?.Invoke();
                }
                else if (centerPlace == Place.dealer)
                {
                    Prop temp = oapc.GetBag_B()[index_B];
                    oapc.RemovePropByIndex_B(index_B);
                    IndexLeftOne_B();
                    RefreshDealerEvent?.Invoke();
                }
            }
            else if (indexPlace_B == Place.warehouse)
            {
                Prop temp = WarehouseManager.GetPropByIndex(index_B);
                WarehouseManager.RemovePropByIndex(index_B);
                oapc.AddProp_B(temp);
                IndexLeftOne_B();
                RefreshWarehouseEvent?.Invoke();
            }
            else if (indexPlace_B == Place.dealer)
            {
                Prop temp = DealerManager.GetStore()[index_B];
                DealerManager.RemoveStoreByIndex(index_B);
                oapc.AddProp_B(temp);
                IndexLeftOne_B();
                RefreshDealerEvent?.Invoke();
            }
            RefreshBag_BEvent?.Invoke();
        }
    }
}
