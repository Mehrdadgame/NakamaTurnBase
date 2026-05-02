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
    /// Avatar selection popup — Singleton.
    ///
    /// SCENE SETUP:
    ///   1. Panel GameObject must be ACTIVE in the Hierarchy (not inactive).
    ///   2. Add a CanvasGroup to the Panel. In Inspector set:
    ///        Alpha = 0,  Blocks Raycasts = false,  Interactable = false
    ///   3. Assign AvatarLibrary SO in the Inspector field.
    ///   4. Call AvatarPopupManager.Instance.Open() from any button.
    /// </summary>
    public class AvatarPopupManager : MonoBehaviour
    {
        public static AvatarPopupManager Instance { get; private set; }

        private const string SelectAvatarRpcId = "SelectAvatarRpc";

        // ── Inspector ─────────────────────────────────────────────────────────────
        [Header("Popup Root  ← Panel must be ACTIVE, CanvasGroup alpha=0")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform popupRect;

        [Header("Avatar Library  ← assign the SO asset here")]
        [SerializeField] private AvatarLibrary avatarLibrary;

        [Header("Grid")]
        [SerializeField] private Transform gridParent;
        [SerializeField] private GameObject avatarItemPrefab;

        [Header("UI")]
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button confirmButton;
        [SerializeField] private TextMeshProUGUI confirmLabel;

        // ── State ─────────────────────────────────────────────────────────────────
        private AvatarData _pendingAvatar;
        private List<AvatarItemUI> _items = new List<AvatarItemUI>();
        private bool _busy;

        // ── Unity ─────────────────────────────────────────────────────────────────
        private void Awake()
        {
            // Open();
            // Singleton
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            // Wire built-in buttons
            if (closeButton != null) closeButton.onClick.AddListener(Close);
            if (confirmButton != null) confirmButton.onClick.AddListener(OnConfirmClicked);

            // NOTE: We do NOT call HideInstant() here.
            // Set the initial state in the Inspector's CanvasGroup component:
            //   Alpha = 0,  Blocks Raycasts = false,  Interactable = false
        }

        // ── Public API ────────────────────────────────────────────────────────────

        /// <summary>Open the popup and build the avatar grid.</summary>
        public void Open()
        {
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
                    confirmLabel.text = "Select";
                else
                    confirmLabel.text = "Buy & Select  " + serverPrice + " Coin";
            }

            if (confirmButton != null)
                confirmButton.gameObject.SetActive(data.id != currentId);

            SetStatus("", Color.white);
        }

        // ── Confirm ───────────────────────────────────────────────────────────────
        private async void OnConfirmClicked()
        {
            if (_pendingAvatar == null || _busy) return;
            _busy = true;

            SetStatus("Processing...", Color.white);
            if (confirmButton != null) confirmButton.interactable = false;

            try
            {
                var payload = JsonUtility.ToJson(new SelectAvatarPayload { avatarId = _pendingAvatar.id });
                var rpc = await NakamaManager.Instance.SendRPC(SelectAvatarRpcId, payload);

                if (rpc == null || string.IsNullOrEmpty(rpc.Payload))
                { SetStatus("No response from server.", Color.red); return; }

                var result = rpc.Payload.Deserialize<SelectAvatarResult>();
                if (result == null || !result.success)
                { SetStatus(result?.error ?? "Failed.", Color.red); return; }

                // Update cached avatar + owned list → fires events → UiManagerHome + ProfileManager refresh
                if (ProfileService.Instance != null)
                {
                    var newOwned = result.ownedAvatars != null
                        ? new System.Collections.Generic.List<string>(result.ownedAvatars)
                        : null;
                    ProfileService.Instance.NotifyAvatarChanged(result.avatarId, newOwned);
                }

                // Deduct from wallet display
                if (_pendingAvatar.price > 0 && WalletManager.Instance != null)
                    await WalletManager.Instance.RefreshAsync();

                SetStatus("Avatar updated!", new Color(0.25f, 1f, 0.25f));
                BuildGrid();

                DOTween.Sequence()
                    .AppendInterval(0.9f)
                    .AppendCallback(Close);
            }
            catch (Exception e)
            {
                Debug.LogWarning("[AvatarPopup] " + e.Message);
                SetStatus("Error: " + e.Message, Color.red);
            }
            finally
            {
                _busy = false;
                if (confirmButton != null) confirmButton.interactable = true;
            }
        }

        // ── Grid ──────────────────────────────────────────────────────────────────
        private void BuildGrid()
        {
            if (gridParent == null || avatarItemPrefab == null) return;

            var lib = GetLibrary();
            if (lib == null)
            {
                Debug.LogWarning("[AvatarPopup] AvatarLibrary is not assigned. " +
                                 "Assign it in AvatarPopupManager Inspector field.");
                return;
            }

            foreach (Transform child in gridParent) Destroy(child.gameObject);
            _items.Clear();
            _pendingAvatar = null;
            if (confirmButton != null) confirmButton.gameObject.SetActive(false);

            string currentId = ProfileService.Instance != null
                ? ProfileService.Instance.CurrentAvatarId : "avatar_0";

            foreach (var avatar in lib.avatars)
            {
                var go = Instantiate(avatarItemPrefab, gridParent);
                var item = go.GetComponent<AvatarItemUI>();
                if (item == null) continue;
                bool isOwned = avatar.price == 0 ||
                               (ProfileService.Instance != null && ProfileService.Instance.IsOwned(avatar.id));
                item.Init(avatar, this, avatar.id == currentId, isOwned);
                _items.Add(item);
            }
        }

        private AvatarLibrary GetLibrary() =>
            avatarLibrary != null ? avatarLibrary : ProfileService.Instance?.AvatarLibrary;

        // ── Visibility — CanvasGroup only, never touches SetActive ────────────────
        private void ShowAnimated()
        {
            if (canvasGroup == null) { Debug.LogWarning("[AvatarPopup] CanvasGroup is not assigned!"); return; }

            DOTween.Kill(canvasGroup);
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
            canvasGroup.DOFade(1f, 0.22f).SetEase(Ease.OutQuad);

            if (popupRect != null)
            {
                popupRect.localScale = Vector3.one * 0.85f;
                popupRect.DOScale(1f, 0.25f).SetEase(Ease.OutBack);
            }
        }

        private void HideAnimated()
        {
            if (canvasGroup == null) return;
            DOTween.Kill(canvasGroup);
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
            canvasGroup.DOFade(0f, 0.18f).SetEase(Ease.InQuad);
        }

        private void SetStatus(string msg, Color color)
        {
            if (statusText == null) return;
            statusText.text = msg;
            statusText.color = color;
        }
    }
}
