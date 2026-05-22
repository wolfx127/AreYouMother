using System;
using System.Collections.Generic;
using Taffy.Data;
using Taffy.OverAllManager;
using Taffy.UI.Pro;
using UnityEngine;
using UnityEngine.UIElements;

namespace Taffy.UI
{
    public class PlayingUIManager:MonoBehaviour
    {
        private PlayingUI_pro playingUIPro = new PlayingUI_pro();
        private bool isNotSubscribed = true;
        
        private VisualElement root;
        private VisualElement barHP_A;
        private VisualElement barHP_B;
        private VisualElement barMP_A;
        private VisualElement barMP_B;
        private VisualElement PropCase;

        [SerializeField] private VisualTreeAsset BagUI;
        [SerializeField] private VisualTreeAsset PropCaseUI;

        private VisualElement BagUI_A;
        private VisualElement BagUI_B;
        private VisualElement propCatalogue_A;
        private VisualElement propCatalogue_B;

        private Label infoNum_playerA;
        private Label infoNum_playerB;


        private void Awake()
        {
            isNotSubscribed = true;
            
            root = GetComponent<UIDocument>().rootVisualElement;
            barHP_A = root.Q<VisualElement>("HP_PlayerA").Q<VisualElement>("CurrentHP");
            barHP_B = root.Q<VisualElement>("HP_PlayerB").Q<VisualElement>("CurrentHP");
            barMP_A = root.Q<VisualElement>("MP_PlayerA").Q<VisualElement>("CurrentMP");
            barMP_B = root.Q<VisualElement>("MP_PlayerB").Q<VisualElement>("CurrentMP");
            infoNum_playerA = root.Q<VisualElement>("Info_PlayerA").Q<Label>("HPandMPnum");
            infoNum_playerB = root.Q<VisualElement>("Info_PlayerB").Q<Label>("HPandMPnum");

            BagUI_A = BagUI.Instantiate();
            BagUI_B = BagUI.Instantiate();
            BagUI_A.style.height = Length.Percent(120);
            BagUI_B.style.height = Length.Percent(120);
        }

        private void OnEnable()
        {
            if (isNotSubscribed) SubscribeEvents();
        }

        private void Start()
        {
            if(isNotSubscribed) SubscribeEvents();
            infoNum_playerA.text = playingUIPro.InfoNum_playerA();
            infoNum_playerB.text = playingUIPro.InfoNum_playerB();

            Debug.Log("player数值文本初始化成功");
        }

        private void OnDisable()
        {
            if (!isNotSubscribed) UnsubscribeEvents();
        }

        private void SubscribeEvents()
        {
            playingUIPro.CheckingProp_AEvent += CheckingProp_A;
            playingUIPro.RemoveBagAt_AEvent += RefreshBag_A;
            playingUIPro.RemoveBagAt_AEvent += CheckingProp_A;
            playingUIPro.CheckingProp_BEvent += CheckingProp_B;
            playingUIPro.RemoveBagAt_BEvent += RefreshBag_B;
            playingUIPro.RemoveBagAt_BEvent += CheckingProp_B;

            if (playingUIPro.pcsc != null)
            {
                playingUIPro.pcsc.UpdateHP_AEvent += UpdateInfo_A;
                playingUIPro.pcsc.UpdateHP_BEvent += UpdateInfo_B;
                playingUIPro.pcsc.UpdateMP_AEvent += UpdateInfo_A;
                playingUIPro.pcsc.UpdateMP_BEvent += UpdateInfo_B;
            }
            playingUIPro.handlerA.OpenBagEvent +=  OpenBag_A;
            playingUIPro.handlerA.CloseBagEvent += CloseBag_A;
            playingUIPro.handlerB.OpenBagEvent +=  OpenBag_B;
            playingUIPro.handlerB.CloseBagEvent += CloseBag_B;

            playingUIPro.Subscribe();
            isNotSubscribed = false;
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
            playingUIPro.RemoveBagAt_AEvent -= RefreshBag_A;
            playingUIPro.RemoveBagAt_AEvent -= CheckingProp_A;
            playingUIPro.CheckingProp_BEvent -= CheckingProp_B;
            playingUIPro.RemoveBagAt_BEvent -= RefreshBag_B;
            playingUIPro.RemoveBagAt_BEvent -= CheckingProp_B;
            isNotSubscribed = true;
        }

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
        
        private void OpenBag_A()
        {
            root.Q<VisualElement>("CenterPivot").Q<VisualElement>("_LeftPivot").Add(BagUI_A);
            playingUIPro.ClampPropIndex_A();
            RefreshBag_A();
            propCatalogue_A = BagUI_A.Q<VisualElement>("PropsCatalogue");
            CheckingProp_A();
        }

        private void OpenBag_B()
        {
            root.Q<VisualElement>("CenterPivot").Q<VisualElement>("_RightPivot").Add(BagUI_B);
            playingUIPro.ClampPropIndex_B();
            RefreshBag_B();
            propCatalogue_B = BagUI_B.Q<VisualElement>("PropsCatalogue");
            CheckingProp_B();
        }

        private void CloseBag_A()
        {
            root.Q<VisualElement>("CenterPivot").Q<VisualElement>("_LeftPivot").Remove(BagUI_A);
            propCatalogue_A = null;
            playingUIPro.SetPrevPropIndex_A(0);
        }

        private void CloseBag_B()
        {
            root.Q<VisualElement>("CenterPivot").Q<VisualElement>("_RightPivot").Remove(BagUI_B);
            propCatalogue_B = null;
            playingUIPro.SetPrevPropIndex_B(0);
        }

        private void RefreshBag_A()
        {
            List<Prop> Bag_A = playingUIPro.GetBag_A();
            int BagCount_A =  Bag_A.Count;
            var BagCatalogue = BagUI_A.Q<VisualElement>("PropsCatalogue");
            BagCatalogue.Clear();
            
            BagUI_A.Q<Label>("BagInfo").text = playingUIPro.GetBagInfo_A();

            for(int i =  0; i < BagCount_A; i++)
            {
                VisualElement propCase = PropCaseUI.Instantiate();
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
                VisualElement propCase = PropCaseUI.Instantiate();
                propCase.style.backgroundImage = new StyleBackground(PropsTool.GetPropImage(Bag_B[i]));
                BagCatalogue.Add(propCase);
                Debug.Log("成功加进一个" + Bag_B[i].name);
            }
            playingUIPro.SetPrevPropIndex_B(playingUIPro.GetPropIndex_B());
        }

        private void CheckingProp_A()
        {
            if (playingUIPro.GetBagCount_A() < 1) return;
            if (playingUIPro.isBagClosed_A || propCatalogue_A == null) return;
            if (propCatalogue_A.childCount == 0) return;
            int cur = playingUIPro.GetPropIndex_A();
            int prev = playingUIPro.GetPrevPropIndex_A();
            propCatalogue_A.ElementAt(prev).Q("CheckingBackground").style.backgroundColor = StyleKeyword.Null;
            propCatalogue_A.ElementAt(cur).Q("CheckingBackground").style.backgroundColor = new Color(0.4f, 0.5f, 0.8f, 0.8f);
            
            DescribeProp_A();
            
            playingUIPro.SetPrevPropIndex_A(cur);
        }

        private void CheckingProp_B()
        {
            if (playingUIPro.GetBagCount_B() < 1) return;
            if (playingUIPro.isBagClosed_B || propCatalogue_B == null) return;
            if (propCatalogue_B.childCount == 0) return;
            int cur = playingUIPro.GetPropIndex_B();
            int prev = playingUIPro.GetPrevPropIndex_B();
            propCatalogue_B.ElementAt(prev).Q("CheckingBackground").style.backgroundColor = StyleKeyword.Null;
            propCatalogue_B.ElementAt(cur).Q("CheckingBackground").style.backgroundColor = new Color(0.4f, 0.5f, 0.8f, 0.8f);
            
            DescribeProp_B();
            
            playingUIPro.SetPrevPropIndex_B(cur);
        }

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
    }
}
