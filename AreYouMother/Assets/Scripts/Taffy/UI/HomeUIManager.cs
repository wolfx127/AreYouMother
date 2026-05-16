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

        private void Awake()
        {
            root = GetComponent<UIDocument>().rootVisualElement;
            engagePlayBtn = root.Q<Button>("EngagePlayBtn");
        }
        
        private void OnEnable()
        {
            engagePlayBtn.clicked += OnEngagePlayBtnClicked;
        }

        private void OnDisable()
        {
            engagePlayBtn.clicked -= OnEngagePlayBtnClicked;
        }
        
        private void OnEngagePlayBtnClicked()
        {
            homeUIPro.ChangeSceneToPlaying();
        }
        
        
    }
}
