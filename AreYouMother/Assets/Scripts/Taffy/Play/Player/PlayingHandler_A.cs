using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Taffy.Play.Player
{
    public class PlayingHandler_A:MonoBehaviour
    {
        public static PlayingHandler_A Instance { get; private set; }

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
            playingInputAction =  new PlayingInputAction();
            DisableChooseProp_A();
        }

        private void OnEnable()
        {
            playingInputAction.PlayerA.Enable();
            playingInputAction.PlayerA.Evacuate.performed += OnEvacuate;
            playingInputAction.PlayerA.OpenOrCloseBag.performed += OpenOrCloseBag;
        }

        private void OnDisable()
        {
            playingInputAction.PlayerA.Evacuate.performed -= OnEvacuate;
            playingInputAction.PlayerA.OpenOrCloseBag.performed -= OpenOrCloseBag;
            playingInputAction.PlayerA.Disable();
        }

        private void Update()
        {
            if (isBagClosed)
            {
                Vector2 moveA = playingInputAction.PlayerA.Move.ReadValue<Vector2>();
                transform.Translate( speed * Time.deltaTime * new Vector3(moveA.x, 0, moveA.y),Space.World);
            }
        }

        //看是否在撤离点内。为什么不用OnTrigglerStay呢？因为Stay是每帧调用，这就一个bool值就解决了
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
            }
        }

        private void OpenOrCloseBag(InputAction.CallbackContext ctx)
        {
            if (isBagClosed)
            {
                isBagClosed = false;
                EnableChooseProp_A();
                Debug.Log("A打开背包");
                OpenBagEvent?.Invoke();
            }
            else
            {
                isBagClosed = true;
                DisableChooseProp_A();
                Debug.Log("A关闭背包");
                CloseBagEvent?.Invoke();
            }
        }

        private void EnableChooseProp_A()
        {
            playingInputAction.PlayerA.Move.Disable();
            playingInputAction.PlayerA.ChooseProp.Enable();
            playingInputAction.PlayerA.RemoveBagAt.Enable();
            playingInputAction.PlayerA.ChooseProp.performed += ChoosePropArrow;
            playingInputAction.PlayerA.RemoveBagAt.performed += RemovePropAt;
        }

        private void DisableChooseProp_A()
        {
            playingInputAction.PlayerA.Move.Enable();
            playingInputAction.PlayerA.ChooseProp.Disable();
            playingInputAction.PlayerA.RemoveBagAt.Disable();
            playingInputAction.PlayerA.ChooseProp.performed -= ChoosePropArrow;
            playingInputAction.PlayerA.RemoveBagAt.performed -= RemovePropAt;
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
