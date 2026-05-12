using System;
using System.Collections;
using Bazaar.Data;
using Bazaar.Poolakey;
using Bazaar.Poolakey.Data;
using RTLTMPro;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Nakama.Helpers
{
    [Serializable]
    public class CoinPackProduct
    {
        public string productId;
        public int coinsAmount;
        public string priceLabel;   // e.g. "۱۰,۰۰۰ تومان" — set in Inspector
        public Button buyButton;
        public RTLTextMeshPro coinsLabel;
        public RTLTextMeshPro priceText;
    }

    [Serializable]
    internal class CoinPurchaseResult { public bool success; public int coinsAwarded; public string error; }

    public class CoinShopManager : MonoBehaviour
    {
        private const string RsaPublicKey = "MIHNMA0GCSqGSIb3DQEBAQUAA4G7ADCBtwKBrwC0vw7ZN00/aJsQ/grNtpgM2UrOesv0nNRtVjYXHS4hSI9xBlacWrAtGjJ46LyfIooBxD3REprgv5d8xZPi7s0TTUW/VtbW5w0pgiEXSip0TGE5S2c41pdxPsqUe3tnSnsxdHoeq7Lnoa21JNLkqLuPShNCSRVjdlF56VxMTL2OKV+vLh3sscj0gkNzBzP21eyeI7OGXsa7Fe6crJePmOUGWWxg6eWe0DF6Nkf3XfkCAwEAAQ==";
        private const string AddCoinsRpcId = "AddCoinsRpc";

        [Header("Products (assign in Inspector)")]
        [SerializeField] private CoinPackProduct[] products;

        [Header("Feedback")]
        [SerializeField] private RTLTextMeshPro statusText;
        [SerializeField] private GameObject loadingOverlay;

        private Payment _payment;
        private bool _connected;
        private bool _busy;

        // ── Lifecycle ─────────────────────────────────────────────────────────────

        private void Awake()
        {
            var securityCheck = SecurityCheck.Enable(RsaPublicKey);
            _payment = new Payment(new PaymentConfiguration(securityCheck));

            for (int i = 0; i < products.Length; i++)
            {
                int idx = i;
                if (products[idx].buyButton != null)
                    products[idx].buyButton.onClick.AddListener(() => OnBuyClicked(idx));

                if (products[idx].coinsLabel != null)
                    products[idx].coinsLabel.text = FormatCoins(products[idx].coinsAmount);

                if (products[idx].priceText != null && !string.IsNullOrEmpty(products[idx].priceLabel))
                    products[idx].priceText.text = PersianTextUtils.FixRTLPriceLabel(products[idx].priceLabel);
            }
        }

        private IEnumerator Start()
        {
            SetStatus("در حال اتصال...", Color.white);
            yield return _payment.Connect(OnConnectResult);
        }

        private void OnDestroy()
        {
            _payment?.Disconnect();
        }

        private void OnEnable()
        {
            SetStatus("", Color.white);

        }

        // ── Connect ───────────────────────────────────────────────────────────────

        private void OnConnectResult(Result<bool> result)
        {
            if (result.status == Status.Success)
            {
                _connected = true;
                SetStatus("", Color.white);
                SetButtonsInteractable(true);
            }
            else
            {
                SetStatus("اتصال به بازار ناموفق بود.", Color.red);
                Debug.LogWarning("[CoinShop] Connect failed: " + result.message);
            }
        }

        // ── Buy ───────────────────────────────────────────────────────────────────

        private async void OnBuyClicked(int index)
        {
            if (!_connected)
            {
                SetStatus("اتصال به بازار برقرار نیست.", Color.red);
                return;
            }
            if (_busy) return;

            var product = products[index];
            _busy = true;
            SetButtonsInteractable(false);
            SetStatus("در حال خرید...", Color.white);
            SetLoading(true);

            try
            {
                var result = await _payment.Purchase(product.productId, SKUDetails.Type.inApp);

                if (result.status != Status.Success)
                {
                    if (result.status == Status.Canceled)
                        SetStatus("خرید لغو شد.", Color.yellow);
                    else
                        SetStatus("خرید ناموفق: " + result.message, Color.red);
                    return;
                }

                var info = result.data;

                // Consume first so Bazaar allows purchasing again
                var consumeResult = await _payment.Consume(info.purchaseToken);
                if (consumeResult.status != Status.Success)
                    Debug.LogWarning("[CoinShop] Consume failed (non-critical): " + consumeResult.message);

                // Immediately show coins in UI
                if (WalletManager.Instance != null)
                    WalletManager.Instance.SetCoins(WalletManager.Instance.Coins + product.coinsAmount);

                SetStatus("+" + FormatCoins(product.coinsAmount) + " کوین دریافت شد!", new Color(0.25f, 1f, 0.25f));

                // Sync coins to server wallet in background (fire-and-forget)
                _ = SyncCoinsToServer(product.coinsAmount);
            }
            catch (Exception e)
            {
                Debug.LogWarning("[CoinShop] Purchase error: " + e.Message);
                SetStatus("خطا: " + e.Message, Color.red);
            }
            finally
            {
                _busy = false;
                SetButtonsInteractable(true);
                SetLoading(false);
            }
        }

        private async System.Threading.Tasks.Task SyncCoinsToServer(int coins)
        {
            try
            {
                var payload = "{\"coins\":" + coins + "}";
                Debug.Log("[CoinShop] Syncing to server: " + payload);
                var rpc = await NakamaManager.Instance.SendRPC(AddCoinsRpcId, payload);
                if (rpc == null || string.IsNullOrEmpty(rpc.Payload))
                {
                    Debug.LogWarning("[CoinShop] Server sync: no response");
                    return;
                }
                var res = rpc.Payload.Deserialize<CoinPurchaseResult>();
                if (res != null && res.success)
                {
                    Debug.Log("[CoinShop] Server confirmed coins: " + res.coinsAwarded);
                    // Refresh from server to get authoritative balance
                    if (WalletManager.Instance != null)
                        await WalletManager.Instance.RefreshAsync();
                }
                else
                {
                    Debug.LogWarning("[CoinShop] Server sync failed: " + (res?.error ?? "unknown"));
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("[CoinShop] SyncCoins error: " + e.Message);
            }
        }

        // ── Helpers ───────────────────────────────────────────────────────────────

        private void SetStatus(string msg, Color color)
        {
            if (statusText == null) return;
            statusText.text = msg;
            statusText.color = color;
        }

        private void SetButtonsInteractable(bool on)
        {
            foreach (var p in products)
                if (p.buyButton != null) p.buyButton.interactable = on;
        }

        private void SetLoading(bool on)
        {
            if (loadingOverlay != null) loadingOverlay.SetActive(on);
        }

        private static string FormatCoins(int amount) =>
            PersianTextUtils.FormatNumber(amount);
    }
}
