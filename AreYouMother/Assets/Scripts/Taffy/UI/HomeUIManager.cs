using System;
using Taffy.Data;
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
            bagCatalogueUI_B.style.flexDirection = FlexDirection.RowReverse;
            propText_A = bagUI_A.Q<VisualElement>("BottomPivot");
            propText_B = bagUI_B.Q<VisualElement>("BottomPivot");
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
            Check_A();
            Check_B();
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
        }
        private void Unsubscribe()
        {
            homeUIPro.Unsubscribe();
            homeUIPro.CheckProp_AEvent -= Check_A;
            homeUIPro.CheckProp_BEvent -= Check_B;
            homeUIPro.RefreshBag_AEvent -= RefreshBag_A;
            homeUIPro.RefreshBag_BEvent -= RefreshBag_B;
            homeUIPro.RefreshWarehouseEvent -= RefreshWarehouse;
            homeUIPro.RefreshDealerEvent -= RefreshDealer;
        }

        private void RefreshBag_A()
        {
            bagCatalogueUI_A.Clear();
            foreach (Prop p in homeUIPro.GetBag_A())
            {
                VisualElement propcase = propCaseAsset.Instantiate().Q<VisualElement>("PropCase");
                propcase.style.backgroundImage = PropsTool.GetPropImage(p);
                bagCatalogueUI_A.Add(propcase);
            }
        }
        private void RefreshBag_B()
        {
            bagCatalogueUI_B.Clear();
            foreach (Prop p in homeUIPro.GetBag_B())
            {
                VisualElement propcase = propCaseAsset.Instantiate().Q<VisualElement>("PropCase");
                propcase.style.backgroundImage = PropsTool.GetPropImage(p);
                bagCatalogueUI_B.Add(propcase);
            }
        }
        private void RefreshWarehouse()
        {
            centerCatalogue.Clear();
            foreach (Prop p in homeUIPro.GetWarehouse())
            {
                VisualElement propcase = propCaseAsset.Instantiate().Q<VisualElement>("PropCase");
                propcase.style.backgroundImage = PropsTool.GetPropImage(p);
                centerCatalogue.Add(propcase);
            }
        }
        private void RefreshDealer()
        {
//TODO:做商人数据
        }

        private void Check_A()
        {
            if (homeUIPro.prevIndexPlace_A == Place.bagA)
            {
                bagCatalogueUI_A.ElementAt(homeUIPro.precIndex_A).style.backgroundColor = StyleKeyword.Null;
            }
            else
            {
                centerCatalogue.ElementAt(homeUIPro.precIndex_A).style.backgroundColor = StyleKeyword.Null;
            }

            if (homeUIPro.indexPlace_A == Place.bagA)
            {
                bagCatalogueUI_A.ElementAt(homeUIPro.precIndex_A).style.backgroundColor = new Color(0.4f, 0.5f, 0.8f, 0.8f);
            }
            else
            {
                centerCatalogue.ElementAt(homeUIPro.precIndex_A).style.backgroundColor = new Color(0.4f, 0.5f, 0.8f, 0.8f);
            }
            
            homeUIPro.KeepUpWithIndex_A();

            propText_A.Q<Label>("PropName").text = homeUIPro.GetCheckingPropName_A();
            propText_A.Q<Label>("PropDescribe").text = homeUIPro.GetCheckingPropDescribe_A();
        }
        private void Check_B()
        {
            
            if (homeUIPro.prevIndexPlace_B == Place.bagB)
            {
                bagCatalogueUI_B.ElementAt(homeUIPro.precIndex_B).style.backgroundColor = StyleKeyword.Null;
            }
            else
            {
                centerCatalogue.ElementAt(homeUIPro.precIndex_B).style.backgroundColor = StyleKeyword.Null;
            }

            if (homeUIPro.indexPlace_B == Place.bagB)
            {
                bagCatalogueUI_B.ElementAt(homeUIPro.precIndex_B).style.backgroundColor = new Color(0.5f, 0.4f, 0.7f, 0.8f);
            }
            else
            {
                centerCatalogue.ElementAt(homeUIPro.precIndex_B).style.backgroundColor = new Color(0.5f, 0.4f, 0.7f, 0.8f);
            }
            
            homeUIPro.KeepUpWithIndex_B();

            propText_B.Q<Label>("PropName").text = homeUIPro.GetCheckingPropName_B();
            propText_B.Q<Label>("PropDescribe").text = homeUIPro.GetCheckingPropDescribe_B();
        }
    }
}
