using System;
using Taffy.OverAllManager;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Taffy.Home
{
    public class HomeHandler:MonoBehaviour
    {
        private PlayingInputAction playerInputAction;
        public static HomeHandler Instance { get; private set; }
        public int index_A = 0;
        public int index_B = 0;
        public int indexplace_A = 0;
        
        public event Action<Vector2Int> ChooseProp_AEvent;
        public event Action<Vector2Int> ChooseProp_BEvent;
        public event Action ReplaceProp_AEvent;
        public event Action ReplaceProp_BEvent;
        public event Action UseProp_AEvent;
        public event Action UseProp_BEvent;

        private void Awake()
        {
            Instance = this;
            playerInputAction = new PlayingInputAction();
        }

        private void OnEnable()
        {
            playerInputAction.PlayerA.Enable();
            playerInputAction.PlayerB.Enable();
            
            playerInputAction.PlayerA.ChooseProp.Enable();
            playerInputAction.PlayerA.ReplaceProp.Enable();
            playerInputAction.PlayerA.UseProp.Enable();
            playerInputAction.PlayerA.ChooseProp.performed += ChooseProp_A;
            playerInputAction.PlayerA.ReplaceProp.performed += ReplaceProp_A;
            playerInputAction.PlayerA.UseProp.performed += UseProp_A;
            
            playerInputAction.PlayerB.ChooseProp.Enable();
            playerInputAction.PlayerB.ReplaceProp.Enable();
            playerInputAction.PlayerB.UseProp.Enable();
            playerInputAction.PlayerB.ChooseProp.performed += ChooseProp_B;
            playerInputAction.PlayerB.ReplaceProp.performed += ReplaceProp_B;
            playerInputAction.PlayerB.UseProp.performed += UseProp_B;
            
            EventBus.Subscribe<ChangeSceneHomeToPlayingEvent>(DisposeInputAction);
        }

        private void OnDisable()
        {
            
            EventBus.Subscribe<ChangeSceneHomeToPlayingEvent>(DisposeInputAction);
            playerInputAction.PlayerA.Disable();
            playerInputAction.PlayerB.Disable();
            
            playerInputAction.PlayerA.ChooseProp.performed -= ChooseProp_A;
            playerInputAction.PlayerA.ReplaceProp.performed -= ReplaceProp_A;
            playerInputAction.PlayerA.UseProp.performed -= UseProp_A;
            playerInputAction.PlayerA.ChooseProp.Disable();
            playerInputAction.PlayerA.ReplaceProp.Disable();
            playerInputAction.PlayerA.UseProp.Disable();
            
            playerInputAction.PlayerB.ChooseProp.performed -= ChooseProp_B;
            playerInputAction.PlayerB.ReplaceProp.performed -= ReplaceProp_B;
            playerInputAction.PlayerB.UseProp.performed -= UseProp_B;
            playerInputAction.PlayerB.ChooseProp.Disable();
            playerInputAction.PlayerB.ReplaceProp.Disable();
            playerInputAction.PlayerB.UseProp.Disable();
        }

        private void ChooseProp_A(InputAction.CallbackContext ctx)
        {
            Vector2 v = ctx.ReadValue<Vector2>();
            ChooseProp_AEvent?.Invoke(new Vector2Int((int)v.x, (int)v.y));
            Debug.Log("更换checkingA");
        }
        private void ChooseProp_B(InputAction.CallbackContext ctx)
        {
            Vector2 v = ctx.ReadValue<Vector2>();
            ChooseProp_BEvent?.Invoke(new Vector2Int((int)v.x, (int)v.y));
            Debug.Log("更换checkingB");
        }

        private void ReplaceProp_A(InputAction.CallbackContext ctx)
        {
            ReplaceProp_AEvent?.Invoke();
            Debug.Log("换道具位置A");
        }

        private void ReplaceProp_B(InputAction.CallbackContext ctx)
        {
            ReplaceProp_BEvent?.Invoke();
            Debug.Log("换道具位置B");
        }

        private void UseProp_A(InputAction.CallbackContext ctx)
        {
            UseProp_AEvent?.Invoke();
            Debug.Log($"A使用了一个道具");
        }

        private void UseProp_B(InputAction.CallbackContext ctx)
        {
            UseProp_BEvent?.Invoke();
            Debug.Log($"B使用了一个道具");
        }
        
        private void DisposeInputAction(ChangeSceneHomeToPlayingEvent evt)
        {
            playerInputAction.Dispose();
        }
    }
}
