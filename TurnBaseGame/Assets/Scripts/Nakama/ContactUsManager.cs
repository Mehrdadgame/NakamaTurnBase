using System;
using System.Collections;
using DG.Tweening;
using RTLTMPro;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Nakama.Helpers
{
    /// <summary>
    /// فرم ارتباط با ما — پیام کاربر رو به سرور ارسال می‌کنه و در استورج ادمین ذخیره میشه.
    /// </summary>
    public class ContactUsManager : MonoBehaviour
    {
        private const string SendContactRpc = "SendContactMessageRpc";

        // ── UI References ─────────────────────────────────────────────────────────
        [Header("Form Fields")]
        [SerializeField] private TMP_InputField subjectInput;   // عنوان (اختیاری)
        [SerializeField] private TMP_InputField messageInput;   // متن پیام (اجباری)

        [Header("Buttons")]
        [SerializeField] private Button sendButton;
        [SerializeField] private Button closeButton;

        [Header("Feedback")]
        [SerializeField] private GameObject loadingIndicator;   // اسپینر یا لودینگ
        [SerializeField] private RTLTextMeshPro statusText;     // یک تکست — موفق یا خطا
        [SerializeField] private RTLTextMeshPro charCountText;  // شمارنده کاراکتر

        [Header("Panel Root (for open/close animation)")]
        [SerializeField] private RectTransform panelRoot;

        private const int MaxMessageLength = 500;
        private bool _isSending = false;

        // ─────────────────────────────────────────────────────────────────────────

        private void OnEnable()
        {
            ResetForm();

            if (sendButton  != null) sendButton.onClick.AddListener(OnSendClicked);
            if (closeButton != null) closeButton.onClick.AddListener(OnCloseClicked);
            if (messageInput != null)
                messageInput.onValueChanged.AddListener(OnMessageChanged);

            // انیمیشن باز شدن پنل
            if (panelRoot != null)
            {
                panelRoot.localScale = Vector3.zero;
                panelRoot.DOScale(Vector3.one, 0.28f).SetEase(Ease.OutBack);
            }
        }

        private void OnDisable()
        {
            if (sendButton   != null) sendButton.onClick.RemoveListener(OnSendClicked);
            if (closeButton  != null) closeButton.onClick.RemoveListener(OnCloseClicked);
            if (messageInput != null) messageInput.onValueChanged.RemoveListener(OnMessageChanged);
        }

        // ── Event Handlers ────────────────────────────────────────────────────────

        private void OnMessageChanged(string value)
        {
            if (charCountText != null)
                charCountText.text = value.Length + " / " + MaxMessageLength;

            // محدودیت طول
            if (value.Length > MaxMessageLength && messageInput != null)
                messageInput.text = value.Substring(0, MaxMessageLength);
        }

        private async void OnSendClicked()
        {
            if (_isSending) return;

            string message = messageInput != null ? messageInput.text.Trim() : "";
            if (string.IsNullOrEmpty(message))
            {
                ShowStatus("لطفاً پیام خود را بنویسید.", Color.yellow);
                return;
            }

            string subject = subjectInput != null ? subjectInput.text.Trim() : "";

            SetLoading(true);
            if (statusText != null) statusText.gameObject.SetActive(false);

            try
            {
                var payload = "{" +
                    "\"subject\":\"" + EscapeJson(subject) + "\"," +
                    "\"message\":\"" + EscapeJson(message) + "\"," +
                    "\"platform\":\"mobile\"" +
                "}";

                await NakamaManager.Instance.SendRPC(SendContactRpc, payload);

                SetLoading(false);
                ShowStatus("✅ پیام شما با موفقیت ارسال شد!", Color.green);
                Debug.Log("[ContactUs] پیام با موفقیت ارسال شد.");
                StartCoroutine(CloseAfterDelay(2f));
            }
            catch (Exception e)
            {
                SetLoading(false);
                ShowStatus("❌ ارسال ناموفق بود. دوباره تلاش کنید.", Color.red);
                Debug.LogWarning("[ContactUs] SendRPC failed: " + e.Message);
            }
        }

        private void OnCloseClicked()
        {
            if (panelRoot != null)
            {
                panelRoot.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack)
                    .OnComplete(() => gameObject.SetActive(false));
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        // ── UI Helpers ────────────────────────────────────────────────────────────

        private void SetLoading(bool active)
        {
            _isSending = active;
            if (loadingIndicator != null) loadingIndicator.SetActive(active);
            if (sendButton       != null) sendButton.interactable = !active;
        }

        private void ShowStatus(string msg, Color color)
        {
            if (statusText == null) return;
            statusText.gameObject.SetActive(true);
            statusText.text      = msg;
            statusText.color     = color;

            // fade in
            var cg = statusText.GetComponent<CanvasGroup>();
            if (cg == null) cg = statusText.gameObject.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
            cg.DOFade(1f, 0.25f);
        }

        private void ResetForm()
        {
            if (subjectInput     != null) subjectInput.text  = "";
            if (messageInput     != null) messageInput.text  = "";
            if (charCountText    != null) charCountText.text = "0 / " + MaxMessageLength;
            if (statusText       != null) statusText.gameObject.SetActive(false);
            if (loadingIndicator != null) loadingIndicator.SetActive(false);
            _isSending = false;
        }

        private IEnumerator CloseAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            OnCloseClicked();
        }

        /// جلوگیری از شکستن JSON با escape کردن کاراکترهای خاص
        private static string EscapeJson(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Replace("\\", "\\\\")
                    .Replace("\"", "\\\"")
                    .Replace("\n", "\\n")
                    .Replace("\r", "\\r")
                    .Replace("\t", "\\t");
        }
    }
}
