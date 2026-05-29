using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Taffy.Play.Player
{
    public class PlayingHandler_A:MonoBehaviour
    {
        public static PlayingHandler_A Instance { get; private set; }

        [SerializeField] private GameObject        OpenContainerTriggerGO;
        [SerializeField] private PlayingTrigger_A  OpenContainerTrigger;
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
        public event Action UsePropEvent;

        private void Awake()
        {
            Instance = this;
            OpenContainerTriggerGO = gameObject.transform.Find("OpenContainerTrigger").gameObject;
            OpenContainerTrigger = OpenContainerTriggerGO.GetComponent<PlayingTrigger_A>();
            playingInputAction =  new PlayingInputAction();
            DisableChooseProp();
            DisableDiscardProp();
            DisableReplaceProp();
        }

        private void OnEnable()
        {
            playingInputAction.PlayerA.Enable();
            playingInputAction.PlayerA.Evacuate.performed += OnEvacuate;//撤退输入->撤退()
            playingInputAction.PlayerA.OpenOrCloseBag.performed += OpenOrCloseBag;//开关背包输入->开关背包()
            playingInputAction.PlayerA.OpenOrCloseContainer.performed += OpenOrCloseContainer;//开关箱子输入->开关箱子()
        }

        private void OnDisable()
        {
            playingInputAction.PlayerA.Evacuate.performed -= OnEvacuate;
            playingInputAction.PlayerA.OpenOrCloseBag.performed -= OpenOrCloseBag;
            playingInputAction.PlayerA.OpenOrCloseContainer.performed -= OpenOrCloseContainer;
            playingInputAction.PlayerA.Disable();
        }

        private void Update()
        {
            if (isBagClosed||isContainerClosed)
            {
                Vector2 moveA = playingInputAction.PlayerA.Move.ReadValue<Vector2>();
                moveDir = new Vector3(moveA.x, 0, moveA.y);
                transform.Translate(speed * Time.deltaTime * moveDir, Space.World);
                if (moveDir != Vector3.zero) OpenContainerTriggerGO.transform.position = transform.position + moveDir.normalized * 1.4f;
            }
        }

        //看是否在撤离点内。为什么不用OnTriggerStay呢？因为Stay是每帧调用，这就一个bool值就解决了
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
                Debug.Log("PlayerA 撤离");
                EvacuateEvent?.Invoke();
                playingInputAction.PlayerA.Disable();
            }
        }

        private void OpenOrCloseBag(InputAction.CallbackContext ctx)
        {
            if (isBagClosed)
            {
                isBagClosed = false;
                EnableChooseProp();
                EnableDiscardProp();
                Debug.Log("A打开背包");
                OpenBagEvent?.Invoke();
            }
            else
            {
                isBagClosed = true;
                DisableChooseProp();
                DisableDiscardProp();
                Debug.Log("A关闭背包");
                CloseBagEvent?.Invoke();
            }
        }

        private void EnableChooseProp()
        {
            playingInputAction.PlayerA.Move.Disable();
            playingInputAction.PlayerA.ChooseProp.Enable();
            playingInputAction.PlayerA.ChooseProp.performed += ChoosePropArrow;
        }

        private void EnableDiscardProp()
        {
            playingInputAction.PlayerA.OpenOrCloseBag.Enable();
            playingInputAction.PlayerA.DiscardProp.Enable();
            playingInputAction.PlayerA.DiscardProp.performed += DiscardProp;
            playingInputAction.PlayerA.UseProp.Enable();
            playingInputAction.PlayerA.UseProp.performed += UseProp;
        }

        private void DisableChooseProp()
        {
            playingInputAction.PlayerA.Move.Enable();
            playingInputAction.PlayerA.ChooseProp.performed -= ChoosePropArrow;
            playingInputAction.PlayerA.ChooseProp.Disable();
            playingInputAction.PlayerA.UseProp.performed -= UseProp;
            playingInputAction.PlayerA.UseProp.Disable();
        }

        private void DisableDiscardProp()
        {
            playingInputAction.PlayerA.DiscardProp.performed -= DiscardProp;
            playingInputAction.PlayerA.DiscardProp.Disable();
            playingInputAction.PlayerA.OpenOrCloseBag.Enable();
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
                    Debug.Log("A打开箱子");
                    OpenContainerEvent?.Invoke();
                }
                else
                {
                    isContainerClosed = true;
                    DisableChooseProp();
                    DisableReplaceProp();
                    Debug.Log("A关闭箱子");
                    CloseContainerEvent?.Invoke();
                }
            }
        }
        
        private void EnableReplaceProp()
        {
            playingInputAction.PlayerA.OpenOrCloseBag.Disable();
            playingInputAction.PlayerA.ReplaceProp.Enable();
            playingInputAction.PlayerA.ReplaceProp.performed += ReplaceProp;
        }

        private void DisableReplaceProp()
        {
            playingInputAction.PlayerA.ReplaceProp.performed -= ReplaceProp;
            playingInputAction.PlayerA.ReplaceProp.Disable();
            playingInputAction.PlayerA.OpenOrCloseBag.Enable();
        }

        private void ReplaceProp(InputAction.CallbackContext ctx)
        {
            ReplacePropEvent?.Invoke();
        }

        private void UseProp(InputAction.CallbackContext ctx)
        {
            UsePropEvent?.Invoke();
        }
    }
}
