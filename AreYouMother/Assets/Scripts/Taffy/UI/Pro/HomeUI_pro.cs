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
        public int bagCols = 5;
        public int warehouseCols = 5;
        public int dealerCols = 5;
        public int index_A = 0;
        public int precIndex_A = 0;
        public int index_B = 0;
        public int precIndex_B = 0;
        public int count_BagA => oapc.GetBag_A().Count;
        public int count_BagB => oapc.GetBag_B().Count;
        public int count_warehouse => WarehouseManager.GetWarehouse().Count;
        public int count_dealer => 0;
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

        private void IndexPlusOne_A()
        {
            if (index_A >= count_A)
            {
                if (indexPlace_A is Place.bagA) indexPlace_A = centerPlace;
                else indexPlace_A = Place.bagA;
                index_A = 0;
            }
            index_A += 1;
        }
        private void IndexPlusOne_B()
        {
            if (index_B >= count_B)
            {
                if (indexPlace_B is Place.bagB) indexPlace_B = centerPlace;
                else indexPlace_B = Place.bagB;
                index_B = 0;
            }
            index_B += 1;
        }

        private void IndexMinusOne_A()
        {
            if (index_A <= 0)
            {
                if(indexPlace_A is Place.bagA) indexPlace_A = centerPlace;
                else indexPlace_A = Place.bagA;
                index_A = count_A-1;
            }
            index_A -= 1;
        }
        private void IndexMinusOne_B()
        {
            if (index_B <= 0)
            {
                if(indexPlace_B is Place.bagB) indexPlace_B = centerPlace;
                else indexPlace_B = Place.bagB;
                index_B = count_B-1;
            }
            index_B -= 1;
        }

        public void KeepUpWithIndex_A()
        {
            precIndex_A = index_A;
            prevIndexPlace_A = indexPlace_A;
        }
        public void KeepUpWithIndex_B()
        {
            precIndex_B = index_B;
            prevIndexPlace_B = indexPlace_B;
        }


        ////////////////////////////////////////////
        
        public List<Prop> GetBag_A() =>  oapc.GetBag_A();
        public List<Prop> GetBag_B() =>  oapc.GetBag_B();
        public List<Prop> GetWarehouse() => WarehouseManager.GetWarehouse();

        public Prop GetCheckingProp_A()
        {
            if (indexPlace_A == Place.bagA) return oapc.GetBag_A()[index_A];
            else if (indexPlace_A == Place.warehouse) return WarehouseManager.GetPropByIndex(index_A);
//TODO:     else if(indexPlace_A == Place.dealer)
            return null;
        }
        public Prop GetCheckingProp_B()
        {
            if (indexPlace_B == Place.bagB) return oapc.GetBag_B()[index_B];
            else if (indexPlace_B == Place.warehouse) return WarehouseManager.GetPropByIndex(index_B);
//TODO:     else if(indexPlace_B == Place.dealer)
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
//TODO:变索引A
            CheckProp_AEvent?.Invoke();
        }

        public void ChangeIndex_B(Vector2Int vec)
        {
//TODO:变索引B
            CheckProp_BEvent?.Invoke();
        }

        public void Replace_A()
        {
//TODO:转移物品A
            RefreshBag_AEvent?.Invoke();
        }

        public void Replace_B()
        {
//TODO:转移物品B
            RefreshBag_BEvent?.Invoke();
        }
    }
}
