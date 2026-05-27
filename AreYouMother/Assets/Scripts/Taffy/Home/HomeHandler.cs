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
        
        public event Action<Vector2Int> ChooseProp_AEvent;
        public event Action<Vector2Int> ChooseProp_BEvent;
        public event Action ReplaceProp_AEvent;
        public event Action ReplaceProp_BEvent;

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
            playerInputAction.PlayerA.ChooseProp.performed += ChooseProp_A;
            playerInputAction.PlayerA.ReplaceProp.performed += ReplaceProp_A;
            
            playerInputAction.PlayerB.ChooseProp.Enable();
            playerInputAction.PlayerB.ReplaceProp.Enable();
            playerInputAction.PlayerB.ChooseProp.performed += ChooseProp_B;
            playerInputAction.PlayerB.ReplaceProp.performed += ReplaceProp_B;
        }

        private void OnDisable()
        {
            playerInputAction.PlayerA.Disable();
            playerInputAction.PlayerB.Disable();
            
            playerInputAction.PlayerA.ChooseProp.performed -= ChooseProp_A;
            playerInputAction.PlayerA.ReplaceProp.performed -= ReplaceProp_A;
            playerInputAction.PlayerA.ChooseProp.Disable();
            playerInputAction.PlayerA.ReplaceProp.Disable();
            
            playerInputAction.PlayerB.ChooseProp.performed -= ChooseProp_B;
            playerInputAction.PlayerB.ReplaceProp.performed -= ReplaceProp_B;
            playerInputAction.PlayerB.ChooseProp.Disable();
            playerInputAction.PlayerB.ReplaceProp.Disable();
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
    }
}
