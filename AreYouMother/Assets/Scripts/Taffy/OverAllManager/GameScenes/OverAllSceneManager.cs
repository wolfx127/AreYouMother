////
//负责更新整个游戏的场景列表，包括每个场景的初始化和收尾工作
////

using System.Collections;
using Taffy.Data.PropData;
using Taffy.Home;
using Taffy.Play.Container;
using Taffy.Play.Player;
using TaffyFrame.EventBus;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Taffy.OverAllManager.GameScenes
{
    public class OverAllSceneManager:MonoBehaviour
    {
        
        private void Awake()
        {
            Debug.Log($"[初始化] OverAllSceneManager.Awake, 所在场景:{gameObject.scene.name}");
            // PropList.BuildList() 必须在 PropRarity.Build() 之前：后者遍历的是 PropList.propSOList，
            // 顺序反了会让四个稀有度桶全建成空的，开箱时 ContainerCreator 的 while 永远抽不到东西 → 主线程卡死
            PropList.BuildList();
            PropBehaviorTable.BuildTable();
            PropRarity.Build();
            ContainerCreatorTool.Build();
            
            WarehouseManager.InitWarehouse();
            DealerManager.InitDealer();
            OverAllStates.ChangeToHome();

            StartCoroutine(InitScenesIemrt());
        }

        private IEnumerator InitScenesIemrt()
        {
            yield return SceneManager.LoadSceneAsync("Home", LoadSceneMode.Additive);
            Debug.Log("场景Home加载成功");
            
            yield return SceneManager.UnloadSceneAsync("Start");
            Debug.Log("场景Start卸载成功");
        }

        private void OnDestroy()
        {
            Debug.Log("[销毁] OverAllSceneManager 被销毁了! 若开局即出现=没挂DontDestroyOnLoad");
        }

        private void OnEnable()
        {
            EventBus.Subscribe<ChangeSceneHomeToPlayingEvent>(ChangeSceneToPlaying);
            EventBus.Subscribe<ChangeScenePlayingToHomeEvent>(ChangeSceneToHome);
            EventBus.Subscribe<ExitGameEvent>(ExitGame);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<ChangeSceneHomeToPlayingEvent>(ChangeSceneToPlaying);
            EventBus.Unsubscribe<ChangeScenePlayingToHomeEvent>(ChangeSceneToHome);
            EventBus.Unsubscribe<ExitGameEvent>(ExitGame);
        }

/////////////////////////////////////////////////////////////////////////////////////////

        private void ChangeSceneToPlaying(ChangeSceneHomeToPlayingEvent evt)
        {
            StartCoroutine(ChangeSceneToPlayingIemrt());
            OverAllStates.ChangeToPlay();
        }
        private IEnumerator ChangeSceneToPlayingIemrt()
        {
            yield return SceneManager.LoadSceneAsync("Play", LoadSceneMode.Additive);
            
            OverAllPlayerController.Instance.GiveDataToPlaying();
            
            yield return SceneManager.UnloadSceneAsync("Home");
        }

        private void ChangeSceneToHome(ChangeScenePlayingToHomeEvent evt)
        {
            StartCoroutine(ChangeSceneToHomeIemrt());
            OverAllStates.ChangeToHome();
        }
        private IEnumerator ChangeSceneToHomeIemrt()
        {
            OverAllPlayerController.Instance.SetBothBag(PlayingHandler_A.Instance.player.bag , PlayingHandler_B.Instance.player.bag);
            
            yield return SceneManager.LoadSceneAsync("Home", LoadSceneMode.Additive);
            yield return SceneManager.UnloadSceneAsync("Play");
        }

        private void ExitGame(ExitGameEvent evt)
        {
            Application.Quit();
        }
    }
}
