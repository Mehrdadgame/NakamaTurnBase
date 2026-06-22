using UnityEngine;
using UnityEngine.SceneManagement;

namespace Nakama.Helpers
{
    /// <summary>
    /// Spawns a <see cref="ChatManager"/> into each battle scene at runtime — no scene edits.
    /// The three game scenes ship as name variants (e.g. 5-FourByThree vs 5-FourByThree1); matching
    /// on the mode keyword covers every variant without touching the huge scene YAML files.
    /// </summary>
    public static class ChatBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Init()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
            // Handle the scene already active when the game booted (defensive).
            TrySpawn(SceneManager.GetActiveScene());
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode) => TrySpawn(scene);

        private static void TrySpawn(Scene scene)
        {
            if (!scene.IsValid() || !IsGameScene(scene.name)) return;
            if (Object.FindObjectOfType<ChatManager>() != null) return;

            var go = new GameObject("ChatManager");
            if (scene.isLoaded) SceneManager.MoveGameObjectToScene(go, scene);
            go.AddComponent<ChatManager>();
        }

        private static bool IsGameScene(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName)) return false;
            return sceneName.Contains("VerticalAndHorizontal")
                || sceneName.Contains("FourByThree")
                || sceneName.Contains("FourByFour");
        }
    }
}
