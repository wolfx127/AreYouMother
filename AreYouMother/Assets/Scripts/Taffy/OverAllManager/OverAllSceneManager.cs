////
//负责更新整个游戏的场景列表，包括每个场景的初始化和收尾工作
////
using System;
using System.Collections;
using System.Collections.Generic;
using Taffy.Data;
using Taffy.Home;
using Taffy.Play.Player;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Taffy.OverAllManager
{
    public class OverAllSceneManager:MonoBehaviour
    {
        private void Awake()
        {
            StartCoroutine(InitScenesIemrt());
            PropOccurProbability.Build();
            WarehouseManager.InitWarehouse();
            DealerManager.InitDealer();
        }

        private IEnumerator InitScenesIemrt()
        {
            yield return SceneManager.LoadSceneAsync("Home", LoadSceneMode.Additive);
            Debug.Log("场景Home加载成功");
            
            yield return SceneManager.UnloadSceneAsync("Start");
            Debug.Log("场景Start卸载成功");
        }

        private void OnEnable()
        {
            EventBus.Subscribe<ChangeSceneHomeToPlayingEvent>(ChangeSceneToPlaying);
            EventBus.Subscribe<ChangeScenePlayingToHomeEvent>(ChangeSceneToHome);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<ChangeSceneHomeToPlayingEvent>(ChangeSceneToPlaying);
            EventBus.Unsubscribe<ChangeScenePlayingToHomeEvent>(ChangeSceneToHome);
        }

/////////////////////////////////////////////////////////////////////////////////////////

        private void ChangeSceneToPlaying(ChangeSceneHomeToPlayingEvent evt)
        {
            StartCoroutine(ChangeSceneToPlayingIemrt());
        }
        private IEnumerator ChangeSceneToPlayingIemrt()
        {
            yield return SceneManager.LoadSceneAsync("Play", LoadSceneMode.Additive);

            EventBus.Publish(new InitialPlayingSceneEvent());

            yield return SceneManager.UnloadSceneAsync("Home");
        }

        private void ChangeSceneToHome(ChangeScenePlayingToHomeEvent evt)
        {
            //切场景前先把对局内两个背包回传给对局外，确保数据落地
            PlayerCurrentStateController.Instance.GiveBags();
            StartCoroutine(ChangeSceneToHomeIemrt());
        }
        private IEnumerator ChangeSceneToHomeIemrt()
        {
            yield return SceneManager.LoadSceneAsync("Home", LoadSceneMode.Additive);

            yield return SceneManager.UnloadSceneAsync("Play");
        }
    }
}
