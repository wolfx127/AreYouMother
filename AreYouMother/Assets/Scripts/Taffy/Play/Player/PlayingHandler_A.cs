using System;
using Cysharp.Threading.Tasks;
using Taffy.OverAllManager;
using TaffyFrame.EventBus;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Taffy.Play.Player
{
    public class PlayingHandler_A:MonoBehaviour
    {
        public static PlayingHandler_A Instance { get; private set; }
        
        private PlayDataManager playData = null;

        
        //碰撞箱Trigger们
        [SerializeField] private GameObject attackTrigger;
        [SerializeField] private GameObject autoJumpTrigger;
        [SerializeField] private GameObject containerTrigger;
        [SerializeField] private GameObject evaluateTrigger;
        
        private PlayingInputAction playingInputAction;
        private BoxCollider boxCollider;
        private Transform transform;
        
        [Header("【视觉子物体】")]
        [SerializeField] private Transform visualRoot;   // 拖入挂 SpriteRenderer+Animator 的子物体，不设则默认为自身

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
            playingInputAction = new PlayingInputAction();
            boxCollider = gameObject.GetComponent<BoxCollider>();
            transform = gameObject.transform;
//TODO:trigger赋值
            DisableChooseProp();
            DisableDiscardProp();
            DisableReplaceProp();
        }

        private void OnEnable()
        {
            EventBus.Subscribe<GetPlayersInfosEvent_A>(InitPlayData);
            
            playingInputAction.PlayerA.Enable();
            playingInputAction.PlayerA.Evacuate.performed += Evacuate;//撤退输入->撤退()
            playingInputAction.PlayerA.OpenOrCloseBag.performed += OpenOrCloseBag;//开关背包输入->开关背包()
            playingInputAction.PlayerA.OpenOrCloseContainer.performed += OpenOrCloseContainer;//开关箱子输入->开关箱子()
            playingInputAction.PlayerA.Attack.performed += Attack;//A 远程
        }

        private void OnDisable()
        {
            playingInputAction.PlayerA.Evacuate.performed -= Evacuate;
            playingInputAction.PlayerA.OpenOrCloseBag.performed -= OpenOrCloseBag;
            playingInputAction.PlayerA.OpenOrCloseContainer.performed -= OpenOrCloseContainer;
            playingInputAction.PlayerA.Attack.performed -= Attack;//A 远程
            playingInputAction.PlayerA.Disable();
        }
        
        private void Update()
        {
            playData.SetBoard();
            playData.TickFSM();
            
//TODO:角色位置和相机位置实时更换
        }

        private void InitPlayData(GetPlayersInfosEvent_A evt)
        {
            playData = new PlayDataManager(evt.HP, evt.MP, evt.bag, evt.bagSize, evt.weapon, evt.defense);
        }

        #region 封装注册输入事件

        private void EnableChooseProp()
        {
            playingInputAction.PlayerA.Move.Disable();
            playingInputAction.PlayerA.ChooseProp.Enable();
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

#endregion

        // 攻击
        private void Attack(InputAction.CallbackContext ctx)
        {
            
        }
        
        //撤离
        private void Evacuate(InputAction.CallbackContext ctx)
        {
            
        }

#region 物品相关，内含索引，交换，丢弃，使用

        public int index = 0;
        public PlayIndexPlace place = PlayIndexPlace.Bag;
        //打开关闭背包
        private void OpenOrCloseBag(InputAction.CallbackContext ctx)
        {
            
        }

        //丢弃物品
        private void DiscardProp(InputAction.CallbackContext ctx)
        {
            DiscardPropEvent?.Invoke();
        }
        
        private void OpenOrCloseContainer(InputAction.CallbackContext ctx)
        {
            
        }
        
        private void ReplaceProp(InputAction.CallbackContext ctx)
        {
            ReplacePropEvent?.Invoke();
        }

        private void UseProp(InputAction.CallbackContext ctx)
        {
            UsePropEvent?.Invoke();
        }
        

        #endregion

        //死亡
        private void Die()
        {
            
        }

        //复活
        private void Remake()
        {
            
        }
        
        /// <summary>
        /// 切回Home场景
        /// </summary>
        private void DisposeInputAction(ChangeScenePlayingToHomeEvent evt)
        {
            playingInputAction.Dispose();
        }
    }
}
