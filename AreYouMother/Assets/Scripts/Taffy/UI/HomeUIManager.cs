using System;
using Taffy.Data;
using Taffy.Data.PropData;
using Taffy.Home;
using Taffy.OverAllManager;
using Taffy.UI.Pro;
using UnityEngine;
using UnityEngine.UIElements;

namespace Taffy.UI
{
    public class HomeUIManager : MonoBehaviour
    {
        private HomeUI_pro homeUIPro = new HomeUI_pro();
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
        

        private void Awake()
        {
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
            engagePlayBtn.clicked += homeUIPro.ChangeSceneToPlaying;
            exitGameBtn.clicked += homeUIPro.ExitGame;
        }

        private void OnDisable()
        {
            engagePlayBtn.clicked -= homeUIPro.ChangeSceneToPlaying;
            exitGameBtn.clicked -= homeUIPro.ExitGame;
            Unsubscribe();
        }

        private void Start()
        {
            Subscribe();
            RefreshBag_A();
            RefreshBag_B();
            RefreshWarehouse();
            centerName.text = "仓库";
            Check_A();
            Check_B();
            UpdatePropertyNum();
            UpdateStateInfo_A();
            UpdateStateInfo_B();
            UpdateRuleTips();
        }

        private void Subscribe()
        {
            homeUIPro.Subscribe();
            homeUIPro.CheckProp_AEvent += Check_A;
            homeUIPro.CheckProp_BEvent += Check_B;
            homeUIPro.RefreshBag_AEvent += RefreshBag_A;
            homeUIPro.RefreshBag_BEvent += RefreshBag_B;
            homeUIPro.RefreshWarehouseEvent += RefreshWarehouse;
            homeUIPro.RefreshDealerEvent += RefreshDealer;
            homeUIPro.UpdatePropertyEvent += UpdatePropertyNum;
            homeUIPro.UpdateRuleTipsEvent += UpdateRuleTips;
            OverAllPlayerController.Instance.UpdateInfo_AEvent += UpdateStateInfo_A;
            OverAllPlayerController.Instance.UpdateInfo_BEvent += UpdateStateInfo_B;
            
            dealerBtn.RegisterCallback<ClickEvent>(ChangeCenter);
        }
        private void Unsubscribe()
        {
            
            OverAllPlayerController.Instance.UpdateInfo_AEvent -= UpdateStateInfo_A;
            OverAllPlayerController.Instance.UpdateInfo_BEvent -= UpdateStateInfo_B;
            
            homeUIPro.Unsubscribe();
            homeUIPro.CheckProp_AEvent -= Check_A;
            homeUIPro.CheckProp_BEvent -= Check_B;
            homeUIPro.RefreshBag_AEvent -= RefreshBag_A;
            homeUIPro.RefreshBag_BEvent -= RefreshBag_B;
            homeUIPro.RefreshWarehouseEvent -= RefreshWarehouse;
            homeUIPro.RefreshDealerEvent -= RefreshDealer;
            homeUIPro.UpdatePropertyEvent -= UpdatePropertyNum;
            homeUIPro.UpdateRuleTipsEvent -= UpdateRuleTips;
            
            dealerBtn.UnregisterCallback<ClickEvent>(ChangeCenter);
        }

        private void RefreshBag_A()
        {
            bagCatalogueUI_A.Clear();
            foreach (Prop p in homeUIPro.GetBag_A())
            {
                VisualElement propcase = propCaseAsset.Instantiate().Q<VisualElement>("PropCase");
//                propcase.style.backgroundImage = PropsTool.GetPropImage(p);
                bagCatalogueUI_A.Add(propcase);
            }

            bagInfoUI_A.text = homeUIPro.GetBagInfo_A();
            Check_A();
        }
        private void RefreshBag_B()
        {
            bagCatalogueUI_B.Clear();
            foreach (Prop p in homeUIPro.GetBag_B())
            {
                VisualElement propcase = propCaseAsset.Instantiate().Q<VisualElement>("PropCase");
//                propcase.style.backgroundImage = PropsTool.GetPropImage(p);
                bagCatalogueUI_B.Add(propcase);
            }
            
            bagInfoUI_B.text = homeUIPro.GetBagInfo_B();
            Check_B();
        }
        private void RefreshWarehouse()
        {
            centerCatalogue.Clear();
            foreach (Prop p in homeUIPro.GetWarehouse())
            {
                VisualElement propcase = propCaseAsset.Instantiate().Q<VisualElement>("PropCase");
                propcase.style.height = 100;
//                propcase.style.backgroundImage = PropsTool.GetPropImage(p);
                centerCatalogue.Add(propcase);
            }
        }
        private void RefreshDealer()
        {
            centerCatalogue.Clear();
            foreach (Prop p in homeUIPro.GetDealerStore())
            {
                VisualElement propcase = propCaseAsset.Instantiate().Q<VisualElement>("PropCase");
                propcase.style.height = 100;
//                propcase.style.backgroundImage = PropsTool.GetPropImage(p);
                centerCatalogue.Add(propcase);
            }
        }

        private void Check_A()
        {
            if (homeUIPro.prevIndexPlace_A == Place.bagA)
            { if (homeUIPro.prevIndex_A < bagCatalogueUI_A.childCount)
                bagCatalogueUI_A.ElementAt(homeUIPro.prevIndex_A).style.backgroundColor = StyleKeyword.Null; }
            else
            { if (homeUIPro.prevIndex_A < centerCatalogue.childCount)
                centerCatalogue.ElementAt(homeUIPro.prevIndex_A).style.backgroundColor = StyleKeyword.Null; }

            if(homeUIPro.indexPlace_A == homeUIPro.indexPlace_B && homeUIPro.index_A == homeUIPro.index_B)
            { if (homeUIPro.prevIndex_A < centerCatalogue.childCount)
                centerCatalogue.ElementAt(homeUIPro.prevIndex_B).style.backgroundColor = new Color(0.5f, 0.4f, 0.7f, 0.8f); }

            if (homeUIPro.indexPlace_A == Place.bagA)
            { if (homeUIPro.index_A < bagCatalogueUI_A.childCount)
                bagCatalogueUI_A.ElementAt(homeUIPro.index_A).style.backgroundColor = new Color(0.4f, 0.5f, 0.8f, 0.8f); }
            else
            { if (homeUIPro.index_A < centerCatalogue.childCount)
                centerCatalogue.ElementAt(homeUIPro.index_A).style.backgroundColor = new Color(0.4f, 0.5f, 0.8f, 0.8f); }

            homeUIPro.KeepUpWithIndex_A();

            propText_A.Q<Label>("PropName").text = homeUIPro.GetCheckingPropName_A();
//TODO            propText_A.Q<Label>("PropDescribe").text = homeUIPro.GetCheckingPropDescribe_A();
        }
        private void Check_B()
        {
            if (homeUIPro.prevIndexPlace_B == Place.bagB)
            { if (homeUIPro.prevIndex_B < bagCatalogueUI_B.childCount)
                bagCatalogueUI_B.ElementAt(homeUIPro.prevIndex_B).style.backgroundColor = StyleKeyword.Null; }
            else
            { if (homeUIPro.prevIndex_B < centerCatalogue.childCount)
                centerCatalogue.ElementAt(homeUIPro.prevIndex_B).style.backgroundColor = StyleKeyword.Null; }

            if(homeUIPro.indexPlace_A == homeUIPro.indexPlace_B && homeUIPro.index_A == homeUIPro.index_B)
            { if (homeUIPro.prevIndex_A < centerCatalogue.childCount)
                centerCatalogue.ElementAt(homeUIPro.prevIndex_A).style.backgroundColor = new Color(0.4f, 0.5f, 0.8f, 0.8f); }

            if (homeUIPro.indexPlace_B == Place.bagB)
            { if (homeUIPro.index_B < bagCatalogueUI_B.childCount)
                bagCatalogueUI_B.ElementAt(homeUIPro.index_B).style.backgroundColor = new Color(0.5f, 0.4f, 0.7f, 0.8f); }
            else
            { if (homeUIPro.index_B < centerCatalogue.childCount)
                centerCatalogue.ElementAt(homeUIPro.index_B).style.backgroundColor = new Color(0.5f, 0.4f, 0.7f, 0.8f); }

            homeUIPro.KeepUpWithIndex_B();

            propText_B.Q<Label>("PropName").text = homeUIPro.GetCheckingPropName_B();
//TODO            propText_B.Q<Label>("PropDescribe").text = homeUIPro.GetCheckingPropDescribe_B();
        }

        private void ChangeCenter(ClickEvent evt)
        {
            homeUIPro.ChangeCenter();
            if (homeUIPro.centerPlace == Place.warehouse)
            {
                RefreshWarehouse();
                centerName.text = "仓库";
            }
            else
            {
                RefreshDealer();
                centerName.text = "商人";
                CenterInfo.text = $"好感度{DealerManager.GetFavoribility()}/100";
                
            }
        }

        private void UpdatePropertyNum()
        {
            propertyNum.text = homeUIPro.PropertyDescribe();
        }

        private void UpdateStateInfo_A()
        {
            stateInfo_A.text = homeUIPro.GetStateInfo_A();
        }

        private void UpdateStateInfo_B()
        {
            stateInfo_B.text = homeUIPro.GetStateInfo_B();
        }

        private void UpdateRuleTips()
        {
            ruleTips.text = homeUIPro.GetRuleTips();
        }
    }
}
