using System.Collections.Generic;
using Taffy.Data.PropData;
using Taffy.Home;
using Taffy.OverAllManager;
using UnityEngine;
using EventBus = Taffy.OverAllManager.EventBus;

namespace Taffy.UI.Pre
{

    public interface IHomeUI_pre
    {
        public void Subscribe(IHomeUIManager homeUI);
        public void Unsubscribe();
        public void ChangeSceneToPlaying();
        public void ExitGame();
        public void ChangeCenter();

        public int GetCount_BagA();
        public int GetCount_BagB();
        public int GetCount_Warehouse();
        public int GetCount_Dealer();
        public List<Texture2D> GetPropImages_BagA();
        public List<Texture2D> GetPropImages_BagB();
        public List<Texture2D> GetPropImages_Warehouse();
        public List<Texture2D> GetPropImages_Dealer();
        public Prop GetChooseProp_A();
        public Prop GetChooseProp_B();

        public int GetProperty();
        public (int hp,int mp,int atk,int def) GetState_A();
        public (int hp,int mp,int atk,int def) GetState_B();
    }

    public class HomeUI_pre : IHomeUI_pre
    {
        private OverAllPlayerController oapc;
        private HomeHandler homeHandler;
        private IHomeUIManager homeUI;
        /// ////////////////////////////////////////

        public void Subscribe(IHomeUIManager homeUI)
        {
            this.homeUI = homeUI;
            oapc = OverAllPlayerController.Instance;
            homeHandler = HomeHandler.Instance;
            Debug.Log($"[UI_P] Subscribe: oapc={(oapc == null ? "NULL" : "OK")}, homeHandler={(homeHandler == null ? "NULL" : "OK")}");
            homeHandler.ChooseProp_AEvent += ChooseProp_A;
            homeHandler.ChooseProp_BEvent += ChooseProp_B;
            homeHandler.ReplacePropEvent += Replace;
            homeHandler.Delete_AEvent += DeleteProp_A;
            homeHandler.Delete_BEvent += DeleteProp_B;

            PropBehaviorTable.table[PropType.Close_Attack].Event += UpdateState_B;
            PropBehaviorTable.table[PropType.Remote_Attack].Event += UpdateState_A;
            PropBehaviorTable.table[PropType.Defend].Event += UpdateState_A;
            PropBehaviorTable.table[PropType.Defend].Event += UpdateState_B;
            PropBehaviorTable.table[PropType.AddMaxBlood].Event += UpdateState_A;
            PropBehaviorTable.table[PropType.AddMaxBlood].Event += UpdateState_B;
            PropBehaviorTable.table[PropType.AddMaxSkill].Event += UpdateState_A;
            PropBehaviorTable.table[PropType.AddMaxSkill].Event += UpdateState_B;
            PropBehaviorTable.table[PropType.AddBlood].Event += UpdateState_A;
            PropBehaviorTable.table[PropType.AddBlood].Event += UpdateState_B;
            
            homeUI.RefreshBag_A();
            homeUI.RefreshBag_B();
            homeUI.RefreshWarehouse();
            homeUI.Check_A(homeHandler.index_A, homeHandler.place_A, homeHandler.index_B, homeHandler.place_B);
            homeUI.Check_B(homeHandler.index_A, homeHandler.place_A, homeHandler.index_B, homeHandler.place_B);
        }

        public void Unsubscribe()
        {
            Debug.Log($"[UI_P] Unsubscribe: homeHandler={(homeHandler == null ? "NULL" : "OK")}");
            homeHandler.ChooseProp_AEvent -= ChooseProp_A;
            homeHandler.ChooseProp_BEvent -= ChooseProp_B;
            homeHandler.ReplacePropEvent -= Replace;
            homeHandler.Delete_AEvent -= DeleteProp_A;
            homeHandler.Delete_BEvent -= DeleteProp_B;
            oapc = null;
            homeHandler = null;
            
            PropBehaviorTable.table[PropType.Close_Attack].Event -= UpdateState_B;
            PropBehaviorTable.table[PropType.Remote_Attack].Event -= UpdateState_A;
            PropBehaviorTable.table[PropType.Defend].Event -= UpdateState_A;
            PropBehaviorTable.table[PropType.Defend].Event -= UpdateState_B;
            PropBehaviorTable.table[PropType.AddMaxBlood].Event -= UpdateState_A;
            PropBehaviorTable.table[PropType.AddMaxBlood].Event -= UpdateState_B;
            PropBehaviorTable.table[PropType.AddMaxSkill].Event -= UpdateState_A;
            PropBehaviorTable.table[PropType.AddMaxSkill].Event -= UpdateState_B;
            PropBehaviorTable.table[PropType.AddBlood].Event -= UpdateState_A;
            PropBehaviorTable.table[PropType.AddBlood].Event -= UpdateState_B;
        }

        public void ChangeSceneToPlaying()
        {
            Debug.Log("[UI_P] 点进入游戏按钮");
            EventBus.Publish(new ChangeSceneHomeToPlayingEvent());
        }
        
        public void ExitGame()
        {
            Debug.Log("[UI_P] 点退出游戏按钮");
            EventBus.Publish(new ExitGameEvent());
        }
      
        public int GetCount_BagA()
        {
            return oapc.GetBag_A().Count;
        }

        public int GetCount_BagB()
        {
            return oapc.GetBag_B().Count;
        }

        public int GetCount_Warehouse()
        {
            return WarehouseManager.GetWarehouseCount();
        }

        public int GetCount_Dealer()
        {
            return DealerManager.store.Count;
        }

        public List<Texture2D> GetPropImages_BagA()
        {
            List<Texture2D> images = new List<Texture2D>();
            Debug.Log($"[UI_P] GetPropImages_BagA 被调用, 帧:{Time.frameCount}, oapc={(oapc == null ? "NULL" : "OK")}");
            foreach (var p in oapc.GetBag_A())
            {
                images.Add(p.image);
            }

            return images;
        }

        public List<Texture2D> GetPropImages_BagB()
        {
            List<Texture2D> images = new List<Texture2D>();
            Debug.Log($"[UI_P] GetPropImages_BagB 被调用, 帧:{Time.frameCount}, oapc={(oapc == null ? "NULL" : "OK")}");
            foreach (var p in oapc.GetBag_B())
            {
                images.Add(p.image);
            }

            return images;
        }

        public List<Texture2D> GetPropImages_Warehouse()
        {
            List<Texture2D> images = new List<Texture2D>();
            Debug.Log($"[UI_P] GetPropImages_Warehouse 被调用, 帧:{Time.frameCount}, oapc={(oapc == null ? "NULL" : "OK")}");
            foreach (var p in WarehouseManager.GetWarehouse())
            {
                images.Add(p.image);
            }

            return images;
        }

        public List<Texture2D> GetPropImages_Dealer()
        {
            List<Texture2D> images = new List<Texture2D>();
            Debug.Log($"[UI_P] GetPropImages_Dealer 被调用, 帧:{Time.frameCount}, oapc={(oapc == null ? "NULL" : "OK")}");
            foreach (var p in DealerManager.store)
            {
                images.Add(p.image);
            }

            return images;
        }

        public Prop GetChooseProp_A()
        {
            if (homeHandler.place_A == HomeIndexPlace.BagA)
            { var bag = oapc.GetBag_A(); return homeHandler.index_A < bag.Count ? bag[homeHandler.index_A] : null; }
            else if (homeHandler.place_A == HomeIndexPlace.Warehouse)
            { var wh = WarehouseManager.GetWarehouse(); return homeHandler.index_A < wh.Count ? wh[homeHandler.index_A] : null; }
            else if (homeHandler.place_A == HomeIndexPlace.Dealer)
            { return homeHandler.index_A < DealerManager.store.Count ? DealerManager.store[homeHandler.index_A] : null; }
            return null;
        }

        public Prop GetChooseProp_B()
        {
            if (homeHandler.place_B == HomeIndexPlace.BagB)
            { var bag = oapc.GetBag_B(); return homeHandler.index_B < bag.Count ? bag[homeHandler.index_B] : null; }
            else if (homeHandler.place_B == HomeIndexPlace.Warehouse)
            { var wh = WarehouseManager.GetWarehouse(); return homeHandler.index_B < wh.Count ? wh[homeHandler.index_B] : null; }
            else if (homeHandler.place_B == HomeIndexPlace.Dealer)
            { return homeHandler.index_B < DealerManager.store.Count ? DealerManager.store[homeHandler.index_B] : null; }
            return null;
        }

        public (int index, HomeIndexPlace place_A) GetChooseIndex_A()
        {
            return (homeHandler.index_A, homeHandler.place_A);
        }

        public (int index, HomeIndexPlace place_B) GetChooseIndex_B()
        {
            return (homeHandler.index_B, homeHandler.place_B);
        }

        public int GetProperty()
        {
            return WarehouseManager.property;
        }

        public (int hp, int mp, int atk, int def) GetState_A()
        {
            return (oapc.maxHP_A,oapc.maxMP_A,oapc.ATK_A,oapc.DEF_A);
        }

        public (int hp, int mp, int atk, int def) GetState_B()
        {
            return (oapc.maxHP_B,oapc.maxMP_B,oapc.ATK_B,oapc.DEF_B);
        }

/// 钩子 /////////////////////////////////////

        /// 注册m层 //////////////////////////
        private void UpdateProperty()
        {
            homeUI.UpdatePropertyNum(WarehouseManager.property);
        }
        private void UpdateState_A()
        {
            homeUI.UpdateStateInfo_A(oapc.maxHP_A,oapc.maxMP_A,oapc.ATK_A,oapc.DEF_A);
        }
        private void UpdateState_B()
        {
            homeUI.UpdateStateInfo_B(oapc.maxHP_B,oapc.maxMP_B,oapc.ATK_B,oapc.DEF_B);
        }
        private void ChangeToWarehouse()
        {
            homeUI.ChangeToWarehouse();
        }
        private void ChangeToDealer()
        {
            homeUI.ChangeToDealer();
        }
        
        private void ChooseProp_A()
        {
            homeUI.Check_A(homeHandler.index_A,homeHandler.place_A,homeHandler.index_B,homeHandler.place_B);
        }

        private void ChooseProp_B()
        {
            homeUI.Check_B(homeHandler.index_A,homeHandler.place_A,homeHandler.index_B,homeHandler.place_B);
        }
        
        private void Replace()
        {
            homeUI.RefreshBag_A();
            homeUI.RefreshBag_B();
            if(OverAllStates.isInWarehouse) homeUI.RefreshWarehouse();
            else if(OverAllStates.isInDealer) homeUI.RefreshDealer();

            homeUI.Check_A(homeHandler.index_A,homeHandler.place_A,homeHandler.index_B,homeHandler.place_B);
            homeUI.Check_B(homeHandler.index_A,homeHandler.place_A,homeHandler.index_B,homeHandler.place_B);
            homeUI.UpdatePropertyNum(WarehouseManager.property);
        }

        private void DeleteProp_A()
        {
            homeUI.RefreshBag_A();
            homeUI.Check_A(homeHandler.index_A,homeHandler.place_A,homeHandler.index_B,homeHandler.place_B);
        }

        private void DeleteProp_B()
        {
            homeUI.RefreshBag_B();
            homeUI.Check_B(homeHandler.index_A,homeHandler.place_A,homeHandler.index_B,homeHandler.place_B);
        }
        /// 注册v层 /////////////////////////////////////////
        public void ChangeCenter()
        {
            if (OverAllStates.isInWarehouse)
            {
                OverAllStates.ChangeToDealer();
                ChangeToDealer();
            }
            else if (OverAllStates.isInDealer)
            {
                OverAllStates.ChangeToWarehouse();
                ChangeToWarehouse();
            }
            homeHandler.ResetIndex();
        }
    }
}
