using System.Collections;
using RTLTMPro;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Nakama.Helpers
{
    /// <summary>
    /// پنل لاگین با ایمیل و رمز عبور.
    ///
    /// Inspector wiring:
    ///   panel         — root GameObject پنل (Show/Hide)
    ///   emailInput    — فیلد ایمیل
    ///   passwordInput — فیلد رمز عبور (ContentType = Password)
    ///   loginButton   — دکمه ورود
    ///   backButton    — دکمه بازگشت (اختیاری)
    ///   statusText    — نمایش خطا / وضعیت
    /// </summary>
    public class EmailLoginPanel : MonoBehaviour
    {
        public static EmailLoginPanel Instance { get; private set; }

        [Header("UI")]
        [SerializeField] private GameObject panel;
        [SerializeField] private TMP_InputField emailInput;
        [SerializeField] private TMP_InputField passwordInput;
        [SerializeField] private Button loginButton;
        [SerializeField] private Button backButton;
        [SerializeField] private RTLTextMeshPro statusText;

        public  bool IsWaitingForLogin { get; private set; }

        // ── Unity ─────────────────────────────────────────────────────────────

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            if (loginButton != null) loginButton.onClick.AddListener(OnLoginClicked);
            if (backButton != null) backButton.onClick.AddListener(Hide);

            if (NakamaManager.Instance != null)
            {
                NakamaManager.Instance.onLoginFail += OnLoginFailed;
                NakamaManager.Instance.onLoginSuccess += OnLoginSuccess;
            }
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
            if (NakamaManager.Instance != null)
            {
                NakamaManager.Instance.onLoginFail -= OnLoginFailed;
                NakamaManager.Instance.onLoginSuccess -= OnLoginSuccess;
            }
        }

        // ── Public ────────────────────────────────────────────────────────────

        public void Show()
        {
            if (panel != null) panel.SetActive(true);
            if (passwordInput != null) passwordInput.text = "";
            SetStatus("", Color.white);
            SetInteractable(true);
            emailInput?.ActivateInputField();
        }

        public void Hide()
        {
            if (passwordInput != null) passwordInput.text = "";
            if (panel != null) panel.SetActive(false);
            IsWaitingForLogin = false;
        }

        // ── Handlers ──────────────────────────────────────────────────────────

        private void OnLoginClicked()
        {
            var email = emailInput != null ? emailInput.text.Trim() : "";
            var password = passwordInput != null ? passwordInput.text : "";

            if (string.IsNullOrEmpty(email) || !email.Contains("@"))
            {
                SetStatus(Localization.L("آدرس ایمیل نامعتبر است.", "Invalid email address."), Color.red);
                return;
            }
            if (password.Length < 6)
            {
                SetStatus(Localization.L("رمز عبور باید حداقل ۶ کاراکتر باشد.", "Password must be at least 6 characters."), Color.red);
                return;
            }

            if (NakamaManager.Instance == null)
            {
                SetStatus(Localization.L("سرویس ورود در دسترس نیست.", "Login service unavailable."), Color.red);
                return;
            }

            SetStatus(Localization.L("در حال ورود...", "Signing in..."), Color.white);
            SetInteractable(false);
            IsWaitingForLogin = true;
            NakamaManager.Instance.LoginWithEmail(email, password);
        }

        private void OnLoginFailed()
        {
            if (!IsWaitingForLogin) return;
            IsWaitingForLogin = false;
            SetStatus(Localization.L("ایمیل یا رمز عبور اشتباه است.", "Wrong email or password."), Color.red);
            SetInteractable(true);
        }

        private void OnLoginSuccess()
        {
            if (!IsWaitingForLogin) return;
            IsWaitingForLogin = false;
            SetStatus(Localization.L("ورود موفق!", "Signed in!"), new Color(0.25f, 1f, 0.25f));
            SetInteractable(false);
            StartCoroutine(ReloadHomeAfterDelay());
        }

        private IEnumerator ReloadHomeAfterDelay()
        {
            yield return new WaitForSecondsRealtime(0.8f);
            SceneManager.LoadScene((int)NinjaBattle.General.Scenes.Home);
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private void SetStatus(string msg, Color color)
        {
            if (statusText == null) return;
            statusText.text = msg;
            statusText.color = color;
        }

        private void SetInteractable(bool on)
        {
            if (loginButton != null) loginButton.interactable = on;
        }
    }
}
