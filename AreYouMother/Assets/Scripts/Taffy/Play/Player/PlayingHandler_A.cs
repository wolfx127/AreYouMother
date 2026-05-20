using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Taffy.Play.Player
{
    public class PlayingHandler_A:MonoBehaviour
    {
        private PlayingInputAction playingInputAction;
        [SerializeField] private float speed = 5f;
        private bool inEvacuateZone;

        private void Awake()
        {
            playingInputAction =  new PlayingInputAction();
        }

        private void OnEnable()
        {
            playingInputAction.PlayerA.Enable();
            playingInputAction.PlayerA.Evacuate.performed += OnEvacuate;
        }

        private void OnDisable()
        {
            playingInputAction.PlayerA.Evacuate.performed -= OnEvacuate;
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
            }
        }
    }
}
