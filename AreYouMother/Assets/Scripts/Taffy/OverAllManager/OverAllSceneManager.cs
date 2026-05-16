using System;
using System.Collections;
using System.Collections.Generic;
using Taffy.Data;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Taffy.OverAllManager
{
    /// <summary>
    /// 负责更新整个游戏的场景列表，包括每个场景的初始化和收尾工作
    /// </summary>
    public class OverAllSceneManager:MonoBehaviour
    {
        private void Awake()
        {
            EventBus.Subscribe<ChangeSceneHomeToPlayingEvent>(ChangeSceneToPlaying);
        }
        
        public void ChangeSceneToPlaying(ChangeSceneHomeToPlayingEvent evt)
        {
            StartCoroutine(ChangeSceneRoutine());
        }

        private IEnumerator ChangeSceneRoutine()
        {
            yield return SceneManager.LoadSceneAsync("Play", LoadSceneMode.Additive);

            EventBus.Publish(new GiveHPandMPEvent());

            yield return SceneManager.UnloadSceneAsync("Home");
        }
    }
}
