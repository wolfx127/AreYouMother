using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Taffy.OverAllManager
{
    public class OverAllSceneManager:MonoBehaviour
    {
        private void Awake()
        {
            EventBus.Subscribe<ChangeSceneHomeToPlayingEvent>(ChangeSceneToPlaying);
        }
        
        public void ChangeSceneToPlaying(ChangeSceneHomeToPlayingEvent evt)
        {
            SceneManager.LoadSceneAsync("Playing");
            SceneManager.UnloadSceneAsync("Home");
        }
    }
}
