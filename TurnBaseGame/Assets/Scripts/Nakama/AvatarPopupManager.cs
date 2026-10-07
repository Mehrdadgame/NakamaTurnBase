using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Nakama.Helpers
{
    [Serializable]
    internal class SelectAvatarPayload { public string avatarId; }

    [Serializable]
    internal class SelectAvatarResult
    {
        public bool success;
        public string avatarId;
        public string[] ownedAvatars;   // updated owned list from server
        public string error;
    }

    /// <summary>
    /// Avatar selection popup - Singleton.
    /// Handles grid population, scrolling, and server avatar selection.
    /// </summary>
    public class AvatarPopupManager : MonoBehaviour
    {
        public static AvatarPopupManager Instance { get; private set; }

        private const string SelectAvatarRpcId = "SelectAvatarRpc";

        // Inspector -------------------------------------------------------------------
        [Header("Popup Root")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform popupRect;

        [Header("Avatar Library")]
        [SerializeField] private AvatarLibrary avatarLibrary;

        [Header("Grid")]
        [SerializeField] private Transform gridParent;
        [SerializeField] private GameObject avatarItemPrefab;
        [Tooltip("0 = unlimited (shows all avatars from AvatarLibrary)")]
        [SerializeField] private int maxVisibleAvatars = 0;

        [Header("UI")]
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button confirmButton;
        [SerializeField] private TextMeshProUGUI confirmLabel;

        // State -----------------------------------------------------------------------
        private AvatarData _pendingAvatar;
        private List<AvatarItemUI> _items = new List<AvatarItemUI>();
        private bool _busy;

        // Unity -----------------------------------------------------------------------
        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();

            // Wire built-in buttons
            if (closeButton != null) closeButton.onClick.AddListener(Close);
            if (confirmButton != null) confirmButton.onClick.AddListener(OnConfirmClicked);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
            if (canvasGroup != null) DOTween.Kill(canvasGroup);
            if (popupRect != null) DOTween.Kill(popupRect);
        }

        // Public API ------------------------------------------------------------------

        /// <summary>Open the popup, activate game object, bring to front, and build the avatar grid.</summary>
        public void Open()
        {
            // Ensure game object is active in hierarchy and in front of all siblings
            gameObject.SetActive(true);
            transform.SetAsLastSibling();

            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();

            BuildGrid();
            SetStatus("", Color.white);
            ShowAnimated();
        }

        /// <summary>Close the popup.</summary>
        public void Close()
        {
            HideAnimated();
        }

        /// <summary>Called by AvatarItemUI when a cell is tapped.</summary>
        public void OnAvatarItemClicked(AvatarData data)
        {
            if (_busy) return;
            _pendingAvatar = data;

            string currentId = ProfileService.Instance != null
                ? ProfileService.Instance.CurrentAvatarId : "avatar_0";

            // Refresh selection rings
            var lib = GetLibrary();
            for (int i = 0; i < _items.Count; i++)
            {
                if (lib == null || i >= lib.avatars.Count) break;
                _items[i].SetSelected(lib.avatars[i].id == data.id);
            }

            // Confirm button label & visibility
            int serverPrice = ProfileService.Instance != null
                ? ProfileService.Instance.GetPrice(data.id)
                : data.price;
            bool owned = serverPrice == 0 ||
                         (ProfileService.Instance != null && ProfileService.Instance.IsOwned(data.id));
            if (confirmLabel != null)
            {
                if (serverPrice == 0 || owned)
                    confirmLabel.text = Localization.L("انتخاب", "Select");
                else
                    confirmLabel.text = Localization.IsPersian
                    ? "خرید و انتخاب  " + PersianTextUtils.FormatNumber(serverPrice) + " تاسی"
                    : "Buy & Select  " + PersianTextUtils.FormatNumber(serverPrice) + " coins";
            }

            if (confirmButton != null)
            {
                confirmButton.gameObject.SetActive(data.id != currentId);
                confirmButton.transform.DOKill();
                confirmButton.transform.localScale = Vector3.one * 0.9f;
                confirmButton.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack).SetUpdate(true);
            }

            SetStatus("", Color.white);
        }

        // Confirm ---------------------------------------------------------------------
        private async void OnConfirmClicked()
        {
            if (_pendingAvatar == null || _busy) return;
            _busy = true;

            SetStatus(Localization.L("در حال پردازش...", "Processing..."), Color.white);
            if (confirmButton != null) confirmButton.interactable = false;

            try
            {
                var payload = JsonUtility.ToJson(new SelectAvatarPayload { avatarId = _pendingAvatar.id });
                var rpc = await NakamaManager.Instance.SendRPC(SelectAvatarRpcId, payload);

                if (rpc == null || string.IsNullOrEmpty(rpc.Payload))
                { SetStatus(Localization.L("پاسخی از سرور دریافت نشد.", "No response from server."), Color.red); return; }

                var result = rpc.Payload.Deserialize<SelectAvatarResult>();
                if (result == null || !result.success)
                { SetStatus(!string.IsNullOrEmpty(result?.error) ? result.error : Localization.L("عملیات ناموفق بود.", "Operation failed."), Color.red); return; }

                // Update cached avatar + owned list
                if (ProfileService.Instance != null)
                {
                    var newOwned = result.ownedAvatars != null
                        ? new System.Collections.Generic.List<string>(result.ownedAvatars)
                        : null;
                    ProfileService.Instance.NotifyAvatarChanged(result.avatarId, newOwned);
                }

                int selectedPrice = ProfileService.Instance != null
                    ? ProfileService.Instance.GetPrice(_pendingAvatar.id)
                    : _pendingAvatar.price;
                if (selectedPrice > 0 && WalletManager.Instance != null)
                    await WalletManager.Instance.RefreshAsync();

                SetStatus(Localization.L("آواتار به‌روزرسانی شد!", "Avatar updated!"), new Color(0.25f, 1f, 0.25f));
                BuildGrid();

                DOTween.Sequence()
                    .AppendInterval(0.75f)
                    .AppendCallback(Close)
                    .SetUpdate(true);
            }
            catch (Exception e)
            {
                Debug.LogWarning("[AvatarPopup] " + e.Message);
                SetStatus(Localization.L("خطا: ", "Error: ") + e.Message, Color.red);
            }
            finally
            {
                _busy = false;
                if (confirmButton != null) confirmButton.interactable = true;
            }
        }

        // Grid ------------------------------------------------------------------------
        private void BuildGrid()
        {
            if (gridParent == null || avatarItemPrefab == null) return;

            var lib = GetLibrary();
            if (lib == null)
            {
                Debug.LogWarning("[AvatarPopup] AvatarLibrary is not assigned.");
                return;
            }

            foreach (Transform child in gridParent) Destroy(child.gameObject);
            _items.Clear();
            _pendingAvatar = null;
            if (confirmButton != null) confirmButton.gameObject.SetActive(false);

            string currentId = ProfileService.Instance != null
                ? ProfileService.Instance.CurrentAvatarId : "avatar_0";

            // If maxVisibleAvatars <= 0, show ALL avatars in the library
            int visibleCount = (maxVisibleAvatars > 0)
                ? Mathf.Min(maxVisibleAvatars, lib.avatars.Count)
                : lib.avatars.Count;

            // Dynamically adjust content height for scrolling through all avatars
            if (gridParent is RectTransform gridRect)
            {
                var grid = gridRect.GetComponent<GridLayoutGroup>();
                if (grid != null)
                {
                    int cols = Mathf.Max(1, grid.constraintCount);
                    int rows = Mathf.CeilToInt((float)visibleCount / cols);
                    float totalHeight = grid.padding.top + grid.padding.bottom +
                                        rows * grid.cellSize.y +
                                        Mathf.Max(0, rows - 1) * grid.spacing.y + 40f;
                    gridRect.sizeDelta = new Vector2(gridRect.sizeDelta.x, Mathf.Max(totalHeight, 900f));
                }
            }

            // Reset scroll position to top
            var scrollRect = GetComponentInChildren<ScrollRect>(true);
            if (scrollRect != null)
                scrollRect.verticalNormalizedPosition = 1f;

            for (int index = 0; index < visibleCount; index++)
            {
                var avatar = lib.avatars[index];
                var go = Instantiate(avatarItemPrefab, gridParent);
                var item = go.GetComponent<AvatarItemUI>();
                if (item == null) continue;
                int serverPrice = ProfileService.Instance != null
                    ? ProfileService.Instance.GetPrice(avatar.id)
                    : avatar.price;
                bool isOwned = serverPrice == 0 ||
                               (ProfileService.Instance != null && ProfileService.Instance.IsOwned(avatar.id));
                item.Init(avatar, this, avatar.id == currentId, isOwned);
                _items.Add(item);

                // Smooth entrance pop animation for polish
                go.transform.localScale = Vector3.one * 0.82f;
                go.transform.DOScale(Vector3.one, 0.22f)
                    .SetEase(Ease.OutBack)
                    .SetDelay(Mathf.Min(index * 0.02f, 0.35f))
                    .SetUpdate(true);
            }
        }

        private AvatarLibrary GetLibrary() =>
            avatarLibrary != null ? avatarLibrary : ProfileService.Instance?.AvatarLibrary;

        // Visibility ------------------------------------------------------------------
        private void ShowAnimated()
        {
            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                DOTween.Kill(canvasGroup);
                canvasGroup.blocksRaycasts = true;
                canvasGroup.interactable = true;
                canvasGroup.DOFade(1f, 0.22f).SetEase(Ease.OutQuad).SetUpdate(true);
            }

            if (popupRect != null)
            {
                DOTween.Kill(popupRect);
                popupRect.localScale = Vector3.one * 0.85f;
                popupRect.DOScale(1f, 0.25f).SetEase(Ease.OutBack).SetUpdate(true);
            }
        }

        private void HideAnimated()
        {
            if (canvasGroup != null)
            {
                DOTween.Kill(canvasGroup);
                canvasGroup.blocksRaycasts = false;
                canvasGroup.interactable = false;
                canvasGroup.DOFade(0f, 0.18f).SetEase(Ease.InQuad).SetUpdate(true);
            }

            if (popupRect != null)
            {
                DOTween.Kill(popupRect);
                popupRect.DOScale(Vector3.one * 0.88f, 0.18f).SetEase(Ease.InQuad).SetUpdate(true);
            }
        }

        private void SetStatus(string msg, Color color)
        {
            if (statusText == null) return;
            statusText.text = msg;
            statusText.color = color;
        }
    }
}