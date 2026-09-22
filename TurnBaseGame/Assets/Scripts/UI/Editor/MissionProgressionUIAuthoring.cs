using System.IO;
using System.Linq;
using Nakama.Helpers;
using NinjaBattle.Game;
using RTLTMPro;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace NinjaBattle.UI.Editor
{
    public static class MissionProgressionUIAuthoring
    {
        private const string RootName = "MissionProgressionUI";
        private const string ToastPrefabPath = "Assets/Prefabs/UI/MissionCompletionToast.prefab";
        private const float DesignWidth = 1080f;
        private const float DesignHeight = 2400f;

        private static readonly Color Gold = new Color32(236, 174, 58, 255);
        private static readonly Color BrightGold = new Color32(255, 221, 120, 255);
        private static readonly Color DeepWood = new Color32(46, 26, 8, 245);
        private static readonly Color Cream = new Color32(255, 221, 162, 255);
        private static readonly Color GreenFill = new Color32(95, 203, 76, 255);

        [MenuItem("Tools/NinjaBattle/UI/Build Mission Progression UI")]
        public static void Build()
        {
            Canvas canvas = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .FirstOrDefault(candidate => candidate.gameObject.scene == UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            if (canvas == null)
            {
                Debug.LogError("MissionProgressionUIAuthoring: active scene has no Canvas.");
                return;
            }

            RectTransform existing = canvas.transform.Find(RootName) as RectTransform;
            if (existing != null)
                Undo.DestroyObjectImmediate(existing.gameObject);

            EnsureRuntimeManagers();
            ChatUiFactory.Font = FindPersianFont();

            FigmaHomeController controller = Object.FindObjectsByType<FigmaHomeController>(
                    FindObjectsInactive.Include, FindObjectsSortMode.None)
                .FirstOrDefault(candidate => candidate.gameObject.scene == UnityEngine.SceneManagement.SceneManager.GetActiveScene());

            RectTransform root = ChatUiFactory.Rect(RootName, canvas.transform);
            Undo.RegisterCreatedObjectUndo(root.gameObject, "Build Mission Progression UI");
            ChatUiFactory.Stretch(root);

            Transform leaderboardPanel = canvas.transform.Find("Panel Leaderboard");
            if (leaderboardPanel != null)
                root.SetSiblingIndex(leaderboardPanel.GetSiblingIndex());

            // 1. Full Screen Background
            RectTransform bg = CreateSpriteNode("Background", root, LoadSprite("background") ?? LoadSprite("BG"), false);
            SetFigmaRect(bg, 0, 0, DesignWidth, DesignHeight);

            // 2. Top Bar
            BuildTopBar(root, controller);

            // 3. Header Ribbon ("ایونت ها")
            RectTransform headerRibbon = CreateSpriteNode("HeaderRibbon", root, LoadSprite("missions_header"), false);
            SetFigmaRect(headerRibbon, 217, 243, 646, 274);

            // 4. Progression & Level Strip
            Image progStrip = ChatUiFactory.Panel("ProgressionStrip", root, DeepWood);
            SetFigmaRect(progStrip.rectTransform, 180, 520, 720, 68);
            Outline stripOutline = progStrip.gameObject.AddComponent<Outline>();
            stripOutline.effectColor = Gold;
            stripOutline.effectDistance = new Vector2(0, -3);

            RTLTextMeshPro levelText = CreateText("LevelText", progStrip.transform, "سطح ۱", 28, BrightGold,
                TextAlignmentOptions.Center);
            SetTopLeft(levelText.rectTransform, 570, 10, 140, 48);

            RTLTextMeshPro titleText = CreateText("TitleText", progStrip.transform, "توریست", 24, Cream,
                TextAlignmentOptions.Center);
            SetTopLeft(titleText.rectTransform, 430, 12, 130, 44);

            Image xpTrack = ChatUiFactory.Panel("XpTrack", progStrip.transform, new Color32(20, 12, 4, 255));
            SetTopLeft(xpTrack.rectTransform, 30, 20, 380, 28);
            xpTrack.gameObject.AddComponent<Outline>().effectColor = new Color32(95, 60, 20, 255);

            Image xpFill = ChatUiFactory.Panel("XpFill", xpTrack.transform, Gold);
            xpFill.type = Image.Type.Filled;
            xpFill.fillMethod = Image.FillMethod.Horizontal;
            xpFill.fillOrigin = 0;
            xpFill.fillAmount = 0.42f;
            ChatUiFactory.Stretch(xpFill.rectTransform);

            RTLTextMeshPro xpText = CreateText("XpText", xpTrack.transform, "۴۲ از ۱۰۰ امتیاز", 20, Color.white,
                TextAlignmentOptions.Center);
            ChatUiFactory.Stretch(xpText.rectTransform);

            // 5. Scrollable Mission Cards Viewport
            Image viewport = ChatUiFactory.Panel("MissionsViewport", root, Color.clear);
            SetFigmaRect(viewport.rectTransform, 150, 605, 780, 1490);
            viewport.gameObject.AddComponent<RectMask2D>();

            RectTransform content = ChatUiFactory.Rect("MissionsContent", viewport.transform);
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            content.anchoredPosition = Vector2.zero;
            content.sizeDelta = Vector2.zero;

            VerticalLayoutGroup layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(10, 10, 15, 20);
            layout.spacing = 28f;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            ContentSizeFitter fitter = content.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            ScrollRect scroll = viewport.gameObject.AddComponent<ScrollRect>();
            scroll.viewport = viewport.rectTransform;
            scroll.content = content;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Elastic;
            scroll.elasticity = 0.08f;
            scroll.scrollSensitivity = 40f;

            // 6. Mission Card Template
            MissionItemView template = CreateMissionCardTemplate(content);
            template.gameObject.SetActive(false);

            // 7. Footer Navigation (Tab 3: Events active)
            if (controller != null)
            {
                HomeMenuAuthoring.BuildSharedFooter(root, controller, 3);
            }

            // 8. MissionsUI Manager Wiring
            MissionsUI missionsUi = root.gameObject.AddComponent<MissionsUI>();
            SerializedObject serialized = new SerializedObject(missionsUi);
            SetObject(serialized, "missionPanel", root.gameObject);
            SetObject(serialized, "missionContainer", content);
            SetObject(serialized, "missionItemTemplate", template);
            SetObject(serialized, "levelText", levelText);
            SetObject(serialized, "titleText", titleText);
            SetObject(serialized, "xpText", xpText);
            SetObject(serialized, "xpFill", xpFill);
            serialized.ApplyModifiedPropertiesWithoutUndo();

            if (controller != null)
                controller.SetMissionsPanel(root.gameObject);

            root.gameObject.SetActive(false);
            EditorUtility.SetDirty(root.gameObject);
            EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            Debug.Log("MissionProgressionUI built successfully as full tab matching Figma 336:676.");
        }

        private static void BuildTopBar(RectTransform root, FigmaHomeController controller)
        {
            RectTransform topBar = ChatUiFactory.Rect("TopBar", root);
            SetFigmaRect(topBar, 74, 117, 932, 110);

            // Settings
            RectTransform settings = CreateRoundedButton("SettingsItem", topBar, Cream);
            SetTopLeft(settings, 0, 4, 103, 103);
            AddPanelDepth(settings.gameObject);
            RectTransform settingsIcon = CreateSpriteNode("Icon", settings, LoadSprite("top_settings_icon") ?? LoadSprite("top_settings"), false);
            SetTopLeft(settingsIcon, 23, 23, 56, 56);
            if (controller != null)
                UnityEventTools.AddPersistentListener(settings.GetComponent<Button>().onClick, controller.OpenProfile);

            // Sound
            RectTransform sound = CreateRoundedButton("SoundItem", topBar, Cream);
            SetTopLeft(sound, 135, 4, 103, 103);
            AddPanelDepth(sound.gameObject);
            RectTransform soundIcon = CreateSpriteNode("Icon", sound, LoadSprite("top_sound_icon"), false);
            SetTopLeft(soundIcon, 20, 20, 64, 64);
            if (controller != null)
                UnityEventTools.AddPersistentListener(sound.GetComponent<Button>().onClick, controller.ToggleAudio);

            // Coin Store Item
            RectTransform coin = CreateRoundedButton("CoinStoreItem", topBar, Cream);
            SetTopLeft(coin, 530, 4, 260, 103);
            AddPanelDepth(coin.gameObject);
            RTLTextMeshPro coinText = CreateText("ValueText", coin.transform, "۱۰۰", 65, new Color32(72, 46, 8, 255), TextAlignmentOptions.Center);
            SetTopLeft(coinText.rectTransform, 25, -6, 110, 108);
            RectTransform coinIcon = CreateSpriteNode("Icon", coin, LoadSprite("top_coin"), false);
            SetTopLeft(coinIcon, 142, -10, 118, 118);
            if (controller != null)
                UnityEventTools.AddPersistentListener(coin.GetComponent<Button>().onClick, controller.OpenShop);

            // Ornate Avatar button
            RectTransform avatarBtn = CreateTransparentButton("AvatarItem", topBar);
            SetTopLeft(avatarBtn, 827, 9, 97, 97);

            Image avatarThumb = ChatUiFactory.Panel("AvatarThumb", avatarBtn, new Color32(230, 195, 140, 255));
            SetTopLeft(avatarThumb.rectTransform, 8, 8, 81, 81);
            avatarThumb.preserveAspect = true;
            avatarThumb.raycastTarget = false;

            RectTransform avatarFrame = CreateSpriteNode("AvatarFrame", avatarBtn, LoadSprite("top_avatar_frame"), false);
            SetTopLeft(avatarFrame, 0, 0, 97, 97);
            avatarFrame.GetComponent<Image>().raycastTarget = false;

            if (controller != null)
                UnityEventTools.AddPersistentListener(avatarBtn.GetComponent<Button>().onClick, controller.OpenProfile);
        }

        private static MissionItemView CreateMissionCardTemplate(Transform parent)
        {
            RectTransform cardRoot = ChatUiFactory.Rect("MissionCardTemplate", parent);
            LayoutElement layout = cardRoot.gameObject.AddComponent<LayoutElement>();
            layout.preferredHeight = 335f;
            layout.minHeight = 335f;
            layout.preferredWidth = 655f;

            // Card background frame (Rectangle 31)
            Image cardBg = CreateImage("CardBg", cardRoot, LoadSprite("mission_card_bg") ?? LoadSprite("Rectangle 31"), Color.white);
            SetTopLeft(cardBg.rectTransform, 0, 8, 655, 327);

            // Title Strip Banner (Rectangle 46)
            Image titleStrip = CreateImage("TitleStrip", cardRoot, LoadSprite("mission_title_strip"), Color.white);
            SetTopLeft(titleStrip.rectTransform, 0, 0, 655, 99);

            // Mission Title on banner
            RTLTextMeshPro title = CreateText("MissionTitle", titleStrip.transform, "۵ تا از حریف‌هات رو شکست بده!", 32, BrightGold,
                TextAlignmentOptions.Center);
            title.fontStyle = FontStyles.Bold;
            ChatUiFactory.Stretch(title.rectTransform);

            // Trophy Badge (image 12002804)
            Image trophy = CreateImage("TrophyBadge", cardRoot, LoadSprite("mission_trophy_badge") ?? LoadSprite("image 12002804"), Color.white);
            SetTopLeft(trophy.rectTransform, 20, 107, 123, 123);

            // Mission Description
            RTLTextMeshPro desc = CreateText("Description", cardRoot, "با شکست حریفان جوایز ویژه بگیر!", 22, Cream,
                TextAlignmentOptions.MidlineRight);
            SetTopLeft(desc.rectTransform, 155, 110, 480, 40);

            // Reward badge
            RTLTextMeshPro reward = CreateText("RewardText", cardRoot, "+۵۰ XP", 24, BrightGold,
                TextAlignmentOptions.MidlineRight);
            SetTopLeft(reward.rectTransform, 155, 150, 180, 36);

            // Progress Track (Rectangle 45)
            Image track = ChatUiFactory.Panel("ProgressTrack", cardRoot, new Color32(24, 14, 5, 255));
            SetTopLeft(track.rectTransform, 155, 190, 470, 34);
            track.gameObject.AddComponent<Outline>().effectColor = new Color32(110, 75, 30, 255);

            // Progress Fill (Rectangle 47)
            Image fill = ChatUiFactory.Panel("ProgressFill", track.transform, GreenFill);
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillOrigin = 0;
            fill.fillAmount = 0.40f;
            ChatUiFactory.Stretch(fill.rectTransform);

            // Progress Text (e.g. ۲ / ۵)
            RTLTextMeshPro progress = CreateText("ProgressText", track.transform, "۲ / ۵", 22, Color.white,
                TextAlignmentOptions.Center);
            progress.fontStyle = FontStyles.Bold;
            ChatUiFactory.Stretch(progress.rectTransform);

            // Action / Status Button (Group 33 / mission_status_btn)
            RectTransform actionBtnRect = ChatUiFactory.Rect("ActionButton", cardRoot);
            SetTopLeft(actionBtnRect, 78, 240, 498, 67);
            Image btnImage = actionBtnRect.gameObject.AddComponent<Image>();
            btnImage.sprite = LoadSprite("mission_status_btn");
            btnImage.type = Image.Type.Simple;
            Button actionBtn = actionBtnRect.gameObject.AddComponent<Button>();
            actionBtn.targetGraphic = btnImage;
            actionBtn.transition = Selectable.Transition.ColorTint;

            RTLTextMeshPro actionText = CreateText("ButtonText", actionBtnRect.transform, "شروع بازی", 32, Color.white,
                TextAlignmentOptions.Center);
            actionText.fontStyle = FontStyles.Bold;
            ChatUiFactory.Stretch(actionText.rectTransform);

            MissionItemView view = cardRoot.gameObject.AddComponent<MissionItemView>();
            view.Configure(title, desc, progress, reward, null, fill, cardBg, actionBtn, actionText);
            return view;
        }

        private static void EnsureRuntimeManagers()
        {
            MissionManager missionManager = Object.FindAnyObjectByType<MissionManager>(FindObjectsInactive.Include);
            if (missionManager == null)
            {
                GameObject managerObject = new GameObject("MissionManager", typeof(MissionManager));
                Undo.RegisterCreatedObjectUndo(managerObject, "Create MissionManager");
                missionManager = managerObject.GetComponent<MissionManager>();
            }

            MissionCompletionToast toastPrefab = AssetDatabase.LoadAssetAtPath<MissionCompletionToast>(ToastPrefabPath);
            if (missionManager != null && toastPrefab != null)
            {
                SerializedObject managerObject = new SerializedObject(missionManager);
                managerObject.FindProperty("missionCompletionToastPrefab").objectReferenceValue = toastPrefab;
                managerObject.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(missionManager);
            }

            if (Object.FindAnyObjectByType<PlayerProgressionManager>(FindObjectsInactive.Include) == null)
            {
                GameObject progressionManager = new GameObject("PlayerProgressionManager", typeof(PlayerProgressionManager));
                Undo.RegisterCreatedObjectUndo(progressionManager, "Create PlayerProgressionManager");
            }
        }

        private static TMP_FontAsset FindPersianFont()
        {
            RTLTextMeshPro text = Object.FindObjectsByType<RTLTextMeshPro>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .FirstOrDefault(candidate => candidate.font != null);
            return text != null ? text.font : TMP_Settings.defaultFontAsset;
        }

        private static Sprite LoadSprite(string name)
        {
            string path = "Assets/Figma/Home/Parts/" + name + ".png";
            if (!File.Exists(path))
                path = "Assets/Figma/Parts/" + name + ".png";
            if (!File.Exists(path))
                path = "Assets/Sprite/misson/" + name + ".png";

            if (File.Exists(path))
            {
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer != null && (importer.textureType != TextureImporterType.Sprite || importer.maxTextureSize < 4096))
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spriteImportMode = SpriteImportMode.Single;
                    importer.alphaIsTransparency = true;
                    importer.mipmapEnabled = false;
                    importer.maxTextureSize = 4096;
                    importer.textureCompression = TextureImporterCompression.CompressedHQ;
                    importer.SaveAndReimport();
                }
                return AssetDatabase.LoadAssetAtPath<Sprite>(path);
            }

            return null;
        }

        private static RectTransform CreateSpriteNode(string name, Transform parent, Sprite sprite, bool button)
        {
            RectTransform rect = ChatUiFactory.Rect(name, parent);
            Image image = rect.gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = false;
            image.raycastTarget = button;
            if (button)
            {
                Button uiButton = rect.gameObject.AddComponent<Button>();
                uiButton.targetGraphic = image;
                uiButton.transition = Selectable.Transition.ColorTint;
            }
            return rect;
        }

        private static Image CreateImage(string name, Transform parent, Sprite sprite, Color color)
        {
            RectTransform rect = ChatUiFactory.Rect(name, parent);
            Image image = rect.gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.color = color;
            image.preserveAspect = false;
            image.raycastTarget = false;
            return image;
        }

        private static RectTransform CreateRoundedButton(string name, Transform parent, Color color)
        {
            Image image = ChatUiFactory.Panel(name, parent, color);
            Button button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            return image.rectTransform;
        }

        private static RectTransform CreateTransparentButton(string name, Transform parent)
        {
            RectTransform rect = ChatUiFactory.Rect(name, parent);
            Image image = rect.gameObject.AddComponent<Image>();
            image.color = Color.clear;
            Button button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.transition = Selectable.Transition.None;
            return rect;
        }

        private static RTLTextMeshPro CreateText(string name, Transform parent, string text, int size,
            Color color, TextAlignmentOptions alignment)
        {
            RTLTextMeshPro label = ChatUiFactory.Text(name, parent, text, size, color, alignment);
            label.PreserveNumbers = true;
            label.raycastTarget = false;
            return label;
        }

        private static void AddPanelDepth(GameObject target)
        {
            Shadow shadow = target.AddComponent<Shadow>();
            shadow.effectColor = new Color32(197, 148, 93, 220);
            shadow.effectDistance = new Vector2(0, -5);
        }

        private static void SetFigmaRect(RectTransform rect, float x, float y, float width, float height)
        {
            rect.anchorMin = new Vector2(x / DesignWidth, 1f - (y + height) / DesignHeight);
            rect.anchorMax = new Vector2((x + width) / DesignWidth, 1f - y / DesignHeight);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.pivot = new Vector2(0.5f, 0.5f);
        }

        private static void SetTopLeft(RectTransform rect, float x, float y, float width, float height)
        {
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(x, -y);
            rect.sizeDelta = new Vector2(width, height);
        }

        private static void SetObject(SerializedObject serializedObject, string propertyName, Object value)
        {
            SerializedProperty prop = serializedObject.FindProperty(propertyName);
            if (prop != null)
                prop.objectReferenceValue = value;
        }
    }
}
