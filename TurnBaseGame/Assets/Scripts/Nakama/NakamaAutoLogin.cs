using System;
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
        public GameObject LoginWithCrypto;
        #endregion

        #region BEHAVIORS

        private void Start()
        {
            if (!PlayerPrefs.HasKey("Web3PublicKey"))
            {
                LoginWithCrypto.SetActive(true);
                LoginWithGoogle.onClick.AddListener(delegate
                {
                    loadingDice.SetActive(true);
                    ///for login and set HXD used (LoginWithSetHXDToStorage) on NakamaManager
                    //NakamaManager.Instance.LoginWithSetHXDToStorage();
                    StartCoroutine(NakamaManager.Instance.LoginWithGoogle());
                    LoginWithGoogle.interactable = false;
                });
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
            LoginWithCrypto.SetActive(false);

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
            NakamaManager.Instance.LoginCustome();
            //if (!PlayerPrefs.HasKey("IdToken"))
            //{

            //    NakamaManager.Instance.LoginWithUdid();
            //    PlayerPrefs.SetString("IdToken", SystemInfo.deviceUniqueIdentifier);
            //}
            //else
            //{
            //    NakamaManager.Instance.LoginWithUdid();
            //}
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
            loadingDice.SetActive(false);
        }

        #endregion
    }
}
