using System;
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

            NakamaManager.Instance.onLoginFail += OnLoginFailed;
            NakamaManager.Instance.onLoginSuccess += OnLoginSuccess;

            // if (panel != null) panel.SetActive(false);
        }

        private void OnDestroy()
        {
            if (NakamaManager.Instance == null) return;
            NakamaManager.Instance.onLoginFail -= OnLoginFailed;
            NakamaManager.Instance.onLoginSuccess -= OnLoginSuccess;
        }

        // ── Public ────────────────────────────────────────────────────────────

        public void Show()
        {
            if (panel != null) panel.SetActive(true);
            SetStatus("", Color.white);
            SetInteractable(true);
        }

        public void Hide()
        {
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
                SetStatus("آدرس ایمیل نامعتبر است.", Color.red);
                return;
            }
            if (password.Length < 6)
            {
                SetStatus("رمز عبور باید حداقل ۶ کاراکتر باشد.", Color.red);
                return;
            }

            SetStatus("در حال ورود...", Color.white);
            SetInteractable(false);
            IsWaitingForLogin = true;
            NakamaManager.Instance.LoginWithEmail(email, password);
        }

        private void OnLoginFailed()
        {
            if (!IsWaitingForLogin) return;
            IsWaitingForLogin = false;
            SetStatus("ایمیل یا رمز عبور اشتباه است.", Color.red);
            SetInteractable(true);
        }

        private void OnLoginSuccess()
        {
            if (!IsWaitingForLogin) return;
            IsWaitingForLogin = false;
            SetStatus("ورود موفق!", new Color(0.25f, 1f, 0.25f));
            SetInteractable(false);
            StartCoroutine(GoHomeAfterDelay());
        }

        private IEnumerator GoHomeAfterDelay()
        {
            yield return new WaitForSeconds(0.8f);
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
