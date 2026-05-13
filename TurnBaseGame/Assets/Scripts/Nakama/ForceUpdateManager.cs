using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Nakama.Helpers
{
    [Serializable]
    public class AppVersionResponse
    {
        public string requiredVersion;
        public string updateUrl;
        public bool   required;
    }

    /// <summary>
    /// On login, fetches the required app version from the server.
    /// If Application.version differs, shows a blocking popup with an update button.
    /// Inspector: assign popupPanel, messageText, updateButton.
    /// </summary>
    public class ForceUpdateManager : MonoBehaviour
    {
        private const string GetAppVersionRpc = "GetAppVersionRpc";

        [SerializeField] private GameObject      popupPanel;
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private Button          updateButton;

        private void Start()
        {
            if (popupPanel != null)
                popupPanel.SetActive(false);

            StartCoroutine(CheckAfterLogin());
        }

        private IEnumerator CheckAfterLogin()
        {
            while (NakamaManager.Instance == null || !NakamaManager.Instance.IsLoggedIn)
                yield return null;

            CheckVersionAsync();
        }

        private async void CheckVersionAsync()
        {
            try
            {
                var result = await NakamaManager.Instance.SendRPC(GetAppVersionRpc, "{}");
                var data   = result.Payload.Deserialize<AppVersionResponse>();

                if (data == null || !data.required || string.IsNullOrEmpty(data.requiredVersion))
                    return;

                if (IsVersionUpToDate(Application.version, data.requiredVersion))
                    return;

                ShowPopup(data.updateUrl);
            }
            catch (Exception e)
            {
                Debug.LogWarning("ForceUpdateManager: version check failed — " + e.Message);
            }
        }

        // Returns true if deviceVersion >= requiredVersion (no update needed).
        private static bool IsVersionUpToDate(string deviceVersion, string requiredVersion)
        {
            var d = ParseVersion(deviceVersion);
            var r = ParseVersion(requiredVersion);
            for (int i = 0; i < Mathf.Max(d.Length, r.Length); i++)
            {
                var dv = i < d.Length ? d[i] : 0;
                var rv = i < r.Length ? r[i] : 0;
                if (dv > rv) return true;
                if (dv < rv) return false;
            }
            return true; // equal
        }

        private static int[] ParseVersion(string v)
        {
            var parts = (v ?? "0").Split('.');
            var result = new int[parts.Length];
            for (int i = 0; i < parts.Length; i++)
                int.TryParse(parts[i], out result[i]);
            return result;
        }

        private void ShowPopup(string updateUrl)
        {
            if (popupPanel == null) return;

            if (updateButton != null)
            {
                updateButton.onClick.RemoveAllListeners();
                updateButton.onClick.AddListener(() => Application.OpenURL(updateUrl));
            }

            if (messageText != null)
                messageText.text = "نسخه جدیدی از بازی موجود است.\nبرای ادامه بازی لطفا بازی را به‌روزرسانی کنید.";

            popupPanel.SetActive(true);

            // Animate in
            var rect = popupPanel.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.localScale = Vector3.zero;
                rect.DOScale(Vector3.one, 0.28f).SetEase(Ease.OutBack);
            }

            var cg = popupPanel.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.alpha = 0f;
                cg.DOFade(1f, 0.22f);
            }
        }
    }
}
