#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Editor
{
    [InitializeOnLoad]
    public static class EnsureSceneAppearOrDisappear
    {
        private static string[] savedScenePaths;

        static EnsureSceneAppearOrDisappear()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode)
            {
                savedScenePaths = new string[EditorSceneManager.sceneCount];
                for (int i = 0; i < EditorSceneManager.sceneCount; i++)
                    savedScenePaths[i] = EditorSceneManager.GetSceneAt(i).path;

                EditorSceneManager.OpenScene("Assets/Scenes/Taffy/Start.unity");
            }
            else if (state == PlayModeStateChange.EnteredEditMode)
            {
                if (savedScenePaths == null) return;
                EditorSceneManager.OpenScene(savedScenePaths[0]);
                for (int i = 1; i < savedScenePaths.Length; i++)
                    EditorSceneManager.OpenScene(savedScenePaths[i], OpenSceneMode.Additive);
                savedScenePaths = null;
            }
        }
    }
}
#endif