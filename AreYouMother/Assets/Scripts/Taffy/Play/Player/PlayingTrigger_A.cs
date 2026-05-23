using System;
using UnityEngine;

namespace Taffy.Play.Player
{
    public class PlayingTrigger_A:MonoBehaviour
    {
        public bool disableOpenContainer = true;
        private void OnTriggerEnter(Collider other)
        {
            if(other.CompareTag("Container")) disableOpenContainer = false;
        }

        private void OnTriggerExit(Collider other)
        {
            if(other.CompareTag("Container")) disableOpenContainer = true;
        }
    }
}
