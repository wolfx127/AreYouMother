using System;
using System.Collections.Generic;
using Taffy.Data;
using Taffy.Data.PropData;
using Taffy.OverAllManager;
using Taffy.Play.Container;
using Taffy.Play.Place;
using Taffy.Play.Player;
using TaffyFrame.EventBus;
using UnityEngine;

namespace Taffy.UI.Pro
{
    public interface IPlayingUI_pre
    {
        public void Subscribe();
        public void Unsubscribe();
        
        public void BackHome();
    }

    public class PlayingUI_pre : IPlayingUI_pre
    {
        private PlayingHandler_A handler_A = PlayingHandler_A.Instance;
        private PlayingHandler_B handler_B = PlayingHandler_B.Instance;
        
        private IPlayingUI playingUI;

        public PlayingUI_pre(){ }
        public PlayingUI_pre(IPlayingUI playingUI) => this.playingUI = playingUI;

        public void Subscribe()
        {
            handler_A.OpenBagEvent += OpenBag_A;
            handler_A.CloseBagEvent += CloseBag_A;
            handler_A.OpenContainerEvent += OpenContainer_A;
            handler_A.CloseContainerEvent += CloseContainer_A;
            handler_A.RefreshBagEvent += RefreshBag_A;
            handler_A.RefreshContainerEvent += RefreshContainer_A;
            handler_A.UpdateChooseEvent += UpdateChoose_A;
            handler_B.OpenBagEvent += OpenBag_B;
            handler_B.CloseBagEvent += CloseBag_B;
            handler_B.OpenContainerEvent += OpenContainer_B;
            handler_B.CloseContainerEvent += CloseContainer_B;
            handler_B.RefreshBagEvent += RefreshBag_B;
            handler_B.RefreshContainerEvent += RefreshContainer_B;
            handler_B.UpdateChooseEvent += UpdateChoose_B;

            handler_A.player.combatData.UpdateHPEvent += UpdateHP_A;
            handler_A.player.combatData.UpdateMPEvent += UpdateMP_A;
            handler_B.player.combatData.UpdateHPEvent += UpdateHP_B;
            handler_B.player.combatData.UpdateMPEvent += UpdateMP_B;

            EventBus.Subscribe<EvacuateEvent>(Evacuate);
        }
        public void Unsubscribe()
        {
            if (handler_A)
            {
                handler_A.OpenBagEvent -= OpenBag_A;
                handler_A.CloseBagEvent -= CloseBag_A;
                handler_A.OpenContainerEvent -= OpenContainer_A;
                handler_A.CloseContainerEvent -= CloseContainer_A;
                handler_A.RefreshBagEvent -= RefreshBag_A;
                handler_A.RefreshContainerEvent -= RefreshContainer_A;
                handler_A.UpdateChooseEvent -= UpdateChoose_A;
                handler_A.player.combatData.UpdateHPEvent -= UpdateHP_A;
                handler_A.player.combatData.UpdateMPEvent -= UpdateMP_A;
            }

            if (handler_B)
            {
                handler_B.OpenBagEvent -= OpenBag_B;
                handler_B.CloseBagEvent -= CloseBag_B;
                handler_B.OpenContainerEvent -= OpenContainer_B;
                handler_B.CloseContainerEvent -= CloseContainer_B;
                handler_B.RefreshBagEvent -= RefreshBag_B;
                handler_B.RefreshContainerEvent -= RefreshContainer_B;
                handler_B.UpdateChooseEvent -= UpdateChoose_B;
                handler_B.player.combatData.UpdateHPEvent -= UpdateHP_B;
                handler_B.player.combatData.UpdateMPEvent -= UpdateMP_B;
            }
            
            EventBus.Unsubscribe<EvacuateEvent>(Evacuate);
        }

        private List<Texture2D> GetImages(List<Prop> list)
        {
            List<Texture2D> images = new List<Texture2D>();
            for(int i = 0;i < list.Count;i++)
            {
                if (list[i] is not null)
                {
                    images.Add(list[i].image);
                }
                else
                {
                    Debug.Log($"没找到道具:索引{i}");
                }
            }
            return images;
        }

////// 钩子 /////////////////
    ////// 注册m ///////
        private void OpenBag_A()
        {
            playingUI.OpenBag_A();
            playingUI.RefreshBag_A(GetImages(handler_A.player.bag), handler_A.player.bag.Count, handler_A.player.bagSize);
            if(handler_A.player.bag.Count>0) playingUI.CheckingProp_A(handler_A.index,handler_A.place);
        }

        private void OpenBag_B()
        {
            playingUI.OpenBag_B();
            playingUI.RefreshBag_B(GetImages(handler_B.player.bag), handler_B.player.bag.Count, handler_B.player.bagSize);
            if(handler_B.player.bag.Count>0) playingUI.CheckingProp_B(handler_B.index,handler_B.place);
        }

        private void CloseBag_A()
        {
            playingUI.CloseBag_A();
        }
        
        private void CloseBag_B() 
        {
            playingUI.CloseBag_B();
        }
        private void OpenContainer_A()
        {
            playingUI.OpenContainer_A();
        }

        private void OpenContainer_B()
        {
            playingUI.OpenContainer_B();
        }

        private void CloseContainer_A()
        {
            playingUI.CloseContainer_A();
        }

        private void CloseContainer_B()
        {
            playingUI.CloseContainer_B();
        }

        private void RefreshBag_A()
        {
            playingUI.RefreshBag_A(GetImages(handler_A.player.bag), handler_A.player.bag.Count, handler_A.player.bagSize);
        }

        private void RefreshBag_B()
        {
            playingUI.RefreshBag_B(GetImages(handler_B.player.bag), handler_B.player.bag.Count, handler_B.player.bagSize);
        }

        private void RefreshContainer_A()
        {
            playingUI.RefreshBag_A(GetImages(handler_A.player.bag), handler_A.player.bag.Count, handler_A.player.bagSize);
            playingUI.RefreshContainer_A(GetImages(handler_A.container.GetAllProps()), handler_A.container.type);
        }

        private void RefreshContainer_B()
        {
            playingUI.RefreshBag_B(GetImages(handler_B.player.bag), handler_B.player.bag.Count, handler_B.player.bagSize);
            playingUI.RefreshContainer_B(GetImages(handler_B.container.GetAllProps()), handler_B.container.type);
        }

        private void UpdateChoose_A()
        {
            playingUI.CheckingProp_A(handler_A.index,handler_A.place);
            Prop temp = null;
            if(handler_A.place == PlayIndexPlace.Bag) temp = handler_A.player.bag[handler_A.index];
            else if(handler_A.place == PlayIndexPlace.Container) temp = handler_A.container.GetPropByIndex(handler_A.index);
            playingUI.DescribeProp_A(temp);
        }

        private void UpdateChoose_B()
        {
            playingUI.CheckingProp_B(handler_B.index,handler_B.place);
            Prop temp = null;
            if(handler_B.place == PlayIndexPlace.Bag) temp = handler_B.player.bag[handler_B.index];
            else if(handler_B.place == PlayIndexPlace.Container) temp = handler_B.container.GetPropByIndex(handler_B.index);
            playingUI.DescribeProp_B(temp);
        }


        private void UpdateHP_A(int hp)
        {
            playingUI.UpdateHP_A(hp);
        }

        private void UpdateHP_B(int hp)
        {
            playingUI.UpdateHP_B(hp);
        }

        private void UpdateMP_A(int mp)
        {
            playingUI.UpdateMP_A(mp);
        }

        private void UpdateMP_B(int mp)
        {
            playingUI.UpdateMP_B(mp);
        }

        private void Evacuate(EvacuateEvent evt)
        {
            playingUI.Evacuate(handler_A.player.isDead, handler_B.player.isDead, handler_A.player.bag.GetTotalProperty(), handler_B.player.bag.GetTotalProperty());
        }
        ////// 被v调用 ///////

        public void BackHome()
        {
            EventBus.Publish(new ChangeScenePlayingToHomeEvent());
        }
    }
}
