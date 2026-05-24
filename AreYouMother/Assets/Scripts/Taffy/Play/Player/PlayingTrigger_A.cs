using System;
using Taffy.Data;
using Taffy.OverAllManager;
using UnityEngine;

namespace Taffy.Play.Player
{
    public class PlayingTrigger_A:MonoBehaviour
    {
        public bool disableOpenContainer = true;
        private void OnTriggerEnter(Collider other)
        {
            if(other.CompareTag("Container"))
            {
                disableOpenContainer = false;
                EventBus.Publish(new GiveContainer_AEvent(other.gameObject.GetComponent<ContainerData>()));
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if(other.CompareTag("Container"))
            {
                disableOpenContainer = true;
                EventBus.Publish(new GiveContainer_AEvent(null));
            }
        }
    }
}
