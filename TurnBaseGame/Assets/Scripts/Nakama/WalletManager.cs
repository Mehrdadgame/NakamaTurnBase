using System;
using System.Collections;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Nakama.Helpers
{
    [Serializable]
    public class WalletData
    {
        public int coins;
    }

    public class WalletManager : MonoBehaviour
    {
        public static WalletManager Instance { get; private set; }

        public int Coins { get; private set; }

        public event Action<int> onCoinsChanged;
        public TextMeshProUGUI CoinText;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            StartCoroutine(WaitAndLoad());
            // بعد از هر re-login هم wallet رو refresh کن
            NakamaManager.Instance.onLoginSuccess += OnLoginSuccess;
        }

        private void OnDestroy()
        {
            if (NakamaManager.Instance != null)
                NakamaManager.Instance.onLoginSuccess -= OnLoginSuccess;
        }

        private async void OnLoginSuccess()
        {
            await RefreshAsync();
        }

        // Polls every frame until NakamaUserManager has finished loading the account,
        // then reads the wallet. Works regardless of whether login happened before or after this Start().
        private IEnumerator WaitAndLoad()
        {
            while (NakamaUserManager.Instance == null || !NakamaUserManager.Instance.LoadingFinished)
                yield return null;

            // Always fetch from server so we get up-to-date balance after a match
            var task = RefreshAsync();
            yield return new WaitUntil(() => task.IsCompleted);
        }

        public void RefreshFromAccount()
        {
            var wallet = NakamaUserManager.Instance.GetWallet<WalletData>();
            Coins = wallet != null ? wallet.coins : 0;
            Debug.Log("WalletManager: Loaded wallet with " + Coins + " coins.");
            onCoinsChanged?.Invoke(Coins);
            // if (CoinText != null)
            //     CoinText.text = Coins.ToString();
        }

        public async Task RefreshAsync()
        {
            try
            {
                var account = await NakamaManager.Instance.Client.GetAccountAsync(NakamaManager.Instance.Session);
                var raw = account.Wallet;
                if (!string.IsNullOrEmpty(raw))
                {
                    var wallet = raw.Deserialize<WalletData>();
                    Coins = wallet != null ? wallet.coins : 0;
                }
                else
                {
                    Coins = 0;
                }
                onCoinsChanged?.Invoke(Coins);
            }
            catch (Exception e)
            {
                Debug.LogWarning("WalletManager.RefreshAsync failed: " + e.Message);
            }
        }

        public void SetCoins(int amount)
        {
            Coins = amount;
            onCoinsChanged?.Invoke(Coins);
        }
    }
}
