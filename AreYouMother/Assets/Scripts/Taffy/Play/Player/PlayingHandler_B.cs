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
        private bool isNotBagOpened = true;

        public event Action EvacuateEvent;
        public event Action OpenBagEvent;
        public event Action CloseBagEvent;

        private void Awake()
        {
            Instance = this;
            playingInputAction = new PlayingInputAction();
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
            Vector2 moveB = playingInputAction.PlayerB.Move.ReadValue<Vector2>();
            transform.Translate( speed * Time.deltaTime * new Vector3(moveB.x, 0, moveB.y),Space.World);
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
            if (isNotBagOpened)
            {
                isNotBagOpened = false;
                Debug.Log("B打开背包");
                OpenBagEvent?.Invoke();
            }
            else
            {
                isNotBagOpened = true;
                Debug.Log("B关闭背包");
                CloseBagEvent?.Invoke();
            }
        }
    }
}
