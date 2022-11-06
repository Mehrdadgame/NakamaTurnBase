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
        [SerializeField] private NakamaCollectionObject nakamaCollectionObjectWallet = NakamaStorageManager.Instance.autoLoadObjects[1];
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
            NakamaStorageManager.Instance.onLoadedData += Instance_onLoadedData;
        }

        private void Instance_onLoadedData()
        {
            var collction = nakamaCollectionObjectWallet.GetValue<WalletData>();

            Debug.Log(collction.hxdAmount);

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
            NakamaManager.Instance.LoginWithUdid();
            countTry++;
            if (countTry>2)
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
