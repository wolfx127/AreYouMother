using System;
using System.Collections.Generic;
using Taffy.Data;
using Taffy.OverAllManager;
using Taffy.UI.Pro;
using UnityEngine;
using UnityEngine.UIElements;
using Index = Taffy.UI.Pro.Index;

namespace Taffy.UI
{
    public class PlayingUIManager:MonoBehaviour
    {
        private PlayingUI_pro playingUIPro = new PlayingUI_pro();
        
        private VisualElement root;
        private VisualElement barHP_A;
        private VisualElement barHP_B;
        private VisualElement barMP_A;
        private VisualElement barMP_B;
        private VisualElement PropCase;

        [SerializeField] private VisualTreeAsset BagUI;
        [SerializeField] private VisualTreeAsset PropCaseUI;
        [SerializeField] private VisualTreeAsset containerUI;

        private VisualElement BagUI_A;
        private VisualElement BagUI_B;
        private VisualElement propCatalogue_A;
        private VisualElement propCatalogue_B;
        private VisualElement containerUI_A;
        private VisualElement containerUI_B;
        

        private Label infoNum_playerA;
        private Label infoNum_playerB;


        private void Awake()
        {
            root = GetComponent<UIDocument>().rootVisualElement;
            barHP_A = root.Q<VisualElement>("HP_PlayerA").Q<VisualElement>("CurrentHP");
            barHP_B = root.Q<VisualElement>("HP_PlayerB").Q<VisualElement>("CurrentHP");
            barMP_A = root.Q<VisualElement>("MP_PlayerA").Q<VisualElement>("CurrentMP");
            barMP_B = root.Q<VisualElement>("MP_PlayerB").Q<VisualElement>("CurrentMP");
            infoNum_playerA = root.Q<VisualElement>("Info_PlayerA").Q<Label>("HPandMPnum");
            infoNum_playerB = root.Q<VisualElement>("Info_PlayerB").Q<Label>("HPandMPnum");

            BagUI_A = BagUI.Instantiate().Q<VisualElement>("root");
            BagUI_B = BagUI.Instantiate().Q<VisualElement>("root");
        }

        private void Start()
        {
            SubscribeEvents();
            infoNum_playerA.text = playingUIPro.InfoNum_playerA();
            infoNum_playerB.text = playingUIPro.InfoNum_playerB();

            Debug.Log("player数值文本初始化成功");
        }

        private void OnDisable()
        {
            UnsubscribeEvents();
        }

        private void SubscribeEvents()
        {
            if (playingUIPro is not null)
            {
                playingUIPro.CheckingProp_AEvent += CheckingProp_A;//上下左右输入->invoke->更新索引()->indexSetter()->invoke->checking()
                playingUIPro.DiscardProp_AEvent += RefreshBag_A;//丢弃道具输入->invoke->丢弃道具()->invoke->刷新背包()
                playingUIPro.DiscardProp_AEvent += CheckingProp_A;//丢弃道具输入->invoke->丢弃道具()->invoke->checking()
                playingUIPro.ReplaceProp_AEvent += RefreshBag_A;//更换道具输入->invoke->更换道具()->invoke->刷新背包()
                playingUIPro.ReplaceProp_AEvent += RefreshContainer_A;//更换道具输入->invoke->更换道具()->invoke->刷新箱子()
                playingUIPro.ReplaceProp_AEvent += CheckingProp_A;//更换道具输入->invoke->更换道具()->invoke->checking()
                
                playingUIPro.CheckingProp_BEvent += CheckingProp_B;
                playingUIPro.DiscardProp_BEvent += RefreshBag_B;
                playingUIPro.DiscardProp_BEvent += CheckingProp_B;
            }
            else return;

            if (playingUIPro.pcsc is not null)
            {
                playingUIPro.pcsc.UpdateHP_AEvent += UpdateInfo_A;//回血()-v
                                                                  //扣血()->invoke->更新状态文本()
                                                                  
                playingUIPro.pcsc.UpdateMP_AEvent += UpdateInfo_A;//回蓝()-v
                                                                  //扣蓝()->invoke->更新状态文本()
                
                playingUIPro.pcsc.UpdateHP_BEvent += UpdateInfo_B;
                playingUIPro.pcsc.UpdateMP_BEvent += UpdateInfo_B;
            }

            if (playingUIPro.handlerA is not null)
            {
                playingUIPro.handlerA.OpenBagEvent += OpenBag_A;//开关背包输入->开关背包()->if(false)invoke->()
                playingUIPro.handlerA.CloseBagEvent += CloseBag_A;//开关背包输入->开关背包()->if(true)invoke->()
                
                playingUIPro.handlerA.OpenContainerEvent += OpenBag_A;//开关箱子输入->开关箱子()->if(false)invoke->()
                playingUIPro.handlerA.OpenContainerEvent += OpenContainer_A;//开关箱子输入->开关箱子()->if(false)invoke->()
                playingUIPro.handlerA.CloseContainerEvent += CloseContainer_A; //开关箱子输入->开关箱子()->if(true)invoke->()
                playingUIPro.handlerA.CloseContainerEvent += CloseBag_A; //开关箱子输入->开关箱子()->if(true)invoke->()
            }
            
            if (playingUIPro.handlerA is not null)
            {
                playingUIPro.handlerB.OpenBagEvent +=  OpenBag_B;
                playingUIPro.handlerB.CloseBagEvent += CloseBag_B;
            }
            
            playingUIPro.Subscribe();
            Debug.Log("playingUI事件注册成功");
        }

        private void UnsubscribeEvents()
        {
            playingUIPro.Unsubscribe();

            if (playingUIPro.pcsc != null)
            {
                playingUIPro.pcsc.UpdateHP_AEvent -= UpdateInfo_A;
                playingUIPro.pcsc.UpdateHP_BEvent -= UpdateInfo_B;
                playingUIPro.pcsc.UpdateMP_AEvent -= UpdateInfo_A;
                playingUIPro.pcsc.UpdateMP_BEvent -= UpdateInfo_B;
            }
            if (playingUIPro.handlerA != null)
            {
                playingUIPro.handlerA.OpenBagEvent -=  OpenBag_A;
                playingUIPro.handlerA.CloseBagEvent -= CloseBag_A;
            }
            if (playingUIPro.handlerB != null)
            {
                playingUIPro.handlerB.OpenBagEvent -=  OpenBag_B;
                playingUIPro.handlerB.CloseBagEvent -= CloseBag_B;
            }

            playingUIPro.CheckingProp_AEvent -= CheckingProp_A;
            playingUIPro.DiscardProp_AEvent -= RefreshBag_A;
            playingUIPro.DiscardProp_AEvent -= CheckingProp_A;
            playingUIPro.CheckingProp_BEvent -= CheckingProp_B;
            playingUIPro.DiscardProp_BEvent -= RefreshBag_B;
            playingUIPro.DiscardProp_BEvent -= CheckingProp_B;
        }

        /// <summary>
        /// playerA:更新HP MP显示
        /// </summary>
        private void UpdateInfo_A()
        {
            Debug.Log("UpdateInfo_A 被调用");
            infoNum_playerA.text = playingUIPro.InfoNum_playerA();
            barHP_A.style.width = Length.Percent(playingUIPro.HPPercent_A());
            barMP_A.style.width = Length.Percent(playingUIPro.MPPercent_A());
        }

        private void UpdateInfo_B()
        {
            Debug.Log("UpdateInfo_B 被调用");
            infoNum_playerB.text = playingUIPro.InfoNum_playerB();
            barHP_B.style.width = Length.Percent(playingUIPro.HPPercent_B());
            barMP_B.style.width = Length.Percent(playingUIPro.MPPercent_B());
        }
        
        /// <summary>
        /// playerA:打开背包|
        /// 响应输入事件，内部执行add UI，然后checking一次（使checking于首位）
        /// </summary>
        private void OpenBag_A()
        {
            root.Q<VisualElement>("CenterPivot").Q<VisualElement>("_LeftPivot").Add(BagUI_A);
            RefreshBag_A();
            propCatalogue_A = BagUI_A.Q<VisualElement>("PropsCatalogue");
            CheckingProp_A();
        }

        private void OpenBag_B()
        {
            root.Q<VisualElement>("CenterPivot").Q<VisualElement>("_RightPivot").Add(BagUI_B);
            RefreshBag_B();
            propCatalogue_B = BagUI_B.Q<VisualElement>("PropsCatalogue");
            CheckingProp_B();
        }

        /// <summary>
        /// playerA:关闭背包|
        /// 响应输入事件，内部执行remove UI，同时复原（归零）checking的index
        /// </summary>
        private void CloseBag_A()
        {
            root.Q<VisualElement>("CenterPivot").Q<VisualElement>("_LeftPivot").Remove(BagUI_A);
            propCatalogue_A = null;
            playingUIPro.ResetIndex_A();
        }

        private void CloseBag_B()
        {
            root.Q<VisualElement>("CenterPivot").Q<VisualElement>("_RightPivot").Remove(BagUI_B);
            propCatalogue_B = null;
            playingUIPro.SetPrevPropIndex_B(new Taffy.UI.Pro.Index(0));
        }

        /// <summary>
        /// 刷新背包UI|
        /// 清空重置bagUI。然后内部拿到Bag数据，轮询Add 道具框UI，同时更新 容量/以容纳 文本
        /// </summary>
        private void RefreshBag_A()
        {
            List<Prop> Bag_A = playingUIPro.GetBag_A();
            int BagCount_A =  Bag_A.Count;
            var BagCatalogue = BagUI_A.Q<VisualElement>("PropsCatalogue");
            BagCatalogue.Clear();
            
            BagUI_A.Q<Label>("BagInfo").text = playingUIPro.GetBagInfo_A();

            for(int i =  0; i < BagCount_A; i++)
            {
                VisualElement propCase = PropCaseUI.Instantiate().Q<VisualElement>("PropCase");
                propCase.style.backgroundImage = new StyleBackground(PropsTool.GetPropImage(Bag_A[i]));
                BagCatalogue.Add(propCase);
                Debug.Log("成功加进一个"+Bag_A[i].name);
            }
            playingUIPro.SetPrevPropIndex_A(playingUIPro.GetPropIndex_A());
        }

        private void RefreshBag_B()
        {
            List<Prop> Bag_B = playingUIPro.GetBag_B();
            var BagCatalogue = BagUI_B.Q<VisualElement>("PropsCatalogue");
            BagCatalogue.Clear();
            
            BagUI_B.Q<Label>("BagInfo").text = playingUIPro.GetBagInfo_B();

            for (int i = 0; i < Bag_B.Count; i++)
            {
                VisualElement propCase = PropCaseUI.Instantiate().Q<VisualElement>("PropCase");
                propCase.style.backgroundImage = new StyleBackground(PropsTool.GetPropImage(Bag_B[i]));
                BagCatalogue.Add(propCase);
                Debug.Log("成功加进一个" + Bag_B[i].name);
            }
            playingUIPro.SetPrevPropIndex_B(playingUIPro.GetPropIndex_B());
        }

        //////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// checking选中道具|
        /// 选中道具有光标、查看选中道具描述|
        /// 更新checking索引,使该索引道具背景变蓝，也就是光标的效果|
        /// 同时记录这次checking的索引，然后便于下次checking先取消那个道具的光标（下次的这次，语义等同于这次的上次），只保留当前checking的光标。这样有一种光标移动的感觉|
        /// 背包UI的精髓
        /// </summary>
        private void CheckingProp_A()
        {
            if (playingUIPro.GetBagCount_A() < 1)
            {
                BagUI_A.Q<Label>("PropName").text = "无";
                BagUI_A.Q<Label>("PropDescribe").text = "无";
                return;
            }
            if (propCatalogue_A == null) return;
            if (propCatalogue_A.childCount == 0) return;
            Index cur = playingUIPro.GetPropIndex_A();
            Index prev = playingUIPro.GetPrevPropIndex_A();
            if (prev.isInContainer)
            {
                if (containerUI_A is null) return;
                containerUI_A.Q("CenterPivot").ElementAt(prev.index).style.backgroundColor = StyleKeyword.Null;
            }
            else
                propCatalogue_A.ElementAt(prev.index).Q("CheckingBackground").style.backgroundColor = StyleKeyword.Null;

            if (cur.isInContainer)
            {
                if (containerUI_A is null) return;
                containerUI_A.Q("CenterPivot").ElementAt(cur.index).style.backgroundColor = new Color(0.4f, 0.5f, 0.8f, 0.8f);
            }
            else
                propCatalogue_A.ElementAt(cur.index).Q("CheckingBackground").style.backgroundColor = new Color(0.4f, 0.5f, 0.8f, 0.8f);

            DescribeProp_A();

            playingUIPro.SetPrevPropIndex_A(playingUIPro.GetPropIndex_A());
        }
        ///////////////////////////////////////////////////////////////////////////////////////////////////

        private void CheckingProp_B()
        {
            if (playingUIPro.GetBagCount_B() < 1)
            {
                BagUI_B.Q<Label>("PropName").text = "无";
                BagUI_B.Q<Label>("PropDescribe").text = "无";
                return;
            }
            if (playingUIPro.isBagClosed_B || propCatalogue_B == null) return;
            if (propCatalogue_B.childCount == 0) return;
            int cur = playingUIPro.GetPropIndex_B().index;
            int prev = playingUIPro.GetPrevPropIndex_B().index;
            propCatalogue_B.ElementAt(prev).Q("CheckingBackground").style.backgroundColor = StyleKeyword.Null;
            propCatalogue_B.ElementAt(cur).Q("CheckingBackground").style.backgroundColor = new Color(0.4f, 0.5f, 0.8f, 0.8f);

            DescribeProp_B();

            playingUIPro.SetPrevPropIndex_B(playingUIPro.GetPropIndex_B());
        }

        /// <summary>
        /// playerA:把描述写进UI文本
        /// </summary>
        private void DescribeProp_A()
        {
            if (BagUI_A is null) return;
            BagUI_A.Q<Label>("PropName").text = playingUIPro.GetCurrentPropName_A();
            BagUI_A.Q<Label>("PropDescribe").text =  playingUIPro.GetCurrentPropDescribe_A();
        }

        private void DescribeProp_B()
        {
            if (BagUI_B is null) return;
            BagUI_B.Q<Label>("PropName").text = playingUIPro.GetCurrentPropName_B();
            BagUI_B.Q<Label>("PropDescribe").text =  playingUIPro.GetCurrentPropDescribe_B();
        }

        /// <summary>
        /// playerA:打开箱子UI
        /// </summary>
        private void OpenContainer_A()
        {
            containerUI_A = containerUI.Instantiate().Q<VisualElement>("Container");
            RefreshContainer_A();
            CheckingProp_A();
        }

        private void CloseContainer_A()
        {
            root.Q<VisualElement>("ContainerPlace_A").Clear();
            playingUIPro.ResetIndex_A();
        }

        /// <summary>
        /// playerA:刷新箱子UI
        /// </summary>
        private void RefreshContainer_A()
        {
            if(containerUI_A is null) return;
            containerUI_A.Q<Label>("ContainerName").text = playingUIPro.GetContainerName_A();
            VisualElement containerPropsCatalogue = containerUI_A.Q<VisualElement>("CenterPivot");
            List<Prop> containerProp = playingUIPro.GetContainerProps_A();
            VisualElement containerPlaceUI = root.Q<VisualElement>("ContainerPlace_A");
            containerPlaceUI.Clear();
            containerPropsCatalogue.Clear();
            if (containerProp is null)
            {
                containerPlaceUI.Add(containerUI_A);
                return;
            }
            for (int i = 0; i < containerProp.Count; i++)
            {
                VisualElement propCase = PropCaseUI.Instantiate().Q<VisualElement>("PropCase");
                propCase.style.backgroundImage
                    = new StyleBackground(PropsTool.GetPropImage(containerProp[i]));
                propCase.style.height = Length.Percent(100);
                containerPropsCatalogue.Add(propCase);
            }
            containerPlaceUI.Add(containerUI_A);
        }
    }
}
