using System;
using UnityEngine;

namespace Taffy.Play.Player
{
    public class PlayingHandler_B:MonoBehaviour
    {
        [SerializeField] private PlayingInputAction playingInputAction;

        private void Awake()
        {
            playingInputAction = new PlayingInputAction();
        }

        private void OnEnable()
        {
            playingInputAction.PlayerB.Enable();
        }

        private void OnDisable()
        {
            playingInputAction.PlayerB.Disable();
        }

        private void Update()
        {
            Vector2 moveB = playingInputAction.PlayerB.Move.ReadValue<Vector2>();
        }
    }
}
