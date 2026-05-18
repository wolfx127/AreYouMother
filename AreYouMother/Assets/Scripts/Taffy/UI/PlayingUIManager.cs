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

        private void Start()
        {
            infoNum_playerA.text = playingUIPro.InfoNum_playerA();
            infoNum_playerB.text = playingUIPro.InfoNum_playerB();
            Debug.Log("player数值文本初始化成功");
        }
    }
}
