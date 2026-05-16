using Taffy.OverAllManager;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using EventBus = Taffy.OverAllManager.EventBus;

namespace Taffy.UI.Pro
{
    public class HomeUI_pro
    {
        public void ChangeSceneToPlaying()
        {
            EventBus.Publish(new ChangeSceneHomeToPlayingEvent());
        }
    }
}
