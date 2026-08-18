using System.Linq;
using NinjaBattle.UI;
using RTLTMPro;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace NinjaBattle.UI.Editor
{
    public static class Vertical3x3Authoring
    {
        private const string RootName = "Vertical3x3FigmaUX";
        private const float Width = 1080f;
        private const float Height = 2400f;

        [MenuItem("Tools/NinjaBattle/UI/Build 3x3 Game UX")]
        public static void Build()
        {
            Canvas canvas = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .FirstOrDefault(c => c.gameObject.scene == SceneManager.GetActiveScene());
            if (canvas == null) { Debug.LogError("Vertical3x3Authoring: Canvas not found."); return; }

            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler != null) { scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(Width, Height); scaler.matchWidthOrHeight = .5f; }

            Transform old = canvas.transform.Find(RootName);
            if (old != null) Object.DestroyImmediate(old.gameObject);

            RectTransform root = Rect(RootName, canvas.transform);
            Stretch(root);
            root.SetAsLastSibling();
            BuildBoardBackground(root);
            RebuildLiveCells(root);
            BuildHud(root);
            BuildResultOverlay(canvas);

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Selection.activeGameObject = root.gameObject;
            Debug.Log("3x3 VerticalAndHorizontal Figma UX built.");
        }

        public static void BuildSceneFromCommandLine()
        {
            Scene scene = EditorSceneManager.OpenScene("Assets/-Scenes/4-VerticalAndHorizontal.unity", OpenSceneMode.Single);
            Build();
            EditorSceneManager.SaveScene(scene);
        }

        private static void BuildBoardBackground(RectTransform root)
        {
            Image sky = Panel("Background", root, new Color32(49, 111, 83, 255)); Stretch(sky.rectTransform);
            Image vignette = Panel("TopShade", root, new Color(0.04f, .08f, .08f, .52f)); Set(vignette.rectTransform, 0, 0, Width, 300);

            Image board = Panel("GameBoard", root, new Color32(248, 170, 92, 255)); Set(board.rectTransform, 126, 485, 828, 1470); Round(board.gameObject, 35, new Color32(145, 79, 31, 255));
            Image inner = Panel("GreenField", root, new Color32(77, 143, 50, 255)); Set(inner.rectTransform, 154, 518, 772, 1400); Round(inner.gameObject, 28, new Color32(43, 105, 41, 255));
            CreateGridPlate(root, "OpponentGridPlate", 238, 600);
            CreateGridPlate(root, "PlayerGridPlate", 238, 1338);
        }

        private static void CreateGridPlate(RectTransform root, string name, float x, float y)
        {
            Image plate = Panel(name, root, new Color32(68, 108, 47, 255)); Set(plate.rectTransform, x, y, 604, 604); Round(plate.gameObject, 32, new Color32(28, 74, 35, 255));
        }

        private static void RebuildLiveCells(RectTransform root)
        {
            ClickInCell[] local = Object.FindObjectsByType<ClickInCell>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .Where(c => c.gameObject.scene == SceneManager.GetActiveScene()).OrderBy(c => c.numberLine).ThenBy(c => c.numberRow).Take(9).ToArray();
            TileDataOpp[] opponent = Object.FindObjectsByType<TileDataOpp>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .Where(c => c.gameObject.scene == SceneManager.GetActiveScene()).OrderBy(c => c.line).ThenBy(c => c.row).Take(9).ToArray();

            PlaceCells(opponent, root, 304, 651, false);
            PlaceCells(local, root, 304, 1390, true);
        }

        private static void PlaceCells<T>(T[] cells, RectTransform root, float startX, float startY, bool player) where T : Component
        {
            for (int i = 0; i < cells.Length; i++)
            {
                Component cell = cells[i];
                RectTransform rect = cell.GetComponent<RectTransform>();
                if (rect == null) continue;
                rect.SetParent(root, false);
                int row = i / 3, col = i % 3;
                Set(rect, startX + col * 159, startY + row * 159, 152, 152);
                Image face = cell.GetComponent<Image>();
                if (face == null) face = cell.gameObject.AddComponent<Image>();
                face.sprite = null;
                face.type = Image.Type.Sliced;
                face.color = player ? new Color32(191, 198, 67, 255) : new Color32(177, 190, 68, 255);
                Round(cell.gameObject, 27, new Color32(62, 112, 35, 255));
                face.raycastTarget = player;

                Image dice = cell.GetComponentsInChildren<Image>(true).FirstOrDefault(image => image != face);
                if (dice != null)
                {
                    RectTransform diceRect = dice.rectTransform;
                    diceRect.anchorMin = diceRect.anchorMax = new Vector2(.5f, .5f);
                    diceRect.pivot = new Vector2(.5f, .5f);
                    diceRect.anchoredPosition = Vector2.zero;
                    diceRect.sizeDelta = new Vector2(112, 112);
                }
            }
        }

        private static void BuildHud(RectTransform root)
        {
            CreatePlayerCard(root, "OpponentCard", 72, 132, 326, 136, new Color32(213, 78, 47, 255), "حریف", "۰");
            CreatePlayerCard(root, "PlayerCard", 682, 132, 326, 136, new Color32(54, 126, 202, 255), "شما", "۰");
            Image timer = Panel("TurnTimer", root, new Color32(69, 61, 73, 245)); Set(timer.rectTransform, 475, 70, 130, 130); Round(timer.gameObject, 65, new Color32(255, 211, 119, 255));
            RTLTextMeshPro time = Label("TimerText", timer.transform, "۳۰", 72, Color.white); Stretch(time.rectTransform);

            RectTransform diceButton = ButtonNode("RollDiceButton", root, new Color32(239, 131, 44, 255)); Set(diceButton, 300, 1135, 480, 132); Round(diceButton.gameObject, 32, new Color32(128, 61, 20, 255));
            RTLTextMeshPro rollText = Label("RollText", diceButton, "تاس بزن!", 62, Color.white); Set(rollText.rectTransform, 28, 13, 300, 105);
            Image die = ImageNode("DiceArtwork", diceButton, LoadSprite("Game/Vertical3x3/raw_1")); Set(die.rectTransform, 370, 23, 82, 82);
            Button original = Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None).FirstOrDefault(b => b.gameObject.name == "DieImage");
            if (original != null) UnityEventTools.AddPersistentListener(diceButton.GetComponent<Button>().onClick, original.onClick.Invoke);

            Image footer = Panel("GameFooter", root, new Color32(255, 221, 162, 255)); Set(footer.rectTransform, 94, 2115, 892, 190); Round(footer.gameObject, 45, new Color32(117, 72, 26, 255));
            AddFooterLabel(footer.transform, "فروشگاه", 40); AddFooterLabel(footer.transform, "کارت‌ها", 215); AddFooterLabel(footer.transform, "خانه", 445); AddFooterLabel(footer.transform, "رویدادها", 625); AddFooterLabel(footer.transform, "لیدربرد", 790);
        }

        private static void CreatePlayerCard(RectTransform root, string name, float x, float y, float w, float h, Color color, string title, string score)
        {
            Image card = Panel(name, root, color); Set(card.rectTransform, x, y, w, h); Round(card.gameObject, 30, new Color(0, 0, 0, .3f));
            RTLTextMeshPro player = Label("Name", card.transform, title, 34, Color.white); Set(player.rectTransform, 22, 12, 200, 55);
            RTLTextMeshPro points = Label("Score", card.transform, score, 55, Color.white); Set(points.rectTransform, 230, 18, 72, 90);
        }

        private static void BuildResultOverlay(Canvas canvas)
        {
            ActionEndGame end = Object.FindFirstObjectByType<ActionEndGame>(FindObjectsInactive.Include);
            if (end == null || end.ResultPanel == null) { Debug.LogWarning("Result panel not found; board UI still built."); return; }
            Transform resultRoot = end.ResultPanel.transform;
            Transform old = resultRoot.Find("FigmaResultVisual"); if (old != null) Object.DestroyImmediate(old.gameObject);
            resultRoot.SetAsLastSibling();
            foreach (Transform child in resultRoot) child.gameObject.SetActive(false);

            RectTransform overlay = Rect("FigmaResultVisual", resultRoot); Stretch(overlay);
            Image dim = Panel("Backdrop", overlay, new Color(0, 0, 0, .62f)); Stretch(dim.rectTransform);
            Image modal = Panel("ResultModal", overlay, new Color32(255, 221, 162, 255)); Set(modal.rectTransform, 90, 390, 900, 1650); Round(modal.gameObject, 86, new Color32(160, 101, 46, 255));

            Image meAvatar = ImageNode("PlayerAvatar", modal.transform, LoadSprite("Game/Result/avatar_me")); Set(meAvatar.rectTransform, 112, 118, 260, 260);
            Image oppAvatar = ImageNode("OpponentAvatar", modal.transform, LoadSprite("Game/Result/avatar_opp")); Set(oppAvatar.rectTransform, 530, 118, 260, 260);
            RTLTextMeshPro meName = Label("MeName", modal.transform, "شما", 42, new Color32(74, 46, 8, 255)); Set(meName.rectTransform, 90, 385, 300, 60);
            RTLTextMeshPro oppName = Label("OppName", modal.transform, "حریف", 42, new Color32(74, 46, 8, 255)); Set(oppName.rectTransform, 510, 385, 300, 60);
            RTLTextMeshPro meScore = Label("MeScore", modal.transform, "۰", 66, new Color32(74, 46, 8, 255)); Set(meScore.rectTransform, 130, 450, 220, 82);
            RTLTextMeshPro oppScore = Label("OppScore", modal.transform, "۰", 66, new Color32(74, 46, 8, 255)); Set(oppScore.rectTransform, 550, 450, 220, 82);
            Image trophy = ImageNode("Trophy", modal.transform, LoadSprite("Game/Result/trophy")); Set(trophy.rectTransform, 75, 545, 750, 600);
            RTLTextMeshPro title = Label("ResultTitle", modal.transform, "برنده شدی!", 75, new Color32(119, 72, 11, 255)); Set(title.rectTransform, 105, 1160, 690, 120);
            RectTransform home = ButtonNode("ReturnHome", modal.transform, new Color32(239, 131, 44, 255)); Set(home, 145, 1350, 610, 170); Round(home.gameObject, 36, new Color32(137, 69, 25, 255));
            RTLTextMeshPro homeLabel = Label("Label", home, "بازگشت به خانه", 50, Color.white); Stretch(homeLabel.rectTransform);

            GameResultPresentation presentation = overlay.gameObject.AddComponent<GameResultPresentation>();
            presentation.Configure(title, trophy, modal);
            UnityEventTools.AddPersistentListener(home.GetComponent<Button>().onClick, presentation.ReturnHome);
            end.ResultPresentation = presentation;
            end.ResultText = title; end.ScoreMe = meScore; end.ScoreOpp = oppScore; end.NameOpp = oppName; end.BackToHome = home.GetComponent<Button>();
            overlay.gameObject.SetActive(true);
            end.ResultPanel.SetActive(false);
        }

        private static void AddFooterLabel(Transform parent, string value, float x)
        {
            RTLTextMeshPro label = Label(value, parent, value, 25, new Color32(72, 46, 8, 255)); Set(label.rectTransform, x, 110, 120, 55);
        }

        private static RectTransform Rect(string name, Transform parent) { var go = new GameObject(name, typeof(RectTransform)); go.transform.SetParent(parent, false); return go.GetComponent<RectTransform>(); }
        private static Image Panel(string name, Transform parent, Color color) { Image image = Rect(name, parent).gameObject.AddComponent<Image>(); image.color = color; image.raycastTarget = false; return image; }
        private static RectTransform ButtonNode(string name, Transform parent, Color color) { Image image = Panel(name, parent, color); image.raycastTarget = true; image.gameObject.AddComponent<Button>().transition = Selectable.Transition.ColorTint; return image.rectTransform; }
        private static Image ImageNode(string name, Transform parent, Sprite sprite) { Image image = Panel(name, parent, Color.white); image.sprite = sprite; image.preserveAspect = true; return image; }
        private static RTLTextMeshPro Label(string name, Transform parent, string value, float size, Color color) { RTLTextMeshPro text = Rect(name, parent).gameObject.AddComponent<RTLTextMeshPro>(); text.font = TMP_Settings.defaultFontAsset; text.text = value; text.fontSize = size; text.color = color; text.alignment = TextAlignmentOptions.Center; text.raycastTarget = false; text.enableAutoSizing = true; text.fontSizeMin = size * .55f; text.fontSizeMax = size; return text; }
        private static void Set(RectTransform r, float x, float y, float w, float h) { r.anchorMin = new Vector2(0, 1); r.anchorMax = new Vector2(0, 1); r.pivot = new Vector2(0, 1); r.anchoredPosition = new Vector2(x, -y); r.sizeDelta = new Vector2(w, h); }
        private static void Stretch(RectTransform r) { r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.offsetMin = Vector2.zero; r.offsetMax = Vector2.zero; }
        private static void Round(GameObject go, float radius, Color outline) { Outline edge = go.GetComponent<Outline>() ?? go.AddComponent<Outline>(); edge.effectColor = outline; edge.effectDistance = new Vector2(0, -5); Shadow shade = go.GetComponent<Shadow>() ?? go.AddComponent<Shadow>(); shade.effectColor = new Color(0, 0, 0, .27f); shade.effectDistance = new Vector2(0, -7); }
        private static Sprite LoadSprite(string resourcePath) => Resources.Load<Sprite>(resourcePath);
    }
}
