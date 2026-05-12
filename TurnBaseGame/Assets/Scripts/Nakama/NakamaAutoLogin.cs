using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Nakama.Helpers
{
    public class NakamaAutoLogin : MonoBehaviour
    {
        #region FIELDS

        [SerializeField] private float retryTime = 5f;
        [SerializeField] private float sceneChangeDelay = 0.5f;
        private int countTry;
        [SerializeField] TextMeshProUGUI dicconnectText;

        #endregion

        #region BEHAVIORS

        private void Start()
        {
            NakamaManager.Instance.onLoginFail    += LoginFailed;
            NakamaManager.Instance.onConnected    += OnConnected;
            NakamaManager.Instance.onLoginSuccess += OnLoginSuccess;
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
            // کمی صبر می‌کنیم تا NakamaUserManager اطلاعات کاربر رو load کنه
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
            Invoke(nameof(TryLogin), retryTime);
        }

        #endregion
    }
}
