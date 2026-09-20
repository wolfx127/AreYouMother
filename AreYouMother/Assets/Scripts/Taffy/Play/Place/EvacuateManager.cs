using TaffyFrame.EventBus;
using UnityEngine;

namespace Taffy.Play.Place
{
    public class EvacuateManager : MonoBehaviour
    {
        private int count = 0;
        private bool hasEvacuated = false;

        public static EvacuateManager Instance = null;

        private void Awake()
        {
            if (!Instance)
            {
                Instance = this;
            }

            count = 0;
            hasEvacuated = false;
        }

        private void OnDestroy()
        {
            Instance = null;
        }

        public void DieOrEvacuate()
        {
            count++;
            Evacuate();
        }

        private void Evacuate()
        {
            if (hasEvacuated) return;
            if (count < 2) return;

            hasEvacuated = true;
            EventBus.Publish(new EvacuateEvent());
        }
    }
}
