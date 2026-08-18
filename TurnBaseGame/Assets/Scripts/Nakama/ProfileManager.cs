using System;
using System.Collections;
using DG.Tweening;
using RTLTMPro;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Nakama.Helpers
{
    [Serializable]
    public class ProfileData
    {
        public string displayName;
        public string email;
        public string phone;
        public bool emailLocked;
        public bool phoneLocked;
        public string avatarId;   // returned by GetProfileRpc
    }

    [Serializable]
    public class UpdateProfileResult
    {
        public string displayName;
        public string email;
        public string phone;
        public bool emailLocked;
        public bool phoneLocked;
        public int coinsAwarded;
        public string error;
    }

    [Serializable]
    internal class UpdateProfilePayload
    {
        public string displayName;
        public string email;
        public string phone;
    }

    public class ProfileManager : MonoBehaviour
    {
        private const string GetProfileRpcId = "GetProfileRpc";
        private const string UpdateProfileRpcId = "UpdateProfileRpc";

        #region INSPECTOR

        [Header("Input Fields")]
        [SerializeField] private TMP_InputField displayNameInput;
        [SerializeField] private TMP_InputField emailInput;
        [SerializeField] private TMP_InputField phoneInput;

        [Header("Lock Icons")]
        [SerializeField] private GameObject emailLockIcon;
        [SerializeField] private GameObject phoneLockIcon;

        [Header("Avatar")]
        [SerializeField] private UnityEngine.UI.Image avatarImage;  // shows current avatar sprite
        [SerializeField] private Button avatarButton; // opens the popup
        [SerializeField] private AvatarPopupManager avatarPopupManager;
        // AvatarPopupManager is a Singleton — no Inspector reference needed

        [Header("Buttons & Feedback")]
        [SerializeField] private Button saveButton;
        [SerializeField] private RTLTextMeshPro statusText;

        [Header("Email Login Setup (optional)")]
        [SerializeField] private TMP_InputField passwordInput;
        [SerializeField] private Button linkEmailButton;
        [SerializeField] private RTLTextMeshPro linkEmailStatus;

        [Header("Coin Bonus Popup (optional)")]
        [SerializeField] private RTLTextMeshPro coinBonusPopup;
        [SerializeField] private RectTransform coinBonusRect;
        [SerializeField] private RTLTextMeshPro displayName; // for refreshing coin display after bonus
        [SerializeField] private RTLTextMeshPro infoPrizeSaveEmail;

        #endregion

        #region UNITY

        private void Awake()
        {
            if (saveButton != null) saveButton.onClick.AddListener(OnSaveClicked);
            if (avatarButton != null) avatarButton.onClick.AddListener(OnAvatarButtonClicked);
            if (linkEmailButton != null && linkEmailButton != saveButton)
                linkEmailButton.onClick.AddListener(OnLinkEmailClicked);
        }

        private void OnEnable()
        {
            SetStatus("", Color.white);
            RefreshDisplayNameLabel(null);
            StartCoroutine(WaitAndLoad());

            // Subscribe to avatar changes so the button image stays in sync
            if (ProfileService.Instance != null)
            {
                ProfileService.Instance.onAvatarChanged += OnAvatarChanged;
                // Show current avatar immediately if already loaded
                if (ProfileService.Instance.IsLoaded)
                    RefreshAvatarImage(ProfileService.Instance.CurrentAvatarId);
            }
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            if (ProfileService.Instance != null)
                ProfileService.Instance.onAvatarChanged -= OnAvatarChanged;
        }

        private void OnAvatarChanged(string avatarId)
        {
            RefreshAvatarImage(avatarId);
        }

        private void RefreshAvatarImage(string avatarId)
        {
            if (avatarImage == null) return;
            if (ProfileService.Instance == null) return;
            var sprite = ProfileService.Instance.GetSprite(avatarId);
            if (sprite != null) avatarImage.sprite = sprite;
        }

        private void OnAvatarButtonClicked()
        {
            var popup = AvatarPopupManager.Instance != null
                ? AvatarPopupManager.Instance
                : avatarPopupManager;

            if (popup != null)
                popup.Open();   // NOT SetActive — must call Open() so grid + fade runs
            else
                Debug.LogWarning("[ProfileManager] AvatarPopupManager not found.");
        }

        #endregion

        #region LOAD

        private IEnumerator WaitAndLoad()
        {
            // Wait until Nakama login + account load is complete
            while (NakamaUserManager.Instance == null || !NakamaUserManager.Instance.LoadingFinished)
                yield return null;

            SetSaveInteractable(false);
            SetStatus("در حال بارگذاری...", Color.white);
            LoadProfileAsync();
        }

        private async void LoadProfileAsync()
        {
            try
            {
                var rpc = await NakamaManager.Instance.SendRPC(GetProfileRpcId, "{}");
                if (rpc == null || string.IsNullOrEmpty(rpc.Payload))
                {
                    SetStatus("بارگذاری پروفایل ناموفق بود.", Color.red);
                    SetSaveInteractable(true);
                    return;
                }

                var data = rpc.Payload.Deserialize<ProfileData>();
                if (data != null)
                {
                    ApplyToUI(data);

                    // Use avatarId from server response — most up-to-date source
                    var avatarId = string.IsNullOrEmpty(data.avatarId) ? "avatar_0" : data.avatarId;
                    RefreshAvatarImage(avatarId);

                    // Also sync ProfileService cache so UiManagerHome stays in sync
                    if (ProfileService.Instance != null &&
                        ProfileService.Instance.CurrentAvatarId != avatarId)
                        ProfileService.Instance.NotifyAvatarChanged(avatarId);
                }

                SetStatus("", Color.white);
            }
            catch (Exception e)
            {
                Debug.LogWarning("[ProfileManager] Load error: " + e.Message);
                SetStatus("خطا در بارگذاری پروفایل.", Color.red);
            }
            finally
            {
                SetSaveInteractable(true);
            }
        }

        private void ApplyToUI(ProfileData data)
        {
            if (displayNameInput != null)
                displayNameInput.text = data.displayName ?? "";

            if (emailInput != null)
            {
                emailInput.text = data.email ?? "";
                emailInput.interactable = !data.emailLocked;
            }
            if (emailLockIcon != null) emailLockIcon.SetActive(data.emailLocked);

            if (phoneInput != null)
            {
                phoneInput.text = data.phone ?? "";
                phoneInput.interactable = !data.phoneLocked;
            }
            if (phoneLockIcon != null) phoneLockIcon.SetActive(data.phoneLocked);
            RefreshDisplayNameLabel(data.displayName);
            if (infoPrizeSaveEmail != null)
                infoPrizeSaveEmail.gameObject.SetActive(!data.emailLocked);
        }

        private void RefreshDisplayNameLabel(string rawDisplayName)
        {
            if (displayName == null) return;

            if (ProfileService.Instance != null)
            {
                displayName.text = ProfileService.Instance.ResolveDisplayNameOrUsername(rawDisplayName);
                return;
            }

            if (!string.IsNullOrWhiteSpace(rawDisplayName))
            {
                displayName.text = rawDisplayName.Trim();
                return;
            }

            var userManager = NakamaUserManager.Instance;
            if (userManager != null && userManager.LoadingFinished)
            {
                var user = userManager.User;
                if (user != null)
                {
                    displayName.text = !string.IsNullOrWhiteSpace(user.Username)
                        ? user.Username.Trim()
                        : string.Empty;
                    return;
                }
            }

            displayName.text = string.Empty;
        }

        #endregion

        #region SAVE

        public void OnSaveClicked()
        {
            var name = displayNameInput != null ? displayNameInput.text.Trim() : "";
            var email = emailInput != null ? emailInput.text.Trim() : "";
            var phone = phoneInput != null ? phoneInput.text.Trim() : "";

            // Validation
            if (name.Length == 0 && email.Length == 0 && phone.Length == 0)
            {
                SetStatus("چیزی برای ذخیره وجود ندارد.", Color.yellow);
                return;
            }
            if (email.Length > 0 && !email.Contains("@"))
            {
                SetStatus("آدرس ایمیل نامعتبر است.", Color.red);
                return;
            }
            if (phone.Length > 0 && phone.Length < 7)
            {
                SetStatus("شماره همراه خیلی کوتاه است.", Color.red);
                return;
            }

            SetSaveInteractable(false);
            SetStatus("در حال ذخیره...", Color.white);
            SaveAsync(name, email, phone);
        }

        private async void SaveAsync(string name, string email, string phone)
        {
            try
            {
                var payload = JsonUtility.ToJson(new UpdateProfilePayload
                {
                    displayName = name,
                    email = email,
                    phone = phone,
                });

                Debug.Log("[ProfileManager] Sending: " + payload);

                var rpc = await NakamaManager.Instance.SendRPC(UpdateProfileRpcId, payload);

                if (rpc == null || string.IsNullOrEmpty(rpc.Payload))
                {
                    SetStatus("پاسخی از سرور دریافت نشد.", Color.red);
                    return;
                }

                Debug.Log("[ProfileManager] Response: " + rpc.Payload);

                var result = rpc.Payload.Deserialize<UpdateProfileResult>();
                if (result == null)
                {
                    SetStatus("خطا در خواندن پاسخ سرور.", Color.red);
                    return;
                }

                // Apply updated profile back to UI
                ApplyToUI(new ProfileData
                {
                    displayName = result.displayName,
                    email = result.email,
                    phone = result.phone,
                    emailLocked = result.emailLocked,
                    phoneLocked = result.phoneLocked,
                });

                SetStatus("پروفایل ذخیره شد!", new Color(0.25f, 1f, 0.25f));

                // Coin bonus
                if (result.coinsAwarded > 0)
                {
                    PlayCoinBonus(result.coinsAwarded);
                    if (WalletManager.Instance != null)
                        await WalletManager.Instance.RefreshAsync();
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("[ProfileManager] Save error: " + e.Message);
                SetStatus("ذخیره ناموفق بود.", Color.red);
            }
            finally
            {
                SetSaveInteractable(true);
            }
        }

        #endregion

        #region LINK EMAIL

        private async void OnLinkEmailClicked()
        {
            var email = emailInput != null ? emailInput.text.Trim() : "";
            var password = passwordInput != null ? passwordInput.text : "";

            if (string.IsNullOrEmpty(email) || !email.Contains("@"))
            {
                SetLinkStatus("ایمیل معتبر وارد کنید.", Color.red);
                return;
            }
            if (password.Length < 6)
            {
                SetLinkStatus("رمز عبور باید حداقل ۶ کاراکتر باشد.", Color.red);
                return;
            }

            if (linkEmailButton != null) linkEmailButton.interactable = false;
            SetLinkStatus("در حال تنظیم...", Color.white);

            try
            {
                await NakamaManager.Instance.LinkEmailAsync(email, password);
                SetLinkStatus("ورود با ایمیل فعال شد!", new Color(0.25f, 1f, 0.25f));
                if (passwordInput != null) passwordInput.text = "";
            }
            catch (Exception e)
            {
                if (e.Message.Contains("already") || e.Message.Contains("4"))
                    SetLinkStatus("این ایمیل قبلاً ثبت شده است.", Color.yellow);
                else
                    SetLinkStatus("خطا: لطفاً دوباره امتحان کنید.", Color.red);
                Debug.LogWarning("[ProfileManager] LinkEmail error: " + e.Message);
            }
            finally
            {
                if (linkEmailButton != null) linkEmailButton.interactable = true;
            }
        }

        private void SetLinkStatus(string msg, Color color)
        {
            if (linkEmailStatus == null) return;
            linkEmailStatus.text = msg;
            linkEmailStatus.color = color;
        }

        #endregion

        #region UI HELPERS

        private void SetStatus(string msg, Color color)
        {
            if (statusText == null) return;
            statusText.text = msg;
            statusText.color = color;
        }

        private void SetSaveInteractable(bool on)
        {
            if (saveButton != null) saveButton.interactable = on;
        }

        private void PlayCoinBonus(int amount)
        {
            if (coinBonusPopup == null) return;

            coinBonusPopup.text = "‏+" + PersianTextUtils.FormatNumber(amount) + " کوین!";
            coinBonusPopup.color = new Color(1f, 0.85f, 0.2f, 1f);
            coinBonusPopup.gameObject.SetActive(true);

            Vector2 startPos = coinBonusRect != null
                ? coinBonusRect.anchoredPosition
                : Vector2.zero;

            coinBonusPopup.transform.localScale = Vector3.one * 0.5f;
            DOTween.Kill(coinBonusPopup.transform);

            var seq = DOTween.Sequence();
            seq.Append(coinBonusPopup.transform.DOScale(1.15f, 0.2f).SetEase(Ease.OutBack));
            seq.Append(coinBonusPopup.transform.DOScale(1f, 0.08f));
            if (coinBonusRect != null)
                seq.Join(coinBonusRect.DOAnchorPosY(startPos.y + 70f, 0.8f).SetEase(Ease.OutQuad));
            seq.AppendInterval(0.3f);
            seq.Append(coinBonusPopup.DOFade(0f, 0.3f));
            seq.OnComplete(() =>
            {
                coinBonusPopup.gameObject.SetActive(false);
                if (coinBonusRect != null)
                    coinBonusRect.anchoredPosition = startPos;
            });
        }

        #endregion
    }
}
