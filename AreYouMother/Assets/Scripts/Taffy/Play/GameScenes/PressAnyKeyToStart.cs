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
            if (!hasStarted && (Keyboard.current.anyKey.wasPressedThisFrame || Mouse.current.leftButton.wasPressedThisFrame || Mouse.current.rightButton.wasPressedThisFrame))
            {
                hasStarted = true;
                SceneManager.LoadSceneAsync("Home");
                SceneManager.UnloadSceneAsync("Start");
                Destroy(gameObject);
            }
        }
    }
}
