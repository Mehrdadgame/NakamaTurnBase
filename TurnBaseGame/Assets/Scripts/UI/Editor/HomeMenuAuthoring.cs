using System.Linq;
using Nakama.Helpers;
using RTLTMPro;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace NinjaBattle.UI.Editor
{
    public static class HomeMenuAuthoring
    {
        private const string RootName = "HomeMenuUX";

        private static readonly Color Gold = new Color(0.92f, 0.63f, 0.18f, 1f);
        private static readonly Color Cream = new Color(1f, 0.95f, 0.76f, 1f);
        private static readonly Color DeepGreen = new Color(0.008f, 0.085f, 0.055f, 0.97f);
        private static readonly Color GreenCard = new Color(0.08f, 0.30f, 0.15f, 0.98f);
        private static readonly Color RedCard = new Color(0.34f, 0.055f, 0.045f, 0.99f);
        private static readonly Color PurpleCard = new Color(0.12f, 0.08f, 0.25f, 0.99f);

        [MenuItem("Tools/NinjaBattle/UI/Build Home UX")]
        public static void Build()
        {
            Canvas canvas = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .FirstOrDefault(candidate => candidate.gameObject.scene == UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            if (canvas == null)
            {
                Debug.LogError("HomeMenuAuthoring: active scene has no Canvas.");
                return;
            }

            RectTransform existing = canvas.transform.Find(RootName) as RectTransform;
            if (existing != null)
                Undo.DestroyObjectImmediate(existing.gameObject);

            ChatUiFactory.Font = FindPersianFont();
            RectTransform root = ChatUiFactory.Rect(RootName, canvas.transform);
            Undo.RegisterCreatedObjectUndo(root.gameObject, "Build Home UX");
            ChatUiFactory.Stretch(root);

            Transform cardUp = canvas.transform.Find("CardUp");
            if (cardUp != null)
                root.SetSiblingIndex(cardUp.GetSiblingIndex());

            BuildHeader(root);
            BuildBrand(root);
            BuildModeCards(root, canvas.transform);
            HideLegacyModeLayer(canvas.transform, root);
            HideProgressionHud(canvas.transform);

            EditorUtility.SetDirty(root.gameObject);
            EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            Selection.activeGameObject = root.gameObject;
            Debug.Log("Reference home UX built successfully.");
        }

        private static void BuildHeader(RectTransform root)
        {
            Image header = ChatUiFactory.Panel("ReferenceHeader", root, new Color(0.005f, 0.035f, 0.03f, 0.96f));
            Anchor(header.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f), new Vector2(0f, -118f), new Vector2(1000f, 210f));
            AddOutline(header.gameObject, new Color(Gold.r, Gold.g, Gold.b, 0.75f));

            Image avatar = ChatUiFactory.Panel("PlayerAvatar", header.transform, new Color(0.18f, 0.12f, 0.05f, 1f));
            Anchor(avatar.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(0f, 0.5f), new Vector2(34f, 0f), new Vector2(112f, 112f));
            AddOutline(avatar.gameObject, Gold);
            RTLTextMeshPro avatarText = CreateText("AvatarInitial", avatar.transform, "ش", 42, Cream, TextAlignmentOptions.Center);
            ChatUiFactory.Stretch(avatarText.rectTransform);

            RTLTextMeshPro player = CreateText("PlayerName", header.transform, "بازیکن", 24, Cream, TextAlignmentOptions.MidlineRight);
            Anchor(player.rectTransform, new Vector2(0f, 0.58f), new Vector2(0f, 0.92f),
                new Vector2(0f, 0.5f), new Vector2(172f, 0f), new Vector2(220f, 0f));
            RTLTextMeshPro level = CreateText("PlayerLevel", header.transform, "سطح ۱", 18, new Color(0.86f, 0.76f, 0.57f, 1f), TextAlignmentOptions.MidlineRight);
            Anchor(level.rectTransform, new Vector2(0f, 0.28f), new Vector2(0f, 0.56f),
                new Vector2(0f, 0.5f), new Vector2(172f, 0f), new Vector2(220f, 0f));

            Image xpTrack = ChatUiFactory.Panel("HeaderXpTrack", header.transform, new Color(0.03f, 0.18f, 0.06f, 1f));
            Anchor(xpTrack.rectTransform, new Vector2(0f, 0.18f), new Vector2(0f, 0.30f),
                new Vector2(0f, 0.5f), new Vector2(172f, 0f), new Vector2(220f, 0f));
            Image xpFill = ChatUiFactory.Panel("HeaderXpFill", xpTrack.transform, new Color(0.18f, 0.75f, 0.15f, 1f));
            Anchor(xpFill.rectTransform, Vector2.zero, new Vector2(0.62f, 1f), new Vector2(0f, 0.5f), Vector2.zero, Vector2.zero);

            Image coins = ChatUiFactory.Panel("CoinsPill", header.transform, new Color(0.07f, 0.08f, 0.055f, 1f));
            Anchor(coins.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), new Vector2(55f, 0f), new Vector2(255f, 84f));
            AddOutline(coins.gameObject, Gold);
            RTLTextMeshPro coinText = CreateText("CoinText", coins.transform, "۲۵٬۰۰۰", 28, Cream, TextAlignmentOptions.Center);
            ChatUiFactory.Stretch(coinText.rectTransform);

            Image charge = ChatUiFactory.Panel("ChargeButton", header.transform, new Color(0.07f, 0.34f, 0.17f, 1f));
            Anchor(charge.rectTransform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
                new Vector2(1f, 0.5f), new Vector2(-160f, 0f), new Vector2(190f, 80f));
            AddOutline(charge.gameObject, Gold);
            RTLTextMeshPro chargeText = CreateText("ChargeText", charge.transform, "شارژ", 25, Cream, TextAlignmentOptions.Center);
            ChatUiFactory.Stretch(chargeText.rectTransform);

            Image menu = ChatUiFactory.Panel("MenuButton", header.transform, new Color(0.08f, 0.09f, 0.07f, 1f));
            Anchor(menu.rectTransform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
                new Vector2(1f, 0.5f), new Vector2(-34f, 0f), new Vector2(92f, 92f));
            AddOutline(menu.gameObject, Gold);
            CreateMenuLine(menu.transform, 16f);
            CreateMenuLine(menu.transform, 0f);
            CreateMenuLine(menu.transform, -16f);
        }

        private static void BuildBrand(RectTransform root)
        {
            RTLTextMeshPro title = CreateText("ReferenceBrandTitle", root, "تاس زن", 54, Gold, TextAlignmentOptions.Center);
            Anchor(title.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), new Vector2(0f, 505f), new Vector2(520f, 80f));
            RTLTextMeshPro subtitle = CreateText("ReferenceBrandSubtitle", root, "بازی ایرانی", 22, Cream, TextAlignmentOptions.Center);
            Anchor(subtitle.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), new Vector2(0f, 445f), new Vector2(300f, 36f));
            RTLTextMeshPro section = CreateText("ReferenceSectionTitle", root, "شروع سریع", 30, Cream, TextAlignmentOptions.Center);
            Anchor(section.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), new Vector2(0f, 270f), new Vector2(420f, 50f));
            RTLTextMeshPro hint = CreateText("ReferenceSectionHint", root, "یک حالت را انتخاب کنید و وارد بازی شوید", 17,
                new Color(0.84f, 0.76f, 0.60f, 1f), TextAlignmentOptions.Center);
            Anchor(hint.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), new Vector2(0f, 225f), new Vector2(620f, 28f));
        }

        private static void BuildModeCards(RectTransform root, Transform canvas)
        {
            Transform quick = canvas.Find("CardUp/JoinMatchButton");
            Transform professional = canvas.Find("CardUp/JoinMatchButton (1)");
            Transform master = canvas.Find("CardUp/JoinMatchButton (2)");

            TryCreateModeCard(root, quick, "تاس سریع", "ورود: ۲۰ هزار", "+۱۰۰ نفر آنلاین", -315f, 550f, GreenCard, false);
            TryCreateModeCard(root, professional, "تاس حرفه‌ای", "ورود: ۵۰ هزار", "+۳۰۰ نفر آنلاین", 0f, 590f, RedCard, true);
            TryCreateModeCard(root, master, "استاد تاس", "ورود: ۱۰۰ هزار", "+۵۰ نفر آنلاین", 315f, 550f, PurpleCard, false);
        }

        private static void TryCreateModeCard(RectTransform root, Transform source, string title, string entry, string online,
            float x, float height, Color color, bool featured)
        {
            try
            {
                CreateModeCard(root, source, title, entry, online, x, height, color, featured);
            }
            catch (System.Exception exception)
            {
                Debug.LogError("HomeMenuAuthoring failed for " + title + ": " + exception);
            }
        }

        private static void CreateModeCard(RectTransform root, Transform source, string title, string entry, string online,
            float x, float height, Color color, bool featured)
        {
            float width = featured ? 300f : 270f;
            Image card = ChatUiFactory.Panel(title + "Card", root, color);
            Anchor(card.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), new Vector2(x, -190f), new Vector2(width, height));
            AddOutline(card.gameObject, featured ? new Color(1f, 0.72f, 0.20f, 1f) : new Color(Gold.r, Gold.g, Gold.b, 0.9f));

            CreateDie(card.transform, new Vector2(-42f, 62f), "••");
            CreateDie(card.transform, new Vector2(42f, 42f), "•••");

            RTLTextMeshPro cardTitle = CreateText("Title", card.transform, title, featured ? 29 : 26, Cream, TextAlignmentOptions.Center);
            Anchor(cardTitle.rectTransform, new Vector2(0.05f, 0.25f), new Vector2(0.95f, 0.39f),
                new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            RTLTextMeshPro cardEntry = CreateText("Entry", card.transform, entry, 18, Cream, TextAlignmentOptions.Center);
            Anchor(cardEntry.rectTransform, new Vector2(0.05f, 0.14f), new Vector2(0.95f, 0.24f),
                new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            RTLTextMeshPro cardOnline = CreateText("Online", card.transform, online, 16,
                new Color(1f, 0.82f, 0.42f, 1f), TextAlignmentOptions.Center);
            Anchor(cardOnline.rectTransform, new Vector2(0.05f, 0.06f), new Vector2(0.95f, 0.14f),
                new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);

            if (featured)
            {
                RTLTextMeshPro badge = CreateText("FeaturedBadge", card.transform, "محبوب", 16, Cream, TextAlignmentOptions.Center);
                Anchor(badge.rectTransform, new Vector2(0.68f, 0.88f), new Vector2(0.98f, 0.98f),
                    new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            }

            if (source != null)
            {
                RectTransform sourceRect = source as RectTransform;
                source.SetParent(root, false);
                Anchor(sourceRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f), new Vector2(x, -190f), new Vector2(width, height));
                Graphic sourceGraphic = source.GetComponent<Graphic>();
                if (sourceGraphic != null)
                {
                    Color sourceColor = sourceGraphic.color;
                    sourceColor.a = 0f;
                    sourceGraphic.color = sourceColor;
                    sourceGraphic.raycastTarget = true;
                }
                foreach (Graphic childGraphic in source.GetComponentsInChildren<Graphic>(true))
                {
                    if (childGraphic == sourceGraphic)
                        continue;
                    Color childColor = childGraphic.color;
                    childColor.a = 0f;
                    childGraphic.color = childColor;
                    childGraphic.raycastTarget = false;
                }
                source.SetAsLastSibling();
            }
        }

        private static void CreateDie(Transform parent, Vector2 position, string pips)
        {
            Image die = ChatUiFactory.Panel("Die", parent, new Color(0.96f, 0.78f, 0.43f, 1f));
            Anchor(die.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), position, new Vector2(96f, 96f));
            AddOutline(die.gameObject, new Color(0.35f, 0.18f, 0.04f, 1f));
            RTLTextMeshPro pipText = CreateText("Pips", die.transform, pips, 28,
                new Color(0.24f, 0.10f, 0.03f, 1f), TextAlignmentOptions.Center);
            ChatUiFactory.Stretch(pipText.rectTransform);
        }

        private static void CreateMenuLine(Transform parent, float y)
        {
            Image line = ChatUiFactory.Panel("MenuLine", parent, Cream, false);
            Anchor(line.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), new Vector2(0f, y), new Vector2(42f, 5f));
        }

        private static void HideLegacyModeLayer(Transform canvas, RectTransform root)
        {
            Transform legacy = canvas.Find("CardUp");
            if (legacy != null)
            {
                Transform logo = legacy.Find("Logo");
                if (logo != null)
                {
                    logo.SetParent(root, false);
                    Anchor(logo as RectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                        new Vector2(0.5f, 0.5f), new Vector2(0f, 500f), new Vector2(430f, 230f));
                }
                legacy.gameObject.SetActive(false);
            }
        }

        private static void HideProgressionHud(Transform canvas)
        {
            Transform hud = canvas.Find("MissionProgressionUI/ProgressionHUD");
            if (hud == null)
                return;

            foreach (Graphic graphic in hud.GetComponentsInChildren<Graphic>(true))
            {
                Color color = graphic.color;
                color.a = 0f;
                graphic.color = color;
            }
        }

        private static void AddOutline(GameObject target, Color color)
        {
            Outline outline = target.AddComponent<Outline>();
            outline.effectColor = color;
            outline.effectDistance = new Vector2(3f, 3f);
        }

        private static TMP_FontAsset FindPersianFont()
        {
            RTLTextMeshPro text = Object.FindObjectsByType<RTLTextMeshPro>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .FirstOrDefault(candidate => candidate.font != null);
            return text != null ? text.font : TMP_Settings.defaultFontAsset;
        }

        private static RTLTextMeshPro CreateText(string name, Transform parent, string text, int size,
            Color color, TextAlignmentOptions alignment)
        {
            RTLTextMeshPro label = ChatUiFactory.Text(name, parent, text, size, color, alignment);
            label.PreserveNumbers = true;
            label.raycastTarget = false;
            return label;
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
    }
}
