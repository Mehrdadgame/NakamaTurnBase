using System;
using System.Linq;
using DG.Tweening;
using RTLTMPro;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace NinjaBattle.UI.Editor
{
    public static class EndGameFigmaAuthoring
    {
        private const string Scene4 = "Assets/-Scenes/4-VerticalAndHorizontal.unity";
        private const string Scene5 = "Assets/-Scenes/5-FourByThree.unity";
        private const string Scene6 = "Assets/-Scenes/6-FourByFour.unity";

        private const string PathCardBg = "Assets/Figma/Images/Rectangle_22_2005_1191.png";
        private const string PathButtonBg = "Assets/Figma/Images/Rectangle_20_2005_1211.png";
        private const string PathDivider = "Assets/Figma/Parts/card_divider_line.png";
        private const string PathBadgeR = "Assets/Figma/Parts/figma_badge_r.png";
        private const string PathP1Frame = "Assets/Figma/Images/image_12002772_2005_1219.png";
        private const string PathP1Avatar = "Assets/Figma/Images/image_12002784_I2005_1188;2005_1258.png";
        private const string PathP2Avatar = "Assets/Figma/Parts/figma_avatar_wizard.png";
        private const string PathFloorShadow = "Assets/Figma/Parts/card_character_shadow.png";
        private const string PathGoldenGlow = "Assets/Figma/Parts/golden_glow_aura.png";
        private const string PathWinTrophy = "Assets/Figma/Parts/win_trophy.png";
        private const string PathLoseDice = "Assets/Figma/Parts/lose_character.png";
        private const string PathStar = "Assets/Figma/Parts/figma_star.png";
        private const string PathFont = "Assets/Font/Samim-Bold-FD SDF.asset";
        private const string PathWinSound = "Assets/Audio/Sounds/Victory.ogg";
        private const string PathLoseSound = "Assets/Audio/Sounds/Defeat.ogg";

        [MenuItem("Tools/Ninja Battle/Apply Figma Win Lose Popup To All Gameplay Scenes")]
        public static void ApplyToAllScenes()
        {
            string currentScene = SceneManager.GetActiveScene().path;

            ApplyToScene(Scene4);
            ApplyToScene(Scene5);
            ApplyToScene(Scene6);

            if (!string.IsNullOrEmpty(currentScene))
                EditorSceneManager.OpenScene(currentScene, OpenSceneMode.Single);

            Debug.Log("[EndGameFigmaAuthoring] Successfully applied Deluxe Figma Win/Lose Popup to all gameplay scenes!");
        }

        [MenuItem("Tools/Ninja Battle/Apply Figma Win Lose Popup To Current Scene")]
        public static void ApplyToCurrentScene()
        {
            Scene scene = SceneManager.GetActiveScene();
            BuildPopupInActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);
            Debug.Log($"[EndGameFigmaAuthoring] Applied Deluxe Figma Win/Lose Popup to {scene.name}!");
        }

        [MenuItem("Tools/Ninja Battle/Preview Win Popup")]
        public static void PreviewWinPopup()
        {
            ActionEndGame end = Object.FindFirstObjectByType<ActionEndGame>(FindObjectsInactive.Include);
            if (end != null && end.ResultPanel != null)
            {
                end.ResultPanel.SetActive(true);
                GameResultPresentation pres = end.ResultPresentation;
                if (pres == null) pres = end.ResultPanel.GetComponentInChildren<GameResultPresentation>(true);
                if (pres != null)
                {
                    pres.gameObject.SetActive(true);
                    pres.ShowPreview(true, 15, 10, "شاهین ۲۲", "سهراب ۱");
                }
            }
        }

        [MenuItem("Tools/Ninja Battle/Preview Lose Popup")]
        public static void PreviewLosePopup()
        {
            ActionEndGame end = Object.FindFirstObjectByType<ActionEndGame>(FindObjectsInactive.Include);
            if (end != null && end.ResultPanel != null)
            {
                end.ResultPanel.SetActive(true);
                GameResultPresentation pres = end.ResultPresentation;
                if (pres == null) pres = end.ResultPanel.GetComponentInChildren<GameResultPresentation>(true);
                if (pres != null)
                {
                    pres.gameObject.SetActive(true);
                    pres.ShowPreview(false, 8, 14, "شاهین ۲۲", "سهراب ۱");
                }
            }
        }

        [MenuItem("Tools/Ninja Battle/Hide Result Popup")]
        public static void HideResultPopup()
        {
            ActionEndGame end = Object.FindFirstObjectByType<ActionEndGame>(FindObjectsInactive.Include);
            if (end != null && end.ResultPanel != null)
            {
                end.ResultPanel.SetActive(false);
            }
        }

        [MenuItem("Tools/Ninja Battle/Preview Iris Closed")]
        public static void PreviewIrisClosed() => SetIrisProgress(1f);

        [MenuItem("Tools/Ninja Battle/Preview Iris Opening")]
        public static void PreviewIrisOpening() => SetIrisProgress(0.85f);

        [MenuItem("Tools/Ninja Battle/Preview Iris Halfway")]
        public static void PreviewIrisHalf() => SetIrisProgress(0.5f);

        [MenuItem("Tools/Ninja Battle/Preview Iris Reset")]
        public static void PreviewIrisReset() => SetIrisProgress(0f);

        private static void SetIrisProgress(float progress)
        {
            CartoonIrisTransitionOverlay overlay = Object.FindFirstObjectByType<CartoonIrisTransitionOverlay>(FindObjectsInactive.Include);
            if (overlay != null)
            {
                overlay.gameObject.SetActive(true);
                Image img = overlay.GetComponent<Image>();
                if (img != null)
                {
                    img.enabled = progress > 0.001f;
                    if (img.material != null)
                    {
                        img.material.SetFloat("_AspectRatio", 0f);
                        img.material.SetFloat("_Progress", progress);
                        img.material.SetVector("_Center", new Vector4(0.5f, 0.46f, 0f, 0f));
                    }
                }
            }
        }

        public static void ApplyToScene(string scenePath)
        {
            try
            {
                Debug.Log($"[EndGameFigmaAuthoring] Opening {scenePath}...");
                Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                BuildPopupInActiveScene();
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                Debug.Log($"[EndGameFigmaAuthoring] Successfully saved {scenePath}!");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[EndGameFigmaAuthoring] Error in {scenePath}: {ex}");
            }
        }

        public static void BuildPopupInActiveScene()
        {
            ActionEndGame end = Object.FindFirstObjectByType<ActionEndGame>(FindObjectsInactive.Include);
            if (end == null || end.ResultPanel == null)
            {
                Debug.LogWarning("[EndGameFigmaAuthoring] ActionEndGame or ResultPanel not found in active scene!");
                return;
            }

            GameObject resultPanel = end.ResultPanel;
            Transform root = resultPanel.transform;

            // Ensure ResultPanel itself is stretched fullscreen
            RectTransform panelRect = resultPanel.GetComponent<RectTransform>();
            if (panelRect != null)
            {
                panelRect.anchorMin = Vector2.zero;
                panelRect.anchorMax = Vector2.one;
                panelRect.offsetMin = Vector2.zero;
                panelRect.offsetMax = Vector2.zero;
            }

            // Remove any old background image on EndGamePanel itself if it dims incorrectly
            Image panelImg = resultPanel.GetComponent<Image>();
            if (panelImg != null)
            {
                panelImg.color = Color.clear;
                panelImg.raycastTarget = false;
            }

            // Hide or clean old legacy children (PopUp, Back To Home, etc.)
            for (int i = 0; i < root.childCount; i++)
            {
                Transform c = root.GetChild(i);
                if (c.name != "FigmaResultVisual")
                {
                    c.gameObject.SetActive(false);
                }
            }

            // Destroy previous FigmaResultVisual to ensure clean rebuild
            Transform existing = root.Find("FigmaResultVisual");
            if (existing != null)
                Object.DestroyImmediate(existing.gameObject);

            // Load assets
            TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(PathFont);
            if (font == null) font = TMP_Settings.defaultFontAsset;

            Sprite sCardBg = LoadSprite(PathCardBg);
            Sprite sButtonBg = LoadSprite(PathButtonBg);
            Sprite sDivider = LoadSprite(PathDivider);
            Sprite sBadgeR = LoadSprite(PathBadgeR);
            Sprite sP1Frame = LoadSprite(PathP1Frame);
            Sprite sP1Avatar = LoadSprite(PathP1Avatar);
            Sprite sP2Avatar = LoadSprite(PathP2Avatar);
            Sprite sFloorShadow = LoadSprite(PathFloorShadow);
            Sprite sGoldenGlow = LoadSprite(PathGoldenGlow);
            Sprite sWinTrophy = LoadSprite(PathWinTrophy, "win_trophy_0");
            Sprite sLoseDice = LoadSprite(PathLoseDice, "lose_character_0");
            Sprite sStar = LoadSprite(PathStar);

            AudioClip winAudio = AssetDatabase.LoadAssetAtPath<AudioClip>(PathWinSound);
            AudioClip loseAudio = AssetDatabase.LoadAssetAtPath<AudioClip>(PathLoseSound);

            // 1. Container: FigmaResultVisual (Full Stretch)
            GameObject visualObj = new GameObject("FigmaResultVisual", typeof(RectTransform));
            visualObj.transform.SetParent(root, false);
            RectTransform visualRect = visualObj.GetComponent<RectTransform>();
            Stretch(visualRect);

            // 2. Dim Backdrop
            GameObject backdropObj = new GameObject("DimBackdrop", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
            backdropObj.transform.SetParent(visualRect, false);
            RectTransform backdropRect = backdropObj.GetComponent<RectTransform>();
            Stretch(backdropRect);
            Image backdropImg = backdropObj.GetComponent<Image>();
            backdropImg.color = new Color(0f, 0f, 0f, 0.72f);
            backdropImg.raycastTarget = true;
            CanvasGroup backdropCg = backdropObj.GetComponent<CanvasGroup>();

            // 3. Modal Card (840 x 1380)
            GameObject modalObj = new GameObject("ModalCard", typeof(RectTransform));
            modalObj.transform.SetParent(visualRect, false);
            RectTransform modalRect = modalObj.GetComponent<RectTransform>();
            modalRect.anchorMin = new Vector2(0.5f, 0.5f);
            modalRect.anchorMax = new Vector2(0.5f, 0.5f);
            modalRect.pivot = new Vector2(0.5f, 0.5f);
            modalRect.anchoredPosition = Vector2.zero;
            modalRect.sizeDelta = new Vector2(840f, 1380f);

            // Card Background Image
            GameObject cardBgObj = new GameObject("CardBg", typeof(RectTransform), typeof(Image));
            cardBgObj.transform.SetParent(modalRect, false);
            RectTransform cardBgRect = cardBgObj.GetComponent<RectTransform>();
            Stretch(cardBgRect);
            Image cardBgImg = cardBgObj.GetComponent<Image>();
            cardBgImg.sprite = sCardBg;
            cardBgImg.type = Image.Type.Simple;
            cardBgImg.preserveAspect = false;
            cardBgImg.raycastTarget = false;

            // Header Divider Line
            GameObject dividerObj = new GameObject("HeaderDivider", typeof(RectTransform), typeof(Image));
            dividerObj.transform.SetParent(modalRect, false);
            RectTransform dividerRect = dividerObj.GetComponent<RectTransform>();
            SetAnchored(dividerRect, 0f, 270f, 760f, 6f);
            Image dividerImg = dividerObj.GetComponent<Image>();
            dividerImg.sprite = sDivider;
            dividerImg.raycastTarget = false;

            // Green "R" Rematch Badge
            GameObject badgeObj = new GameObject("BadgeR", typeof(RectTransform), typeof(Image));
            badgeObj.transform.SetParent(modalRect, false);
            RectTransform badgeRect = badgeObj.GetComponent<RectTransform>();
            SetAnchored(badgeRect, 418f, 270f, 68f, 68f);
            Image badgeImg = badgeObj.GetComponent<Image>();
            badgeImg.sprite = sBadgeR;
            badgeImg.preserveAspect = true;
            badgeImg.raycastTarget = false;

            // Player 1 Header Group (Left: -185, 460)
            GameObject p1Group = new GameObject("Player1Group", typeof(RectTransform));
            p1Group.transform.SetParent(modalRect, false);
            RectTransform p1GroupRect = p1Group.GetComponent<RectTransform>();
            SetAnchored(p1GroupRect, -185f, 460f, 260f, 300f);

            GameObject p1FrameObj = new GameObject("Frame", typeof(RectTransform), typeof(Image));
            p1FrameObj.transform.SetParent(p1GroupRect, false);
            RectTransform p1FrameRect = p1FrameObj.GetComponent<RectTransform>();
            SetAnchored(p1FrameRect, 0f, 50f, 145f, 145f);
            Image p1FrameImg = p1FrameObj.GetComponent<Image>();
            p1FrameImg.sprite = sP1Frame;
            p1FrameImg.preserveAspect = true;

            GameObject p1AvatarObj = new GameObject("Avatar", typeof(RectTransform), typeof(Image));
            p1AvatarObj.transform.SetParent(p1FrameRect, false);
            RectTransform p1AvatarRect = p1AvatarObj.GetComponent<RectTransform>();
            Stretch(p1AvatarRect, 8f);
            Image p1AvatarImg = p1AvatarObj.GetComponent<Image>();
            p1AvatarImg.sprite = sP1Avatar;
            p1AvatarImg.preserveAspect = true;

            RTLTextMeshPro p1Name = CreateLabel("Name", p1GroupRect, "shahin ۲۲", 32, new Color32(140, 46, 27, 255), font);
            SetAnchored(p1Name.rectTransform, 0f, -42f, 240f, 40f);

            RTLTextMeshPro p1Score = CreateLabel("Score", p1GroupRect, "۱۵", 58, new Color32(74, 46, 8, 255), font);
            SetAnchored(p1Score.rectTransform, 0f, -98f, 140f, 65f);

            // Player 2 Header Group (Right: 185, 460)
            GameObject p2Group = new GameObject("Player2Group", typeof(RectTransform));
            p2Group.transform.SetParent(modalRect, false);
            RectTransform p2GroupRect = p2Group.GetComponent<RectTransform>();
            SetAnchored(p2GroupRect, 185f, 460f, 260f, 300f);

            GameObject p2AvatarObj = new GameObject("Avatar", typeof(RectTransform), typeof(Image));
            p2AvatarObj.transform.SetParent(p2GroupRect, false);
            RectTransform p2AvatarRect = p2AvatarObj.GetComponent<RectTransform>();
            SetAnchored(p2AvatarRect, 0f, 50f, 145f, 145f);
            Image p2AvatarImg = p2AvatarObj.GetComponent<Image>();
            p2AvatarImg.sprite = sP2Avatar;
            p2AvatarImg.preserveAspect = true;

            RTLTextMeshPro p2Name = CreateLabel("Name", p2GroupRect, "sohrab ۱", 32, new Color32(35, 69, 130, 255), font);
            SetAnchored(p2Name.rectTransform, 0f, -42f, 240f, 40f);

            RTLTextMeshPro p2Score = CreateLabel("Score", p2GroupRect, "۱۰", 58, new Color32(74, 46, 8, 255), font);
            SetAnchored(p2Score.rectTransform, 0f, -98f, 140f, 65f);

            // Center Visuals (pos: 0, 20)
            GameObject centerObj = new GameObject("CenterVisual", typeof(RectTransform));
            centerObj.transform.SetParent(modalRect, false);
            RectTransform centerRect = centerObj.GetComponent<RectTransform>();
            SetAnchored(centerRect, 0f, 20f, 600f, 500f);

            // Floor drop shadow
            GameObject shadowObj = new GameObject("ShadowFloor", typeof(RectTransform), typeof(Image));
            shadowObj.transform.SetParent(centerRect, false);
            RectTransform shadowRect = shadowObj.GetComponent<RectTransform>();
            SetAnchored(shadowRect, 0f, -145f, 380f, 85f);
            Image shadowImg = shadowObj.GetComponent<Image>();
            shadowImg.sprite = sFloorShadow;
            shadowImg.color = Color.white;
            shadowImg.raycastTarget = false;

            // Win Root
            GameObject winRoot = new GameObject("WinRoot", typeof(RectTransform));
            winRoot.transform.SetParent(centerRect, false);
            RectTransform winRect = winRoot.GetComponent<RectTransform>();
            SetAnchored(winRect, 0f, 0f, 600f, 500f);

            GameObject glowObj = new GameObject("GoldenGlow", typeof(RectTransform), typeof(Image));
            glowObj.transform.SetParent(winRect, false);
            RectTransform glowRect = glowObj.GetComponent<RectTransform>();
            SetAnchored(glowRect, 0f, 30f, 490f, 490f);
            Image glowImg = glowObj.GetComponent<Image>();
            glowImg.sprite = sGoldenGlow;
            glowImg.color = new Color(1f, 0.88f, 0.5f, 0.75f);
            glowImg.raycastTarget = false;

            GameObject trophyObj = new GameObject("WinTrophy", typeof(RectTransform), typeof(Image));
            trophyObj.transform.SetParent(winRect, false);
            RectTransform trophyRect = trophyObj.GetComponent<RectTransform>();
            SetAnchored(trophyRect, 0f, 35f, 440f, 390f);
            Image trophyImg = trophyObj.GetComponent<Image>();
            trophyImg.sprite = sWinTrophy;
            trophyImg.preserveAspect = true;
            trophyImg.raycastTarget = false;

            // Lose Root
            GameObject loseRoot = new GameObject("LoseRoot", typeof(RectTransform));
            loseRoot.transform.SetParent(centerRect, false);
            RectTransform loseRect = loseRoot.GetComponent<RectTransform>();
            SetAnchored(loseRect, 0f, 0f, 600f, 500f);

            GameObject diceObj = new GameObject("LoseDice", typeof(RectTransform), typeof(Image));
            diceObj.transform.SetParent(loseRect, false);
            RectTransform diceRect = diceObj.GetComponent<RectTransform>();
            SetAnchored(diceRect, 0f, 20f, 370f, 370f);
            Image diceImg = diceObj.GetComponent<Image>();
            diceImg.sprite = sLoseDice;
            diceImg.preserveAspect = true;
            diceImg.raycastTarget = false;

            GameObject starsRoot = new GameObject("LoseStarsRoot", typeof(RectTransform));
            starsRoot.transform.SetParent(loseRect, false);
            RectTransform starsRect = starsRoot.GetComponent<RectTransform>();
            SetAnchored(starsRect, 0f, 165f, 200f, 60f);

            CreateStar("Star1", starsRect, sStar, -65f, 15f, 36f);
            CreateStar("Star2", starsRect, sStar, 0f, 40f, 44f);
            CreateStar("Star3", starsRect, sStar, 65f, 15f, 36f);

            // Result Title (pos: 0, -260)
            RTLTextMeshPro resultTitle = CreateLabel("ResultTitle", modalRect, "برنده شدی!", 72, new Color32(109, 62, 12, 255), font);
            SetAnchored(resultTitle.rectTransform, 0f, -260f, 640f, 95f);

            // Orange Return Home Button (pos: 0, -435, size: 520, 130)
            GameObject buttonObj = new GameObject("ReturnHomeButton", typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObj.transform.SetParent(modalRect, false);
            RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
            SetAnchored(buttonRect, 0f, -435f, 520f, 130f);
            Image buttonImg = buttonObj.GetComponent<Image>();
            buttonImg.sprite = sButtonBg;
            buttonImg.type = Image.Type.Simple;
            buttonImg.raycastTarget = true;
            Button returnBtn = buttonObj.GetComponent<Button>();
            returnBtn.transition = Selectable.Transition.ColorTint;

            RTLTextMeshPro returnLabel = CreateLabel("Label", buttonRect, "بازگشت به خانه", 46, Color.white, font);
            Stretch(returnLabel.rectTransform);

            // AudioSource
            AudioSource aSrc = visualObj.AddComponent<AudioSource>();
            aSrc.playOnAwake = false;

            // GameResultPresentation Component
            GameResultPresentation presentation = visualObj.AddComponent<GameResultPresentation>();
            
            SerializedObject so = new SerializedObject(presentation);
            SetObjectProp(so, "backdropGroup", backdropCg);
            SetObjectProp(so, "modalCard", modalRect);
            SetObjectProp(so, "cardBackground", cardBgImg);
            SetObjectProp(so, "player1Avatar", p1AvatarImg);
            SetObjectProp(so, "player2Avatar", p2AvatarImg);
            SetObjectProp(so, "player1Name", p1Name);
            SetObjectProp(so, "player2Name", p2Name);
            SetObjectProp(so, "player1Score", p1Score);
            SetObjectProp(so, "player2Score", p2Score);
            SetObjectProp(so, "badgeR", badgeImg);
            SetObjectProp(so, "shadowFloor", shadowImg);
            SetObjectProp(so, "winContainer", winRect);
            SetObjectProp(so, "goldenGlowAura", glowImg);
            SetObjectProp(so, "winTrophy", trophyImg);
            SetObjectProp(so, "loseContainer", loseRect);
            SetObjectProp(so, "loseDice", diceImg);
            SetObjectProp(so, "loseStars", starsRect);
            SetObjectProp(so, "resultTitle", resultTitle);
            SetObjectProp(so, "returnHomeButton", returnBtn);
            SetObjectProp(so, "returnHomeLabel", returnLabel);
            SetObjectProp(so, "winFanfareClip", winAudio);
            SetObjectProp(so, "loseSoundClip", loseAudio);
            SetObjectProp(so, "audioSource", aSrc);
            so.ApplyModifiedPropertiesWithoutUndo();

            // Wire to ActionEndGame
            SerializedObject endSo = new SerializedObject(end);
            SetObjectProp(endSo, "ResultPresentation", presentation);
            SetObjectProp(endSo, "ResultText", resultTitle);
            SetObjectProp(endSo, "ScoreMe", p1Score);
            SetObjectProp(endSo, "ScoreOpp", p2Score);
            SetObjectProp(endSo, "NameOpp", p2Name);
            SetObjectProp(endSo, "BackToHome", returnBtn);
            endSo.ApplyModifiedPropertiesWithoutUndo();

            // Make sure visualObj is active, and ResultPanel is inactive by default until end of game
            visualObj.SetActive(true);
            resultPanel.SetActive(false);

            EditorUtility.SetDirty(end);
            EditorUtility.SetDirty(resultPanel);
        }

        private static RTLTextMeshPro CreateLabel(string name, Transform parent, string text, float size, Color color, TMP_FontAsset font)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(RTLTextMeshPro));
            go.transform.SetParent(parent, false);
            RTLTextMeshPro tmp = go.GetComponent<RTLTextMeshPro>();
            if (font != null) tmp.font = font;
            tmp.text = text;
            tmp.fontSize = size;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = size * 0.6f;
            tmp.fontSizeMax = size * 1.05f;
            return tmp;
        }

        private static void CreateStar(string name, Transform parent, Sprite sprite, float x, float y, float size)
        {
            GameObject starObj = new GameObject(name, typeof(RectTransform), typeof(Image));
            starObj.transform.SetParent(parent, false);
            RectTransform r = starObj.GetComponent<RectTransform>();
            SetAnchored(r, x, y, size, size);
            Image img = starObj.GetComponent<Image>();
            img.sprite = sprite;
            img.preserveAspect = true;
            img.raycastTarget = false;
        }

        private static void SetAnchored(RectTransform r, float x, float y, float w, float h)
        {
            r.anchorMin = new Vector2(0.5f, 0.5f);
            r.anchorMax = new Vector2(0.5f, 0.5f);
            r.pivot = new Vector2(0.5f, 0.5f);
            r.anchoredPosition = new Vector2(x, y);
            r.sizeDelta = new Vector2(w, h);
        }

        private static void Stretch(RectTransform r, float padding = 0f)
        {
            r.anchorMin = Vector2.zero;
            r.anchorMax = Vector2.one;
            r.offsetMin = new Vector2(padding, padding);
            r.offsetMax = new Vector2(-padding, -padding);
        }

        private static Sprite LoadSprite(string path, string subSpriteName = null)
        {
            if (string.IsNullOrEmpty(subSpriteName))
            {
                Sprite direct = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (direct != null) return direct;
            }

            var all = AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (var o in all)
            {
                if (o is Sprite s)
                {
                    if (string.IsNullOrEmpty(subSpriteName) || s.name == subSpriteName)
                        return s;
                }
            }
            return null;
        }

        private static void SetObjectProp(SerializedObject so, string propName, Object value)
        {
            var p = so.FindProperty(propName);
            if (p != null) p.objectReferenceValue = value;
            else Debug.LogWarning($"[EndGameFigmaAuthoring] Property '{propName}' not found on {so.targetObject.GetType().Name}");
        }
    }
}
