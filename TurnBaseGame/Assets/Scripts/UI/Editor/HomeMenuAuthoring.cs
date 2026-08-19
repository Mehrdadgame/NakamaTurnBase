using System.Linq;
using Game;
using Nakama.Helpers;
using NinjaBattle.UI;
using RTLTMPro;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace NinjaBattle.UI.Editor
{
    public static class HomeMenuAuthoring
    {
        private const string RootName = "HomeMenuUX";
        private const string PartsPath = "Assets/Resources/Home/Parts/";
        private const string AtlasItemsPath = "Assets/Art/FigmaItemAtlas/";
        private const float DesignWidth = 1080f;
        private const float DesignHeight = 2400f;

        private static readonly Color Cream = new Color32(255, 221, 162, 255);
        private static readonly Color Brown = new Color32(72, 46, 8, 255);
        private static readonly Color ActiveCream = new Color32(245, 203, 122, 255);

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

            FigmaItemAtlasBuilder.Build();

            RectTransform existing = canvas.transform.Find(RootName) as RectTransform;
            if (existing != null)
                Undo.DestroyObjectImmediate(existing.gameObject);

            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(DesignWidth, DesignHeight);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0.5f;
                EditorUtility.SetDirty(scaler);
            }

            ChatUiFactory.Font = FindPersianFont();
            RectTransform root = ChatUiFactory.Rect(RootName, canvas.transform);
            Undo.RegisterCreatedObjectUndo(root.gameObject, "Build componentized Figma Home UX");
            ChatUiFactory.Stretch(root);

            Transform leaderboardPanel = canvas.transform.Find("Panel Leaderboard");
            root.SetSiblingIndex(leaderboardPanel != null ? leaderboardPanel.GetSiblingIndex() : canvas.transform.childCount - 1);

            Button quick = FindDeep(canvas.transform, "JoinMatchButton (2)")?.GetComponent<Button>();
            Button professional = FindDeep(canvas.transform, "JoinMatchButton (1)")?.GetComponent<Button>();
            Button master = FindDeep(canvas.transform, "JoinMatchButton")?.GetComponent<Button>();
            Button leaderboard = FindDeep(canvas.transform, "Button Leaderboard")?.GetComponent<Button>();
            Button chest = FindDeep(canvas.transform, "chest button")?.GetComponent<Button>();
            GameObject shop = canvas.transform.Find("Panel  shop")?.gameObject;
            GameObject profile = canvas.transform.Find("profile")?.gameObject;
            GameObject leaderboardView = leaderboardPanel != null ? leaderboardPanel.gameObject : null;

            FigmaHomeController controller = root.gameObject.AddComponent<FigmaHomeController>();
            controller.Configure(quick, professional, master, leaderboard, chest, shop, profile, leaderboardView);
            controller.SetMissionsPanel(canvas.transform.Find("MissionProgressionUI")?.gameObject);

            BuildBackground(root);
            BuildPromo(root, controller);
            BuildStartButton(root, controller);
            BuildChestRow(root, controller);
            BuildTopBar(root, controller);
            BuildFooter(root, controller);
            BuildModePopup(root, controller);
            BuildLeaderboardScreen(leaderboardPanel, controller);
            BuildShopScreen(shop != null ? shop.transform : null, controller);
            HomeProfileShopAuthoring.Build(canvas.transform, profile != null ? profile.transform : null,
                shop != null ? shop.transform : null, controller);

            EditorUtility.SetDirty(root.gameObject);
            EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            Selection.activeGameObject = root.gameObject;
            Debug.Log("Componentized Figma home UX built successfully from node 99:330.");
        }

        public static void BuildSceneFromCommandLine()
        {
            var scene = EditorSceneManager.OpenScene("Assets/-Scenes/2-Home.unity", OpenSceneMode.Single);
            Build();
            EditorSceneManager.SaveScene(scene);
        }




        private static void BuildBackground(RectTransform root)
        {
            RectTransform background = CreateSpriteNode("Background", root, LoadSprite("background"), false);
            SetFigmaRect(background, 0, 0, DesignWidth, DesignHeight);
        }

        private static void BuildPromo(RectTransform root, FigmaHomeController controller)
        {
            RectTransform promo = CreateSpriteNode("PromoBanner", root, LoadSprite("promo_banner"), true);
            SetFigmaRect(promo, 147, 348, 786, 263);
            UnityEventTools.AddPersistentListener(promo.GetComponent<Button>().onClick, controller.OpenShop);
        }

        private static void BuildStartButton(RectTransform root, FigmaHomeController controller)
        {
            RectTransform start = CreateSpriteNode("StartCTA", root, LoadSprite("start_cta"), true);
            SetFigmaRect(start, 256, 1409, 521, 193);
            start.GetComponent<Button>().transition = Selectable.Transition.None;
            UnityEventTools.AddPersistentListener(start.GetComponent<Button>().onClick, controller.OpenModePopup);
        }

        private static void BuildChestRow(RectTransform root, FigmaHomeController controller)
        {
            RectTransform row = ChatUiFactory.Rect("ChestRow", root);
            SetFigmaRect(row, 104, 1750, 866, 341);

            CreateChestItem(row, "LockedChest12h", 0, "chest_raw_8", "۱۲ ساعت", false, controller);
            CreateChestItem(row, "LockedChest8h", 265, "chest_raw_8", "۸ ساعت", false, controller);
            CreateChestItem(row, "ReadyChest", 530, "chest_raw_4", "بازکن!", true, controller);
        }

        private static void CreateChestItem(RectTransform row, string name, float x, string iconName, string status,
            bool ready, FigmaHomeController controller)
        {
            RectTransform item = CreateTransparentButton(name, row);
            SetTopLeft(item, x, 0, ready ? 336 : 265, 341);
            item.GetComponent<Button>().interactable = ready;

            RectTransform icon = CreateSpriteNode("Icon", item, LoadSprite(iconName), false);
            if (ready)
                icon.GetComponent<Image>().sprite = LoadAtlasSprite("figma_item_20");
            SetTopLeft(icon, 55, 0, 220, 220);

            Image pill = ChatUiFactory.Panel("StatusPill", item,
                ready ? new Color32(135, 188, 45, 255) : new Color32(255, 227, 174, 255));
            SetTopLeft(pill.rectTransform, 80, 273, 168, 68);
            pill.raycastTarget = false;
            Shadow pillShadow = pill.gameObject.AddComponent<Shadow>();
            pillShadow.effectColor = ready ? new Color32(72, 115, 20, 190) : new Color32(184, 145, 91, 170);
            pillShadow.effectDistance = new Vector2(0, -3);

            RTLTextMeshPro label = CreateLabel("StatusText", pill.transform, status, ready ? 40 : 35,
                TextAlignmentOptions.Center);
            label.color = ready ? Color.white : new Color(0.37f, 0.24f, 0.11f, 0.72f);
            ChatUiFactory.Stretch(label.rectTransform);

            if (ready)
                UnityEventTools.AddPersistentListener(item.GetComponent<Button>().onClick, controller.OpenChest);
        }

        private static void BuildTopBar(RectTransform root, FigmaHomeController controller)
        {
            RectTransform topBar = ChatUiFactory.Rect("TopBar", root);
            SetFigmaRect(topBar, 74, 117, 932, 110);

            RectTransform settings = CreateRoundedButton("SettingsItem", topBar, Cream);
            SetTopLeft(settings, 0, 4, 103, 103);
            AddPanelDepth(settings.gameObject);
            RectTransform settingsIcon = CreateSpriteNode("Icon", settings, LoadSprite("top_settings"), false);
            SetTopLeft(settingsIcon, 23, 22, 56, 56);
            UnityEventTools.AddPersistentListener(settings.GetComponent<Button>().onClick, controller.OpenProfile);

            RectTransform gem = CreateRoundedButton("GemStoreItem", topBar, Cream);
            SetTopLeft(gem, 422, 6, 238, 103);
            AddPanelDepth(gem.gameObject);
            RTLTextMeshPro gemText = CreateLabel("ValueText", gem, "۱۰۰", 65, TextAlignmentOptions.Center);
            SetTopLeft(gemText.rectTransform, 38, -5, 82, 108);
            CreateCroppedIcon("Icon", gem, LoadAtlasSprite("figma_item_19"), 127, 8, 107, 80, 5, -5, 96, 96);
            UnityEventTools.AddPersistentListener(gem.GetComponent<Button>().onClick, controller.OpenShop);

            RectTransform coin = CreateRoundedButton("CoinStoreItem", topBar, Cream);
            SetTopLeft(coin, 694, 7, 238, 103);
            AddPanelDepth(coin.gameObject);
            RTLTextMeshPro coinText = CreateLabel("ValueText", coin, "۱۰۰", 65, TextAlignmentOptions.Center);
            SetTopLeft(coinText.rectTransform, 38, -6, 82, 108);
            RectTransform coinIcon = CreateSpriteNode("Icon", coin, LoadSprite("top_coin"), false);
            SetTopLeft(coinIcon, 119, -11, 118, 118);
            UnityEventTools.AddPersistentListener(coin.GetComponent<Button>().onClick, controller.OpenShop);

            BindDynamicCoinText(coinText);
        }

        private static void BuildFooter(RectTransform root, FigmaHomeController controller)
        {
            // The shipped scene keeps its footer directly under the canvas; reuse it
            // so a rebuild wires navigation to the live items instead of duplicating it.
            RectTransform existingFooter = root.parent.Find("Footer") as RectTransform;
            if (existingFooter != null && existingFooter.Find("EventsItem") != null)
            {
                controller.ConfigureNavigation(
                    existingFooter.Find("ActiveHighlight") as RectTransform,
                    existingFooter.Find("StoreItem") as RectTransform,
                    existingFooter.Find("CardsItem") as RectTransform,
                    existingFooter.Find("HomeItem") as RectTransform,
                    existingFooter.Find("EventsItem") as RectTransform,
                    existingFooter.Find("LeaderboardItem") as RectTransform);
                return;
            }

            RectTransform footer = CreateSpriteNode("Footer", root, LoadSprite("footer_base"), false);
            SetFigmaRect(footer, 94, 2119, 893, 193);

            RectTransform highlight = ChatUiFactory.Panel("ActiveHighlight", footer, ActiveCream).rectTransform;
            SetTopLeft(highlight, 338, -7, 189, 200);
            highlight.SetAsFirstSibling();
            AddActiveDepth(highlight.gameObject);

            RectTransform store = CreateNavItem("StoreItem", footer, 0, 0, 178, 193);
            CreateCroppedIcon("Icon", store, LoadAtlasSprite("figma_item_04"), 65, 35, 87, 87, 0, 0, 87, 87);
            CreateCroppedIcon("Badge", store, LoadSprite("footer_store_badge"), 81, 81, 41, 41, -5, -5, 51, 51);
            AddNavLabel(store, "فروشگاه", 25, 20, 124, 177, 55);
            UnityEventTools.AddPersistentListener(store.GetComponent<Button>().onClick, controller.SelectStore);

            RectTransform cards = CreateNavItem("CardsItem", footer, 178, 0, 160, 193);
            CreateCroppedIcon("Icon", cards, LoadAtlasSprite("figma_item_17"), 32, 35, 94, 94, 0, 0, 94, 94);
            AddNavLabel(cards, "کارت ها", 25, 0, 124, 134, 55);
            UnityEventTools.AddPersistentListener(cards.GetComponent<Button>().onClick, controller.SelectCards);

            RectTransform home = CreateNavItem("HomeItem", footer, 338, -7, 189, 200);
            CreateCroppedIcon("Icon", home, LoadAtlasSprite("figma_item_13"), 47, 19, 110, 110, 0, 0, 110, 110);
            AddNavLabel(home, "خانه", 33, 0, 132, 189, 62);
            UnityEventTools.AddPersistentListener(home.GetComponent<Button>().onClick, controller.SelectHome);

            RectTransform eventsItem = CreateNavItem("EventsItem", footer, 527, 0, 163, 193);
            CreateCroppedIcon("Icon", eventsItem, LoadAtlasSprite("figma_item_06"), 46, 33, 92, 92, 0, 0, 92, 92);
            AddNavLabel(eventsItem, "ایونت ها", 25, 0, 124, 163, 55);
            UnityEventTools.AddPersistentListener(eventsItem.GetComponent<Button>().onClick, controller.SelectEvents);

            RectTransform leaderboard = CreateNavItem("LeaderboardItem", footer, 690, 0, 203, 193);
            CreateCroppedIcon("Icon", leaderboard, LoadAtlasSprite("figma_item_01"), 54, 35, 90, 90, 0, 0, 90, 90);
            AddNavLabel(leaderboard, "لیدربرد", 25, 0, 124, 203, 55);
            UnityEventTools.AddPersistentListener(leaderboard.GetComponent<Button>().onClick, controller.SelectLeaderboard);

            controller.ConfigureNavigation(highlight, store, cards, home, eventsItem, leaderboard);
        }

        private static void BuildModePopup(RectTransform root, FigmaHomeController controller)
        {
            RectTransform popup = ChatUiFactory.Rect("GameModePopup", root);
            ChatUiFactory.Stretch(popup);
            CanvasGroup canvasGroup = popup.gameObject.AddComponent<CanvasGroup>();

            RectTransform artwork = CreateSpriteNode("BlurredBackdropAndModal", popup, LoadSprite("mode_popup_full"), true);
            ChatUiFactory.Stretch(artwork);
            Button closeButton = artwork.GetComponent<Button>();
            closeButton.transition = Selectable.Transition.None;
            UnityEventTools.AddPersistentListener(closeButton.onClick, controller.CloseModePopup);

            CreateModeHotspot(popup, "QuickModeButton", 138, 902, 796, 237, controller.SelectQuickMode);
            CreateModeHotspot(popup, "ProfessionalModeButton", 138, 1154, 796, 237, controller.SelectProfessionalMode);
            CreateModeHotspot(popup, "MasterModeButton", 138, 1406, 796, 237, controller.SelectMasterMode);
            CreateModeHotspot(popup, "SelectButton", 313, 1710, 466, 159, controller.SelectQuickMode);

            controller.ConfigureModePopup(root.Find("StartCTA") as RectTransform, popup, canvasGroup);
            popup.gameObject.SetActive(false);
        }

        private static void CreateModeHotspot(Transform parent, string name, float x, float y, float width, float height,
            UnityEngine.Events.UnityAction action)
        {
            RectTransform hotspot = CreateTransparentButton(name, parent);
            SetFigmaRect(hotspot, x, y, width, height);
            UnityEventTools.AddPersistentListener(hotspot.GetComponent<Button>().onClick, action);
        }

        private static void BuildLeaderboardScreen(Transform leaderboardPanel, FigmaHomeController controller)
        {
            if (leaderboardPanel == null)
                return;

            Transform old = leaderboardPanel.Find("FigmaLeaderboardScreen");
            if (old != null)
                Undo.DestroyObjectImmediate(old.gameObject);
            Transform legacyNavigation = leaderboardPanel.Find("FigmaLeaderboardNavigation");
            if (legacyNavigation != null)
                Undo.DestroyObjectImmediate(legacyNavigation.gameObject);

            // This is the clean Figma background asset, not a flattened screenshot of
            // the leaderboard. All player-facing values are composed below at runtime.
            RectTransform screen = CreateSpriteNode("FigmaLeaderboardScreen", leaderboardPanel, LoadSprite("leaderboard_background"), false);
            ChatUiFactory.Stretch(screen);
            screen.SetAsLastSibling();

            RectTransform dynamicLayer = ChatUiFactory.Rect("DynamicLeaderboardContent", screen);
            ChatUiFactory.Stretch(dynamicLayer);
            LeaderboardManager manager = leaderboardPanel.GetComponent<LeaderboardManager>();

            RectTransform titlePlate = CreateSpriteNode("LeaderboardTitlePlate", dynamicLayer,
                LoadSprite("leaderboard_title_ribbon"), false);
            SetFigmaRect(titlePlate, 173, 248, 735, 245);
            RTLTextMeshPro title = CreateLabel("Title", titlePlate, "جدول امتیازات", 62, TextAlignmentOptions.Center);
            title.color = Color.white;
            ChatUiFactory.Stretch(title.rectTransform);

            // Both tabs use the same warm active plate so the selected state remains
            // unambiguous when switching periods; the inactive tab is dimmed below.
            RectTransform monthlyButton = CreateSpriteNode("MonthlyTab", dynamicLayer, LoadSprite("leaderboard_tab_weekly"), true);
            SetFigmaRect(monthlyButton, 173, 558, 356, 73);
            monthlyButton.GetComponent<Button>().transition = Selectable.Transition.None;
            RTLTextMeshPro monthlyLabel = CreateLabel("Label", monthlyButton, "ماهانه", 42, TextAlignmentOptions.Center);
            monthlyLabel.color = Color.white;
            ChatUiFactory.Stretch(monthlyLabel.rectTransform);

            RectTransform weeklyButton = CreateSpriteNode("WeeklyTab", dynamicLayer, LoadSprite("leaderboard_tab_weekly"), true);
            SetFigmaRect(weeklyButton, 552, 558, 356, 73);
            weeklyButton.GetComponent<Button>().transition = Selectable.Transition.None;
            RTLTextMeshPro weeklyLabel = CreateLabel("Label", weeklyButton, "هفتگی", 42, TextAlignmentOptions.Center);
            weeklyLabel.color = Color.white;
            ChatUiFactory.Stretch(weeklyLabel.rectTransform);

            LeaderboardPodiumView pod1 = CreateLeaderboardPodium(dynamicLayer, "FirstPlace", 460, 750, 160, 235);
            LeaderboardPodiumView pod2 = CreateLeaderboardPodium(dynamicLayer, "SecondPlace", 205, 780, 150, 205);
            LeaderboardPodiumView pod3 = CreateLeaderboardPodium(dynamicLayer, "ThirdPlace", 730, 780, 150, 205);

            // The leaderboard data can contain many players. Keep the Figma frame as the
            // visual boundary, but make its data area a real Unity ScrollRect.
            Image viewport = ChatUiFactory.Panel("LeaderboardViewport", dynamicLayer, new Color(0f, 0f, 0f, 0f));
            SetFigmaRect(viewport.rectTransform, 163, 1055, 753, 703);
            viewport.raycastTarget = true;
            viewport.gameObject.AddComponent<RectMask2D>();

            RectTransform rowContainer = ChatUiFactory.Rect("LeaderboardRows", viewport.transform);
            rowContainer.anchorMin = new Vector2(0f, 1f);
            rowContainer.anchorMax = new Vector2(1f, 1f);
            rowContainer.pivot = new Vector2(0.5f, 1f);
            rowContainer.anchoredPosition = Vector2.zero;
            rowContainer.sizeDelta = Vector2.zero;
            VerticalLayoutGroup rowsLayout = rowContainer.gameObject.AddComponent<VerticalLayoutGroup>();
            rowsLayout.padding = new RectOffset(0, 0, 0, 0);
            rowsLayout.spacing = 4f;
            rowsLayout.childAlignment = TextAnchor.UpperCenter;
            rowsLayout.childControlWidth = true;
            rowsLayout.childControlHeight = true;
            rowsLayout.childForceExpandWidth = true;
            rowsLayout.childForceExpandHeight = false;
            ContentSizeFitter contentFitter = rowContainer.gameObject.AddComponent<ContentSizeFitter>();
            contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            ScrollRect leaderboardScroll = viewport.gameObject.AddComponent<ScrollRect>();
            leaderboardScroll.viewport = viewport.rectTransform;
            leaderboardScroll.content = rowContainer;
            leaderboardScroll.horizontal = false;
            leaderboardScroll.vertical = true;
            leaderboardScroll.movementType = ScrollRect.MovementType.Elastic;
            leaderboardScroll.elasticity = 0.08f;
            leaderboardScroll.scrollSensitivity = 42f;

            GameObject rowTemplate = CreateLeaderboardRowTemplate(dynamicLayer);

            RectTransform ownBar = CreateSpriteNode("OwnRankBar", dynamicLayer,
                LoadAtlasSprite("figma_item_05"), false);
            SetFigmaRect(ownBar, 205, 1772, 670, 72);
            RTLTextMeshPro myRank = CreateLabel("MyRank", ownBar, "رتبه شما: --", 30,
                TextAlignmentOptions.Center);
            SetTopLeft(myRank.rectTransform, 0, 0, 320, 72);
            RTLTextMeshPro myScore = CreateLabel("MyScore", ownBar, "-- دایسو", 30,
                TextAlignmentOptions.Center);
            SetTopLeft(myScore.rectTransform, 350, 0, 320, 72);

            Image statePanel = ChatUiFactory.Panel("LeaderboardState", dynamicLayer,
                new Color32(80, 48, 16, 230));
            SetFigmaRect(statePanel.rectTransform, 163, 1055, 753, 703);
            Button stateButton = statePanel.gameObject.AddComponent<Button>();
            stateButton.targetGraphic = statePanel;
            stateButton.transition = Selectable.Transition.ColorTint;
            RTLTextMeshPro stateText = CreateLabel("StateText", statePanel.transform,
                "در حال دریافت رتبه‌ها...", 42, TextAlignmentOptions.Center);
            stateText.color = Color.white;
            stateText.enableAutoSizing = true;
            stateText.fontSizeMin = 28;
            stateText.fontSizeMax = 44;
            stateText.margin = new Vector4(70, 40, 70, 40);
            ChatUiFactory.Stretch(stateText.rectTransform);
            if (manager != null)
                UnityEventTools.AddPersistentListener(stateButton.onClick, manager.RefreshCurrent);
            statePanel.gameObject.SetActive(false);

            RectTransform timerPanel = CreateSpriteNode("ResetTimerPanel", dynamicLayer, LoadSprite("leaderboard_timer"), false);
            SetFigmaRect(timerPanel, 205, 1850, 670, 207);
            RTLTextMeshPro timerText = CreateLabel("ResetTimerText", timerPanel, "ریست هفتگی\n--:--:--", 43,
                TextAlignmentOptions.Center);
            timerText.color = Color.white;
            timerText.enableAutoSizing = true;
            timerText.fontSizeMin = 30;
            timerText.fontSizeMax = 52;
            ChatUiFactory.Stretch(timerText.rectTransform);

            RectTransform navigation = ChatUiFactory.Rect("FigmaLeaderboardNavigation", screen);
            ChatUiFactory.Stretch(navigation);
            Image footerHome = ChatUiFactory.Panel("FooterHomeButton", navigation, Cream);
            SetFigmaRect(footerHome.rectTransform, 350, 2150, 380, 105);
            footerHome.gameObject.AddComponent<Outline>().effectColor = new Color32(157, 101, 60, 255);
            footerHome.GetComponent<Outline>().effectDistance = new Vector2(0f, -4f);
            Button footerHomeButton = footerHome.gameObject.AddComponent<Button>();
            footerHomeButton.transition = Selectable.Transition.None;
            RTLTextMeshPro homeLabel = CreateLabel("Label", footerHome.transform, "خانه", 40, TextAlignmentOptions.Center);
            homeLabel.color = Brown;
            ChatUiFactory.Stretch(homeLabel.rectTransform);
            UnityEventTools.AddPersistentListener(footerHomeButton.onClick, controller.CloseLeaderboard);

            if (manager != null)
            {
                SerializedObject serialized = new SerializedObject(manager);
                SetObjectReference(serialized, "pod1Root", pod1.root);
                SetObjectReference(serialized, "pod2Root", pod2.root);
                SetObjectReference(serialized, "pod3Root", pod3.root);
                SetObjectReference(serialized, "pod1Avatar", pod1.avatar);
                SetObjectReference(serialized, "pod2Avatar", pod2.avatar);
                SetObjectReference(serialized, "pod3Avatar", pod3.avatar);
                SetObjectReference(serialized, "pod1Name", pod1.name);
                SetObjectReference(serialized, "pod2Name", pod2.name);
                SetObjectReference(serialized, "pod3Name", pod3.name);
                SetObjectReference(serialized, "pod1Reward", pod1.reward);
                SetObjectReference(serialized, "pod2Reward", pod2.reward);
                SetObjectReference(serialized, "pod3Reward", pod3.reward);
                SetObjectReference(serialized, "rowContainer", rowContainer);
                SetObjectReference(serialized, "rowPrefab", rowTemplate);
                SetObjectReference(serialized, "leaderboardScroll", leaderboardScroll);
                SetObjectReference(serialized, "myRankText", myRank);
                SetObjectReference(serialized, "myScoreText", myScore);
                SetObjectReference(serialized, "statePanel", statePanel.gameObject);
                SetObjectReference(serialized, "stateText", stateText);
                SetObjectReference(serialized, "weeklyButton", weeklyButton.GetComponent<Button>());
                SetObjectReference(serialized, "monthlyButton", monthlyButton.GetComponent<Button>());
                SetObjectReference(serialized, "resetTimerText", timerText);
                serialized.FindProperty("tabActiveColor").colorValue = Color.white;
                serialized.FindProperty("tabInactiveColor").colorValue = new Color(0.62f, 0.52f, 0.42f, 1f);
                serialized.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(manager);
            }
        }

        private struct LeaderboardPodiumView
        {
            public GameObject root;
            public Image avatar;
            public RTLTextMeshPro name;
            public RTLTextMeshPro reward;
        }

        private static LeaderboardPodiumView CreateLeaderboardPodium(Transform parent, string name, float x, float y,
            float width, float height)
        {
            RectTransform root = ChatUiFactory.Rect(name, parent);
            SetFigmaRect(root, x, y, width, height);

            Image avatarFrame = ChatUiFactory.Panel("AvatarFrame", root, new Color32(255, 221, 162, 255));
            SetTopLeft(avatarFrame.rectTransform, (width - 120) * 0.5f, 0, 120, 120);
            Outline avatarOutline = avatarFrame.gameObject.AddComponent<Outline>();
            avatarOutline.effectColor = new Color32(91, 50, 17, 255);
            avatarOutline.effectDistance = new Vector2(0f, -4f);

            Image avatar = ChatUiFactory.Panel("Avatar", avatarFrame.transform, Color.clear);
            SetTopLeft(avatar.rectTransform, 8, 8, 104, 104);
            avatar.raycastTarget = false;
            avatar.preserveAspect = true;

            Image namePlate = ChatUiFactory.Panel("NamePlate", root, new Color32(91, 50, 17, 245));
            SetTopLeft(namePlate.rectTransform, 0, 118, width, 52);
            namePlate.raycastTarget = false;
            RTLTextMeshPro playerName = CreateLabel("PlayerName", namePlate.transform, "---", 28, TextAlignmentOptions.Center);
            playerName.color = Color.white;
            playerName.enableAutoSizing = true;
            playerName.fontSizeMin = 18;
            playerName.fontSizeMax = 28;
            playerName.enableWordWrapping = false;
            playerName.overflowMode = TextOverflowModes.Ellipsis;
            ChatUiFactory.Stretch(playerName.rectTransform);

            Image rewardPlate = ChatUiFactory.Panel("RewardPlate", root, new Color32(255, 221, 162, 248));
            SetTopLeft(rewardPlate.rectTransform, 0, 174, width, 42);
            rewardPlate.raycastTarget = false;
            RTLTextMeshPro reward = CreateLabel("Reward", rewardPlate.transform, "", 22, TextAlignmentOptions.Center);
            reward.enableAutoSizing = true;
            reward.fontSizeMin = 16;
            reward.fontSizeMax = 22;
            reward.enableWordWrapping = false;
            reward.overflowMode = TextOverflowModes.Ellipsis;
            ChatUiFactory.Stretch(reward.rectTransform);

            return new LeaderboardPodiumView
            {
                root = root.gameObject,
                avatar = avatar,
                name = playerName,
                reward = reward
            };
        }

        private static GameObject CreateLeaderboardRowTemplate(Transform parent)
        {
            // Figma row is deliberately composed from an empty plate plus live data.
            // Do not use a row screenshot here: it would carry a sample avatar/text
            // and creates the white boxed duplicate seen in the previous build.
            Image rowBackground = ChatUiFactory.Panel("LeaderboardRowTemplate", parent, new Color32(242, 206, 151, 255));
            RectTransform row = rowBackground.rectTransform;
            SetTopLeft(row, -2000, 0, 753, 131);
            Outline rowOutline = row.gameObject.AddComponent<Outline>();
            rowOutline.effectColor = new Color32(157, 101, 60, 255);
            rowOutline.effectDistance = new Vector2(0f, -4f);
            LayoutElement layout = row.gameObject.AddComponent<LayoutElement>();
            layout.preferredHeight = 131;
            layout.minHeight = 131;
            layout.flexibleWidth = 1;

            RTLTextMeshPro rank = CreateLabel("Rank", row, "--", 38, TextAlignmentOptions.Center);
            SetTopLeft(rank.rectTransform, 18, 21, 78, 88);
            rank.enableAutoSizing = true;
            rank.fontSizeMin = 22;
            rank.fontSizeMax = 38;
            rank.enableWordWrapping = false;

            Image avatar = ChatUiFactory.Panel("Avatar", row, Color.clear);
            SetTopLeft(avatar.rectTransform, 111, 15, 96, 96);
            avatar.raycastTarget = false;
            avatar.preserveAspect = true;

            RTLTextMeshPro playerName = CreateLabel("PlayerName", row, "---", 35, TextAlignmentOptions.Center);
            SetTopLeft(playerName.rectTransform, 214, 27, 205, 78);
            playerName.enableAutoSizing = true;
            playerName.fontSizeMin = 18;
            playerName.fontSizeMax = 35;
            playerName.enableWordWrapping = false;
            playerName.overflowMode = TextOverflowModes.Ellipsis;

            RTLTextMeshPro score = CreateLabel("Score", row, "-- دایسو", 31, TextAlignmentOptions.Center);
            SetTopLeft(score.rectTransform, 548, 27, 185, 78);
            score.enableAutoSizing = true;
            score.fontSizeMin = 16;
            score.fontSizeMax = 31;
            score.enableWordWrapping = false;
            score.overflowMode = TextOverflowModes.Ellipsis;

            LeaderboardRowUI rowUi = row.gameObject.AddComponent<LeaderboardRowUI>();
            rowUi.avatarImage = avatar;
            rowUi.rankText = rank;
            rowUi.nameText = playerName;
            rowUi.scoreText = score;
            rowUi.rewardText = null;
            rowUi.rowBackground = rowBackground;
            row.gameObject.SetActive(false);
            return row.gameObject;
        }

        private static void SetObjectReference(SerializedObject serialized, string propertyName, Object value)
        {
            SerializedProperty property = serialized.FindProperty(propertyName);
            if (property != null)
                property.objectReferenceValue = value;
        }

        private static void BuildShopScreen(Transform shopPanel, FigmaHomeController controller)
        {
            if (shopPanel == null)
                return;

            Transform oldLoadingOverlay = shopPanel.Find("ShopLoadingOverlay");
            if (oldLoadingOverlay != null)
                Undo.DestroyObjectImmediate(oldLoadingOverlay.gameObject);

            Image panelImage = shopPanel.GetComponent<Image>();
            if (panelImage != null)
            {
                panelImage.sprite = LoadSprite("shop_screen");
                panelImage.type = Image.Type.Simple;
                panelImage.preserveAspect = false;
                EditorUtility.SetDirty(panelImage);
            }

            Transform logo = shopPanel.Find("logo");
            if (logo != null)
                logo.gameObject.SetActive(false);

            Transform close = shopPanel.Find("Button close");
            if (close != null)
                close.gameObject.SetActive(false);

            RectTransform items = shopPanel.Find("Item shops") as RectTransform;
            if (items != null)
            {
                SetTopLeft(items, 130, 650, 820, 1250);
                items.localScale = Vector3.one;
                GridLayoutGroup grid = items.GetComponent<GridLayoutGroup>();
                if (grid != null)
                {
                    grid.padding = new RectOffset(0, 0, 0, 0);
                    grid.startCorner = GridLayoutGroup.Corner.UpperRight;
                    grid.startAxis = GridLayoutGroup.Axis.Horizontal;
                    grid.childAlignment = TextAnchor.UpperCenter;
                    grid.cellSize = new Vector2(360, 250);
                    grid.spacing = new Vector2(80, 55);
                    grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                    grid.constraintCount = 2;
                    EditorUtility.SetDirty(grid);
                }
            }

            CoinShopManager shopManager = shopPanel.GetComponent<CoinShopManager>();
            if (shopManager != null)
            {
                SerializedObject serializedShop = new SerializedObject(shopManager);
                RTLTextMeshPro statusText = serializedShop.FindProperty("statusText")?.objectReferenceValue as RTLTextMeshPro;
                if (statusText != null)
                {
                    SetFigmaRect(statusText.rectTransform, 145, 560, 790, 72);
                    statusText.fontSize = 32;
                    statusText.enableAutoSizing = true;
                    statusText.fontSizeMin = 22;
                    statusText.fontSizeMax = 34;
                    statusText.alignment = TextAlignmentOptions.Center;
                    statusText.raycastTarget = false;
                }

                Image loadingOverlay = ChatUiFactory.Panel("ShopLoadingOverlay", shopPanel,
                    new Color32(72, 46, 8, 215));
                ChatUiFactory.Stretch(loadingOverlay.rectTransform);
                loadingOverlay.transform.SetAsLastSibling();
                RTLTextMeshPro loadingText = CreateLabel("LoadingText", loadingOverlay.transform,
                    "در حال انجام خرید...", 42, TextAlignmentOptions.Center);
                loadingText.color = Color.white;
                ChatUiFactory.Stretch(loadingText.rectTransform);
                loadingOverlay.gameObject.SetActive(false);
                SetObjectReference(serializedShop, "loadingOverlay", loadingOverlay.gameObject);
                serializedShop.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(shopManager);
            }

            Transform oldNavigation = shopPanel.Find("FigmaShopNavigation");
            if (oldNavigation != null)
                Undo.DestroyObjectImmediate(oldNavigation.gameObject);

            RectTransform navigation = ChatUiFactory.Rect("FigmaShopNavigation", shopPanel);
            ChatUiFactory.Stretch(navigation);
            navigation.SetAsLastSibling();

            // ── Clean Navigation for Shop Frame ──
            // 1. Home tab
            RectTransform homeButton = CreateTransparentButton("HomeButton", navigation);
            SetFigmaRect(homeButton, 432, 2119, 189, 193);
            UnityEventTools.AddPersistentListener(homeButton.GetComponent<Button>().onClick, controller.CloseShop);

            RectTransform cardsButton = CreateTransparentButton("CardsButton", navigation);
            SetFigmaRect(cardsButton, 272, 2119, 160, 193);
            UnityEventTools.AddPersistentListener(cardsButton.GetComponent<Button>().onClick, controller.SelectCards);

            RectTransform eventsButton = CreateTransparentButton("EventsButton", navigation);
            SetFigmaRect(eventsButton, 621, 2119, 163, 193);
            UnityEventTools.AddPersistentListener(eventsButton.GetComponent<Button>().onClick, controller.SelectEvents);

            RectTransform leaderboardButton = CreateTransparentButton("LeaderboardButton", navigation);
            SetFigmaRect(leaderboardButton, 784, 2119, 203, 193);
            UnityEventTools.AddPersistentListener(leaderboardButton.GetComponent<Button>().onClick, controller.OpenLeaderboardFromShop);

            RectTransform profileButton = CreateTransparentButton("ProfileButton", navigation);
            SetFigmaRect(profileButton, 194, 117, 103, 103);
            UnityEventTools.AddPersistentListener(profileButton.GetComponent<Button>().onClick, controller.OpenProfile);
        }

        private static RectTransform CreateSpriteNode(string name, Transform parent, Sprite sprite, bool button)
        {
            RectTransform rect = ChatUiFactory.Rect(name, parent);
            Image image = rect.gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.type = Image.Type.Simple;
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

        private static RectTransform CreateNavItem(string name, Transform footer, float x, float y, float width, float height)
        {
            RectTransform item = CreateTransparentButton(name, footer);
            SetTopLeft(item, x, y, width, height);
            return item;
        }

        private static void CreateCroppedIcon(string name, Transform parent, Sprite sprite,
            float x, float y, float width, float height, float innerX, float innerY, float innerWidth, float innerHeight)
        {
            RectTransform viewport = ChatUiFactory.Rect(name, parent);
            SetTopLeft(viewport, x, y, width, height);
            viewport.gameObject.AddComponent<RectMask2D>();
            RectTransform image = CreateSpriteNode("Artwork", viewport, sprite, false);
            SetTopLeft(image, innerX, innerY, innerWidth, innerHeight);
        }

        private static void AddNavLabel(Transform parent, string text, int size, float x, float y, float width, float height)
        {
            RTLTextMeshPro label = CreateLabel("Label", parent, text, size, TextAlignmentOptions.Center);
            SetTopLeft(label.rectTransform, x, y, width, height);
        }

        private static RTLTextMeshPro CreateLabel(string name, Transform parent, string text, int size, TextAlignmentOptions alignment)
        {
            RTLTextMeshPro label = ChatUiFactory.Text(name, parent, text, size, Brown, alignment);
            label.fontStyle = FontStyles.Bold;
            label.raycastTarget = false;
            label.PreserveNumbers = true;
            return label;
        }

        private static void AddPanelDepth(GameObject target)
        {
            Shadow shadow = target.AddComponent<Shadow>();
            shadow.effectColor = new Color32(197, 148, 93, 220);
            shadow.effectDistance = new Vector2(0, -5);
        }

        private static void AddActiveDepth(GameObject target)
        {
            Shadow shadow = target.AddComponent<Shadow>();
            shadow.effectColor = new Color32(94, 66, 34, 210);
            shadow.effectDistance = new Vector2(0, -5);
            Outline outline = target.AddComponent<Outline>();
            outline.effectColor = new Color32(207, 147, 60, 255);
            outline.effectDistance = new Vector2(2, -2);
        }

        private static void AddModalDepth(GameObject target)
        {
            Shadow shadow = target.AddComponent<Shadow>();
            shadow.effectColor = new Color32(86, 49, 16, 190);
            shadow.effectDistance = new Vector2(0, -14);
            Outline outline = target.AddComponent<Outline>();
            outline.effectColor = new Color32(197, 148, 93, 255);
            outline.effectDistance = new Vector2(4, -4);
        }

        private static void BindDynamicCoinText(RTLTextMeshPro coinText)
        {
            UiManagerHome manager = Object.FindObjectsByType<UiManagerHome>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .FirstOrDefault(candidate => candidate.gameObject.scene == UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            if (manager == null)
                return;

            SerializedObject serializedManager = new SerializedObject(manager);
            SerializedProperty coinProperty = serializedManager.FindProperty("Cointext");
            if (coinProperty != null)
            {
                coinProperty.objectReferenceValue = coinText;
                serializedManager.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(manager);
            }
        }

        private static Sprite LoadSprite(string name)
        {
            string path = PartsPath + name + ".png";
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

        private static Sprite LoadAtlasSprite(string name)
        {
            string path = AtlasItemsPath + name + ".png";
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (Object asset in assets)
            {
                if (asset is Sprite sprite &&
                    (sprite.name == name || sprite.name.StartsWith(name + "_")))
                    return sprite;
            }

            Debug.LogError($"HomeMenuAuthoring: sprite '{name}' was not found at {path}.");
            return null;
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

        private static Transform FindDeep(Transform parent, string exactName)
        {
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

        private static TMP_FontAsset FindPersianFont()
        {
            RTLTextMeshPro text = Object.FindObjectsByType<RTLTextMeshPro>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .FirstOrDefault(candidate => candidate.font != null);
            return text != null ? text.font : TMP_Settings.defaultFontAsset;
        }
    }
}
