using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Nakama.Helpers
{
    public class NakamaAutoLogin : MonoBehaviour
    {
        #region FIELDS

        [SerializeField] private float retryTime = 5f;
        [SerializeField] private float sceneChangeDelay = 0.5f;
        private int countTry;
        [SerializeField] TextMeshProUGUI dicconnectText;

        [Header("Email Login (optional)")]
        [SerializeField] private Button           emailLoginButton;
        [SerializeField] private EmailLoginPanel  emailLoginPanel;

        #endregion

        #region BEHAVIORS

        private void Start()
        {
            NakamaManager.Instance.onLoginFail    += LoginFailed;
            NakamaManager.Instance.onConnected    += OnConnected;
            NakamaManager.Instance.onLoginSuccess += OnLoginSuccess;

            if (emailLoginButton != null)
                emailLoginButton.onClick.AddListener(ShowEmailLogin);

            TryLogin();
        }

        private void OnDestroy()
        {
            NakamaManager.Instance.onLoginFail    -= LoginFailed;
            NakamaManager.Instance.onConnected    -= OnConnected;
            NakamaManager.Instance.onLoginSuccess -= OnLoginSuccess;
        }

        private void OnConnected()
        {
            if (dicconnectText != null)
                dicconnectText.text = "در حال بارگذاری...";
            countTry = 0;
        }

        private void OnLoginSuccess()
        {
            if (dicconnectText != null)
                dicconnectText.text = "متصل شد!";
            StartCoroutine(GoToHome());
        }

        private IEnumerator GoToHome()
        {
            yield return new WaitForSeconds(sceneChangeDelay);
            SceneManager.LoadScene((int)NinjaBattle.General.Scenes.Home);
        }

        private void TryLogin()
        {
            NakamaManager.Instance.LoginWithUdid();
            countTry++;
            if (countTry > 2 && dicconnectText != null)
                dicconnectText.text = "اینترنت خود را بررسی کنید...";
        }

        private void LoginFailed()
        {
            // اگه EmailLoginPanel داره منتظر جواب هست، retry نزن
            if (EmailLoginPanel.Instance != null && EmailLoginPanel.Instance.IsWaitingForLogin)
                return;
            Invoke(nameof(TryLogin), retryTime);
        }

        public void ShowEmailLogin()
        {
            if (emailLoginPanel != null)
                emailLoginPanel.Show();
        }

        #endregion
    }
}
