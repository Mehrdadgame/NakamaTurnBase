using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Nakama.Helpers
{
    public class NakamaAutoLogin : MonoBehaviour
    {
        #region FIELDS

        [SerializeField] private float retryTime = 20f;
        private int countTry;
        [SerializeField] TextMeshProUGUI dicconnectText;
        public GameObject loadingDice;
        public Button LoginWithGoogle;
        #endregion

        #region BEHAVIORS

        private void Start()
        {
            if (!PlayerPrefs.HasKey("Web3Token"))
            {
                LoginWithGoogle.gameObject.SetActive(true);
                LoginWithGoogle.onClick.AddListener(delegate { StartCoroutine(NakamaManager.Instance.LoginWithGoogle()); }); 
            }
            else
            {
              
                NakamaManager.Instance.onLoginFail += LoginFailed;
                NakamaManager.Instance.onConnected += Instance_onConnected;
                TryLogin();
                LoginWithGoogle.onClick.RemoveAllListeners();
            }


        }

        private void Instance_onConnected()
        {
            dicconnectText.text = "Loading...";
            countTry = 0;
            LoginWithGoogle.gameObject.SetActive(false);

        }



        private void OnDestroy()
        {
            NakamaManager.Instance.onLoginFail -= LoginFailed;
            NakamaManager.Instance.onConnected -= Instance_onConnected;
        }

        /// <summary>
        /// It tries to login to the server.
        /// </summary>
        private void TryLogin()
        {
            NakamaManager.Instance.LoginTask();
            loadingDice.SetActive(true);
            countTry++;
            if (countTry > 2)
            {
                dicconnectText.text = "Please check internet...";
            }
        }

        /// <summary>
        /// If the login fails, try again in a few seconds
        /// </summary>
        private void LoginFailed()
        {
            Invoke(nameof(TryLogin), retryTime);
        }

        #endregion
    }
}
