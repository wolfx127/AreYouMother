using System;
using System.Collections.Generic;
using System.Text;
using Taffy.Data;
using Taffy.Data.PropData;
using Taffy.OverAllManager;
using Taffy.Play.Player;
using Taffy.UI.Pro;
using TaffyFrame.EventBus;
using UnityEngine;
using UnityEngine.UIElements;

namespace Taffy.UI
{
    public interface IPlayingUI
    {
        void UpdateHP_A(int hp);
        void UpdateHP_B(int hp);
        void UpdateMP_A(int mp);
        void UpdateMP_B(int mp);

        void OpenBag_A();
        void OpenBag_B();

        void CloseBag_A();
        void CloseBag_B();

        void RefreshBag_A(List<Texture2D> PropImage, int bagCount, int bagSize);
        void RefreshBag_B(List<Texture2D> PropImage, int bagCount, int bagSize);

        void CheckingProp_A(int index, PlayIndexPlace place);
        void CheckingProp_B(int index, PlayIndexPlace place);

        void DescribeProp_A(Prop prop);
        void DescribeProp_B(Prop prop);

        void OpenContainer_A();
        void OpenContainer_B();

        void CloseContainer_A();
        void CloseContainer_B();

        void RefreshContainer_A(List<Texture2D> PropImage, ContainerType type);
        void RefreshContainer_B(List<Texture2D> PropImage, ContainerType type);

        public void Evacuate(bool isDead_A, bool isDead_B, int property_A, int property_B);
    }

    public class PlayingUIManager:MonoBehaviour , IPlayingUI
    {
        private IPlayingUI_pre playingUIPre;
        
        /// <summary>
        /// 根模板
        /// </summary>
        private VisualElement root;
        
        /// <summary>
        /// 血条，蓝条
        /// </summary>
        private VisualElement barHP_A;
        private VisualElement barHP_B;
        private VisualElement barMP_A;
        private VisualElement barMP_B;
        /// <summary>
        /// 血蓝描述文本
        /// </summary>
        private Label textHPMP_A;
        private Label textHPMP_B;
        
        /// <summary>
        /// 道具框模板
        /// </summary>
        private VisualElement PropCase;
        /// <summary>
        /// 背包模板
        /// </summary>
        private VisualElement BagUI_A;
        private VisualElement BagUI_B;
        /// <summary>
        /// 背包栏
        /// </summary>
        private VisualElement propCatalogue_A;
        private VisualElement propCatalogue_B;
        /// <summary>
        /// 背包道具数量文本
        /// </summary>
        private Label propCountText_A;
        private Label propCountText_B;
        /// <summary>
        /// 箱子模板
        /// </summary>
        private VisualElement containerUI_A;
        private VisualElement containerUI_B;
        /// <summary>
        /// 箱子栏
        /// </summary>
        private VisualElement containerCatalogue_A;
        private VisualElement containerCatalogue_B;
        /// <summary>
        /// 箱子类型文本
        /// </summary>
        private Label containerTextType_A;
        private Label containerTextType_B;
        /// <summary>
        /// 道具描述文本
        /// </summary>
        private Label propDescribe_A;
        private Label propDescribe_B;
        /// <summary>
        /// 结算面板
        /// </summary>
        private VisualElement settle;
        private Label settleStateText;
        private Label summaryText;
        private Label lostPropertyText;
        private Button backHomeBtn;

        [SerializeField] private VisualTreeAsset BagUI;
        [SerializeField] private VisualTreeAsset PropCaseUI;
        [SerializeField] private VisualTreeAsset containerUI;
        [SerializeField] private VisualTreeAsset SettleUI;

        
        private void Awake()
        {
            root = GetComponent<UIDocument>().rootVisualElement;
            barHP_A = root.Q<VisualElement>("HP_PlayerA").Q<VisualElement>("CurrentHP");
            barHP_A.style.transformOrigin = new TransformOrigin(Length.Percent(0), Length.Percent(50));
            barHP_B = root.Q<VisualElement>("HP_PlayerB").Q<VisualElement>("CurrentHP");
            barHP_B.style.transformOrigin = new TransformOrigin(Length.Percent(0), Length.Percent(50));
            barMP_A = root.Q<VisualElement>("MP_PlayerA").Q<VisualElement>("CurrentMP");
            barMP_A.style.transformOrigin = new TransformOrigin(Length.Percent(0), Length.Percent(50));
            barMP_B = root.Q<VisualElement>("MP_PlayerB").Q<VisualElement>("CurrentMP");
            barMP_B.style.transformOrigin = new TransformOrigin(Length.Percent(0), Length.Percent(50));
            textHPMP_A = root.Q<VisualElement>("Info_PlayerA").Q<Label>("HPandMPnum");
            textHPMP_B = root.Q<VisualElement>("Info_PlayerB").Q<Label>("HPandMPnum");
            
            BagUI_A = BagUI.Instantiate().Q<VisualElement>("root");
            BagUI_B = BagUI.Instantiate().Q<VisualElement>("root");
            
            propCatalogue_A = BagUI_A.Q<VisualElement>("PropsCatalogue");
            propCatalogue_B = BagUI_B.Q<VisualElement>("PropsCatalogue");
            propDescribe_A = BagUI_A.Q<Label>("PropDescribe");
            propDescribe_B = BagUI_B.Q<Label>("PropDescribe");
            propCountText_A = BagUI_A.Q<Label>("BagInfo");
            propCountText_B = BagUI_B.Q<Label>("BagInfo");
            containerUI_A = containerUI.Instantiate().Q<VisualElement>("Container");
            containerUI_B = containerUI.Instantiate().Q<VisualElement>("Container");
            containerTextType_A = containerUI_A.Q<Label>("ContainerName");
            containerTextType_B = containerUI_B.Q<Label>("ContainerName");
            containerCatalogue_A = containerUI_A.Q("CenterPivot");
            containerCatalogue_B = containerUI_B.Q("CenterPivot");

            root.Q<VisualElement>("CenterPivot").Q<VisualElement>("_LeftPivot").Add(BagUI_A);
            root.Q<VisualElement>("CenterPivot").Q<VisualElement>("_RightPivot").Add(BagUI_B);
            root.Q<VisualElement>("ContainerPlace_A").Add(containerUI_A);
            root.Q<VisualElement>("ContainerPlace_B").Add(containerUI_B);
            BagUI_A.style.display = DisplayStyle.None;
            BagUI_B.style.display = DisplayStyle.None;
            containerUI_A.style.display = DisplayStyle.None;
            containerUI_B.style.display = DisplayStyle.None;

            settle = SettleUI.Instantiate();
            // 结算模板的 root 是 100%x100%，这里给它绝对定位铺满屏幕；
            // 它是 UIDocument 根节点的最后一个子节点 → 画在最上层，能盖住 HUD/背包/箱子
            settle.style.position = Position.Absolute;
            settle.style.left = 0f;
            settle.style.top = 0f;
            settle.style.right = 0f;
            settle.style.bottom = 0f;
            settleStateText = settle.Q<Label>("SettleStateText");
            summaryText = settle.Q<Label>("SummaryText");
            backHomeBtn = settle.Q<Button>("BackHomeBtn");
            root.Add(settle);
            settle.style.display = DisplayStyle.None;   // 平时藏起来，结算时才显示
        }

        private void OnEnable()
        {
            playingUIPre = new PlayingUI_pre(this);
            playingUIPre.Subscribe();
            // 按钮在这里订阅：playingUIPre 是本帧才 new 的，写在 Awake 里必然是空引用
            if (backHomeBtn != null) backHomeBtn.clicked += BackHome;
        }

        private void BackHome()
        {
            playingUIPre.BackHome();
        }

        private void Start()
        {
            
        }

        private void OnDisable()
        {
            if (backHomeBtn != null) backHomeBtn.clicked -= BackHome;
            playingUIPre.Unsubscribe();
        }


        /// <summary>
        /// playerA:更新HP MP显示
        /// </summary>
        private int _hp_A = 0;
        private int _maxHp_A = 0;
        private int _hp_B = 0;
        private int _maxHp_B = 0;
        private int _mp_A = 0;
        private int _maxMp_A = 0;
        private int _mp_B = 0;
        private int _maxMp_B = 0;
        public void UpdateHP_A(int hp)
        {
            _hp_A = hp;
            _maxHp_A = Mathf.Max(_hp_A, _maxHp_A);
            float hpRatio = _maxHp_A > 0 ? (float)_hp_A / _maxHp_A : 0f;
            barHP_A.style.scale = new Scale(new Vector3(Mathf.Clamp01(hpRatio), 1f, 1f));
            UpdateInfo_A();
        }

        public void UpdateHP_B(int hp)
        {
            _hp_B = hp;
            _maxHp_B = Mathf.Max(_hp_B, _maxHp_B);
            float hpRatio = _maxHp_B > 0 ? (float)_hp_B / _maxHp_B : 0f;
            barHP_B.style.scale = new Scale(new Vector3(Mathf.Clamp01(hpRatio), 1f, 1f));
            UpdateInfo_B();
        }

        public void UpdateMP_A(int mp)
        {
            _mp_A = mp;
            _maxMp_A = Mathf.Max(_mp_A, _maxMp_A);
            float mpRatio = _maxMp_A > 0 ? (float)_mp_A / _maxMp_A : 0f;
            barMP_A.style.scale = new Scale(new Vector3(Mathf.Clamp01(mpRatio), 1f, 1f));
            UpdateInfo_A();
        }

        public void UpdateMP_B(int mp)
        {
            _mp_B = mp;
            _maxMp_B = Mathf.Max(_mp_B, _maxMp_B);
            float mpRatio = _maxMp_B > 0 ? (float)_mp_B / _maxMp_B : 0f;
            barMP_B.style.scale = new Scale(new Vector3(Mathf.Clamp01(mpRatio), 1f, 1f));
            UpdateInfo_B();
        }

        private void UpdateInfo_A()
        {
            textHPMP_A.text = $"HP:{_hp_A}/{_maxHp_A} " + '\n' + $"MP:{_mp_A}/{_maxMp_A}";
        }

        private void UpdateInfo_B()
        {
            textHPMP_B.text = $"HP:{_hp_B}/{_maxHp_B}" + '\n' + $"MP:{_mp_B}/{_maxMp_B}";
            Debug.Log("UpdateInfo_B 被调用");
        }
        
        /// <summary>
        /// playerA:打开背包|
        /// 响应输入事件，内部执行add UI，然后checking一次（使checking于首位）
        /// </summary>
        private int prevIndex_A = 0;
        private int prevIndex_B = 0;
        private PlayIndexPlace prevPlace_A = PlayIndexPlace.Bag;
        private PlayIndexPlace prevPlace_B = PlayIndexPlace.Bag;
        
        public void OpenBag_A()
        {
            Debug.Log("打开背包A");
            BagUI_A.style.display = DisplayStyle.Flex;
        }

        public void OpenBag_B()
        {
            Debug.Log("打开背包B");
            BagUI_B.style.display = DisplayStyle.Flex;
        }

        /// <summary>
        /// playerA:关闭背包|
        /// 响应输入事件，内部执行remove UI，同时复原（归零）checking的index
        /// </summary>
        public void CloseBag_A()
        {
            Debug.Log("关闭背包A");
            
            BagUI_A.style.display = DisplayStyle.None;
        }

        public void CloseBag_B()
        {
            Debug.Log("关闭背包B");
            BagUI_B.style.display = DisplayStyle.None;
        }

        /// <summary>
        /// 刷新背包UI|
        /// 清空重置bagUI。然后内部拿到Bag数据，轮询Add 道具框UI，同时更新 容量/以容纳 文本
        /// </summary>
        public void RefreshBag_A(List<Texture2D> PropImage, int bagCount, int bagSize)
        {
            propCountText_A.text = $"道具数量/背包大小:{bagCount}/{bagSize}";
            propCatalogue_A.Clear();
            foreach (var image in PropImage)
            {
                VisualElement v = PropCaseUI.Instantiate().Q("PropCase");
                v.style.backgroundImage = image;
                propCatalogue_A.Add(v);
            }
        }

        public void RefreshBag_B(List<Texture2D> PropImage, int bagCount, int bagSize)
        {
            propCountText_B.text = $"道具数量/背包大小:{bagCount}/{bagSize}";
            propCatalogue_B.Clear();
            foreach (var image in PropImage)
            {
                VisualElement v = PropCaseUI.Instantiate().Q("PropCase");
                v.style.backgroundImage = image;
                propCatalogue_B.Add(v);
            }
        }

        //////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// checking选中道具|
        /// 选中道具有光标、查看选中道具描述|
        /// 更新checking索引,使该索引道具背景变蓝，也就是光标的效果|
        /// 同时记录这次checking的索引，然后便于下次checking先取消那个道具的光标（下次的这次，语义等同于这次的上次），只保留当前checking的光标。这样有一种光标移动的感觉|
        /// 背包UI的精髓
        /// </summary>
        public void CheckingProp_A(int index, PlayIndexPlace place)
        {
            try
            {
                if (prevPlace_A == PlayIndexPlace.Bag)
                    propCatalogue_A[prevIndex_A].style.backgroundColor = new Color(0, 0, 0, 0);
                else if (prevPlace_A == PlayIndexPlace.Container)
                    containerCatalogue_A[prevIndex_A].style.backgroundColor = new Color(0, 0, 0, 0);
            }
            catch(Exception  e)
            {
                Debug.LogWarning(e);
            }

            if(place == PlayIndexPlace.Bag)
            {
                if (index < 0 || index >= propCatalogue_A.childCount) return;
                propCatalogue_A[index].style.backgroundColor = new Color(0.2f, 0.3f, 0.9f, 0.7f);
            }
            else if(place == PlayIndexPlace.Container)
            {
                if (index < 0 || index >= containerCatalogue_A.childCount) return;
                containerCatalogue_A[index].style.backgroundColor = new Color(0.2f, 0.3f, 0.9f, 0.7f);
            }
            
            prevIndex_A = index;
            prevPlace_A = place;
        }

        public void CheckingProp_B(int index, PlayIndexPlace place)
        {
            try
            {
                if (prevPlace_B == PlayIndexPlace.Bag)
                    propCatalogue_B[prevIndex_B].style.backgroundColor = new Color(0, 0, 0, 0);
                else if (prevPlace_B == PlayIndexPlace.Container)
                    containerCatalogue_B[prevIndex_B].style.backgroundColor = new Color(0, 0, 0, 0);
            }
            catch(Exception  e)
            {
                Debug.LogWarning(e);
            }

            if(place == PlayIndexPlace.Bag)
            {
                if (index < 0 || index >= propCatalogue_B.childCount) return;
                propCatalogue_B[index].style.backgroundColor = new Color(0.2f, 0.3f, 0.9f, 0.7f);
            }
            else if(place == PlayIndexPlace.Container)
            {
                if (index < 0 || index >= containerCatalogue_B.childCount) return;
                containerCatalogue_B[index].style.backgroundColor = new Color(0.2f, 0.3f, 0.9f, 0.7f);
            }
            
            prevIndex_B = index;
            prevPlace_B = place;
        }

        /// <summary>
        /// playerA:把描述写进UI文本
        /// </summary>
        public void DescribeProp_A(Prop prop)
        {
            propDescribe_A.text = prop != null ? prop.description : string.Empty;
        }

        public void DescribeProp_B(Prop prop)
        {
            propDescribe_B.text = prop != null ? prop.description : string.Empty;
        }

        /// <summary>
        /// playerA:打开箱子UI
        /// </summary>
        public void OpenContainer_A()
        {
            Debug.Log("打开箱子A");
            containerUI_A.style.display = DisplayStyle.Flex;
            BagUI_A.style.display = DisplayStyle.Flex;
        }

        public void OpenContainer_B()
        {
            Debug.Log("打开箱子B");
            containerUI_B.style.display = DisplayStyle.Flex;
            BagUI_B.style.display = DisplayStyle.Flex;
        }
        
        public void CloseContainer_A()
        {
            Debug.Log("关闭箱子A");
            containerUI_A.style.display = DisplayStyle.None;
            BagUI_A.style.display = DisplayStyle.None;
        }
        
        public void CloseContainer_B()
        {
            Debug.Log("关闭箱子B");
            containerUI_B.style.display = DisplayStyle.None;
            BagUI_B.style.display = DisplayStyle.None;
        }

        /// <summary>
        /// playerA:刷新箱子UI
        /// </summary>
        public void RefreshContainer_A(List<Texture2D> PropImage, ContainerType type)
        {
            containerTextType_A.text = type.ToString();
            containerCatalogue_A.Clear();
            foreach (var image in PropImage)
            {
                VisualElement v = PropCaseUI.Instantiate().Q("PropCase");
                v.style.height = 75f;
                v.style.width = 75f;
                v.style.backgroundImage = image;
                containerCatalogue_A.Add(v);
            }
        }


        public void RefreshContainer_B(List<Texture2D> PropImage, ContainerType type)
        {
            containerTextType_B.text = type.ToString();
            containerCatalogue_B.Clear();
            foreach (var image in PropImage)
            {
                VisualElement v = PropCaseUI.Instantiate().Q("PropCase");
                v.style.height = 75f;
                v.style.width = 75f;
                v.style.backgroundImage = image;
                containerCatalogue_B.Add(v);
            }
        }

/////// 结算画面 //////////////////////////////////////////////////////

        public void Evacuate(bool isDead_A, bool isDead_B, int property_A, int property_B)
        {
            bool success_A = !isDead_A;
            bool success_B = !isDead_B;

            if (!success_A && !success_B) settleStateText.text = "无人生还";
            else if (success_A && success_B) settleStateText.text = "全员撤离成功";
            else settleStateText.text = "部分撤离成功";

            StringBuilder describe = new StringBuilder();
            if (isDead_A) describe.Append("A阵亡       ");
            else describe.Append("A撤离成功    ");
            describe.Append("带出物品价值" + property_A + '\n');
            
            if (isDead_B) describe.Append("B阵亡       ");
            else describe.Append("B撤离成功    ");
            describe.Append("带出物品价值" + property_B + '\n');

            describe.Append($"带出物品总价值{property_A+property_B}");
            
            summaryText.text = describe.ToString();
            settle.style.display = DisplayStyle.Flex;   // 显示结算面板
        }
    }
}
