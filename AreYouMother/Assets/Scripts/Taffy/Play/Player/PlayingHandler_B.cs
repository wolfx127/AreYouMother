using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Taffy.Play.Player
{
    public class PlayingHandler_B:MonoBehaviour
    {
        public static PlayingHandler_B Instance { get; private set; }

        private PlayingInputAction playingInputAction;
        [SerializeField] private float speed = 5f;
        private bool inEvacuateZone;
        public bool isBagClosed = true;

        public event Action EvacuateEvent;
        public event Action OpenBagEvent;
        public event Action CloseBagEvent;
        public event Action<Vector2Int> ChoosePropArrowEvent;
        public event Action RemovePropAtEvent;

        private void Awake()
        {
            Instance = this;
            playingInputAction = new PlayingInputAction();
            DisableChooseProp_B();
        }

        private void OnEnable()
        {
            playingInputAction.PlayerB.Enable();
            playingInputAction.PlayerB.Evacuate.performed += OnEvacuate;
            playingInputAction.PlayerB.OpenOrCloseBag.performed += OpenOrCloseBag;
        }

        private void OnDisable()
        {
            playingInputAction.PlayerB.Evacuate.performed -= OnEvacuate;
            playingInputAction.PlayerB.OpenOrCloseBag.performed -= OpenOrCloseBag;
            playingInputAction.PlayerB.Disable();
        }

        private void Update()
        {
            if (isBagClosed)
            {
                Vector2 moveB = playingInputAction.PlayerB.Move.ReadValue<Vector2>();
                transform.Translate( speed * Time.deltaTime * new Vector3(moveB.x, 0, moveB.y),Space.World);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("EvacuateZone")) inEvacuateZone = true;
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("EvacuateZone")) inEvacuateZone = false;
        }

        private void OnEvacuate(InputAction.CallbackContext ctx)
        {
            if (inEvacuateZone)
            {
                Debug.Log("PlayerB 撤离");
                EvacuateEvent?.Invoke();
            }
        }

        private void OpenOrCloseBag(InputAction.CallbackContext ctx)
        {
            if (isBagClosed)
            {
                isBagClosed = false;
                EnableChooseProp_B();
                Debug.Log("B打开背包");
                OpenBagEvent?.Invoke();
            }
            else
            {
                isBagClosed = true;
                DisableChooseProp_B();
                Debug.Log("B关闭背包");
                CloseBagEvent?.Invoke();
            }
        }

        private void EnableChooseProp_B()
        {
            playingInputAction.PlayerB.Move.Disable();
            playingInputAction.PlayerB.ChooseProp.Enable();
            playingInputAction.PlayerB.RemoveBagAt.Enable();
            playingInputAction.PlayerB.ChooseProp.performed += ChoosePropArrow;
            playingInputAction.PlayerB.RemoveBagAt.performed += RemovePropAt;
        }

        private void DisableChooseProp_B()
        {
            playingInputAction.PlayerB.Move.Enable();
            playingInputAction.PlayerB.ChooseProp.Disable();
            playingInputAction.PlayerB.RemoveBagAt.Disable();
            playingInputAction.PlayerB.ChooseProp.performed -= ChoosePropArrow;
            playingInputAction.PlayerB.RemoveBagAt.performed -= RemovePropAt;
        }

        private void ChoosePropArrow(InputAction.CallbackContext ctx)
        {
            Vector2 raw = ctx.ReadValue<Vector2>();
            Vector2Int dir;
            if (Mathf.Abs(raw.x) >= Mathf.Abs(raw.y))
                dir = raw.x > 0 ? Vector2Int.right : Vector2Int.left;
            else
                dir = raw.y > 0 ? Vector2Int.up : Vector2Int.down;
            ChoosePropArrowEvent?.Invoke(dir);
        }

        private void RemovePropAt(InputAction.CallbackContext ctx)
        {
            RemovePropAtEvent?.Invoke();
        }
    }
}
