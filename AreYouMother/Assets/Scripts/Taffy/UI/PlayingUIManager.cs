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
        
        
        private void Awake()
        {
            root = GetComponent<UIDocument>().rootVisualElement;
            currentHP_playerA = root.Q<VisualElement>("HP_PlayerA").Q<VisualElement>("CurrentHP");
            currentHP_playerB = root.Q<VisualElement>("HP_PlayerB").Q<VisualElement>("CurrentHP");
            currentMP_playerA = root.Q<VisualElement>("MP_PlayerA").Q<VisualElement>("CurrentMP");
            currentMP_playerB = root.Q<VisualElement>("MP_PlayerB").Q<VisualElement>("CurrentMP");
        }

        private void OnEnable()
        {
            playingUIPro.SubscribeEvents();
        }

        private void OnDisable()
        {
            playingUIPro.UnSubscribeEvents();
        }

        private void UpdateHPandMPUI()
        {
            
        }
    }

}
