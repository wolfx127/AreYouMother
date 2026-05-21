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
        private bool isNotBagOpened = true;

        public event Action EvacuateEvent;
        public event Action OpenBagEvent;
        public event Action CloseBagEvent;

        private void Awake()
        {
            Instance = this;
            playingInputAction =  new PlayingInputAction();
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
            Vector2 moveA = playingInputAction.PlayerA.Move.ReadValue<Vector2>();
            transform.Translate( speed * Time.deltaTime * new Vector3(moveA.x, 0, moveA.y),Space.World);
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
            if (isNotBagOpened)
            {
                isNotBagOpened = false;
                Debug.Log("A打开背包");
                OpenBagEvent?.Invoke();
            }
            else
            {
                isNotBagOpened = true;
                Debug.Log("A关闭背包");
                CloseBagEvent?.Invoke();
            }
        }
    }
}
