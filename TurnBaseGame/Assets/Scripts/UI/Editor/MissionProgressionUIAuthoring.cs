using System.Linq;
using Nakama.Helpers;
using NinjaBattle.Game;
using RTLTMPro;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace NinjaBattle.UI.Editor
{
    public static class MissionProgressionUIAuthoring
    {
        private const string AtlasPath = "Assets/Sprite/Home/Copilot_20260508_152525 (2).png";
        private const string JournalBackgroundPath = "Assets/Sprite/UI/mission-journal-bg-v2.png";
        private const string RootName = "MissionProgressionUI";
        private const string ToastPrefabPath = "Assets/Prefabs/UI/MissionCompletionToast.prefab";

        private static readonly Color Gold = new Color(0.78f, 0.52f, 0.16f, 1f);
        private static readonly Color BrightGold = new Color(1f, 0.82f, 0.34f, 1f);
        private static readonly Color DeepGreen = new Color(0.008f, 0.055f, 0.039f, 0.97f);
        private static readonly Color CardGreen = new Color(0.015f, 0.105f, 0.075f, 0.98f);
        private static readonly Color Cream = new Color(1f, 0.96f, 0.80f, 1f);

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

            RectTransform root = ChatUiFactory.Rect(RootName, canvas.transform);
            Undo.RegisterCreatedObjectUndo(root.gameObject, "Build Mission Progression UI");
            ChatUiFactory.Stretch(root);
            root.SetAsLastSibling();

            Sprite hudSprite = LoadAtlasSprite("Copilot_20260508_152525 (2)_20");
            Sprite missionIcon = LoadAtlasSprite("Copilot_20260508_152525 (2)_38");

            Image hud = ChatUiFactory.Panel("ProgressionHUD", root, new Color(0.018f, 0.14f, 0.095f, 0.97f));
            Outline hudOutline = hud.gameObject.AddComponent<Outline>();
            hudOutline.effectColor = new Color(0.62f, 0.40f, 0.10f, 0.78f);
            hudOutline.effectDistance = new Vector2(2f, 2f);
            Anchor(hud.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -600f), new Vector2(620f, 60f));

            Button openButton = CreateImageButton("OpenMissionsButton", hud.transform, missionIcon, Color.white);
            Anchor(openButton.GetComponent<RectTransform>(), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(0.5f, 0.5f), new Vector2(46f, 0f), new Vector2(52f, 52f));

            Image badgeBorder = ChatUiFactory.Panel("LevelBadge", hud.transform, Gold);
            badgeBorder.raycastTarget = false;
            Anchor(badgeBorder.rectTransform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
                new Vector2(0.5f, 0.5f), new Vector2(-44f, 0f), new Vector2(82f, 56f));

            Image levelBadge = ChatUiFactory.Panel("Inner", badgeBorder.transform, new Color(0.02f, 0.26f, 0.16f, 1f));
            levelBadge.raycastTarget = false;
            levelBadge.rectTransform.anchorMin = Vector2.zero;
            levelBadge.rectTransform.anchorMax = Vector2.one;
            levelBadge.rectTransform.offsetMin = new Vector2(4f, 4f);
            levelBadge.rectTransform.offsetMax = new Vector2(-4f, -4f);

            RTLTextMeshPro levelText = CreateText("LevelText", levelBadge.transform, "سطح ۱", 17, BrightGold,
                TextAlignmentOptions.Center);
            ChatUiFactory.Stretch(levelText.rectTransform);

            RTLTextMeshPro titleText = CreateText("TitleText", hud.transform, "توریست", 16, Cream,
                TextAlignmentOptions.Center);
            Anchor(titleText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f), new Vector2(0f, -5f), new Vector2(300f, 18f));

            Image xpTrack = ChatUiFactory.Panel("XpTrack", hud.transform, new Color(0f, 0.06f, 0.04f, 0.95f));
            xpTrack.raycastTarget = false;
            Anchor(xpTrack.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f), new Vector2(0f, 14f), new Vector2(360f, 10f));

            Image xpFill = ChatUiFactory.Panel("XpFill", xpTrack.transform, BrightGold);
            xpFill.type = Image.Type.Filled;
            xpFill.fillMethod = Image.FillMethod.Horizontal;
            xpFill.fillOrigin = 0;
            xpFill.fillAmount = 0.42f;
            ChatUiFactory.Stretch(xpFill.rectTransform);

            RTLTextMeshPro xpText = CreateText("XpText", hud.transform, "۴۲ از ۱۰۰ امتیاز", 13, Cream,
                TextAlignmentOptions.Center);
            Anchor(xpText.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f), new Vector2(0f, 25f), new Vector2(300f, 16f));

            Image dimmer = CreateImage("MissionPanel", root, null, new Color(0f, 0.025f, 0.018f, 0.88f));
            ChatUiFactory.Stretch(dimmer.rectTransform);

            Image panelBorder = CreateImage("JournalBackground", dimmer.transform,
                AssetDatabase.LoadAssetAtPath<Sprite>(JournalBackgroundPath), Color.white);
            panelBorder.type = Image.Type.Simple;
            panelBorder.preserveAspect = false;
            Anchor(panelBorder.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(930f, 1440f));

            Image panel = CreateImage("PanelTint", panelBorder.transform, null, new Color(0.004f, 0.03f, 0.022f, 0.30f));
            panel.raycastTarget = true;
            ChatUiFactory.Stretch(panel.rectTransform);

            Image header = CreateImage("Header", panel.transform, null, Color.clear);
            Anchor(header.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f), new Vector2(0f, -70f), new Vector2(700f, 92f));

            RTLTextMeshPro headerText = CreateText("HeaderText", header.transform, "ماموریت‌های روزانه", 38, BrightGold,
                TextAlignmentOptions.Center);
            ChatUiFactory.Stretch(headerText.rectTransform);

            Button closeButton = ChatUiFactory.RoundButton("CloseButton", panel.transform,
                new Color(0.50f, 0.06f, 0.04f, 1f), "×", Cream, 52, 76f);
            Anchor(closeButton.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(0.5f, 0.5f), new Vector2(-54f, -54f), new Vector2(64f, 64f));

            RTLTextMeshPro summaryText = CreateText("MissionSummary", panel.transform, "۰ از ۴ انجام شده", 27, Cream,
                TextAlignmentOptions.Center);
            Anchor(summaryText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f), new Vector2(0f, -165f), new Vector2(650f, 40f));

            ScrollRect scrollRect = CreateMissionScroll(panel.transform, out RectTransform content);
            MissionItemView template = CreateMissionTemplate(content);

            RTLTextMeshPro footer = CreateText("Footer", panel.transform,
                "با انجام مأموریت‌ها امتیاز بگیر و سطح خودت را بالا ببر.", 24, new Color(0.88f, 0.78f, 0.55f, 1f),
                TextAlignmentOptions.Center);
            Anchor(footer.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f), new Vector2(0f, 46f), new Vector2(790f, 50f));

            MissionsUI controller = root.gameObject.AddComponent<MissionsUI>();
            SerializedObject controllerObject = new SerializedObject(controller);
            SetObject(controllerObject, "missionPanel", dimmer.gameObject);
            SetObject(controllerObject, "openButton", openButton);
            SetObject(controllerObject, "closeButton", closeButton);
            SetObject(controllerObject, "missionContainer", content);
            SetObject(controllerObject, "missionItemTemplate", template);
            SetObject(controllerObject, "missionSummaryText", summaryText);
            SetObject(controllerObject, "levelText", levelText);
            SetObject(controllerObject, "titleText", titleText);
            SetObject(controllerObject, "xpText", xpText);
            SetObject(controllerObject, "xpFill", xpFill);
            controllerObject.ApplyModifiedPropertiesWithoutUndo();

            scrollRect.verticalNormalizedPosition = 1f;
            template.gameObject.SetActive(false);
            dimmer.gameObject.SetActive(false);

            EditorUtility.SetDirty(root.gameObject);
            EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            Selection.activeGameObject = root.gameObject;
            Debug.Log("Mission and progression UI built successfully.");
        }

        private static ScrollRect CreateMissionScroll(Transform parent, out RectTransform content)
        {
            Image viewportImage = ChatUiFactory.Panel("MissionViewport", parent, new Color(0f, 0f, 0f, 0.13f));
            Anchor(viewportImage.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f), new Vector2(0f, -245f), new Vector2(830f, 940f));
            viewportImage.gameObject.AddComponent<RectMask2D>();

            content = ChatUiFactory.Rect("MissionContent", viewportImage.transform);
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            content.anchoredPosition = Vector2.zero;
            content.sizeDelta = Vector2.zero;

            VerticalLayoutGroup layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(18, 18, 18, 18);
            layout.spacing = 18f;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            ContentSizeFitter fitter = content.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            ScrollRect scroll = viewportImage.gameObject.AddComponent<ScrollRect>();
            scroll.viewport = viewportImage.rectTransform;
            scroll.content = content;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Elastic;
            scroll.scrollSensitivity = 32f;
            return scroll;
        }

        private static MissionItemView CreateMissionTemplate(Transform parent)
        {
            Image outer = ChatUiFactory.Panel("MissionItemTemplate", parent, new Color(0.28f, 0.18f, 0.07f, 0.92f));
            LayoutElement layout = outer.gameObject.AddComponent<LayoutElement>();
            layout.preferredHeight = 190f;
            layout.minHeight = 190f;

            Image card = ChatUiFactory.Panel("CardBackground", outer.transform, CardGreen);
            card.rectTransform.anchorMin = Vector2.zero;
            card.rectTransform.anchorMax = Vector2.one;
            card.rectTransform.offsetMin = new Vector2(5f, 5f);
            card.rectTransform.offsetMax = new Vector2(-5f, -5f);

            RTLTextMeshPro title = CreateText("Title", card.transform, "بردن یک مسابقه", 31, BrightGold,
                TextAlignmentOptions.MidlineRight);
            Anchor(title.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(1f, 1f), new Vector2(-35f, -30f), new Vector2(-240f, 48f));

            RTLTextMeshPro description = CreateText("Description", card.transform,
                "یک مسابقه را با پیروزی تمام کن.", 23, Cream, TextAlignmentOptions.MidlineRight);
            Anchor(description.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(1f, 1f), new Vector2(-35f, -88f), new Vector2(-240f, 70f));

            Image rewardBadge = ChatUiFactory.Panel("RewardBadge", card.transform, new Color(0.22f, 0.16f, 0.05f, 1f));
            Anchor(rewardBadge.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(0f, 1f), new Vector2(24f, -26f), new Vector2(180f, 62f));

            RTLTextMeshPro reward = CreateText("Reward", rewardBadge.transform, "۵۰ XP", 26, BrightGold,
                TextAlignmentOptions.Center);
            ChatUiFactory.Stretch(reward.rectTransform);

            RTLTextMeshPro completed = CreateText("Completed", card.transform, "✓", 45,
                new Color(0.45f, 1f, 0.48f, 1f), TextAlignmentOptions.Center);
            Anchor(completed.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(0.5f, 0.5f), new Vector2(115f, -4f), new Vector2(70f, 70f));

            Image progressTrack = ChatUiFactory.Panel("ProgressTrack", card.transform, new Color(0f, 0.055f, 0.035f, 1f));
            Anchor(progressTrack.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f),
                new Vector2(0.5f, 0f), new Vector2(0f, 22f), new Vector2(-64f, 18f));

            Image progressFill = ChatUiFactory.Panel("ProgressFill", progressTrack.transform, BrightGold);
            progressFill.type = Image.Type.Filled;
            progressFill.fillMethod = Image.FillMethod.Horizontal;
            progressFill.fillOrigin = 0;
            progressFill.fillClockwise = true;
            progressFill.fillAmount = 0.5f;
            progressFill.raycastTarget = false;
            ChatUiFactory.Stretch(progressFill.rectTransform);

            RTLTextMeshPro progress = CreateText("Progress", card.transform, "۱ / ۲", 22, Cream,
                TextAlignmentOptions.Center);
            Anchor(progress.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f), new Vector2(0f, 56f), new Vector2(300f, 32f));

            MissionItemView view = outer.gameObject.AddComponent<MissionItemView>();
            SerializedObject viewObject = new SerializedObject(view);
            SetObject(viewObject, "titleText", title);
            SetObject(viewObject, "descriptionText", description);
            SetObject(viewObject, "progressText", progress);
            SetObject(viewObject, "rewardText", reward);
            SetObject(viewObject, "completedText", completed);
            SetObject(viewObject, "progressFill", progressFill);
            SetObject(viewObject, "cardBackground", card);
            viewObject.ApplyModifiedPropertiesWithoutUndo();
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

        private static Image CreateImage(string name, Transform parent, Sprite sprite, Color color)
        {
            RectTransform rect = ChatUiFactory.Rect(name, parent);
            Image image = rect.gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.color = color;
            image.preserveAspect = sprite != null;
            return image;
        }

        private static Button CreateImageButton(string name, Transform parent, Sprite sprite, Color color)
        {
            Image image = CreateImage(name, parent, sprite, color);
            Button button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            return button;
        }

        private static RTLTextMeshPro CreateText(string name, Transform parent, string text, int size,
            Color color, TextAlignmentOptions alignment)
        {
            RTLTextMeshPro label = ChatUiFactory.Text(name, parent, text, size, color, alignment);
            label.PreserveNumbers = true;
            label.raycastTarget = false;
            return label;
        }

        private static Sprite LoadAtlasSprite(string spriteName)
        {
            return AssetDatabase.LoadAllAssetsAtPath(AtlasPath)
                .OfType<Sprite>()
                .FirstOrDefault(sprite => sprite.name == spriteName);
        }

        private static void Anchor(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 pivot, Vector2 position, Vector2 size)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        private static void SetObject(SerializedObject serializedObject, string propertyName, Object value)
        {
            serializedObject.FindProperty(propertyName).objectReferenceValue = value;
        }
    }
}
