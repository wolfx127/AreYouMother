using System;
using Cysharp.Threading.Tasks;
using Taffy.Data.PropData;
using Taffy.OverAllManager;
using Taffy.Play.Bullet;
using Taffy.Play.Container;
using Taffy.Play.Place;
using Taffy.Play.Trigger;
using TaffyFrame.EventBus;
using UnityEngine;
using UnityEngine.InputSystem;
using EventBus = TaffyFrame.EventBus.EventBus;

namespace Taffy.Play.Player
{
    public class PlayingHandler_B:MonoBehaviour
    {
        public static float MaxDistance => PlayingHandler_A.MaxDistance;
        public static PlayingHandler_B Instance { get; private set; }
        
        public PlayDataManager player = new PlayDataManager(1,1,null,1,null,null);
        public Rigidbody rbA;

        
        [SerializeField] private GameObject attackTrigger;
        [SerializeField] private GameObject containerTrigger;
        [SerializeField] private GameObject evaluateTrigger;
        [SerializeField] private GameObject pursueTrigger;
        [SerializeField] private GameObject enemyCreateTrigger;

        public const float ContainerTriggerDistance = 2f;
        
        private PlayingInputAction playingInputAction;
        private Rigidbody rb;
        private SpriteRenderer spriteRenderer;
        private Animator animator;
        private Color baseColor = Color.white;
        private Vector2 originalSize = Vector2.zero;
        private EvacuateTrigger evacuateTrigger;
        private bool isDeadHandled = false;
        [SerializeField] private float walkAnimTime = 1f;
        [SerializeField] private float idleAnimTime = 3f;
        private (float x, float z) forward = (0,1);
        [InspectorName("移动速度")]public float speed = 10;
        public float AttackCD = 0.2f;
        private bool canAttack = true;
        public float AttackRadius = 2f;

        private Sprite image;
        

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
            rb = gameObject.GetComponent<Rigidbody>();
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
            image = spriteRenderer.sprite;
            animator = GetComponent<Animator>();
            attackStateHash = Animator.StringToHash(attackStateName);
            animator.enabled = false;
            originalSize = spriteRenderer.size;
            baseColor = spriteRenderer.color;
            
            attackTrigger = transform.Find("PlayerAttackTrigger").gameObject;
            var atkcol = attackTrigger.GetComponent<PlayerAttackTrigger>();
            atkcol.enabled = true;
            var atkcollider = attackTrigger.GetComponent<CapsuleCollider>();
            atkcollider.enabled = true; atkcollider.isTrigger = true;
            attackTrigger.SetActive(false);
            containerTrigger = transform.Find("ContainerTrigger").gameObject;
            var contcol = containerTrigger.GetComponent<ContainerTrigger>();
            contcol.enabled = true;
            var contcollider = containerTrigger.GetComponent<CapsuleCollider>();
            contcollider.enabled = true; contcollider.isTrigger = true; contcollider.radius = 0.8f; contcollider.height = 1.6f; contcollider.center = Vector3.zero;
            containerTrigger.SetActive(true);
            evaluateTrigger = transform.Find("EvacuateTrigger").gameObject;
            evacuateTrigger = evaluateTrigger.GetComponent<EvacuateTrigger>();
            evacuateTrigger.enabled = true;
            var evacollider = evaluateTrigger.GetComponent<CapsuleCollider>();
            evacollider.enabled = true; evacollider.isTrigger = true; evacollider.radius = 0.8f; evacollider.height = 1.6f; evacollider.center = new Vector3(0f, -0.83f, 0f);
            evaluateTrigger.SetActive(false);
            pursueTrigger = transform.Find("PursueTrigger").gameObject;
            var purcol = pursueTrigger.GetComponent<PursueTrigger>();
            purcol.enabled = true;
            var purcollider = pursueTrigger.GetComponent<CapsuleCollider>();
            purcollider.enabled = true; purcollider.isTrigger = true; purcollider.radius = 20f; purcollider.height = 40f; purcollider.center = Vector3.zero;
            pursueTrigger.GetComponent<PursueTrigger>().playerName = 'B';
            pursueTrigger.SetActive(true);
            enemyCreateTrigger = transform.Find("EnemyCreateTrigger").gameObject;
            var createcol = enemyCreateTrigger.GetComponent<CreateEnemyTrigger>();
            createcol.enabled = true;
            var createcollider = enemyCreateTrigger.GetComponent<CapsuleCollider>();
            createcollider.enabled = true; createcollider.isTrigger = true; createcollider.radius = 50f; createcollider.height = 100f; createcollider.center = Vector3.zero;
            enemyCreateTrigger.SetActive(true);
        }

        private void OnEnable()
        {
            EventBus.Subscribe<GetPlayersInfosEvent_B>(InitPlayData);
            
            SubscribeInputAction();
            if (evacuateTrigger != null) evacuateTrigger.EvacuateEvent += Evacuate;
            
            playingInputAction.PlayerB.Enable();
            EnableMove();
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<GetPlayersInfosEvent_B>(InitPlayData);
            
            UnsubscribeInputAction();
            if (evacuateTrigger != null) evacuateTrigger.EvacuateEvent -= Evacuate;
            
            playingInputAction.PlayerB.Disable();
        }

        private void Start()
        {
            
        }

        private void Update()
        {
            if (player.isDead || player.isEvacuate) return;
            if (playingInputAction.PlayerB.Attack.WasPressedThisFrame() && canAttack)
            {
                Debug.Log("玩家B攻击");
                Attack();
                TaskMgr.AddTask(AttackCD, () => canAttack = true);
            }

            player.SetBoard(player.combatData.HP, player.combatData.MP, player.isIdle, player.isWalk, player.isAttack, player.isInjury);
            player.TickFSM();
            
//TODO:相机位置实时更换
        }

        private void FixedUpdate()
        {
            Move();
        }

        //移动
        private void Move()
        {
            Vector2 v = playingInputAction.PlayerB.Move.ReadValue<Vector2>().normalized;
            Vector3 target = rb.position + new Vector3(v.x, 0f, v.y) * (speed * Time.fixedDeltaTime);
            float dx = target.x - rbA.position.x;
            float dz = target.z - rbA.position.z;
            if (dx * dx + dz * dz > MaxDistance * MaxDistance)
            {
                float d = Mathf.Sqrt(dx * dx + dz * dz);
                target = new Vector3(
                    rbA.position.x + dx / d * MaxDistance,
                    target.y,
                    rbA.position.z + dz / d * MaxDistance);
            }
            rb.MovePosition(target);
            if (v.sqrMagnitude > 0.0001f)
            {
                forward.x = v.x;
                forward.z = v.y;
                spriteRenderer.flipX = v.x < 0f;
                player.isWalk = true;
                player.isIdle = false;
            }
            else
            {
                player.isWalk = false;
                player.isIdle = true;
            }
            containerTrigger.transform.localPosition =
                new Vector3(forward.x, 0f, forward.z).normalized * ContainerTriggerDistance;
        }
        
        /// <summary>
        /// 显示移动范围
        /// </summary>
        private void OnDrawGizmos()
        {
            if (rbA is null) return;
            Gizmos.color = new Color(1f, 1f, 0f, 0.5f);
            Gizmos.DrawWireSphere(rbA.position, MaxDistance);
        }

        private void InitPlayData(GetPlayersInfosEvent_B evt)
        {
            player.WriteInfo(evt.HP, evt.MP, evt.bag, evt.bagSize, evt.weapon, evt.defense);
            player.IdleAnimEvent += IdleAnim;
            player.WalkAnimEvent += WalkAnim;
            player.InjuryAnimEvent += InjuryAnim;
            player.AttackAnimEvent += AttackAnim;
            player.DeadEvent += Die;
        }

#region 注册输入事件

        private void SubscribeInputAction()
        {
            playingInputAction.PlayerB.Evacuate.performed += EvacuateInput;
            playingInputAction.PlayerB.OpenOrCloseBag.performed += OpenOrCloseBag;
            playingInputAction.PlayerB.ChooseProp.performed += ChooseNextProp;
            playingInputAction.PlayerB.DiscardProp.performed += DiscardProp;
            playingInputAction.PlayerB.OpenOrCloseContainer.performed += OpenOrCloseContainer;
            playingInputAction.PlayerB.ReplaceProp.performed += ReplaceProp;
            playingInputAction.PlayerB.UseProp.performed += UseProp;
        }

        private void UnsubscribeInputAction()
        {
            playingInputAction.PlayerB.Evacuate.performed -= EvacuateInput;
            playingInputAction.PlayerB.OpenOrCloseBag.performed -= OpenOrCloseBag;
            playingInputAction.PlayerB.ChooseProp.performed -= ChooseNextProp;
            playingInputAction.PlayerB.DiscardProp.performed -= DiscardProp;
            playingInputAction.PlayerB.OpenOrCloseContainer.performed -= OpenOrCloseContainer;
            playingInputAction.PlayerB.ReplaceProp.performed -= ReplaceProp;
            playingInputAction.PlayerB.UseProp.performed -= UseProp;
        }

        private void EnableMove()
        {
            playingInputAction.PlayerB.Move.Enable();
            playingInputAction.PlayerB.Attack.Enable();
            playingInputAction.PlayerB.OpenOrCloseBag.Enable();
            playingInputAction.PlayerB.OpenOrCloseContainer.Enable();
            playingInputAction.PlayerB.Evacuate.Enable();
            playingInputAction.PlayerB.UseProp.Disable();
            playingInputAction.PlayerB.ReplaceProp.Disable();
            playingInputAction.PlayerB.DiscardProp.Disable();
            playingInputAction.PlayerB.ChooseProp.Disable();
        }

        private void EnableChooseBag()
        {
            playingInputAction.PlayerB.OpenOrCloseBag.Enable();
            playingInputAction.PlayerB.ChooseProp.Enable();
            playingInputAction.PlayerB.DiscardProp.Enable();
            playingInputAction.PlayerB.ReplaceProp.Enable();
            playingInputAction.PlayerB.UseProp.Enable();
            playingInputAction.PlayerB.Move.Disable();
            playingInputAction.PlayerB.Attack.Disable();
            playingInputAction.PlayerB.OpenOrCloseContainer.Disable();
            playingInputAction.PlayerB.Evacuate.Disable();
        }

        private void EnableChooseContainer()
        {
            playingInputAction.PlayerB.OpenOrCloseContainer.Enable();
            playingInputAction.PlayerB.ChooseProp.Enable();
            playingInputAction.PlayerB.DiscardProp.Enable();
            playingInputAction.PlayerB.ReplaceProp.Enable();
            playingInputAction.PlayerB.UseProp.Enable();
            playingInputAction.PlayerB.Move.Disable();
            playingInputAction.PlayerB.Attack.Disable();
            playingInputAction.PlayerB.OpenOrCloseBag.Disable();
            playingInputAction.PlayerB.Evacuate.Disable();
        }
        #endregion

        // 攻击
        private async void Attack()
        {
            Debug.Log("玩家B,uniTask攻击");
            canAttack = false;
            var trigger = attackTrigger.GetComponent<PlayerAttackTrigger>();
            trigger.ATK = player.combatData.ATK;
            attackTrigger.transform.position = transform.position + new Vector3(forward.x, 0, forward.z) * AttackRadius;
            attackTrigger.SetActive(true);
            player.isAttack = true;

            await UniTask.WaitForFixedUpdate();
            await UniTask.WaitForFixedUpdate();
            attackTrigger.SetActive(false);
            player.isAttack = false;
        }
        
        private void EvacuateInput(InputAction.CallbackContext ctx)
        {
            if (player.isDead || player.isEvacuate) return;
            if (evaluateTrigger.activeSelf) return;
            evaluateTrigger.SetActive(true);
        }
        
        private void Evacuate()
        {
            if (player.isDead || player.combatData.HP <= 0) return;
            if (player.isEvacuate) return;
            
            evaluateTrigger.SetActive(false);
            spriteRenderer.color = Color.mediumSpringGreen;
            player.isEvacuate = true;
            Debug.Log("B撤离成功");

            UnsubscribeInputAction();
            playingInputAction.PlayerB.Disable();
            gameObject.layer = LayerMask.NameToLayer("Soul");

            if (EvacuateManager.Instance) EvacuateManager.Instance.DieOrEvacuate();
        }
        
        //死亡
        private void Die()
        {
            if (isDeadHandled) return;
            isDeadHandled = true;
            
            evaluateTrigger.SetActive(false);
            spriteRenderer.color = new Color(0.9f, 0.9f, 0.9f, 0.9f);
            
            UnsubscribeInputAction();
            playingInputAction.PlayerB.Disable();
            playingInputAction.PlayerB.Move.Enable();
            gameObject.layer = LayerMask.NameToLayer("Soul");
            
            if (EvacuateManager.Instance) EvacuateManager.Instance.DieOrEvacuate();
        }

        //复活
        private void Remake()
        {
            
        }

////////// 动画 //////////////////////////////////////////////////
        private float walkTimeCounter = 0;
        private float minSizePercent = 0.7f;
        private void WalkAnim()
        {
            if (walkAnimTime <= 0f) return;
            walkTimeCounter += Time.deltaTime;
            if (walkTimeCounter >= walkAnimTime * 2f) walkTimeCounter = 0f;

            spriteRenderer.size = new Vector2(
                spriteRenderer.size.x,
                Mathf.Lerp(originalSize.y, originalSize.y * minSizePercent,
                    Mathf.Sin(Mathf.PI * Mathf.PingPong(walkTimeCounter, walkAnimTime) / walkAnimTime)));
        }

        private float idleTimeCounter = 0;
        private void IdleAnim()
        {
            if (idleAnimTime <= 0f) return;
            idleTimeCounter += Time.deltaTime;
            if (idleTimeCounter >= idleAnimTime * 2f) idleTimeCounter = 0f;

            spriteRenderer.size = new Vector2(
                spriteRenderer.size.x,
                Mathf.Lerp(originalSize.y, originalSize.y * minSizePercent,
                    Mathf.Sin(Mathf.PI * Mathf.PingPong(idleTimeCounter, idleAnimTime) / idleAnimTime)));
        }

        private void InjuryAnim(float tintDuration)
        {
            if (spriteRenderer == null) return;
            if (animator != null) animator.enabled = false;   // 关掉 Animator：否则 Write Defaults 会把染红抹回默认色
            spriteRenderer.color = new Color(0.9f, 0f, 0.2f, 1f);
            TaskMgr.AddTask(tintDuration, () =>
            {
                // 回滚到初始颜色（而不是"挨打那一刻的颜色"），死亡后不再覆盖死亡色
                if (spriteRenderer != null && !player.isDead) spriteRenderer.color = baseColor;
            });
        }

        [Header("攻击动画")] [SerializeField] private string attackStateName = "PlayerBAttack";
        private int attackStateHash;
        private int lastAttackEventFrame = -1;
        private void AttackAnim(float duration)
        {
            if (!animator || !spriteRenderer) return;

            if (Time.frameCount != lastAttackEventFrame + 1)
            {
                animator.enabled = true;
                animator.Play(attackStateHash, 0, 0f);
                animator.Update(0f);

                TaskMgr.AddTask(duration, () =>
                {
                    if (animator) animator.enabled = false;
                    if (spriteRenderer) spriteRenderer.sprite = image;
                });
            }
            lastAttackEventFrame = Time.frameCount;
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
                place = PlayIndexPlace.Bag;   // 关箱后复位索引，避免指向已关闭的箱子
                index = 0;
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
            if (container.GetCount() > 0)
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
                Debug.Log($"已从背包交换道具");
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
                Debug.Log($"已从箱子交换道具");
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
