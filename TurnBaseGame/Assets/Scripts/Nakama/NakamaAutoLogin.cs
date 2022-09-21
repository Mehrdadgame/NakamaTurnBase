using TMPro;
using UnityEngine;

namespace Nakama.Helpers
{
    public class NakamaAutoLogin : MonoBehaviour
    {
        #region FIELDS

        [SerializeField] private float retryTime = 5f;
        private int countTry;
        [SerializeField] TextMeshProUGUI dicconnectText;
        #endregion

        #region BEHAVIORS

        private void Start()
        {
            NakamaManager.Instance.onLoginFail += LoginFailed;
            NakamaManager.Instance.onConnected += Instance_onConnected;
            TryLogin();
        }

        private void Instance_onConnected()
        {
            dicconnectText.text = "Loading...";
            countTry = 0;
        }

        private void OnDestroy()
        {
            NakamaManager.Instance.onLoginFail -= LoginFailed;
            NakamaManager.Instance.onConnected -= Instance_onConnected;
        }

        private void TryLogin()
        {
            NakamaManager.Instance.LoginWithUdid();
            countTry++;
            if (countTry>2)
            {
                dicconnectText.text = "please check internet...";
            }
        }

        private void LoginFailed()
        {
            Invoke(nameof(TryLogin), retryTime);
        }

        #endregion
    }
}
