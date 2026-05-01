using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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

        public event Action onCoinsChanged;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            NakamaUserManager.Instance.onLoaded += OnUserLoaded;
        }

        private void OnDestroy()
        {
            if (NakamaUserManager.Instance != null)
                NakamaUserManager.Instance.onLoaded -= OnUserLoaded;
        }

        private void OnUserLoaded()
        {
            RefreshFromAccount();
        }

        public void RefreshFromAccount()
        {
            var wallet = NakamaUserManager.Instance.GetWallet<WalletData>();
            Coins = wallet != null ? wallet.coins : 0;
            onCoinsChanged?.Invoke();
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
                onCoinsChanged?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogWarning("WalletManager.RefreshAsync failed: " + e.Message);
            }
        }

        public void SetCoins(int amount)
        {
            Coins = amount;
            onCoinsChanged?.Invoke();
        }
    }
}
