using System.Linq;
using Nakama.Helpers;
using RTLTMPro;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;

namespace NinjaBattle.UI.Editor
{
    public static class HomeProfileShopAuthoring
    {
        private const float DesignWidth = 1080f;
        private const float DesignHeight = 2400f;
        private static readonly Color Cream = new Color32(255, 220, 161, 255);
        private static readonly Color CreamDark = new Color32(242, 198, 126, 255);
        private static readonly Color Brown = new Color32(72, 46, 8, 255);
        private static readonly Color Orange = new Color32(238, 113, 28, 255);

        public static void Build(Transform canvas, Transform profilePanel, Transform shopPanel,
            FigmaHomeController controller)
        {
            if (profilePanel != null)
                BuildProfile(profilePanel, controller);
            if (shopPanel != null)
                BuildShop(shopPanel);
        }

        private static void BuildProfile(Transform profilePanel, FigmaHomeController controller)
        {
            ProfileManager manager = Object.FindObjectsByType<ProfileManager>(
                    FindObjectsInactive.Include, FindObjectsSortMode.None)
                .FirstOrDefault(item => item.gameObject.scene == profilePanel.gameObject.scene);
            if (manager == null)
                return;

            SerializedObject serialized = new SerializedObject(manager);
            TMP_InputField displayName = GetReference<TMP_InputField>(serialized, "displayNameInput");
            TMP_InputField email = GetReference<TMP_InputField>(serialized, "emailInput");
            TMP_InputField phone = GetReference<TMP_InputField>(serialized, "phoneInput");
            TMP_InputField password = GetReference<TMP_InputField>(serialized, "passwordInput");
            Button avatarButton = GetReference<Button>(serialized, "avatarButton");
            Button saveButton = GetReference<Button>(serialized, "saveButton");
            AvatarPopupManager avatarPopup = GetReference<AvatarPopupManager>(serialized, "avatarPopupManager");

            Transform oldLayout = profilePanel.Find("FigmaProfileLayout");
            if (oldLayout != null)
            {
                ReparentIfInside(displayName, oldLayout, profilePanel);
                ReparentIfInside(email, oldLayout, profilePanel);
                ReparentIfInside(phone, oldLayout, profilePanel);
                ReparentIfInside(password, oldLayout, profilePanel);
                ReparentIfInside(avatarButton, oldLayout, profilePanel);
                ReparentIfInside(saveButton, oldLayout, profilePanel);
                Undo.DestroyObjectImmediate(oldLayout.gameObject);
            }

            Image panelImage = profilePanel.GetComponent<Image>();
            if (panelImage != null)
            {
                panelImage.sprite = LoadSprite("Assets/Resources/Home/Parts/background.png");
                panelImage.color = Color.white;
                panelImage.type = Image.Type.Simple;
            }

            RectTransform layout = ChatUiFactory.Rect("FigmaProfileLayout", profilePanel);
            Stretch(layout);
            Image dim = ChatUiFactory.Panel("BackdropDim", layout, new Color(0.08f, 0.04f, 0.01f, 0.42f));
            Stretch(dim.rectTransform);
            dim.raycastTarget = false;

            Image modal = ChatUiFactory.Panel("ProfileCard", layout, Cream);
            SetFigmaRect(modal.rectTransform, 105, 315, 870, 1720);
            AddDepth(modal.gameObject);

            RTLTextMeshPro title = CreateText("Title", modal.transform, "پروفایل", 64, Brown);
            SetTopLeft(title.rectTransform, 235, 55, 400, 100);

            Button closeButton = CreateButton("CloseButton", layout, Cream, "×", 54);
            SetFigmaRect(closeButton.GetComponent<RectTransform>(), 875, 250, 105, 105);
            UnityEventTools.AddPersistentListener(closeButton.onClick, controller.CloseProfile);

            PlaceAvatar(avatarButton, modal.transform);
            PlaceInput(displayName, modal.transform, 760, "نام نمایشی");
            PlaceInput(email, modal.transform, 910, "ایمیل");
            PlaceInput(phone, modal.transform, 1060, "شماره همراه");
            PlaceInput(password, modal.transform, 1210, "رمز ورود");

            if (saveButton != null)
            {
                saveButton.transform.SetParent(modal.transform, false);
                SetTopLeft((RectTransform)saveButton.transform, 185, 1410, 500, 125);
                StylePrimaryButton(saveButton, "ذخیره");
            }

            RTLTextMeshPro status = CreateText("ProfileStatus", modal.transform, "", 30, Brown);
            SetTopLeft(status.rectTransform, 90, 1540, 690, 70);
            SetReference(serialized, "statusText", status);

            Button linkButton = CreateButton("LinkEmailButton", modal.transform, CreamDark,
                "فعال‌سازی ورود با ایمیل", 30);
            SetTopLeft((RectTransform)linkButton.transform, 185, 1620, 500, 100);
            SetReference(serialized, "linkEmailButton", linkButton);

            RTLTextMeshPro linkStatus = CreateText("LinkEmailStatus", modal.transform, "", 26, Brown);
            SetTopLeft(linkStatus.rectTransform, 90, 1720, 690, 60);
            SetReference(serialized, "linkEmailStatus", linkStatus);

            if (avatarPopup != null)
                StyleAvatarPopup(avatarPopup);

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(manager);
            layout.SetAsLastSibling();
            if (avatarPopup != null)
                avatarPopup.transform.SetAsLastSibling();
        }

        private static void BuildShop(Transform shopPanel)
        {
            Transform oldChrome = shopPanel.Find("FigmaShopChrome");
            if (oldChrome != null)
                Undo.DestroyObjectImmediate(oldChrome.gameObject);
            Transform oldDynamic = shopPanel.Find("FigmaShopDynamic");
            if (oldDynamic != null)
                Undo.DestroyObjectImmediate(oldDynamic.gameObject);

            RectTransform items = shopPanel.Find("Item shops") as RectTransform;
            if (items != null)
            {
                SetFigmaRect(items, 178, 620, 724, 1160);
                GridLayoutGroup grid = items.GetComponent<GridLayoutGroup>();
                if (grid != null)
                {
                    grid.cellSize = new Vector2(322, 251);
                    grid.spacing = new Vector2(80, 40);
                    grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                    grid.constraintCount = 2;
                }
            }

            RectTransform dynamicLayer = ChatUiFactory.Rect("FigmaShopDynamic", shopPanel);
            Stretch(dynamicLayer);
            dynamicLayer.SetAsLastSibling();
            Image valueMask = ChatUiFactory.Panel("CoinValueMask", dynamicLayer, Cream);
            SetFigmaRect(valueMask.rectTransform, 770, 122, 112, 91);
            valueMask.raycastTarget = false;
            RTLTextMeshPro balance = CreateText("WalletValue", valueMask.transform, "۰", 47, Brown);
            Stretch(balance.rectTransform);
            WalletBalanceView walletView = valueMask.gameObject.AddComponent<WalletBalanceView>();
            walletView.Configure(balance);

            CoinShopManager manager = shopPanel.GetComponent<CoinShopManager>();
            if (manager == null)
                return;

            SerializedObject serialized = new SerializedObject(manager);
            SerializedProperty products = serialized.FindProperty("products");
            if (products != null)
            {
                for (int i = 0; i < products.arraySize; i++)
                {
                    SerializedProperty product = products.GetArrayElementAtIndex(i);
                    Button buy = product.FindPropertyRelative("buyButton").objectReferenceValue as Button;
                    RTLTextMeshPro coins = product.FindPropertyRelative("coinsLabel").objectReferenceValue as RTLTextMeshPro;
                    RTLTextMeshPro price = product.FindPropertyRelative("priceText").objectReferenceValue as RTLTextMeshPro;
                    RestoreProductCard(buy, coins, price);
                    if (buy != null && buy.transform.parent == items)
                        buy.transform.SetSiblingIndex(i);
                }
            }

            Transform loadingOverlay = shopPanel.Find("ShopLoadingOverlay");
            if (loadingOverlay != null)
                loadingOverlay.SetAsLastSibling();

            EditorUtility.SetDirty(manager);
        }

        private static void StyleAvatarPopup(AvatarPopupManager popup)
        {
            SerializedObject serialized = new SerializedObject(popup);
            RectTransform popupRect = GetReference<RectTransform>(serialized, "popupRect");
            Transform gridParent = GetReference<Transform>(serialized, "gridParent");
            Button close = GetReference<Button>(serialized, "closeButton");
            Button confirm = GetReference<Button>(serialized, "confirmButton");

            if (popupRect != null)
            {
                SetFigmaRect(popupRect, 85, 360, 910, 1600);
                Image image = popupRect.GetComponent<Image>();
                if (image != null)
                    image.color = Cream;
                AddDepthIfMissing(popupRect.gameObject);
            }

            if (gridParent is RectTransform gridRect)
            {
                SetTopLeft(gridRect, 70, 220, 770, 900);
                GridLayoutGroup grid = gridRect.GetComponent<GridLayoutGroup>();
                if (grid != null)
                {
                    grid.cellSize = new Vector2(230, 250);
                    grid.spacing = new Vector2(38, 34);
                    grid.padding = new RectOffset(0, 0, 0, 0);
                    grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                    grid.constraintCount = 3;
                }
            }

            if (close != null)
                StyleSecondaryButton(close, "×");
            if (confirm != null)
                StylePrimaryButton(confirm, "انتخاب");

            TextMeshProUGUI currentStatus = GetReference<TextMeshProUGUI>(serialized, "statusText");
            TextMeshProUGUI confirmLabel = GetReference<TextMeshProUGUI>(serialized, "confirmLabel");
            if (popupRect != null && (currentStatus == null || currentStatus == confirmLabel))
            {
                TextMeshProUGUI status = CreateTmpText("AvatarStatus", popupRect, "", 28, Brown);
                SetTopLeft(status.rectTransform, 70, 1240, 770, 70);
                SetReference(serialized, "statusText", status);
            }

            serialized.FindProperty("maxVisibleAvatars").intValue = 8;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(popup);
        }

        private static void RestoreProductCard(Button button, RTLTextMeshPro coins, RTLTextMeshPro price)
        {
            if (button == null)
                return;

            Image image = button.GetComponent<Image>();
            if (image != null)
            {
                image.sprite = null;
                image.color = Color.clear;
            }
            Shadow shadow = button.GetComponent<Shadow>();
            if (shadow != null)
                Object.DestroyImmediate(shadow);
            button.transition = Selectable.Transition.ColorTint;

            if (coins != null)
            {
                coins.color = Color.white;
                coins.fontSize = 30;
                coins.enableAutoSizing = true;
                coins.fontSizeMin = 20;
                coins.fontSizeMax = 32;
            }
            if (price != null)
            {
                price.color = Color.white;
                price.fontSize = 28;
                price.enableAutoSizing = true;
                price.fontSizeMin = 20;
                price.fontSizeMax = 30;
                Image pricePlate = price.transform.parent.GetComponent<Image>();
                if (pricePlate != null)
                    pricePlate.color = new Color32(216, 54, 38, 255);
                if (price.transform.parent is RectTransform priceRect)
                {
                    priceRect.anchoredPosition = new Vector2(0, -104);
                    priceRect.sizeDelta = new Vector2(285, 58);
                }
            }
        }

        private static void PlaceAvatar(Button avatarButton, Transform parent)
        {
            if (avatarButton == null)
                return;
            avatarButton.transform.SetParent(parent, false);
            SetTopLeft((RectTransform)avatarButton.transform, 300, 190, 270, 270);
            Image image = avatarButton.GetComponent<Image>();
            if (image != null)
                image.preserveAspect = true;
            AddDepthIfMissing(avatarButton.gameObject);
        }

        private static void PlaceInput(TMP_InputField input, Transform parent, float y, string placeholder)
        {
            if (input == null)
                return;
            input.transform.SetParent(parent, false);
            SetTopLeft((RectTransform)input.transform, 90, y, 690, 115);
            Image image = input.GetComponent<Image>();
            if (image != null)
            {
                image.sprite = null;
                image.color = CreamDark;
            }
            input.textComponent.color = Brown;
            input.textComponent.fontSize = 34;
            if (input.placeholder is TMP_Text placeholderText)
            {
                placeholderText.text = placeholder;
                placeholderText.color = new Color(Brown.r, Brown.g, Brown.b, 0.55f);
            }
        }

        private static void StylePrimaryButton(Button button, string label)
        {
            Image image = button.GetComponent<Image>();
            if (image != null)
            {
                image.sprite = null;
                image.color = Orange;
            }
            SetButtonLabel(button, label, Color.white);
            AddDepthIfMissing(button.gameObject);
        }

        private static void StyleSecondaryButton(Button button, string label)
        {
            Image image = button.GetComponent<Image>();
            if (image != null)
            {
                image.sprite = null;
                image.color = Cream;
            }
            SetButtonLabel(button, label, Brown);
        }

        private static void SetButtonLabel(Button button, string value, Color color)
        {
            TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);
            if (label == null)
                return;
            label.text = value;
            label.color = color;
            label.fontSize = Mathf.Max(label.fontSize, 32f);
            label.alignment = TextAlignmentOptions.Center;
        }

        private static Button CreateButton(string name, Transform parent, Color color, string label, int fontSize)
        {
            Image image = ChatUiFactory.Panel(name, parent, color);
            Button button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            RTLTextMeshPro text = CreateText("Label", image.transform, label, fontSize, Brown);
            Stretch(text.rectTransform);
            return button;
        }

        private static RTLTextMeshPro CreateText(string name, Transform parent, string value, int size, Color color)
        {
            RTLTextMeshPro text = ChatUiFactory.Text(name, parent, value, size, color, TextAlignmentOptions.Center);
            text.fontStyle = FontStyles.Bold;
            text.raycastTarget = false;
            text.PreserveNumbers = true;
            return text;
        }

        private static TextMeshProUGUI CreateTmpText(string name, Transform parent, string value, int size, Color color)
        {
            RectTransform rect = ChatUiFactory.Rect(name, parent);
            TextMeshProUGUI text = rect.gameObject.AddComponent<TextMeshProUGUI>();
            text.text = value;
            text.font = ChatUiFactory.Font;
            text.fontSize = size;
            text.color = color;
            text.alignment = TextAlignmentOptions.Center;
            text.raycastTarget = false;
            return text;
        }

        private static void AddDepth(GameObject target)
        {
            Shadow shadow = target.AddComponent<Shadow>();
            shadow.effectColor = new Color32(83, 45, 10, 180);
            shadow.effectDistance = new Vector2(0, -10);
            Outline outline = target.AddComponent<Outline>();
            outline.effectColor = new Color32(173, 111, 48, 255);
            outline.effectDistance = new Vector2(3, -3);
        }

        private static void AddDepthIfMissing(GameObject target)
        {
            if (target.GetComponent<Shadow>() == null)
            {
                Shadow shadow = target.AddComponent<Shadow>();
                shadow.effectColor = new Color32(112, 68, 22, 155);
                shadow.effectDistance = new Vector2(0, -5);
            }
        }

        private static void ReparentIfInside(Component component, Transform root, Transform fallback)
        {
            if (component != null && component.transform.IsChildOf(root))
                component.transform.SetParent(fallback, false);
        }

        private static T GetReference<T>(SerializedObject serialized, string name) where T : Object
        {
            SerializedProperty property = serialized.FindProperty(name);
            return property != null ? property.objectReferenceValue as T : null;
        }

        private static void SetReference(SerializedObject serialized, string name, Object value)
        {
            SerializedProperty property = serialized.FindProperty(name);
            if (property != null)
                property.objectReferenceValue = value;
        }

        private static Sprite LoadSprite(string path)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static Sprite LoadAtlasSprite(string name)
        {
            string path = "Assets/Art/FigmaItemAtlas/" + name + ".png";
            foreach (Object asset in AssetDatabase.LoadAllAssetsAtPath(path))
                if (asset is Sprite sprite && (sprite.name == name || sprite.name.StartsWith(name + "_")))
                    return sprite;
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

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
