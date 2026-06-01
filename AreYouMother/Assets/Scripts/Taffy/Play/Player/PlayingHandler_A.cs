using System;
using Cysharp.Threading.Tasks;
using Taffy.OverAllManager;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Taffy.Play.Player
{
    public class PlayingHandler_A:MonoBehaviour
    {
        public static PlayingHandler_A Instance { get; private set; }

        [SerializeField] private GameObject        OpenContainerTriggerGO;
        [SerializeField] private PlayingTrigger_A  OpenContainerTrigger;
        private PlayingInputAction playingInputAction;
        
        [SerializeField] private float speed = 5f;
        private Vector3 moveDir = Vector3.forward;
        private Vector3 lastFacing = Vector3.forward;   // 最近一次的移动朝向（停下时保留）

        [Header("【自动攀爬】")]
        [SerializeField] private float maxStepHeight = 0.5f;
        [SerializeField] private LayerMask obstacleLayer = ~0;
        private BoxCollider _boxCollider;
        private Vector3 _halfExtents;

        // ===== PlayerA 远程冷却 =====
        [SerializeField] private float attackCD = 0.5f;   // 攻击冷却（秒）
        private bool canAttack = true;
        // ===== PlayerA 远程冷却结束 =====

        // ===== 动画控制 =====
        private Animator _animator;
        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int AttackHash = Animator.StringToHash("Attack");
        private static readonly int IsDeadHash = Animator.StringToHash("IsDead");

        [Header("【视觉子物体】")]
        [SerializeField] private Transform visualRoot;   // 拖入挂 SpriteRenderer+Animator 的子物体，不设则默认为自身

        private Transform VisualRoot => visualRoot != null ? visualRoot : transform;

        private  bool  inEvacuateZone;
        public   bool  isBagClosed           = true;
        public   bool  isContainerClosed     = true;
        public   bool  DisableOpenContainer => OpenContainerTrigger.disableOpenContainer;

        public event Action               OpenBagEvent;
        public event Action               CloseBagEvent;
        public event Action<Vector2Int>   ChoosePropArrowEvent;
        public event Action               DiscardPropEvent;
        public event Action               OpenContainerEvent;
        public event Action               CloseContainerEvent;
        public event Action               ReplacePropEvent;
        public event Action UsePropEvent;

        private void Awake()
        {
            Instance = this;
            OpenContainerTriggerGO = gameObject.transform.Find("OpenContainerTrigger").gameObject;
            OpenContainerTrigger = OpenContainerTriggerGO.GetComponent<PlayingTrigger_A>();
            playingInputAction =  new PlayingInputAction();
            _boxCollider = GetComponent<BoxCollider>();
            _halfExtents = _boxCollider != null ? _boxCollider.size * 0.5f : new Vector3(0.3f, 1f, 0.3f);
            _animator = VisualRoot.GetComponent<Animator>();
            DisableChooseProp();
            DisableDiscardProp();
            DisableReplaceProp();
        }

        private void OnEnable()
        {
            playingInputAction.PlayerA.Enable();
            playingInputAction.PlayerA.Evacuate.performed += OnEvacuate;//撤退输入->撤退()
            playingInputAction.PlayerA.OpenOrCloseBag.performed += OpenOrCloseBag;//开关背包输入->开关背包()
            playingInputAction.PlayerA.OpenOrCloseContainer.performed += OpenOrCloseContainer;//开关箱子输入->开关箱子()
            playingInputAction.PlayerA.Attack.performed += OnAttack;//A 远程
            EventBus.Subscribe<ChangeScenePlayingToHomeEvent>(DisposeInputAction);
        }

        private void OnDisable()
        {
            playingInputAction.PlayerA.Evacuate.performed -= OnEvacuate;
            playingInputAction.PlayerA.OpenOrCloseBag.performed -= OpenOrCloseBag;
            playingInputAction.PlayerA.OpenOrCloseContainer.performed -= OpenOrCloseContainer;
            playingInputAction.PlayerA.Attack.performed -= OnAttack;//A 远程
            playingInputAction.PlayerA.Disable();
            EventBus.Unsubscribe<ChangeScenePlayingToHomeEvent>(DisposeInputAction);
        }

        private void Start()
        {
            PlayerCurrentStateController.Instance.Dead_AEvent += Die;
        }

        private void Update()
        {
            if (isBagClosed||isContainerClosed)
            {
                Vector2 moveA = playingInputAction.PlayerA.Move.ReadValue<Vector2>();
                moveDir = new Vector3(moveA.x, 0, moveA.y);
                Vector3 moveAmount = speed * Time.deltaTime * moveDir;
                transform.position = StepUpMovement.MoveWithStepUp(
                    transform.position, moveAmount,
                    _halfExtents, maxStepHeight, obstacleLayer);
                if (moveDir != Vector3.zero)
                {
                    lastFacing = moveDir.normalized;
                    OpenContainerTriggerGO.transform.position = transform.position + moveDir.normalized * 1.4f;
                }
                _animator.SetFloat(SpeedHash, moveDir.magnitude);
            }
        }

        // 视觉子物体面朝相机 + 保持垂直地面（根节点不转，碰撞箱保持垂直）
        private void LateUpdate()
        {
            var cam = Camera.main;
            if (cam == null) return;
            Vector3 forward = cam.transform.forward;
            forward.y = 0;
            if (forward.sqrMagnitude > 0.0001f)
                VisualRoot.rotation = Quaternion.LookRotation(forward);
        }

        //看是否在撤离点内。为什么不用OnTriggerStay呢？因为Stay是每帧调用，这就一个bool值就解决了
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
                EventBus.Publish(new Evacuate_AEvent());
                playingInputAction.PlayerA.Disable();
            }
        }

        // ===== PlayerA 远程：发射子弹（只打 EnemyB）=====
        private void OnAttack(InputAction.CallbackContext ctx)
        {
            // 背包或箱子打开时不允许攻击
            if (!isBagClosed || !isContainerClosed) return;
            // 冷却中不允许攻击
            if (!canAttack) return;

            canAttack = false;
            _animator.SetTrigger(AttackHash);
            Debug.Log("A攻击");
            OpenContainerTrigger.GetAttackEnemies(lastFacing);
            // attackCD 秒后恢复攻击，Forget 表示不等待
            TaskMgr.AddTask(() => canAttack = true, attackCD).Forget();
        }
        // ===== PlayerA 远程结束 =====

        private void OpenOrCloseBag(InputAction.CallbackContext ctx)
        {
            if (isBagClosed)
            {
                isBagClosed = false;
                EnableChooseProp();
                EnableDiscardProp();
                Debug.Log("A打开背包");
                OpenBagEvent?.Invoke();
            }
            else
            {
                isBagClosed = true;
                DisableChooseProp();
                DisableDiscardProp();
                Debug.Log("A关闭背包");
                CloseBagEvent?.Invoke();
            }
        }

        private void EnableChooseProp()
        {
            playingInputAction.PlayerA.Move.Disable();
            playingInputAction.PlayerA.ChooseProp.Enable();
            playingInputAction.PlayerA.ChooseProp.performed += ChoosePropArrow;
        }

        private void EnableDiscardProp()
        {
            playingInputAction.PlayerA.OpenOrCloseBag.Enable();
            playingInputAction.PlayerA.DiscardProp.Enable();
            playingInputAction.PlayerA.DiscardProp.performed += DiscardProp;
            playingInputAction.PlayerA.UseProp.Enable();
            playingInputAction.PlayerA.UseProp.performed += UseProp;
        }

        private void DisableChooseProp()
        {
            playingInputAction.PlayerA.Move.Enable();
            playingInputAction.PlayerA.ChooseProp.performed -= ChoosePropArrow;
            playingInputAction.PlayerA.ChooseProp.Disable();
            playingInputAction.PlayerA.UseProp.performed -= UseProp;
            playingInputAction.PlayerA.UseProp.Disable();
        }

        private void DisableDiscardProp()
        {
            playingInputAction.PlayerA.DiscardProp.performed -= DiscardProp;
            playingInputAction.PlayerA.DiscardProp.Disable();
            playingInputAction.PlayerA.OpenOrCloseBag.Enable();
        }

        private void ChoosePropArrow(InputAction.CallbackContext ctx)
        {
            Vector2 raw = ctx.ReadValue<Vector2>();
            Vector2Int dir;
            if (Mathf.Abs(raw.x) >= Mathf.Abs(raw.y))
                dir = raw.x > 0 ? Vector2Int.right : Vector2Int.left;
            else
                dir = raw.y > 0 ? Vector2Int.up : Vector2Int.down;
            ChoosePropArrowEvent?.Invoke(dir);
        }

        private void DiscardProp(InputAction.CallbackContext ctx)
        {
            DiscardPropEvent?.Invoke();
        }
        
        private void OpenOrCloseContainer(InputAction.CallbackContext ctx)
        {
            if (!DisableOpenContainer)
            {
                if (isContainerClosed)
                {
                    isContainerClosed = false;
                    EnableChooseProp();
                    EnableReplaceProp();
                    Debug.Log("A打开箱子");
                    OpenContainerEvent?.Invoke();
                }
                else
                {
                    isContainerClosed = true;
                    DisableChooseProp();
                    DisableReplaceProp();
                    Debug.Log("A关闭箱子");
                    CloseContainerEvent?.Invoke();
                }
            }
        }
        
        private void EnableReplaceProp()
        {
            playingInputAction.PlayerA.OpenOrCloseBag.Disable();
            playingInputAction.PlayerA.ReplaceProp.Enable();
            playingInputAction.PlayerA.ReplaceProp.performed += ReplaceProp;
        }

        private void DisableReplaceProp()
        {
            playingInputAction.PlayerA.ReplaceProp.performed -= ReplaceProp;
            playingInputAction.PlayerA.ReplaceProp.Disable();
            playingInputAction.PlayerA.OpenOrCloseBag.Enable();
        }

        private void ReplaceProp(InputAction.CallbackContext ctx)
        {
            ReplacePropEvent?.Invoke();
        }

        private void UseProp(InputAction.CallbackContext ctx)
        {
            UsePropEvent?.Invoke();
        }

        private void Die()
        {
            _animator.SetBool(IsDeadHash, true);
            DisableChooseProp();
            DisableDiscardProp();
            DisableReplaceProp();
            playingInputAction.PlayerA.Evacuate.performed -= OnEvacuate;
            playingInputAction.PlayerA.Evacuate.Disable();
            playingInputAction.PlayerA.OpenOrCloseBag.performed -= OpenOrCloseBag;
            playingInputAction.PlayerA.OpenOrCloseBag.Disable();
            playingInputAction.PlayerA.OpenOrCloseContainer.performed -= OpenOrCloseContainer;
            playingInputAction.PlayerA.OpenOrCloseContainer.Disable();
            playingInputAction.PlayerA.Attack.performed -= OnAttack;//A 远程
            playingInputAction.PlayerA.Attack.Disable();
        }

        private void Remake()
        {
            _animator.SetBool(IsDeadHash, false);
            playingInputAction.PlayerA.Evacuate.performed += OnEvacuate;
            playingInputAction.PlayerA.Evacuate.Enable();
            playingInputAction.PlayerA.OpenOrCloseBag.performed += OpenOrCloseBag;
            playingInputAction.PlayerA.OpenOrCloseBag.Enable();
            playingInputAction.PlayerA.OpenOrCloseContainer.performed += OpenOrCloseContainer;
            playingInputAction.PlayerA.OpenOrCloseContainer.Enable();
            playingInputAction.PlayerA.Attack.performed += OnAttack;//A 远程
            playingInputAction.PlayerA.Attack.Enable();
            isBagClosed = true;
            isContainerClosed = true;
        }

        //切回Home场景时，注销所有输入注册
        private void DisposeInputAction(ChangeScenePlayingToHomeEvent evt)
        {
            playingInputAction.Dispose();
        }
    }
}
