using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace NinjaBattle.UI.Editor
{
    public static class BattleSceneAuthoring
    {
        private const string BackdropName = "FigmaBattleBackdrop";
        private const string ChromeName = "FigmaBattleChrome";

        private sealed class LayoutProfile
        {
            public string scenePath;
            public int expectedCells;
            public Vector2 groundMePosition;
            public Vector2 groundOppPosition;
        }

        private static readonly LayoutProfile[] Profiles =
        {
            new LayoutProfile
            {
                scenePath = "Assets/-Scenes/4-VerticalAndHorizontal.unity",
                expectedCells = 9,
                groundMePosition = new Vector2(-0.24249268f, -1.9707031f),
                groundOppPosition = Vector2.zero
            },
            new LayoutProfile
            {
                scenePath = "Assets/-Scenes/5-FourByThree.unity",
                expectedCells = 12,
                groundMePosition = new Vector2(-30.999998f, -89.99994f),
                groundOppPosition = new Vector2(-35.099995f, -51.299957f)
            },
            new LayoutProfile
            {
                scenePath = "Assets/-Scenes/6-FourByFour.unity",
                expectedCells = 16,
                groundMePosition = new Vector2(-0.24249263f, -39.99997f),
                groundOppPosition = new Vector2(0.000000059604645f, 11.0000305f)
            }
        };

        [MenuItem("Tools/NinjaBattle/UI/Build Open Battle UX")]
        public static void BuildOpenScene()
        {
            string path = SceneManager.GetActiveScene().path;
            LayoutProfile profile = Profiles.FirstOrDefault(item => item.scenePath == path);
            if (profile == null)
            {
                Debug.LogError("BattleSceneAuthoring: the active scene is not one of the three build battle scenes.");
                return;
            }

            if (Build(profile))
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

        [MenuItem("Tools/NinjaBattle/UI/Build All Battle UX")]
        public static void BuildAllScenes()
        {
            foreach (LayoutProfile profile in Profiles)
            {
                if (!File.Exists(profile.scenePath))
                {
                    Debug.LogWarning("BattleSceneAuthoring: missing scene " + profile.scenePath);
                    continue;
                }

                Scene scene = EditorSceneManager.OpenScene(profile.scenePath, OpenSceneMode.Single);
                if (Build(profile))
                    EditorSceneManager.SaveScene(scene);
            }

            Debug.Log("BattleSceneAuthoring: all three battle scenes were authored and validated.");
        }

        public static void BuildAllFromCommandLine()
        {
            BuildAllScenes();
        }

        private static bool Build(LayoutProfile profile)
        {
            Canvas canvas = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .FirstOrDefault(item => item.gameObject.scene == SceneManager.GetActiveScene());
            if (canvas == null)
            {
                Debug.LogError("BattleSceneAuthoring: Canvas was not found in " + profile.scenePath);
                return false;
            }

            Transform panel = FindDeep(canvas.transform, "Panel");
            Transform groundMe = FindDeep(panel, "GroundMe");
            Transform groundOpp = FindDeep(panel, "GroundOpp");
            Transform sendEvent = FindDeep(panel, "Send Event");
            if (!Validate(profile, groundMe, groundOpp, sendEvent))
                return false;

            DestroyChild(canvas.transform, BackdropName);
            DestroyChild(canvas.transform, ChromeName);

            if (panel != null)
            {
                Image legacyPanel = panel.GetComponent<Image>();
                if (legacyPanel != null)
                    legacyPanel.color = Color.white;
            }

            RestoreBoard((RectTransform)groundMe, new Vector2(0.5f, 0.25381246f),
                profile.groundMePosition, new Vector2(1080f, 852.25f), new Vector2(0.504405f, 0.49330097f));
            RestoreBoard((RectTransform)groundOpp, new Vector2(0.499f, 0.7182709f),
                profile.groundOppPosition, new Vector2(1080f, 852.3f), new Vector2(0.5f, 0.5f));
            RestoreRect((RectTransform)sendEvent, new Vector2(0.5f, 0.5f), new Vector2(-44f, 6f),
                new Vector2(392f, 261.3f), new Vector2(0.5f, 0.5f));
            DestroyChild(sendEvent, "FigmaRollLabel");
            RemoveAddedShadow(sendEvent.gameObject);
            Image sendImage = sendEvent.GetComponent<Image>();
            if (sendImage != null)
            {
                sendImage.sprite = LoadSprite("Assets/Sprite/Ui page Game 1/Copilot_20260510_144912.png");
                sendImage.color = Color.white;
            }
            RestorePlayerPanel(FindDeep(canvas.transform, "Playe1"), new Vector2(-207f, -0.000015259f),
                new Vector2(357f, 382.7f));
            RestorePlayerPanel(FindDeep(canvas.transform, "Player2"), new Vector2(276.8f, 0.40009f),
                new Vector2(356.3f, 381.9f));

            EditorUtility.SetDirty(canvas.gameObject);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            return true;
        }

        private static void RestoreBoard(RectTransform board, Vector2 anchor, Vector2 position, Vector2 size,
            Vector2 pivot)
        {
            RestoreRect(board, anchor, position, size, pivot);
            Image image = board.GetComponent<Image>();
            if (image != null)
                image.color = Color.white;
            RemoveAddedShadow(board.gameObject);
        }

        private static void RestorePlayerPanel(Transform player, Vector2 position, Vector2 size)
        {
            if (player is RectTransform rect)
                RestoreRect(rect, new Vector2(0.5f, 0.5f), position, size, new Vector2(0.5f, 0.5f));
        }

        private static void RestoreRect(RectTransform rect, Vector2 anchor, Vector2 position, Vector2 size,
            Vector2 pivot)
        {
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = pivot;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            rect.localScale = Vector3.one;
        }

        private static void RemoveAddedShadow(GameObject target)
        {
            Shadow shadow = target.GetComponent<Shadow>();
            if (shadow != null)
                Object.DestroyImmediate(shadow);
        }

        private static bool Validate(LayoutProfile profile, Transform groundMe, Transform groundOpp,
            Transform sendEvent)
        {
            if (groundMe == null || groundOpp == null || sendEvent == null)
            {
                Debug.LogError("BattleSceneAuthoring: GroundMe, GroundOpp or Send Event is missing in " +
                               profile.scenePath);
                return false;
            }

            int mine = CountGameplayCells(groundMe, "ClickInCell");
            int opponent = CountGameplayCells(groundOpp, "TileDataOpp");
            if (mine != profile.expectedCells || opponent != profile.expectedCells)
            {
                Debug.LogError($"BattleSceneAuthoring: expected {profile.expectedCells} cells per side in " +
                               $"{profile.scenePath}, found mine={mine}, opponent={opponent}. Scene was not changed.");
                return false;
            }

            if (FindDeep(groundMe.parent, "DieImage") == null)
            {
                Debug.LogError("BattleSceneAuthoring: DieImage is missing. Scene was not changed.");
                return false;
            }

            return true;
        }

        private static int CountGameplayCells(Transform root, string componentName)
        {
            return root.GetComponentsInChildren<MonoBehaviour>(true)
                .Count(component => component != null && component.GetType().Name == componentName);
        }

        private static Sprite LoadSprite(string path)
        {
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite != null)
                return sprite;

            foreach (Object asset in AssetDatabase.LoadAllAssetsAtPath(path))
                if (asset is Sprite childSprite)
                    return childSprite;
            return null;
        }

        private static void DestroyChild(Transform parent, string name)
        {
            Transform child = parent.Find(name);
            if (child != null)
                Undo.DestroyObjectImmediate(child.gameObject);
        }

        private static Transform FindDeep(Transform parent, string exactName)
        {
            if (parent == null)
                return null;
            foreach (Transform child in parent)
            {
                if (child.name == exactName)
                    return child;
                Transform nested = FindDeep(child, exactName);
                if (nested != null)
                    return nested;
            }
            return null;
        }

    }
}
