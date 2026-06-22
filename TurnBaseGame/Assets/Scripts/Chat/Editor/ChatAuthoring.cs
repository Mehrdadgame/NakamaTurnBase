#if UNITY_EDITOR
using System.IO;
using Nakama.Helpers;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Nakama.Helpers.EditorTools
{
    /// <summary>
    /// Authors the in-game chat UI as real, editable GameObjects into the battle scenes, and bakes
    /// the procedural rounded/circle sprites as assets so the references persist. Re-runnable
    /// (idempotent — it deletes any previous ChatSystem/ChatRoot before rebuilding).
    /// Menu: Tools ▸ Chat.
    /// </summary>
    public static class ChatAuthoring
    {
        private const string ResDir = "Assets/Resources/Chat";

        // The actual scenes in EditorBuildSettings (verified buildIndex 3/4/5).
        // The "...1" variants exist on disk but are NOT in the build.
        private static readonly string[] BuildScenes =
        {
            "Assets/-Scenes/4-VerticalAndHorizontal.unity",
            "Assets/-Scenes/5-FourByThree.unity",
            "Assets/-Scenes/6-FourByFour.unity",
        };

        // Creates the message-bubble prefab at Resources/Chat/ChatBubble.prefab. Its text is an
        // RTLTextMeshPro with the Vazir font and Farsi shaping ON, so Persian renders correctly.
        // ChatManager.AddBubble loads + instantiates this prefab (no scene edits needed).
        [MenuItem("Tools/Chat/Create Bubble Prefab")]
        public static void CreateBubblePrefab()
        {
            EnsureFolder("Assets/Resources");
            EnsureFolder(ResDir);
            if (!File.Exists(ResDir + "/chat_rounded.png")) BakeSprites();

            var font = LoadPersianFont();
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(ResDir + "/chat_rounded.png");

            var bubble = new GameObject("ChatBubble", typeof(RectTransform));
            var img = bubble.AddComponent<Image>();
            img.sprite = sprite;
            img.type = Image.Type.Sliced;
            img.color = Color.white;

            var msg = new GameObject("Msg", typeof(RectTransform));
            msg.transform.SetParent(bubble.transform, false);
            var tmp = msg.AddComponent<RTLTMPro.RTLTextMeshPro>();
            if (font != null) tmp.font = font;
            tmp.fontSize = 30;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Right;
            tmp.Farsi = true;            // Persian (not Arabic) shaping
            tmp.ForceFix = true;
            var mrt = tmp.rectTransform;
            mrt.anchorMin = Vector2.zero; mrt.anchorMax = Vector2.one;
            mrt.offsetMin = new Vector2(24, 14); mrt.offsetMax = new Vector2(-24, -14);

            var path = ResDir + "/ChatBubble.prefab";
            PrefabUtility.SaveAsPrefabAsset(bubble, path);
            Object.DestroyImmediate(bubble);
            AssetDatabase.SaveAssets();
            Debug.Log("[ChatAuthoring] Bubble prefab created: " + path + " (Vazir + Farsi=" + (font != null) + ")");
        }

        [MenuItem("Tools/Chat/Bake UI Sprites")]
        public static void BakeSprites()
        {
            EnsureFolder("Assets/Resources");
            EnsureFolder(ResDir);
            BakeRounded(ResDir + "/chat_rounded.png");
            BakeCircle(ResDir + "/chat_circle.png");
            AssetDatabase.Refresh();
            ChatUiFactory.ClearCache();
            Debug.Log("[ChatAuthoring] Baked chat sprites into " + ResDir);
        }

        [MenuItem("Tools/Chat/Author Open Scene")]
        public static void AuthorOpenScene()
        {
            BakeSprites();
            if (AuthorActiveScene())
            {
                EditorSceneManager.SaveOpenScenes();
                Debug.Log("[ChatAuthoring] Authored the open scene.");
            }
        }

        [MenuItem("Tools/Chat/Author All 3 Build Scenes")]
        public static void AuthorAll()
        {
            BakeSprites();
            foreach (var path in BuildScenes)
            {
                if (!File.Exists(path)) { Debug.LogWarning("[ChatAuthoring] missing scene: " + path); continue; }
                EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                if (AuthorActiveScene())
                    EditorSceneManager.SaveOpenScenes();
            }
            Debug.Log("[ChatAuthoring] Authored all build scenes.");
        }

        private static bool AuthorActiveScene()
        {
            var canvasGo = GameObject.Find("Canvas");
            if (canvasGo == null) { Debug.LogWarning("[ChatAuthoring] No 'Canvas' in the active scene."); return false; }

            // Idempotent — drop any previously authored objects first.
            var oldSys = GameObject.Find("ChatSystem");
            if (oldSys != null) Object.DestroyImmediate(oldSys);
            var oldRoot = canvasGo.transform.Find("ChatRoot");
            if (oldRoot != null) Object.DestroyImmediate(oldRoot.gameObject);

            ChatUiFactory.Font = LoadPersianFont();

            var go = new GameObject("ChatSystem");
            SceneManager.MoveGameObjectToScene(go, SceneManager.GetActiveScene());
            var cm = go.AddComponent<ChatManager>();
            cm.BuildHierarchy(canvasGo.transform);

            EditorUtility.SetDirty(cm);
            EditorUtility.SetDirty(go);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            return true;
        }

        private static TMP_FontAsset LoadPersianFont()
        {
            var f = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Font/Vazir-Bold SDF.asset");
            if (f != null) return f;
            foreach (var guid in AssetDatabase.FindAssets("Vazir t:TMP_FontAsset"))
            {
                var asset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(guid));
                if (asset != null) return asset;
            }
            foreach (var guid in AssetDatabase.FindAssets("t:TMP_FontAsset"))
            {
                var asset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(guid));
                if (asset != null) return asset;
            }
            return null;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parent = Path.GetDirectoryName(path).Replace("\\", "/");
            var name = Path.GetFileName(path);
            if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, name);
        }

        private static void BakeRounded(string path)
        {
            int r = ChatUiFactory.RoundedRadius;
            int size = r * 2 + 6;
            var tex = ChatUiFactory.BuildRoundedTexturePublic(size, size, r);
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            var imp = (TextureImporter)AssetImporter.GetAtPath(path);
            ConfigureSprite(imp);
            imp.spriteBorder = new Vector4(r, r, r, r);
            imp.SaveAndReimport();
        }

        private static void BakeCircle(string path)
        {
            int size = 96;
            var tex = ChatUiFactory.BuildRoundedTexturePublic(size, size, size / 2);
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            var imp = (TextureImporter)AssetImporter.GetAtPath(path);
            ConfigureSprite(imp);
            imp.SaveAndReimport();
        }

        private static void ConfigureSprite(TextureImporter imp)
        {
            imp.textureType = TextureImporterType.Sprite;
            imp.spriteImportMode = SpriteImportMode.Single;
            imp.alphaIsTransparency = true;
            imp.mipmapEnabled = false;
            imp.wrapMode = TextureWrapMode.Clamp;
            imp.filterMode = FilterMode.Bilinear;
            imp.spritePixelsPerUnit = 100;
        }
    }
}
#endif
