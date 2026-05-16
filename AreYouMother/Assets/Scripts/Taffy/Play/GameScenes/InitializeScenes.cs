using UnityEngine;
using UnityEngine.SceneManagement;

namespace Taffy.Play.GameScenes
{
    public class InitializeScenes:MonoBehaviour
    {
        private void Awake()
        {
            SceneManager.LoadSceneAsync("Constant",LoadSceneMode.Additive);
            Debug.Log("场景Constant加载成功");
        }
    }
}
