using System;
using Taffy.OverAllManager;
using Taffy.Play.Player;
using UnityEngine;

namespace Taffy.Play.Place
{
    public class EvacuateManager : MonoBehaviour
    {
        public static EvacuateManager Instance { get; private set; }

        private bool evacuated_A;
        private bool evacuated_B;
        private bool dead_A;
        private bool dead_B;
        
        [SerializeField] private GameObject playerA;
        [SerializeField] private GameObject playerB;

        private void Awake()
        {
            Instance = this;
        }

        private void OnEnable()
        {
            EventBus.Subscribe<Evacuate_AEvent>(OnEvacuate_A);
            EventBus.Subscribe<Evacuate_BEvent>(OnEvacuate_B);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<Evacuate_AEvent>(OnEvacuate_A);
            EventBus.Unsubscribe<Evacuate_BEvent>(OnEvacuate_B);
            PlayerCurrentStateController.Instance.Dead_AEvent -= OnDead_A;
            PlayerCurrentStateController.Instance.Dead_BEvent -= OnDead_B;
        }

        private void Start()
        {
            PlayerCurrentStateController.Instance.Dead_AEvent += OnDead_A;
            PlayerCurrentStateController.Instance.Dead_BEvent += OnDead_B;
        }

        private void OnEvacuate_A(Evacuate_AEvent evt) { evacuated_A = true; CheckSettle(); }
        private void OnEvacuate_B(Evacuate_BEvent evt) { evacuated_B = true; CheckSettle(); }
        private void OnDead_A()                        { dead_A = true;      CheckSettle(); }
        private void OnDead_B()                        { dead_B = true;      CheckSettle(); }

        private void CheckSettle()
        {
            if (dead_A && dead_B)
            {
                EventBus.Publish(new FailEvacuateEvent());
                PlayerCurrentStateController.Instance.GiveBags();
                PlayerCurrentStateController.Instance.ResetAll();
                return;
            }

            if (dead_A && evacuated_B)
            {
                EventBus.Publish(new Only_B_SuccessEvacuateEvent());
                PlayerCurrentStateController.Instance.Reset_A();
                PlayerCurrentStateController.Instance.GiveBags();
                return;
            }

            if (dead_B && evacuated_A)
            {
                EventBus.Publish(new Only_A_SuccessEvacuateEvent());
                PlayerCurrentStateController.Instance.Reset_B();
                PlayerCurrentStateController.Instance.GiveBags();
                return;
            }

            if (evacuated_A && evacuated_B)
            {
                EventBus.Publish(new AllSuccessEvacuateEvent());
                PlayerCurrentStateController.Instance.GiveBags();
                return;
            }
        }
    }
}
