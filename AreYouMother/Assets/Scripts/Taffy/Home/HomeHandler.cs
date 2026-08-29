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
        private int index_A = 0;
        private int index_B = 0;
        
        private OverAllPlayerController oapc = OverAllPlayerController.Instance;
        
        public event Action<int> ChooseProp_AEvent;
        public event Action<int> ChooseProp_BEvent;
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

            EventBus.Unsubscribe<ChangeSceneHomeToPlayingEvent>(DisposeInputAction);
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
            int count = oapc.GetBag_A().Count;
            if (count == 0) return;

            if      (v.x == 0 && v.y > 0)  index_A = UITools.GetNeighborIndex(index_A, count, BagDirection.Up);
            else if (v.x == 0 && v.y < 0)  index_A = UITools.GetNeighborIndex(index_A, count, BagDirection.Down);
            else if (v.x < 0 && v.y == 0)  index_A = UITools.GetNeighborIndex(index_A, count, BagDirection.Left);
            else if (v.x > 0 && v.y == 0)  index_A = UITools.GetNeighborIndex(index_A, count, BagDirection.Right);

            ChooseProp_AEvent?.Invoke(index_A);
            Debug.Log("更换checkingA");
        }
        private void ChooseProp_B(InputAction.CallbackContext ctx)
        {
            Vector2 v = ctx.ReadValue<Vector2>();
            int count = oapc.GetBag_B().Count;
            if (count == 0) return;

            if      (v.x == 0 && v.y > 0)  index_B = UITools.GetNeighborIndex(index_B, count, BagDirection.Up);
            else if (v.x == 0 && v.y < 0)  index_B = UITools.GetNeighborIndex(index_B, count, BagDirection.Down);
            else if (v.x < 0 && v.y == 0)  index_B = UITools.GetNeighborIndex(index_B, count, BagDirection.Left);
            else if (v.x > 0 && v.y == 0)  index_B = UITools.GetNeighborIndex(index_B, count, BagDirection.Right);

            ChooseProp_AEvent?.Invoke(index_A);
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
