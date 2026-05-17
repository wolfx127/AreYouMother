using System;
using Taffy.UI.Pro;
using UnityEngine;
using UnityEngine.UIElements;

namespace Taffy.UI
{
    public class PlayingUIManager:MonoBehaviour
    {
        private PlayingUI_pro playingUIPro = new PlayingUI_pro();
        private VisualElement root;
        private VisualElement currentHP_playerA;
        private VisualElement currentHP_playerB;
        private VisualElement currentMP_playerA;
        private VisualElement currentMP_playerB;

        private Label infoNum_playerA;
        private Label infoNum_playerB;
        
        
        private void Awake()
        {
            root = GetComponent<UIDocument>().rootVisualElement;
            currentHP_playerA = root.Q<VisualElement>("HP_PlayerA").Q<VisualElement>("CurrentHP");
            currentHP_playerB = root.Q<VisualElement>("HP_PlayerB").Q<VisualElement>("CurrentHP");
            currentMP_playerA = root.Q<VisualElement>("MP_PlayerA").Q<VisualElement>("CurrentMP");
            currentMP_playerB = root.Q<VisualElement>("MP_PlayerB").Q<VisualElement>("CurrentMP");
            infoNum_playerA = root.Q<VisualElement>("Info_PlayerA").Q<Label>("HPandMPnum");
            infoNum_playerB = root.Q<VisualElement>("Info_PlayerB").Q<Label>("HPandMPnum");
        }

        private void OnEnable()
        {
            playingUIPro.SubscribeEvents();
        }

        private void OnDisable()
        {
            playingUIPro.UnSubscribeEvents();
        }

        private void Start()
        {
            infoNum_playerA.text = playingUIPro.InfoNum_playerA();
            infoNum_playerB.text = playingUIPro.InfoNum_playerB();
            Debug.Log("player数值文本初始化成功");
        }
    }
}
