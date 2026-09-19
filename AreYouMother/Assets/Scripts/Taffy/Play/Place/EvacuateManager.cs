using System;
using Taffy.OverAllManager;
using Taffy.Play.Player;
using TaffyFrame.EventBus;
using UnityEngine;

namespace Taffy.Play.Place
{
    public class EvacuateManager : MonoBehaviour
    {
        private int count = 0;
        
        public static EvacuateManager Instance = null;

        private void Awake()
        {
            if (!Instance)
            {
                Instance = this;
            }

            count = 0;
        }

        private void OnDestroy()
        {
            Instance = null;
        }

        public event Action EvacuateEvent;

        public void DieOrEvacuate()
        {
            count++;
        }

        private void Evacuate()
        {
            if (count >= 2)
            {
                EvacuateEvent?.Invoke();
            }
        }
    }
}
