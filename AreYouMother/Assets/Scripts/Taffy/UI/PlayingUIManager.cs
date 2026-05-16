using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Taffy.UI
{
    public class PlayingUIManager:MonoBehaviour
    {
        private VisualElement root;
        private VisualElement currentHP_playerA;
        private VisualElement currentHP_playerB;
        private VisualElement currentMP_playerA;
        private VisualElement currentMP_playerB;
        
        private int maxHP_playerA;
        private int maxHP_playerB;
        private int maxMP_playerA;
        private int maxMP_playerB;
        
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
            
        }
        
        private void InitialBaseState(PlayerAorB player,int maxHP,int maxMP)
        {
            if (player is PlayerAorB.playerA)
            {
                maxHP_playerA = maxHP;
                maxMP_playerA = maxMP;
            }
            if(player is PlayerAorB.playerB)
            {
                maxHP_playerB = maxHP;
                maxMP_playerB = maxMP;
            }
        }

        private void updateHP(PlayerAorB player,int curHP)
        {
            if (player is PlayerAorB.playerA)
            {
                currentHP_playerA.style.width = curHP * 1.0f / maxHP_playerA * 100;
            }
            if (player is PlayerAorB.playerB)
            {
                currentHP_playerB.style.width = curHP * 1.0f / maxHP_playerB * 100;
            }
        }
    }

    enum PlayerAorB
    {
        playerA,
        playerB
    }
}
