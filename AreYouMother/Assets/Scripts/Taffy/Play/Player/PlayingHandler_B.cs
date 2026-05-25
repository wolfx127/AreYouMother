using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Taffy.Play.Player
{
    public class PlayingHandler_B:MonoBehaviour
    {
        public static PlayingHandler_B Instance { get; private set; }

        [SerializeField] private GameObject        OpenContainerTriggerGO;
        [SerializeField] private PlayingTrigger_B  OpenContainerTrigger;
        private PlayingInputAction playingInputAction;

        [SerializeField] private float speed = 5f;
        private Vector3 moveDir = Vector3.forward;

        private  bool  inEvacuateZone;
        public   bool  isBagClosed           = true;
        public   bool  isContainerClosed     = true;
        public   bool  DisableOpenContainer => OpenContainerTrigger.disableOpenContainer;

        public event Action               EvacuateEvent;
        public event Action               OpenBagEvent;
        public event Action               CloseBagEvent;
        public event Action<Vector2Int>   ChoosePropArrowEvent;
        public event Action               DiscardPropEvent;
        public event Action               OpenContainerEvent;
        public event Action               CloseContainerEvent;
        public event Action               ReplacePropEvent;

        private void Awake()
        {
            Instance = this;
            OpenContainerTriggerGO = gameObject.transform.Find("OpenContainerTrigger").gameObject;
            OpenContainerTrigger = OpenContainerTriggerGO.GetComponent<PlayingTrigger_B>();
            playingInputAction = new PlayingInputAction();
            DisableChooseProp();
            DisableDiscardProp();
            DisableReplaceProp();
        }

        private void OnEnable()
        {
            playingInputAction.PlayerB.Enable();
            playingInputAction.PlayerB.Evacuate.performed += OnEvacuate;
            playingInputAction.PlayerB.OpenOrCloseBag.performed += OpenOrCloseBag;
            playingInputAction.PlayerB.OpenOrCloseContainer.performed += OpenOrCloseContainer;
        }

        private void OnDisable()
        {
            playingInputAction.PlayerB.Evacuate.performed -= OnEvacuate;
            playingInputAction.PlayerB.OpenOrCloseBag.performed -= OpenOrCloseBag;
            playingInputAction.PlayerB.OpenOrCloseContainer.performed -= OpenOrCloseContainer;
            playingInputAction.PlayerB.Disable();
        }

        private void Update()
        {
            if (isBagClosed||isContainerClosed)
            {
                Vector2 moveB = playingInputAction.PlayerB.Move.ReadValue<Vector2>();
                moveDir = new Vector3(moveB.x, 0, moveB.y);
                transform.Translate(speed * Time.deltaTime * moveDir, Space.World);
                if (moveDir != Vector3.zero) OpenContainerTriggerGO.transform.position = transform.position + moveDir.normalized * 1.4f;
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
                EnableChooseProp();
                EnableDiscardProp();
                Debug.Log("B打开背包");
                OpenBagEvent?.Invoke();
            }
            else
            {
                isBagClosed = true;
                DisableChooseProp();
                DisableDiscardProp();
                Debug.Log("B关闭背包");
                CloseBagEvent?.Invoke();
            }
        }

        private void EnableChooseProp()
        {
            playingInputAction.PlayerB.Move.Disable();
            playingInputAction.PlayerB.ChooseProp.Enable();
            playingInputAction.PlayerB.ChooseProp.performed += ChoosePropArrow;
        }

        private void EnableDiscardProp()
        {
            playingInputAction.PlayerB.OpenOrCloseBag.Enable();
            playingInputAction.PlayerB.DiscardProp.Enable();
            playingInputAction.PlayerB.DiscardProp.performed += DiscardProp;
        }

        private void DisableChooseProp()
        {
            playingInputAction.PlayerB.Move.Enable();
            playingInputAction.PlayerB.ChooseProp.performed -= ChoosePropArrow;
            playingInputAction.PlayerB.ChooseProp.Disable();
        }

        private void DisableDiscardProp()
        {
            playingInputAction.PlayerB.DiscardProp.performed -= DiscardProp;
            playingInputAction.PlayerB.DiscardProp.Disable();
            playingInputAction.PlayerB.OpenOrCloseBag.Enable();
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

        private void DiscardProp(InputAction.CallbackContext ctx)
        {
            DiscardPropEvent?.Invoke();
        }

        private void OpenOrCloseContainer(InputAction.CallbackContext ctx)
        {
            if (!DisableOpenContainer)
            {
                if (isContainerClosed)
                {
                    isContainerClosed = false;
                    EnableChooseProp();
                    EnableReplaceProp();
                    Debug.Log("B打开箱子");
                    OpenContainerEvent?.Invoke();
                }
                else
                {
                    isContainerClosed = true;
                    DisableChooseProp();
                    DisableReplaceProp();
                    Debug.Log("B关闭箱子");
                    CloseContainerEvent?.Invoke();
                }
            }
        }

        private void EnableReplaceProp()
        {
            playingInputAction.PlayerB.OpenOrCloseBag.Disable();
            playingInputAction.PlayerB.ReplaceProp.Enable();
            playingInputAction.PlayerB.ReplaceProp.performed += ReplaceProp;
        }

        private void DisableReplaceProp()
        {
            playingInputAction.PlayerB.ReplaceProp.performed -= ReplaceProp;
            playingInputAction.PlayerB.ReplaceProp.Disable();
            playingInputAction.PlayerB.OpenOrCloseBag.Enable();
        }

        private void ReplaceProp(InputAction.CallbackContext ctx)
        {
            ReplacePropEvent?.Invoke();
        }
    }
}
