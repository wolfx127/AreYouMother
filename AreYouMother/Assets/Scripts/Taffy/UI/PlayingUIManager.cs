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
        private VisualElement root;
        private VisualElement barHP_A;
        private VisualElement barHP_B;
        private VisualElement barMP_A;
        private VisualElement barMP_B;

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
        }

        private Button debugInjuryA;
        private Button debugInjuryB;

        private void OnEnable()
        {
            if (playingUIPro.pcsc != null) SubscribeEvents();
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
            if (playingUIPro.pcsc != null) UnsubscribeEvents();
        }

        private void SubscribeEvents()
        {
            playingUIPro.pcsc.UpdateHP_AEvent += UpdateInfo_A;
            playingUIPro.pcsc.UpdateHP_BEvent += UpdateInfo_B;
            playingUIPro.pcsc.UpdateMP_AEvent += UpdateInfo_A;
            playingUIPro.pcsc.UpdateMP_BEvent += UpdateInfo_B;
        }

        private void UnsubscribeEvents()
        {
            playingUIPro.pcsc.UpdateHP_AEvent -= UpdateInfo_A;
            playingUIPro.pcsc.UpdateHP_BEvent -= UpdateInfo_B;
            playingUIPro.pcsc.UpdateMP_AEvent -= UpdateInfo_A;
            playingUIPro.pcsc.UpdateMP_BEvent -= UpdateInfo_B;
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
    }
}
