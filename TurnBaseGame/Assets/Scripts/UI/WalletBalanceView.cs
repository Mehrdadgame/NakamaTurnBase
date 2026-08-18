using System.Collections;
using Nakama.Helpers;
using RTLTMPro;
using UnityEngine;

namespace NinjaBattle.UI
{
    public sealed class WalletBalanceView : MonoBehaviour
    {
        [SerializeField] private RTLTextMeshPro valueText;

        public void Configure(RTLTextMeshPro target)
        {
            valueText = target;
        }

        private void OnEnable()
        {
            if (WalletManager.Instance == null)
            {
                StartCoroutine(WaitForWallet());
                return;
            }

            Subscribe();
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            if (WalletManager.Instance != null)
                WalletManager.Instance.onCoinsChanged -= Refresh;
        }

        private IEnumerator WaitForWallet()
        {
            while (WalletManager.Instance == null)
                yield return null;

            Subscribe();
        }

        private void Subscribe()
        {
            WalletManager.Instance.onCoinsChanged -= Refresh;
            WalletManager.Instance.onCoinsChanged += Refresh;
            Refresh(WalletManager.Instance.Coins);
        }

        private void Refresh(int amount)
        {
            if (valueText != null)
                valueText.text = PersianTextUtils.FormatNumber(amount);
        }
    }
}
