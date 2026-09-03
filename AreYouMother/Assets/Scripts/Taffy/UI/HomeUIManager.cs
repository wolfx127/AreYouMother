using System;
using Taffy.Data;
using Taffy.Data.PropData;
using Taffy.Home;
using Taffy.OverAllManager;
using Taffy.UI.Pre;
using Taffy.UI.Pro;
using UnityEngine;
using UnityEngine.UIElements;

namespace Taffy.UI
{
    public interface IHomeUIManager
    {
        public void ChangeToWarehouse();
        public void ChangeToDealer();
        public void UpdatePropertyNum(int property);
        public void UpdateStateInfo_A(int HP, int MP, int ATK, int DEF);
        public void UpdateStateInfo_B(int HP, int MP, int ATK, int DEF);
        public void Check_A(int index,HomeIndexPlace place);
        public void Check_B(int index,HomeIndexPlace place);
        public void RefreshBag_A();
        public void RefreshBag_B();
        public void RefreshWarehouse();
        public void RefreshDealer();
    }

    public class HomeUIManager : MonoBehaviour, IHomeUIManager
    {
        private IHomeUI_pre homeUIPre = new HomeUI_pre();
        private VisualElement root;
        private Button engagePlayBtn;
        private Button exitGameBtn;
        [SerializeField] private VisualTreeAsset propCaseAsset;
        [SerializeField] private VisualTreeAsset BagAsset;
        private VisualElement bagUI_A;
        private VisualElement bagUI_B;
        private VisualElement bagCatalogueUI_A;
        private VisualElement bagCatalogueUI_B;
        private VisualElement propText_A;
        private VisualElement propText_B;
        private VisualElement centerCatalogue;
        private Label bagInfoUI_A;
        private Label bagInfoUI_B;
        private Button dealerBtn;
        private Label centerName;
        private Label propertyNum;
        private Label ruleTips;
        private Label stateInfo_A;
        private Label stateInfo_B;
        private Label CenterInfo;

        private (int index, HomeIndexPlace place) prevChoose_A = (-1, HomeIndexPlace.BagA);
        private (int index, HomeIndexPlace place) prevChoose_B = (-1, HomeIndexPlace.BagB);

        private static readonly Color ColorA = new Color(0.8f, 0.3f, 0.4f, 0.7f);
        private static readonly Color ColorB = new Color(0.3f, 0.4f, 0.8f, 0.7f);
        private static readonly Color ColorBoth = new Color(0.55f, 0.35f, 0.6f, 0.8f);   // 蓝(A)+紫(B)的蓝紫色
        

        private void Awake()
        {
            Debug.Log($"[UI_V] Awake, 所在场景:{gameObject.scene.name}");
            root = GetComponent<UIDocument>().rootVisualElement;
            engagePlayBtn = root.Q<Button>("EngagePlayBtn");
            exitGameBtn = root.Q<Button>("ExitGameBtn");
            bagUI_A = BagAsset.Instantiate().Q<VisualElement>("root");
            bagUI_B = BagAsset.Instantiate().Q<VisualElement>("root");
            var cp = root.Q<VisualElement>("CenterPivot");
            cp.Q<VisualElement>("_LeftPivot").Add(bagUI_A);
            cp.Q<VisualElement>("_RightPivot").Add(bagUI_B);
            centerCatalogue = root.Q<VisualElement>("CenterCatalogue");
            bagCatalogueUI_A = bagUI_A.Q<VisualElement>("PropsCatalogue");
            bagCatalogueUI_B = bagUI_B.Q<VisualElement>("PropsCatalogue");
            propText_A = bagUI_A.Q<VisualElement>("BottomPivot");
            propText_B = bagUI_B.Q<VisualElement>("BottomPivot");
            bagInfoUI_A = bagUI_A.Q<Label>("BagInfo");
            bagInfoUI_B = bagUI_B.Q<Label>("BagInfo");
            dealerBtn = root.Q<Button>("DealerBtn");
            centerName = root.Q<Label>("CenterName");
            propertyNum = root.Q<Label>("PropertyNum");
            ruleTips = root.Q<Label>("RuleTips");
            stateInfo_A = root.Q<Label>("PlayerAStateInfo");
            stateInfo_B = root.Q<Label>("PlayerBStateInfo");
            CenterInfo = root.Q<Label>("CenterInfo");
        }
        
        private void OnEnable()
        {
            engagePlayBtn.clicked += homeUIPre.ChangeSceneToPlaying;
            exitGameBtn.clicked += homeUIPre.ExitGame;
            dealerBtn.clicked += homeUIPre.ChangeCenter;
        }

        private void OnDisable()
        {
            engagePlayBtn.clicked -= homeUIPre.ChangeSceneToPlaying;
            exitGameBtn.clicked -= homeUIPre.ExitGame;
            dealerBtn.clicked -= homeUIPre.ChangeCenter;
            Unsubscribe();
        }

        private void OnDestroy()
        {
            Debug.Log("[销毁] HomeUIManager 被销毁了");
        }

        private void Start()
        {
            Subscribe();
            RefreshBag_A();
            RefreshBag_B();
            RefreshWarehouse();
            centerName.text = "仓库";
            UpdatePropertyNum(homeUIPre.GetProperty());
            UpdateStateInfo_A(homeUIPre.GetState_A().hp,homeUIPre.GetState_A().mp,homeUIPre.GetState_A().atk,homeUIPre.GetState_A().def);
            UpdateStateInfo_B(homeUIPre.GetState_B().hp,homeUIPre.GetState_B().mp,homeUIPre.GetState_B().atk,homeUIPre.GetState_B().def);
        }

        private void Subscribe()
        {
            homeUIPre.Subscribe(this);
        }
        private void Unsubscribe()
        {
            homeUIPre.Unsubscribe();
        }

        public void RefreshBag_A()
        {
            bagCatalogueUI_A.Clear();
            foreach (var i in homeUIPre.GetPropImages_BagA())
            {
                VisualElement propcase = propCaseAsset.Instantiate().Q<VisualElement>("PropCase");
                propcase.style.backgroundImage = i;
                bagCatalogueUI_A.Add(propcase);
            }

            bagInfoUI_A.text = $"背包上限/ 现存道具数 : 20/{homeUIPre.GetCount_BagA()}";
        }
        public void RefreshBag_B()
        {
            bagCatalogueUI_B.Clear();
            foreach (var tex in homeUIPre.GetPropImages_BagB())
            {
                VisualElement propcase = propCaseAsset.Instantiate().Q<VisualElement>("PropCase");
                propcase.style.backgroundImage = tex;
                bagCatalogueUI_B.Add(propcase);
            }

            bagInfoUI_B.text = $"背包上限/ 现存道具数 : 20/{homeUIPre.GetCount_BagB()}";
        }
        public void RefreshWarehouse()
        {
            centerCatalogue.Clear();
            foreach (var tex in homeUIPre.GetPropImages_Warehouse())
            {
                VisualElement propcase = propCaseAsset.Instantiate().Q<VisualElement>("PropCase");
                propcase.style.height = 100;
                propcase.style.backgroundImage = tex;
                centerCatalogue.Add(propcase);
            }
        }
        public void RefreshDealer()
        {
            centerCatalogue.Clear();
            foreach (var tex in homeUIPre.GetPropImages_Dealer())
            {
                VisualElement propcase = propCaseAsset.Instantiate().Q<VisualElement>("PropCase");
                propcase.style.height = 100;
                propcase.style.backgroundImage = tex;
                centerCatalogue.Add(propcase);
            }
        }

        public void Check_A(int index,HomeIndexPlace place)
        {
            if (index == prevChoose_A.index && place == prevChoose_A.place) return;

            var old = prevChoose_A;
            prevChoose_A = (index, place);
            RepaintCell(old.index, old.place);   // 离开的格子按剩余者重算
            RepaintCell(index, place);           // 新落的格子按双方重算

            var prop = homeUIPre.GetChooseProp_A();
            propText_A.Q<Label>("PropName").text = prop?.name ?? "";
            propText_A.Q<Label>("PropDescribe").text = prop?.description ?? "";
        }
        public void Check_B(int index,HomeIndexPlace place)
        {
            if (index == prevChoose_B.index && place == prevChoose_B.place) return;

            var old = prevChoose_B;
            prevChoose_B = (index, place);
            RepaintCell(old.index, old.place);
            RepaintCell(index, place);

            var prop = homeUIPre.GetChooseProp_B();
            propText_B.Q<Label>("PropName").text = prop?.name ?? "";
            propText_B.Q<Label>("PropDescribe").text = prop?.description ?? "";
        }

        /// <summary>按 A、B 当前光标重算某格的背景色：无 / 单人 / 双人重叠</summary>
        private void RepaintCell(int index, HomeIndexPlace place)
        {
            if (index < 0) return;
            var container = GetContainer(place);
            if (index >= container.childCount) return;

            bool onA = prevChoose_A.index == index && prevChoose_A.place == place;
            bool onB = prevChoose_B.index == index && prevChoose_B.place == place;

            if (onA && onB) container.ElementAt(index).style.backgroundColor = ColorBoth;
            else if (onA)   container.ElementAt(index).style.backgroundColor = ColorA;
            else if (onB)   container.ElementAt(index).style.backgroundColor = ColorB;
            else            container.ElementAt(index).style.backgroundColor = StyleKeyword.Null;
        }

        private VisualElement GetContainer(HomeIndexPlace place)
        {
            if (place == HomeIndexPlace.BagA) return bagCatalogueUI_A;
            if (place == HomeIndexPlace.BagB) return bagCatalogueUI_B;
            return centerCatalogue;   // Warehouse / Dealer 共用中间栏
        }

        public void ChangeToWarehouse()
        {
            RefreshWarehouse();
            UpdateRuleTips_Warehouse();
            Debug.Log("切换至仓库");
        }

        public void ChangeToDealer()
        {
            RefreshDealer();
            UpdateRuleTips_Dealer();
            Debug.Log("切换至商人");
        }

        public void UpdatePropertyNum(int property)
        {
            propertyNum.text = $"总资产:{property}";
        }

        public void UpdateStateInfo_A(int HP, int MP, int ATK, int DEF)
        {
            stateInfo_A.text = $"HP上限:{HP}" + '\n' + $"MP上限:{MP}" + '\n' 
                               + $"当前攻击力:{ATK}  当前防御力:{DEF}";
        }

        public void UpdateStateInfo_B(int HP, int MP, int ATK, int DEF)
        {
            stateInfo_A.text = $"HP上限:{HP}" + '\n' + $"MP上限:{MP}" + '\n' 
                               + $"当前攻击力:{ATK}  当前防御力:{DEF}";
        }


        private void UpdateRuleTips_Warehouse()
        {
            ruleTips.text = "这里是仓库，带出来的道具都可以放在这里，只能在背包里使用道具" + '\n' 
                                                                    + "使用道具说明：首先它必须是能使用的道具。武器和护甲也可以使用，数值将用于战斗" + '\n'
                                                                    + "玩家无法从仓库拿取不属于自己的道具" + '\n'
                                                                    + "玩家1按'F'交换仓库道具，按'Z'使用道具" + '\n' 
                                                                    + "玩家2按'小键盘0'交换仓库道具,按'小键盘9'使用道具";
        }

        private void UpdateRuleTips_Dealer()
        {
            ruleTips.text = "这里是商人，可以用总资产和他交易物品，好感度越高，卖的品质越高。买卖一次成交概不退货" + '\n' 
                                                                         + "玩家无法购买不属于自己的道具" + '\n'
                                                                         + "玩家1按'F'买卖道具" + '\n'
                                                                         + "玩家2按'小键盘0'买卖道具";
        }
    }
}
