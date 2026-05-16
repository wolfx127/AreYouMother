using Taffy.OverAllManager;
using UnityEngine;
using UnityEngine.SceneManagement;

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
