using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Taffy.Data.PropData;
using Taffy.OverAllManager;
using Taffy.Play.Bullet;
using Taffy.Play.Container;
using Taffy.Play.Trigger;
using TaffyFrame.EventBus;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using EventBus = TaffyFrame.EventBus.EventBus;

namespace Taffy.Play.Player
{
    public class PlayingHandler_A:MonoBehaviour
    {
        public static PlayingHandler_A Instance { get; private set; }
        
        public PlayDataManager player = new PlayDataManager(1,1,new List<Prop>(),1,null,null);

        
        //碰撞箱Trigger们
        [SerializeField] private GameObject attackTrigger;
        [SerializeField] private GameObject autoJumpTrigger;
        [SerializeField] private GameObject containerTrigger;
        [SerializeField] private GameObject evaluateTrigger;
        [SerializeField] private GameObject pursueTrigger;
        
        private PlayingInputAction playingInputAction;
        private BoxCollider collider;
        private Rigidbody rb;
        private (float x, float z) forward = (0,1);
        [InspectorName("移动速度")]public float speed = 10;
        public float AttackCD = 0.2f;
        private float AttackCDCounter = 0;
        public float AttackRadius = 120f;
        
        [Header("【视觉子物体】")]
        [SerializeField] private Transform visualRoot;   // 拖入挂 SpriteRenderer+Animator 的子物体，不设则默认为自身


        [ContextMenu("测试：扣血")]
        public void TestDamage()
        {
            player.combatData.HP -= 10;
        }
        
        public event Action OpenBagEvent;
        public event Action CloseBagEvent;
        public event Action OpenContainerEvent;
        public event Action CloseContainerEvent;
        public event Action UpdateChooseEvent;
        public event Action RefreshBagEvent;
        public event Action RefreshContainerEvent;
        

        private void Awake()
        {
            Instance = this;
            playingInputAction = new PlayingInputAction();
            collider = gameObject.GetComponent<BoxCollider>();
            rb = gameObject.GetComponent<Rigidbody>();
            
            attackTrigger = transform.Find("PlayerAttackTrigger").gameObject;
            attackTrigger.GetComponent<PlayerAttackTrigger>().enabled = true;
            attackTrigger.SetActive(false);
            autoJumpTrigger = transform.Find("AutoJumpTrigger").gameObject;
            autoJumpTrigger.GetComponent<AutoJumpTrigger>().enabled = true;
            autoJumpTrigger.SetActive(true);
            containerTrigger = transform.Find("ContainerTrigger").gameObject;
            containerTrigger.GetComponent<ContainerTrigger>().enabled = true;
            containerTrigger.SetActive(true);
            evaluateTrigger = transform.Find("EvacuateTrigger").gameObject;
            evaluateTrigger.GetComponent<EvacuateTrigger>().enabled = true;
            evaluateTrigger.SetActive(false);
            pursueTrigger = transform.Find("PursueTrigger").gameObject;
            pursueTrigger.GetComponent<PursueTrigger>().enabled = true;
            pursueTrigger.SetActive(true);
            
            
            
            AttackCDCounter = AttackCD;
        }

        private void OnEnable()
        {
            EventBus.Subscribe<GetPlayersInfosEvent_A>(InitPlayData);
            
            SubscribeInputAction();
            
            playingInputAction.PlayerA.Enable();
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<GetPlayersInfosEvent_A>(InitPlayData);
            
            UnsubscribeInputAction();
            
            playingInputAction.PlayerA.Disable();
        }

        private void Start()
        {
            
        }

        private void Update()
        {
            if (AttackCDCounter > 0f)
            {
                AttackCDCounter -= Time.deltaTime;
                if (playingInputAction.PlayerA.Attack.WasPressedThisFrame())
                {
                    Debug.Log("玩家A攻击冷却中");
                }
            }
            if (playingInputAction.PlayerA.Attack.WasPressedThisFrame() && AttackCDCounter <= 0f)
            {
                Attack();
                AttackCDCounter = AttackCD;
            }

            player.SetBoard();
            player.TickFSM();
            
//TODO:相机位置实时更换
        }

        private void FixedUpdate()
        {
            Vector2 v = playingInputAction.PlayerA.Move.ReadValue<Vector2>().normalized;
            if (v.sqrMagnitude > 0.01f)
            {
                v.Normalize();
                forward = (v.x, v.y);
            }
            rb.MovePosition(rb.position + new Vector3(v.x, 0f, v.y) * (speed * Time.fixedDeltaTime));
        }

        private void InitPlayData(GetPlayersInfosEvent_A evt)
        {
            player.WriteInfo(evt.HP, evt.MP, evt.bag, evt.bagSize, evt.weapon, evt.defense);
        }

        #region 注册输入事件

        private void SubscribeInputAction()
        {
            playingInputAction.PlayerA.Evacuate.performed += Evacuate;
            playingInputAction.PlayerA.OpenOrCloseBag.performed += OpenOrCloseBag;
            playingInputAction.PlayerA.ChooseProp.performed += ChooseNextProp;
            playingInputAction.PlayerA.DiscardProp.performed += DiscardProp;
            playingInputAction.PlayerA.OpenOrCloseContainer.performed += OpenOrCloseContainer;
            playingInputAction.PlayerA.ReplaceProp.performed += ReplaceProp;
            playingInputAction.PlayerA.UseProp.performed += UseProp;
        }

        private void UnsubscribeInputAction()
        {
            playingInputAction.PlayerA.Evacuate.performed -= Evacuate;
            playingInputAction.PlayerA.OpenOrCloseBag.performed -= OpenOrCloseBag;
            playingInputAction.PlayerA.ChooseProp.performed -= ChooseNextProp;
            playingInputAction.PlayerA.DiscardProp.performed -= DiscardProp;
            playingInputAction.PlayerA.OpenOrCloseContainer.performed -= OpenOrCloseContainer;
            playingInputAction.PlayerA.ReplaceProp.performed -= ReplaceProp;
            playingInputAction.PlayerA.UseProp.performed -= UseProp;
        }

        private void EnableMove()
        {
            playingInputAction.PlayerA.Move.Enable();
            playingInputAction.PlayerA.Attack.Enable();
            playingInputAction.PlayerA.OpenOrCloseBag.Enable();
            playingInputAction.PlayerA.OpenOrCloseContainer.Enable();
            playingInputAction.PlayerA.Evacuate.Enable();
            playingInputAction.PlayerA.UseProp.Disable();
            playingInputAction.PlayerA.ReplaceProp.Disable();
            playingInputAction.PlayerA.DiscardProp.Disable();
            playingInputAction.PlayerA.ChooseProp.Disable();
        }

        private void EnableChooseBag()
        {
            playingInputAction.PlayerA.OpenOrCloseBag.Enable();
            playingInputAction.PlayerA.ChooseProp.Enable();
            playingInputAction.PlayerA.DiscardProp.Enable();
            playingInputAction.PlayerA.ReplaceProp.Enable();
            playingInputAction.PlayerA.UseProp.Enable();
            playingInputAction.PlayerA.Move.Disable();
            playingInputAction.PlayerA.Attack.Disable();
            playingInputAction.PlayerA.OpenOrCloseContainer.Disable();
            playingInputAction.PlayerA.Evacuate.Disable();
        }

        private void EnableChooseContainer()
        {
            playingInputAction.PlayerA.OpenOrCloseContainer.Enable();
            playingInputAction.PlayerA.ChooseProp.Enable();
            playingInputAction.PlayerA.DiscardProp.Enable();
            playingInputAction.PlayerA.ReplaceProp.Enable();
            playingInputAction.PlayerA.UseProp.Enable();
            playingInputAction.PlayerA.Move.Disable();
            playingInputAction.PlayerA.Attack.Disable();
            playingInputAction.PlayerA.OpenOrCloseBag.Disable();
            playingInputAction.PlayerA.Evacuate.Disable();
        }
        #endregion

        // 攻击
        private void Attack()
        {
            GameObject go = BulletPool.Instance.GetBullet(transform,forward.x,forward.z);
            Debug.Log("玩家A攻击");
        }
        
        //撤离
        private void Evacuate(InputAction.CallbackContext ctx)
        {
            
        }
        
        //死亡
        private void Die()
        {
            
        }

        //复活
        private void Remake()
        {
            
        }

#region 物品相关，内含索引，交换，丢弃，使用

        public int index = 0;
        public PlayIndexPlace place = PlayIndexPlace.Bag;
        int cols = 5;
        public ContainerData container = null;
        
        private bool isBagOpen = false;
        private void OpenOrCloseBag(InputAction.CallbackContext ctx)
        {
            if (isBagOpen)
            {
                isBagOpen = !isBagOpen;
                EnableMove();
                CloseBagEvent?.Invoke();
            }
            else
            {
                isBagOpen = !isBagOpen;
                EnableChooseBag();
                place = PlayIndexPlace.Bag;
                index = 0;
                OpenBagEvent?.Invoke();
                RefreshBagEvent?.Invoke();
                if(player.bag.Count > 0)
                {
                    UpdateChooseEvent?.Invoke();
                }
            }
        }

        private bool isContainerOpen = false;
        private void OpenOrCloseContainer(InputAction.CallbackContext ctx)
        {
            if (isContainerOpen)
            {
                isContainerOpen = false;
                EnableMove();
                CloseContainerEvent?.Invoke();
                container = null;
                return;
            }

            if (containerTrigger.GetComponent<ContainerTrigger>().GetVictims().Count <= 0) return;
            container = containerTrigger.GetComponent<ContainerTrigger>().GetVictims()[0].GetComponent<ContainerData>();
            if (container is null) return;

            isContainerOpen = true;
            OpenContainerEvent?.Invoke();
            EnableChooseContainer();
            index = 0;
            place = PlayIndexPlace.Bag;

            if (player.bag.Count > 0)
            {
                RefreshBagEvent?.Invoke();
                UpdateChooseEvent?.Invoke();
            }
            else if (container.GetCount() > 0)
            {
                place = PlayIndexPlace.Container;
                RefreshContainerEvent?.Invoke();
                UpdateChooseEvent?.Invoke();
            }
        }
        
        private void DiscardProp(InputAction.CallbackContext ctx)
        {
            if (player.bag.Count <= 0 && (!isContainerOpen || container.GetCount() <= 0)) return;
            Prop temp = null;
            if(place == PlayIndexPlace.Bag)
            {
                if (player.bag.Count <= 0) return;
                temp = player.bag[index];
                player.bag.RemoveAt(index);
                if (index >= player.bag.Count)
                {
                    index = player.bag.Count - 1;
                }

                if (player.bag.Count == 0)
                {
                    index = 0;
                    if (isContainerOpen && container.GetCount() > 0)
                        place = PlayIndexPlace.Container;
                }
            }
            else if(place == PlayIndexPlace.Container)
            {
                temp = container.GetPropByIndex(index);
                container.RemovePropByIndex(index);
                if (container.GetCount() <= 0)
                {
                    place = PlayIndexPlace.Bag;
                    index = 0;
                }
            }
            
            if(temp is null) return;
            
//TODO: 弹出一个散落的道具预制体

            if (isBagOpen)
            {
                RefreshBagEvent?.Invoke();
            }
            else if (isContainerOpen)
            {
                RefreshContainerEvent?.Invoke();
            }
        }

        private void ChooseNextProp(InputAction.CallbackContext ctx)
        {
            if (player.bag.Count <= 0 && (!isContainerOpen || container.GetCount() <= 0)) return;

            Vector2 v = ctx.ReadValue<Vector2>();
            int count = place == PlayIndexPlace.Bag ? player.bag.Count : container.GetCount();
            int rowStart = index / cols * cols;
            int lastInRow = rowStart + cols - 1;
            if (lastInRow >= count) lastInRow = count - 1;
            switch (v)
            {
                //向右
                case { x: > 0, y: 0 }:
                    if (count <= 0) break;
                    
                    if (index < lastInRow) index++;
                    else index = rowStart;
                    break;
            //向左
                case { x: < 0, y: 0 }:
                    if (count <= 0) break;

                    if (index > rowStart) index--;
                    else index = lastInRow;
                    break;
                //向上
                case { x: 0, y: > 0 }:
                    index -= cols;
                    if (place == PlayIndexPlace.Bag && index < 0)
                    {
                        index += cols;
                        if (isContainerOpen && container.GetCount() > index)
                        {
                            if (container.GetCount() % cols > index)
                                index += container.GetCount() - container.GetCount() % cols;
                            else
                                index += container.GetCount() - container.GetCount() % cols - cols;
                            place = PlayIndexPlace.Container;
                        }
                        else
                        {
                            if(player.bag.Count % cols > index)
                                index += player.bag.Count - player.bag.Count % cols;
                            else
                                index += player.bag.Count - player.bag.Count % cols - cols;
                        }
                    }
                    else if (place == PlayIndexPlace.Container && index < 0)
                    {
                        index += cols;
                        if(player.bag.Count > index)
                        {
                            if (player.bag.Count % cols > index)
                                index += player.bag.Count - player.bag.Count % cols;
                            else
                                index += player.bag.Count - player.bag.Count % cols - cols;
                            place = PlayIndexPlace.Bag;
                        }
                        else
                        {
                            if (container.GetCount() % cols > index)
                                index += container.GetCount() - container.GetCount() % cols;
                            else
                                index += container.GetCount() - container.GetCount() % cols - cols;
                        }
                    }

                    break;
                //向下
                case { x: 0, y: < 0 }:
                    index += cols;
                    if (place == PlayIndexPlace.Bag && index >= player.bag.Count)
                    {
                        index %= cols;
                        if(isContainerOpen && container.GetCount() > index)
                            place = PlayIndexPlace.Container;
                    }
                    else if (place == PlayIndexPlace.Container && index >= container.GetCount())
                    {
                        index %= cols;
                        if (player.bag.Count > index)
                            place = PlayIndexPlace.Bag;
                    }

                    break;
            }
            UpdateChooseEvent?.Invoke();
        }

        private void ReplaceProp(InputAction.CallbackContext ctx)
        {
            if (player.bag.Count <= 0 && (!isContainerOpen || container.GetCount() <= 0)) return;
            if (!isContainerOpen) return;
            if (place == PlayIndexPlace.Bag)
            {
                if (container.GetCount() >= container.length) return;
                container.AddProp(player.bag[index]);
                player.bag.RemoveAt(index);
                if (index >= player.bag.Count)
                {
                    if (player.bag.Count <= 0)
                    {
                        index = 0;
                        place = PlayIndexPlace.Container;
                    }
                    else index = player.bag.Count - 1;
                }
                RefreshBagEvent?.Invoke();
                RefreshContainerEvent?.Invoke();
                UpdateChooseEvent?.Invoke();
            }
            else if (place == PlayIndexPlace.Container)
            {
                if (player.bag.Count >= player.bagSize) return;
                player.bag.Add(container.GetPropByIndex(index));
                container.RemovePropByIndex(index);
                if (index >= container.GetCount())
                {
                    if (container.GetCount() <= 0)
                    {
                        index = 0;
                        place = PlayIndexPlace.Bag;
                    }
                    else index = container.GetCount() - 1;
                }
                RefreshBagEvent?.Invoke();
                RefreshContainerEvent?.Invoke();
                UpdateChooseEvent?.Invoke();
            }
        }

        private void UseProp(InputAction.CallbackContext ctx)
        {
            if (player.bag.Count <= 0 && (!isContainerOpen || container.GetCount() <= 0)) return;
            if (place == PlayIndexPlace.Bag)
            {
                if (!player.bag[index].Execute()) return;
                player.bag.RemoveAt(index);

                if (player.bag.Count > 0)
                {
                    index = Mathf.Min(index, player.bag.Count - 1);
                }
                else
                {
                    place = isContainerOpen && container.GetCount() > 0
                        ? PlayIndexPlace.Container : PlayIndexPlace.Bag;
                    index = 0;
                }

                RefreshBagEvent?.Invoke();
                UpdateChooseEvent?.Invoke();
            }
            else if (place == PlayIndexPlace.Container)
            {
                if (!container.GetPropByIndex(index).Execute()) return;
                container.RemovePropByIndex(index);

                if (container.GetCount() > 0)
                {
                    index = Mathf.Min(index, container.GetCount() - 1);
                }
                else
                {
                    place = PlayIndexPlace.Bag;
                    index = 0;
                }

                RefreshContainerEvent?.Invoke();
                UpdateChooseEvent?.Invoke();
            }
        }

        #endregion

    }
}
