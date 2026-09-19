using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Taffy.Play.Player;
using TaffyFrame.Trigger;
using UnityEngine;

namespace Taffy.Play.Trigger
{
    public class EvacuateTrigger : MonoBehaviour
    {
        private float stayTime = 5f;
        private float counter = 5f;
        
        private List<Collider> list = new List<Collider>();
        private bool isEvacuate = false;
        
        private Vector3 originalPosition = Vector3.zero;
        
        public event Action EvacuateEvent;
        
        
        private void OnTriggerEnter(Collider other)
        {
            list.Add(other);
        }

        private void Awake()
        {
            originalPosition = transform.position;
        }

        private void Update()
        {
            if (transform.position != originalPosition)
            {
                EvacuateEvent = null;
                gameObject.SetActive(false);
            }
            inEvacuate();
            if(isEvacuate)
            {
                counter -= Time.deltaTime;
                if (counter <= 0)
                {
                    EvacuateEvent?.Invoke();
                }
            }
            else
            {
                EvacuateEvent = null;
                gameObject.SetActive(false);
            }
        }

        private async void inEvacuate()
        {
            if (isEvacuate) return;
            await UniTask.WaitForFixedUpdate();
            foreach (var item in list)
            {
                if (item.CompareTag("EvacuateZone"))
                {
                    isEvacuate = true;
                    return;
                }
            }
        }
    }
}
