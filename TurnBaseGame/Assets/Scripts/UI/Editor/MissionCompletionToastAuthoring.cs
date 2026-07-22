using TMPro;
using UnityEditor;
using UnityEngine;

namespace NinjaBattle.UI.Editor
{
    public static class MissionCompletionToastAuthoring
    {
        private const string FolderPath = "Assets/Prefabs/UI";
        private const string PrefabPath = FolderPath + "/MissionCompletionToast.prefab";
        private const string VazirPath = "Assets/Font/Vazir-Bold SDF.asset";

        [MenuItem("Tools/NinjaBattle/UI/Create Mission Completion Toast Prefab")]
        public static void CreatePrefab()
        {
            EnsureFolder("Assets/Prefabs");
            EnsureFolder(FolderPath);

            GameObject root = new GameObject("MissionCompletionToast");
            MissionCompletionToast toast = root.AddComponent<MissionCompletionToast>();
            SerializedObject serializedToast = new SerializedObject(toast);
            serializedToast.FindProperty("vazirFont").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(VazirPath);
            serializedToast.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("MissionCompletionToast prefab created with Vazir font: " + PrefabPath);
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
                return;

            string parent = System.IO.Path.GetDirectoryName(path).Replace("\\", "/");
            string folder = System.IO.Path.GetFileName(path);
            AssetDatabase.CreateFolder(parent, folder);
        }
    }
}
