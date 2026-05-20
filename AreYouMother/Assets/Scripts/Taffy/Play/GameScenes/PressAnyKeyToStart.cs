////
//一次性脚本，在游戏的第一个场景中，可以让用户按下任意键进入游戏
////
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Taffy.Play.GameScenes
{
    public class PressAnyKeyToStart : MonoBehaviour
    {
        private bool hasStarted = false;
        private void Update()
        {
            if (!hasStarted && (Keyboard.current.anyKey.wasPressedThisFrame))
            {
                hasStarted = true;
                SceneManager.LoadSceneAsync("Constant",LoadSceneMode.Additive);
                Debug.Log("场景Constant加载成功");
                Destroy(gameObject);
            }
        }
    }
}
