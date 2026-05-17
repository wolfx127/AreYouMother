using System;
using Taffy.UI.Pro;
using UnityEngine;
using UnityEngine.UIElements;

namespace Taffy.UI
{
    public class HomeUIManager : MonoBehaviour
    {
        private HomeUI_pro homeUIPro = new HomeUI_pro();
        private VisualElement root;
        private Button engagePlayBtn;
        private Button exitGameBtn;

        private void Awake()
        {
            root = GetComponent<UIDocument>().rootVisualElement;
            engagePlayBtn = root.Q<Button>("EngagePlayBtn");
            exitGameBtn = root.Q<Button>("ExitGameBtn");
        }
        
        private void OnEnable()
        {
            engagePlayBtn.clicked += homeUIPro.ChangeSceneToPlaying;
            exitGameBtn.clicked += homeUIPro.ExitGame;
        }

        private void OnDisable()
        {
            engagePlayBtn.clicked -= homeUIPro.ChangeSceneToPlaying;
            exitGameBtn.clicked -= homeUIPro.ExitGame;
        }
    }
}
