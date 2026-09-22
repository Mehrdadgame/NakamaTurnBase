using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace NinjaBattle.Editor
{
    /// <summary>
    /// The cellParticle prefab (UIParticle) shipped with localScale (0,0,0) on its
    /// root — and every scene instance overrides the scale with 0 too — so board
    /// cell particles never rendered. This tool resets prefab + all instances to 1.
    /// </summary>
    public static class CellParticleScaleRepair
    {
        private const string PrefabPath = "Assets/VFX/cellParticle.prefab";

        private static readonly string[] BattleScenes =
        {
            "Assets/-Scenes/4-VerticalAndHorizontal.unity",
            "Assets/-Scenes/5-FourByThree1.unity",
            "Assets/-Scenes/6-FourByFour1.unity"
        };

        [MenuItem("Tools/NinjaBattle/Validation/Repair Cell Particle Scales")]
        public static void Repair()
        {
            RepairPrefab();
            int total = 0;
            foreach (string scenePath in BattleScenes)
                total += RepairScene(scenePath);
            Debug.Log($"CellParticleScaleRepair: done. Prefab reset + {total} scene instances fixed.");
        }

        private static void RepairPrefab()
        {
            GameObject root = PrefabUtility.LoadPrefabContents(PrefabPath);
            try
            {
                if (root.transform.localScale != Vector3.one)
                {
                    root.transform.localScale = Vector3.one;
                    PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
                    Debug.Log("CellParticleScaleRepair: prefab root scale reset to 1.");
                }
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static int RepairScene(string scenePath)
        {
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            int fixedCount = 0;

            foreach (ParticleSystem ps in Object.FindObjectsByType<ParticleSystem>(
                         FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                GameObject source = PrefabUtility.GetCorrespondingObjectFromSource(ps.gameObject);
                if (source == null || AssetDatabase.GetAssetPath(source) != PrefabPath)
                    continue;
                if (ps.transform.localScale == Vector3.one)
                    continue;

                ps.transform.localScale = Vector3.one;
                EditorUtility.SetDirty(ps.transform);
                fixedCount++;
            }

            if (fixedCount > 0)
                EditorSceneManager.SaveScene(scene);
            Debug.Log($"CellParticleScaleRepair: {scenePath} → {fixedCount} instances fixed.");
            return fixedCount;
        }
    }
}
