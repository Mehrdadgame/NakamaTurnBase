using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NinjaBattle.UI.Editor
{
    public static class GameUiAuthoring
    {
        private const string HomeScenePath = "Assets/-Scenes/2-Home.unity";

        [MenuItem("Tools/NinjaBattle/UI/Build Complete Game UX")]
        public static void BuildCompleteGameUx()
        {
            Scene homeScene = EditorSceneManager.OpenScene(HomeScenePath, OpenSceneMode.Single);
            HomeMenuAuthoring.Build();
            EditorSceneManager.SaveScene(homeScene);

            BattleSceneAuthoring.BuildAllScenes();
            AssetDatabase.SaveAssets();
            Debug.Log("GameUiAuthoring: Home, Shop, Profile, Avatar and all battle layouts were built successfully.");
        }

        public static void BuildFromCommandLine()
        {
            BuildCompleteGameUx();
        }
    }
}
