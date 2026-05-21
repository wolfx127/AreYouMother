using System;
using Taffy.UI.Pro;
using UnityEditor;
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

        [SerializeField] private VisualTreeAsset BagUI;

        private VisualElement BagUI_A;
        private VisualElement BagUI_B;

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
            if (playingUIPro.pcsc != null) SubscribeEvents();
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
            if (playingUIPro.pcsc != null) UnsubscribeEvents();
        }

        private void SubscribeEvents()
        {
            playingUIPro.pcsc.UpdateHP_AEvent += UpdateInfo_A;
            playingUIPro.pcsc.UpdateHP_BEvent += UpdateInfo_B;
            playingUIPro.pcsc.UpdateMP_AEvent += UpdateInfo_A;
            playingUIPro.pcsc.UpdateMP_BEvent += UpdateInfo_B;
            playingUIPro.handlerA.OpenBagEvent +=  OpenBag_A;
            playingUIPro.handlerA.CloseBagEvent += CloseBag_A;
            playingUIPro.handlerB.OpenBagEvent +=  OpenBag_B;
            playingUIPro.handlerB.CloseBagEvent += CloseBag_B;
            
            BagUI_A.Q<VisualElement>("PageUpBtn").RegisterCallback<ClickEvent>(PageUpBag_A);
            BagUI_A.Q<VisualElement>("PageDownBtn").RegisterCallback<ClickEvent>(PageDown_A);
            BagUI_B.Q<VisualElement>("PageUpBtn").RegisterCallback<ClickEvent>(PageUpBag_B);
            BagUI_B.Q<VisualElement>("PageDownBtn").RegisterCallback<ClickEvent>(PageDown_B);
            isNotSubscribed = false;
            Debug.Log("playingUI事件注册成功");
        }

        private void UnsubscribeEvents()
        {
            playingUIPro.pcsc.UpdateHP_AEvent -= UpdateInfo_A;
            playingUIPro.pcsc.UpdateHP_BEvent -= UpdateInfo_B;
            playingUIPro.pcsc.UpdateMP_AEvent -= UpdateInfo_A;
            playingUIPro.pcsc.UpdateMP_BEvent -= UpdateInfo_B;
            playingUIPro.handlerA.OpenBagEvent -=  OpenBag_A;
            playingUIPro.handlerA.CloseBagEvent -= CloseBag_A;
            playingUIPro.handlerB.OpenBagEvent -=  OpenBag_B;
            playingUIPro.handlerB.CloseBagEvent -= CloseBag_B;
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

        private void PageUpBag_A(ClickEvent evt)
        {
            Debug.Log("A背包切换到下一页");
        }

        private void PageUpBag_B(ClickEvent evt)
        {
            
        }
        
        private void PageDown_A(ClickEvent  evt)
        {
        }

        private void PageDown_B(ClickEvent evt)
        {
        }

        private void OpenBag_A()
        {
            root.Q<VisualElement>("CenterPivot").Q<VisualElement>("_LeftPivot").Add(BagUI_A);
        }

        private void OpenBag_B()
        {
            root.Q<VisualElement>("CenterPivot").Q<VisualElement>("_RightPivot").Add(BagUI_B);
        }

        private void CloseBag_A()
        {
            root.Q<VisualElement>("CenterPivot").Q<VisualElement>("_LeftPivot").Remove(BagUI_A);
        }
        
        private void CloseBag_B()
        {
            root.Q<VisualElement>("CenterPivot").Q<VisualElement>("_RightPivot").Remove(BagUI_B);
        }
    }
}
